from helper import *

def getRow(node):
    st = symboltable.GetOrCreate(node["pstid"])
    return st.RowAt(node["rowid"])

def postorder_function(node):
    getRow(node)["return_type"] = node["return_type"]["identifier"]["value"]

def postorder_declaration_statement(node):
    getRow(node)["type"] = node["type"]["identifier"]["value"]

def postorder_integer(node):
    node["type"] = "int"

# TODO:
# Expression type checking? We don't have expressions yet