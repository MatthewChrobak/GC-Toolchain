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
        raise Exception("The class {0} at {1}:{2} is already defined", class_symboltable_id, class_row, class_column)

    symboltable.GetOrCreate(class_symboltable_id)

def postorder_class(node):
    leaveNamespace()

def preorder_function(node):
    function_name = node["function_name"]["value"]
    row = node["function_name"]["row"]
    column = node["function_name"]["column"]
    function_symboltable_id = currentNamespaceID() + "::" + function_name

    enterNamespace(function_name)

    if symboltable.Exists(function_symboltable_id):
        raise Exception("The function {0} at {1!s}:{2!s} is already defined", function_symboltable_id, row, column)

    symboltable.GetOrCreate(function_symboltable_id)

def postorder_function(node):
    leaveNamespace()