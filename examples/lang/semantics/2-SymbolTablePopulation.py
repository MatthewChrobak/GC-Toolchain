from NamespaceHelper import *

registerCounter = 0

def GetRegister():
    global registerCounter
    register = "%" + str(registerCounter)
    registerCounter += 1
    return register

def ResetRegisters():
    global registerCounter
    registerCounter = 0

def getPropertyValueIfExists(node, key, defaultValue):
    if node.Contains(key):
        return node[key]["value"]
    else:
        return defaultValue

def preorder_class(node):
    class_name = node["class_name"]["value"]

    row, id = symboltable.GetOrCreate(node["pstid"]).CreateRow()
    node["rowid"] = id
    row["name"] = class_name
    row["entity_type"] = "class"

    access_modifier_key = "access_modifier"
    access_modifier_value = getPropertyValueIfExists(node, access_modifier_key, "default")
    row[access_modifier_key] = access_modifier_value

    static_key = "static"
    static_value = node.Contains(static_key)
    row[static_key] = static_value

def preorder_field(node):
    field_name = node["field_name"]["value"]

    row = node["field_name"]["row"]
    column = node["field_name"]["column"]

    class_symboltable = symboltable.GetOrCreate(node["pstid"])

    if class_symboltable.RowExistsWhere("name", field_name, "entity_type", "field"):
        raise Exception("Field {0}-{1} at {2!s}:{3!s} is already defined in {4}".format("field", field_name, row, column, currentNamespaceID()))

    row, id = class_symboltable.CreateRow()
    node["rowid"] = id
    row["name"] = field_name
    row["entity_type"] = "field"

    static_key = "static"
    static_value = node.Contains(static_key)
    row[static_key] = static_value

    access_modifier_key = "access_modifier"
    access_modifier_value = getPropertyValueIfExists(node, access_modifier_key, "default")
    row[access_modifier_key] = access_modifier_value

def preorder_function(node):
    ResetRegisters()
    function_name = node["function_name"]["value"]

    row = node["function_name"]["row"]
    column = node["function_name"]["column"]

    class_symboltable = symboltable.GetOrCreate(node["pstid"])

    if class_symboltable.RowExistsWhere("name", function_name, "entity_type", "function"):
        raise Exception("Function {0}-{1} at {2!s}:{3!s} is already defined in {4}".format("function", function_name, row, column, currentNamespaceID()))

    row, id = class_symboltable.CreateRow()
    node["rowid"] = id
    row["name"] = function_name
    row["entity_type"] = "function"

    static_key = "static"
    static_value = node.Contains(static_key)
    row[static_key] = static_value

    access_modifier_key = "access_modifier"
    access_modifier_value = getPropertyValueIfExists(node, access_modifier_key, "default")
    row[access_modifier_key] = access_modifier_value

def preorder_function_parameter(node):
    parameter_name = node["parameter_name"]["value"]

    row = node["parameter_name"]["row"]
    column = node["parameter_name"]["column"]

    function_symboltable = symboltable.GetOrCreate(node["pstid"])

    if function_symboltable.RowExistsWhere("name", parameter_name):
        raise Exception("Function parameter {0} in {1} at {2!s}:{3!s} already exists".format(parameter_name, currentNamespaceID(), row, column))

    row, id = function_symboltable.CreateRow()
    node["rowid"] = id
    row["name"] = parameter_name
    row["entity_type"] = "parameter"

def postorder_declaration_statement(node):
    variable_name = node["variable_name"]["value"]

    row = node["variable_name"]["row"]
    column = node["variable_name"]["column"]

    function_symboltable = symboltable.GetOrCreate(node["pstid"])

    if function_symboltable.RowExistsWhere("name", variable_name):
        raise Exception("Variable {0} at {1!s}:{2!s} already exists in the scope".format(variable_name, row, column))

    row, id = function_symboltable.CreateRow()
    node["rowid"] = id
    row["name"] = variable_name
    row["entity_type"] = "variable"
    row["register"] = GetRegister()

def postorder_integer(node):
    parentSymbolTable = symboltable.GetOrCreate(node["pstid"])
    label = node["value"]
    register = GetRegister()
    
    row, id = parentSymbolTable.CreateRow()
    node["rowid"] = id
    node["register"] = register
    node["label"] = label

    row["entity_type"] = "subcalculationstackspace"
    row["register"] = register
    row["label"] = label

