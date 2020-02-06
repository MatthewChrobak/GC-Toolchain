from NamespaceHelper import *

def getPropertyValueIfExists(node, key, defaultValue):
    if node.Contains(key):
        return node[key]["value"]
    else:
        return defaultValue

def currentSymbolTable():
    return symboltable.GetOrCreate(currentNamespaceID())

def preorder_global(node):
    enterNamespace("global")

def postorder_global(node):
    leaveNamespace()

def preorder_namespace(node):
    if node.Contains("namespace_name"):
        id = node["namespace_name"]["value"]
        enterNamespace(id)

def postorder_namespace(node):
    if node.Contains("namespace_name"):
        leaveNamespace()

def preorder_class(node):
    parent_symboltable = currentSymbolTable()
    class_name = node["class_name"]["value"]
    enterNamespace(class_name)

    row = parent_symboltable.CreateRow()
    row["name"] = class_name
    row["type"] = "class"

    access_modifier_key = "access_modifier"
    access_modifier_value = getPropertyValueIfExists(node, access_modifier_key, "default")
    row[access_modifier_key] = access_modifier_value

    static_key = "static"
    static_value = node.Contains(static_key)
    row[static_key] = static_value

def postorder_class(node):
    leaveNamespace()

def preorder_field(node):
    field_name = node["field_name"]["value"]
    field_type = node["field_type"]["value"]

    row = node["field_name"]["row"]
    column = node["field_name"]["column"]

    class_symboltable = currentSymbolTable()

    if class_symboltable.RowExistsWhere("name", field_name, "type", field_type):
        raise Exception("Field {0}-{1} at {2!s}:{3!s} is already defined in {4}", field_type, field_name, row, column, currentNamespaceID())

    row = class_symboltable.CreateRow()
    row["name"] = field_name
    row["type"] = field_type
    row["member_type"] = "field"

    static_key = "static"
    static_value = node.Contains(static_key)
    row[static_key] = static_value

    access_modifier_key = "access_modifier"
    access_modifier_value = getPropertyValueIfExists(node, access_modifier_key, "default")
    row[access_modifier_key] = access_modifier_value

def preorder_function(node):
    function_name = node["function_name"]["value"]
    return_type = node["return_type"]["value"]

    row = node["function_name"]["value"]
    column = node["function_name"]["column"]

    class_symboltable = currentSymbolTable()
    enterNamespace(function_name)

    if class_symboltable.RowExistsWhere("name", function_name, "type", return_type):
        raise Exception("Function {0}-{1} at {2!s}:{3!s} is already defined in {4}", return_type, function_name, row, column, currentNamespaceID())

    row = class_symboltable.CreateRow()
    row["name"] = function_name
    row["type"] = return_type
    row["member_type"] = "function"

    static_key = "static"
    static_value = node.Contains(static_key)
    row[static_key] = static_value

    access_modifier_key = "access_modifier"
    access_modifier_value = getPropertyValueIfExists(node, access_modifier_key, "default")
    row[access_modifier_key] = access_modifier_value

def postorder_function(node):
    leaveNamespace()

def preorder_function_parameter(node):
    parameter_name = node["parameter_name"]["value"]
    parameter_type = node["parameter_type"]["value"]

    row = node["parameter_name"]["row"]
    column = node["parameter_name"]["column"]

    function_symboltable = currentSymbolTable()

    if function_symboltable.RowExistsWhere("name", parameter_name, "type", parameter_type):
        raise Exception("Function parameter {0}-{1} in {2} at {3!s}:{4!s} already exists", parameter_type, parameter_name, currentNamespaceID(), row, column)

    row = function_symboltable.CreateRow()
    row["name"] = parameter_name
    row["type"] = parameter_type