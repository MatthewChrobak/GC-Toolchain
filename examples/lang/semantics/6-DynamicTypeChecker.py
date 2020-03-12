from NamespaceHelper import *
from TypeHelper import *

def resolveType(type, namespace):
    global internal_types

    separator =  "" if type.startswith("::") else "::"

    if type in internal_types:
        return type

    level = 0
    while True:
        stid = getParentNamespaceID(level, namespace)
        potentialType = stid + separator + type
        if symboltable.Exists(potentialType):
            return potentialType
    
        if len(stid) == 0:
            return None

        level += 1

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
    global internal_types
    # Exit out early if we've already computed it.
    if node.Contains("type"):
        return

    # Offset in the sense that we have (lvalue) (access, lvalue) (access, lvalue)
    # when what we want is             (lvalue, access) (lvalue, access) (lvalue)    
    offset_components = node.AsArray("lvalue_component")

    componentPairs = []
    for i in range(len(offset_components) - 1):
        offset_component = offset_components[i + 1]
        if offset_component.Contains("dot"):
            access = "dot"
        elif offset_component.Contains("scope"):
            access = "scope"
        else:
            access = ""
        componentPairs.append((offset_components[i], access))
    componentPairs.append((offset_components[len(offset_components) - 1], ""))

    stid = None
    canBeStatic = True
    for componentPair in componentPairs:
        component, access = componentPair
        if component.Contains("identifier"):
            identifier_key = "identifier"
        elif component.Contains("function_identifier"):
            identifier_key = "function_identifier"
        else:
            identifier_key = None
        identifier, loc = getNodeValues(component, identifier_key)

        next_stid = None
        if access not in ["scope", "dot", ""]:
            Error("Uknown accessor {0} at {1}:{2}".format(access, loc[0], loc[1]))
            return

        if stid is not None:
            if not symboltable.Exists(stid):
                Error("Unable to find symbol table with id {0} at {1}:{2}".format(stid, loc[0], loc[1]))
                return

        # Are we at the first element?
        if stid is None:
            # is the first one a static element?
            if access == "scope":
                next_stid = resolveType(identifier, node["pstid"])
            else:
                # Non-static. Check in LFC scope.
                canBeStatic = False
                next_stid = LFC(identifier, node["pstid"])
        else:
            if canBeStatic:
                if access == "scope":
                    next_stid = resolveType(stid + "::" + identifier, node["pstid"])
                else:
                    # Using dot to access a static member.
                    st = symboltable.GetOrCreate(stid)
                    if not st.RowExistsWhere("name", identifier, "static", True):
                        Error("Unable to find static element {0} at {1}:{2} in {3}".format(identifier, loc[0], loc[1], stid))
                        return
                    row = st.GetRowWhere("name", identifier, "static", True)

                    # Get the type of the element. For functions, this includes return type.
                    next_stid = resolveType(row["type"], node["pstid"])
                    canBeStatic = False
            else:
                if access == "scope":
                    Error("Static element {0} at {1}:{2} must be preceded with a type".format(identifier, loc[0], loc[1]))
                    return
                else:
                    st = symboltable.GetOrCreate(stid)
                    if not st.RowExistsWhere("name", identifier, "static", False):
                        Error("Unable to find member element {0} at {1}:{2} in {3}".format(identifier, loc[0], loc[1], stid))
                        return
                    row = st.GetRowWhere("name", identifier, "static", False)
                    next_stid = resolveType(row["type"], node["pstid"])

        if next_stid is None:
            Error("Unable to find symbol table with id {0} at {1}:{2}".format(identifier if stid is None else stid, loc[0], loc[1]))
            return
        else:
            stid = next_stid

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

        if st.RowExistsWhere("name", identifier, "static", False):
            return parent
        else:
            level += 1
    return None