from NamespaceHelper import *
from RegisterHelper import *

def GetRegisterFor(node):
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    register = GetNextRegister()
    row["register"] = register
    node["register"] = register

def postorder_function(node):
    ResetRegisters()

def postorder_declaration_statement(node):
    GetRegisterFor(node)

def postorder_integer(node):
    GetRegisterFor(node)

def postorder_string(node):
    GetRegisterFor(node)

def postorder_char(node):
    GetRegisterFor(node)

def postorder_rvalue(node):
    GetRegisterFor(node)

def postorder_expression(node):
    GetRegisterFor(node)

def postorder_lhs(node):
    GetRegisterFor(node)

def postorder_rhs(node):
    GetRegisterFor(node)

def postorder_lvalue(node):
    GetRegisterFor(node)

def postorder_lvalue_component(node):
    if not node.Contains("allocate_register"):
        return
    
    GetRegisterFor(node)

def postorder_indice(node):
    GetRegisterFor(node)