from NamespaceHelper import *

def createSubCalculationStackSpace(node, label):
    pst = symboltable.GetOrCreate(getPSTID(node))

    row, id = pst.CreateRow()
    node["rowid"] = id
    node["label"] = label

    row["entity_type"] = "subcalculationstackspace"
    row["label"] = label

def createRowInPSTID_BuiltIn(entityType, parentSTID, entityName):
    pst = symboltable.GetOrCreate(parentSTID)

    if pst.RowExistsWhere("name", entityName, "entity_type", entityType):
        Error("The built in {0} '{1}' is already defined in {2}".format(entityType, entityName, parentSTID))
        return
    
    row, id = pst.CreateRow()
    row["rowid"] = id
    row["name"] = entityName
    row["entity_type"] = entityType
    return row

def createRowInPSTID(node, entityName, entityType, loc):
    pstid = getPSTID(node)
    pst = symboltable.GetOrCreate(pstid)

    if pst.RowExistsWhere("name", entityName, "entity_type", entityType):
        Error("The {0} '{1}' at {2!s}:{3!s} is already defined in {4}".format(entityType, entityName, loc[0], loc[2], pstid))
        return None
    else:
        row, id = pst.CreateRow()
        node["rowid"] = id
        row["name"] = entityName
        row["entity_type"] = entityType
        return row
def getPropertyValueIfExists(node, key, defaultValue):
    if node.Contains(key):
        return node[key]["value"]
    else:
        return defaultValue

def extractAccessModifierAndStatic(node, row):
    access_modifier_key = "access_modifier"
    row[access_modifier_key] = getPropertyValueIfExists(node, access_modifier_key, "default")

    static_key = "static"
    row[static_key] = node.Contains(static_key)

def preorder_global(node):

    createRowInPSTID_BuiltIn("function", "::global", "print_str")
    st = symboltable.GetOrCreate("::global::print_str")
    row, id = st.CreateRow()
    row["entity_type"] = "variable"
    row["is_parameter"] = True
    row["parameter_index"] = 0
    row["type"] = "char*"

    createRowInPSTID_BuiltIn("function", "::global", "print_int")
    st = symboltable.GetOrCreate("::global::print_int")
    row, id = st.CreateRow()
    row["entity_type"] = "variable"
    row["is_parameter"] = True
    row["parameter_index"] = 0
    row["type"] = "int"

    createRowInPSTID_BuiltIn("function", "::global", "print_float")
    st = symboltable.GetOrCreate("::global::print_float")
    row, id = st.CreateRow()
    row["entity_type"] = "variable"
    row["is_parameter"] = True
    row["parameter_index"] = 0
    row["type"] = "float"

def preorder_class(node):
    className, loc = getNodeValues(node, "class_name")
    row = createRowInPSTID(node, className, "class", loc)

    if row is not None:
        extractAccessModifierAndStatic(node, row)

def preorder_field(node):
    fieldName, loc = getNodeValues(node, "field_name")
    row = createRowInPSTID(node, fieldName, "field", loc)

    if row is not None:
        extractAccessModifierAndStatic(node, row)

parameterIndex = 0
def preorder_function(node, isFree=False):
    global parameterIndex
    parameterIndex = 0
    functionName, loc = getNodeValues(node, "function_name")
    row = createRowInPSTID(node, functionName, "function", loc)

    if row is not None:
        extractAccessModifierAndStatic(node, row)
        row["is_free"] = isFree

def preorder_free_function(node):
    preorder_function(node, True)

def preorder_function_parameter(node):
    global parameterIndex
    parameterName, loc = getNodeValues(node, "parameter_name")
    row = createRowInPSTID(node, parameterName, "variable", loc)
    row["is_parameter"] = True
    row["parameter_index"] = parameterIndex
    parameterIndex += 1

def postorder_declaration_statement(node):
    variableName, loc = getNodeValues(node, "variable_name")

    function_st = symboltable.GetOrCreate(node["pstid"])

    level = 0
    st = function_st
    while True:
        if st.GetMetaData("symboltable_type") != "local" and st.GetMetaData("symboltable_type") != "function":
            break

        if st.RowExistsWhere("name", variableName):
            Error("Variable {0} at {1!s}:{2!s} already exists in the scope".format(variableName, loc[0], loc[1]))
            return

        level += 1
        st = symboltable.GetOrCreate(getParentNamespaceID(level, node["pstid"]))

    row, id = function_st.CreateRow()
    node["rowid"] = id
    row["name"] = variableName
    row["entity_type"] = "variable"

def postorder_integer(node):
    createSubCalculationStackSpace(node, node["value"])

def postorder_string(node):
    createSubCalculationStackSpace(node, node["value"])

def postorder_char(node):
    createSubCalculationStackSpace(node, node["value"])

def postorder_rvalue(node):
    label = ""
    if node.Contains("positive"):
        label += "+"
    if node.Contains("negative"):
        label += "-"
    if node.Contains("address_of"):
        label += "&"
        node["pointer_operator"] = "address_of"
    if node.Contains("value_of"):
        label += "*"
        node["pointer_operator"] = "value_of"

    if node.Contains("integer"):
        label += node["integer"]["label"]
    if node.Contains("char"):
        label += node["char"]["label"]
    if node.Contains("string"):
        label += node["string"]["label"]

    if node.Contains("lvalue"):
        label += node["lvalue"]["label"]
    if node.Contains("expression"):
        label += "(" + node["expression"]["label"] + ")"
    if node.Contains("rvalue"):
        label += node["rvalue"]["label"]

    createSubCalculationStackSpace(node, label)

def postorder_lvalue(node):
    components = node.AsArray("lvalue_component")
    label = components[len(components) - 1]["label"]
    createSubCalculationStackSpace(node, label)
    
def postorder_lvalue_component(node):
    if not node.Contains("allocate_register"):
        return

    label = ""
    access = getPropertyValueIfExists(node, "dot", "")
    access = getPropertyValueIfExists(node, "arrow", access)
    access = getPropertyValueIfExists(node, "scope", access)

    if access is not "":
        label += access

    if node.Contains("identifier"):
        label += node["identifier"]["value"]

    if node.Contains("function_identifier"):
        label += node["function_identifier"]["value"]

    createSubCalculationStackSpace(node, label)

def postorder_expression(node):
    handlePostOrderExpression(node)
def postorder_lhs(node):
    handlePostOrderExpression(node)
def postorder_rhs(node):
    handlePostOrderExpression(node)

def handlePostOrderExpression(node):
    label = ""

    if node.Contains("lhs") and node.Contains("rhs"):
        lhs = node["lhs"]
        rhs = node["rhs"]

        operator = node["operator"]["value"]
        node["operator"] = operator
        label = "(" + lhs["label"] + operator + rhs["label"] +")"
    else:
        label = node["rvalue"]["label"]

    createSubCalculationStackSpace(node, label)