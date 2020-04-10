from helper import *

def pstid(node):
    node["pstid"] = currentNamespaceID()

def preorder_global(node):
    enterNamespace("global")
    symboltable.GetOrCreate(currentNamespaceID())

    gst = symboltable.GetOrCreate("::global")
    row, id = gst.CreateRow()
    row["name"] = "print_int"
    row["return_type"] = "void"
    row["entity_type"] = "function"

    st = symboltable.GetOrCreate("::global::print_int")
    st.SetMetaData("st_type", "function")
    row, id = st.CreateRow()
    row["name"] = "arg"
    row["entity_type"] = "variable"
    row["is_parameter"] = True
    row["parameter_index"] = 0
    row["type"] = "int"

def preorder_function(node):
    pstid(node)

    function_name, loc = GetValue(node["identifier"])
    enterNamespace(function_name)
    ErrorIf(symboltable.Exists(currentNamespaceID()), "The function {0} at {1} already exists".format(function_name, loc))
    
    st = symboltable.GetOrCreate(currentNamespaceID())
    st.SetMetaData("st_type", "function")

    node["stid"] = currentNamespaceID()
    node["function_name"] = function_name

def postorder_function(node):
    leaveNamespace()

def preorder_declaration_statement(node):
    pstid(node)

def preorder_lvalue(node):
    pstid(node)
    
def preorder_lvalue_component(node):
    pstid(node)

def preorder_function_call(node):
    pstid(node)

def preorder_integer(node):
    pstid(node)

def preorder_rvalue(node):
    pstid(node)

def preorder_function_parameter(node):
    pstid(node)

def preorder_function_argument(node):
    pstid(node)