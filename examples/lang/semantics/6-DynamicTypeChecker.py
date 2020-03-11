from NamespaceHelper import *
from TypeHelper import *

classNamespaceID = None

def setType(node, type):
    node["type"] = type
    pst = symboltable.GetOrCreate(node["pstid"])
    row = pst.RowAt(node["rowid"])
    row["type"] = type

def handle_lvalue_type(node):
    global internal_types

def preorder_class(node):
    global classNamespaceID
    classNamespaceID = node["stid"]
def postorder_class(node):
    global classNamespaceID
    classNamespaceID = None

def postorder_lvalue(node):
    # Exit out early if we've already computed it.
    if node.Contains("type"):
        return

    # Offset in the sense that we have (lvalue) (access, lvalue) (access, lvalue)
    # when what we want is             (lvalue, access) (lvalue, access) (lvalue)    
    offset_components = node.AsArray("lvalue_component")

    componentPairs = []
    for i in range(len(offset_components) - 1):
        offset_component = offset_components[i]
        if offset_component.Contains("dot"):
            access = "dot"
        elif offset_component.Contains("scope"):
            access = "scope"
        else:
            access = ""
        componentPairs.append((offset_component, access))

    # Keep accessing staticly until we start accessing members
    stid = None
    canBeStatic = True
    for componentPair in componentPairs:
        component, access = componentPair

        if node.Contains("identifier"):
            identifier_key = "identifier"
        if node.contains("function_identifier"):
            identifier_key = "function_identifier"
            canBeStatic = False
        identifier, loc = getNodeValues(node, identifier_key)

        if access in ["dot", ""]:
            canBeStatic = False

            # First lvalue_component?
            if stid is None:
                stid = LFC(identifier, node["pstid"])
        else:
            if not canAccessViaStatic:
                Error("Unable to resolve the type {0} at {1}:{2}".format(identifier, loc[0], loc[1]))
                return

            # First lvalue_component?
            if stid is None:
                stid = getStaticSymbolTableID(identi)



def getStaticSymbolTableID(id):


# LOCAL or FUNCTION or CLASS - returns the symboltableID that contains the identifier. None if none of them have it.
def LFC(identifier, namespace):
    level = 0
    while True:
        parent = getParentNamespaceID(level, namespace)
        if not symboltable.Exists(parent):
            log.WriteLineError("Unknown parent symbol table in LFC {0}".format(parent))
            return None
        st = symboltable.GetOrCreate(parent)
        if not st.GetMetaData("symboltable_type") in ["local", "function", "class"]:
            break

        if st.RowExistsWhere("name", identifier):
            return parent
        else:
            level += 1
    return None