def postorder_rvalue(node):
    parentSymbolTable = symboltable.GetOrCreate(node["pstid"])
    register = GetRegister()
    label = ""

    if node.Contains("integer"):
        label = node["integer"]["label"]

    if node.Contains("lvalue"):
        label = node["lvalue"]["label"]

    if node.Contains("expression"):
        label = "(" + node["expression"]["label"] + ")"

    row, id = parentSymbolTable.CreateRow()
    node["rowid"] = id
    node["register"] = register
    node["label"] = label

    row["entity_type"] = "subcalculationstackspace"
    row["register"] = register
    row["label"] = label

def postorder_expression(node):
    handlePostOrderExpression(node)

def postorder_lhs(node):
    handlePostOrderExpression(node)

def postorder_rhs(node):
    handlePostOrderExpression(node)

def handlePostOrderExpression(node):

    parentSymbolTable = symboltable.GetOrCreate(node["pstid"])
    register = GetRegister()
    label = ""

    row, id = parentSymbolTable.CreateRow()
    node["rowid"] = id
    node["register"] = register

    if node.Contains("lhs") and node.Contains("rhs"):
        lhs = node["lhs"]
        rhs = node["rhs"]

        operator = ""
        operator = getPropertyValueIfExists(node, "geq", operator)
        operator = getPropertyValueIfExists(node, "leq", operator)
        operator = getPropertyValueIfExists(node, "lt", operator)
        operator = getPropertyValueIfExists(node, "gt", operator)
        operator = getPropertyValueIfExists(node, "gt", operator)
        operator = getPropertyValueIfExists(node, "neq", operator)
        operator = getPropertyValueIfExists(node, "eq", operator)
        operator = getPropertyValueIfExists(node, "+", operator)
        operator = getPropertyValueIfExists(node, "-", operator)
        operator = getPropertyValueIfExists(node, "*", operator)
        operator = getPropertyValueIfExists(node, "/", operator)
        operator = getPropertyValueIfExists(node, "%", operator)
        operator = getPropertyValueIfExists(node, "<<", operator)
        operator = getPropertyValueIfExists(node, ">>", operator)
        operator = getPropertyValueIfExists(node, "&&", operator)
        operator = getPropertyValueIfExists(node, "||", operator)
        operator = getPropertyValueIfExists(node, "&", operator)
        operator = getPropertyValueIfExists(node, "|", operator)

        label = "(" + lhs["label"] + operator + rhs["label"] + ")"
    else:
        label = node["rvalue"]["label"]
    node["label"] = label

    row["entity_type"] = "subcalculationstackspace"
    row["register"] = register
    row["label"] = label

previousLValueComponent = None
def postorder_lvalue(node):
    global previousLValueComponent

    if not node.Contains("allocate_register"):
        return

    previousLValueComponent = None

    parentSymbolTable = symboltable.GetOrCreate(node["pstid"])
    register = GetRegister()
    label = ""

    lvalue_components = node.AsArray("lvalue_component")
    label = lvalue_components[len(lvalue_components) - 1]["label"]

    row, id = parentSymbolTable.CreateRow()
    node["rowid"] = id
    node["register"] = register
    node["label"] = label

    row["entity_type"] = "subcalculationstackspace"
    row["label"] = label
    row["register"] = register

def postorder_lvalue_component(node):
    global previousLValueComponent

    if not node.Contains("allocate_register"):
        return

    parentSymbolTable = symboltable.GetOrCreate(node["pstid"])
    register = GetRegister()
    label = ""

    if previousLValueComponent is not None:
        label += previousLValueComponent["label"]
    previousLValueComponent = node

    access = getPropertyValueIfExists(node, "dot", "")
    access = getPropertyValueIfExists(node, "arrow", access)
    access = getPropertyValueIfExists(node, "scope", access)

    if access is not "":
        label += access

    if node.Contains("identifier"):
        label += node["identifier"]["value"]

    row, id = parentSymbolTable.CreateRow()
    node["rowid"] = id
    node["register"] = register
    node["label"] = label

    row["entity_type"] = "subcalculationstackspace"
    row["label"] = label
    row["register"] = register