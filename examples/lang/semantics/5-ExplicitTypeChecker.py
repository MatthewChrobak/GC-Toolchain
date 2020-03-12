from NamespaceHelper import *
from TypeHelper import *

def resolveType(type, namespace):
    global internal_types

    if type in internal_types:
        return type

    level = 0
    while True:
        stid = getParentNamespaceID(level, namespace)
        potentialType = stid + "::" + type
        print(potentialType)
        if symboltable.Exists(potentialType):
            if symboltable.GetOrCreate(potentialType).GetMetaData("symboltable_type") == "class":
                return potentialType
        
        if len(stid) == 0:
            return None

        level += 1

def setType(node, type):
    pst = symboltable.GetOrCreate(node["pstid"])
    row = pst.RowAt(node["rowid"])
    node["type"] = type
    row["type"] = type

def handleType(node, type_key):
    global internal_types
    type = node[type_key]
    lvalue = type["lvalue_type"]
    lvalue_components = lvalue.AsArray("lvalue_component")
    _, loc = getNodeValues(lvalue_components[0], "identifier")

    knownType = ""

    for component in lvalue_components:
        if len(knownType) != 0 and not component.Contains("scope"):
            Error("Type at {0}:{1} must be resolved with scope operators".format(loc[0], loc[1]))
            return
        if component.Contains("scope"):
            knownType += "::"
        knownType += component["identifier"]["value"]

    resolvedType = resolveType(knownType, node["pstid"])

    if resolvedType is None:
        Error("Unknown type {0} at {1}:{2}".format(knownType, loc[0], loc[1]))
        return


    # knownTypeId = None
    # if symboltable.Exists(knownType):
    #     knownTypeId = knownType

    # if symboltable.Exists("::global::" + knownType):
    #     knownTypeId = "::global::" + knownType
    
    # if knownTypeId is not None:
    #     if symboltable.GetOrCreate(knownTypeId).GetMetaData("symboltable_type") != "class":
    #         Error("Type of {0} at {1}:{2} needs to be a class or internal type".format(knownType, loc[0], loc[1]))
    #         return
    
    # if knownTypeId is None:
    #     if not knownType in internal_types:
    #         Error("Unknown type {0} at {1}:{2}".format(knownType, loc[0], loc[1]))
    #         return
    #     else:
    #         knownTypeId = knownType

    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    row["type"] = resolvedType
    return row

def preorder_free_function(node):
    handleType(node, "return_type")

def preorder_function(node):
    handleType(node, "return_type")

def preorder_field(node):
    handleType(node, "field_type")
    
def preorder_function_parameter(node):
    handleType(node, "parameter_type")

def preorder_declaration_statement(node):
    handleType(node, "variable_type")

def preorder_integer(node):
    global internal_types
    setType(node, internal_types[0])

def preorder_string(node):
    global internal_types
    setType(node, internal_types[1])

def preorder_char(node):
    global internal_types
    setType(node, internal_types[2])