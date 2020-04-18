from helper import *

def getRow(node):
    st = symboltable.GetOrCreate(node["pstid"])
    return st.RowAt(node["rowid"])

def postorder_function(node):
    type, loc = GetValue(node["return_type"]["identifier"])

    if not isValidReturnType(type):
        Error("Unknown return type '{0}' at {1}".format(type, loc))

    getRow(node)["return_type"] = type

def postorder_declaration_statement(node):
    type, loc = GetValue(node["type"]["identifier"])

    if not isValidType(type):
        Error("Unknown variable type '{0}' at {1}".format(type, loc))

    getRow(node)["type"]  = type

def postorder_integer(node):
    node["type"] = "int"

def postorder_float(node):
    node["type"] = "float"