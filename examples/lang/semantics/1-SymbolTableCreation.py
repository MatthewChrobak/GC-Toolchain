from NamespaceHelper import *

def createSymbolTable(node, nameKey, entity_type):
    name, loc = getNodeValues(node, nameKey)
    enterNamespace(node, name)

    if name == "global" and (entity_type == "namespace" or entity_type == "class") :
        Error("The {0} at {1}:{2} cannot be named global".format(entity_type, loc[0], loc[1]))

    if symboltable.Exists(currentNamespaceID()):
        Error("The {0} {1} at {2}:{3} is already defined".format(entity_type, name, loc[0], loc[1]))
    else:
        st = symboltable.GetOrCreate(currentNamespaceID())
        st.SetMetaData("symboltable_type", entity_type)


def preorder_global(node):
    enterNamespace(node, "global")
def postorder_global(node):
    leaveNamespace()


def preorder_namespace(node):
    if node.Contains("namespace_name"):     
        createSymbolTable(node, "namespace_name", "namespace")
def postorder_namespace(node):
    if node.Contains("namespace_name"):
        leaveNamespace()


def preorder_class(node):
    createSymbolTable(node, "class_name", "class")
def postorder_class(node):
    leaveNamespace()

def preorder_free_function(node):
    preorder_function(node, True)
def postorder_free_function(node):
    postorder_function(node)


def preorder_function(node, isFree=False):
    createSymbolTable(node, "function_name", "function")
def postorder_function(node):
    leaveNamespace()


def preorder_statement_scope(node):
    scope, loc = getNodeValues(node, "scope_start")

    enterNamespace(node, "{0}_{1}".format(loc[0], loc[1]))
    
    if symboltable.Exists(currentNamespaceID()):
        Error("The scope {0} at {1!s}:{1!s} is already defined".format(currentNamespaceID(), loc[0], loc[1]))
    else:
        st = symboltable.GetOrCreate(currentNamespaceID())
        st.SetMetaData("symboltable_type", "local")
def postorder_statement_scope(node):
    leaveNamespace()


def preorder_field(node):
    setPSTID(node)
def preorder_function_parameter(node):
    setPSTID(node)
def preorder_declaration_statement(node):
    setPSTID(node)
def preorder_integer(node):
    setPSTID(node)
def preorder_string(node):
    setPSTID(node)
def preorder_char(node):
    setPSTID(node)
def preorder_rvalue(node):
    setPSTID(node)
def preorder_expression(node):
    setPSTID(node)
def preorder_lhs(node):
    setPSTID(node)
def preorder_rhs(node):
    setPSTID(node)
def preorder_lvalue_type(node):
    setPSTID(node)
def preorder_lvalue(node):
    setPSTID(node)
    for component in node.AsArray("lvalue_component"):
        component["allocate_register"] = ""
def preorder_lvalue_component(node):
    setPSTID(node)
def preorder_indice(node):
    setPSTID(node)
def preorder_type(node):
    setPSTID(node)