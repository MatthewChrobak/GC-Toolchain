from NamespaceHelper import *

def preorder_global(node):
    enterNamespace("global")

def postorder_global(node):
    leaveNamespace()

def preorder_namespace(node):
    if node.Contains("namespace_name"):
        id = node["namespace_name"]["value"]
        enterNamespace(id)
        symboltable.GetOrCreate(currentNamespaceID())

def postorder_namespace(node):
    if node.Contains("namespace_name"):
        leaveNamespace()

def preorder_class(node):   
    classname_node = node["class_name"]
    parent_namespace_id = currentNamespaceID()

    class_name = classname_node["value"]
    class_row = str(classname_node["row"])
    class_column = str(classname_node["column"])

    enterNamespace(class_name)
    class_symboltable_id = parent_namespace_id + "::" + class_name
    
    if symboltable.Exists(class_symboltable_id):
        raise Exception("The class {0} at {1}:{2} is already defined".format(class_symboltable_id, class_row, class_column))

    symboltable.GetOrCreate(class_symboltable_id)

    node["pstid"] = parent_namespace_id
    node["stid"] = class_symboltable_id

def postorder_class(node):
    leaveNamespace()

def preorder_function(node):
    function_name = node["function_name"]["value"]
    row = node["function_name"]["row"]
    column = node["function_name"]["column"]
    parent_namespace_id = currentNamespaceID()
    function_symboltable_id = currentNamespaceID() + "::" + function_name

    enterNamespace(function_name)

    if symboltable.Exists(function_symboltable_id):
        raise Exception("The function {0} at {1!s}:{2!s} is already defined".format(function_symboltable_id, row, column))

    symboltable.GetOrCreate(function_symboltable_id)

    node["pstid"] = parent_namespace_id
    node["stid"] = function_symboltable_id

def postorder_function(node):
    leaveNamespace()

def setpstid(node):
    node["pstid"] = currentNamespaceID()

def preorder_field(node):
    setpstid(node)

def preorder_function_parameter(node):
    setpstid(node)

def preorder_declaration_statement(node):
    setpstid(node)

def preorder_integer(node):
    setpstid(node)

def preorder_rvalue(node):
    setpstid(node)

def preorder_expression(node):
    setpstid(node)
def preorder_lhs(node):
    setpstid(node)
def preorder_rhs(node):
    setpstid(node)