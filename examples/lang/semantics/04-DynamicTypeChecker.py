from helper import *

def preorder_lvalue(node):
    stid = node["pstid"]
    lvalueType = None

    for component in node.AsArray("lvalue_component"):
        identifier, loc = GetValue(component["identifier"])
        parent = LFC(identifier, stid)
        ErrorIf(parent is None, "LFC was unable to find an entity in {0} for {1}".format(stid, identifier))


        row = symboltable.GetOrCreate(parent).GetRowWhere("name", identifier)
        type = row["return_type"] if component.Contains("function_call") else row["type"]

        row = symboltable.GetOrCreate(component["pstid"]).RowAt(component["rowid"])
        row["dynamic_pstid"] = parent
        row["dynamic_type"] = type

        stid = parent
        lvalueType = type

    st = symboltable.GetOrCreate(node["pstid"])
    row = st.RowAt(node["rowid"])
    node["type"] = lvalueType
    row["type"] = lvalueType
    row["dynamic_type"] = lvalueType
    row["dynamic_pstid"] = parent

def postorder_lvalue_component(node):
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    node["dynamic_pstid"] = row["dynamic_pstid"]
    node["dynamic_type"] = row["dynamic_type"]

def postorder_rvalue(node):
    possibleChildren = ["lvalue", "integer"]
    for child in possibleChildren:
        if node.Contains(child):
            node["type"] = node[child]["type"]
            break

def postorder_function_argument(node):
    node["type"] = node["rvalue"]["type"]

def postorder_function_parameter(node):
    identifier, loc = GetValue(node["type"]["identifier"])
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    row["type"] = identifier

def LFC(entityName, startingNamespace):
    level = 0
    namespace = startingNamespace
    while True:
        print(namespace + " " + entityName)
        namespace = getParentNamespaceID(level, namespace)
        ErrorIfNot(symboltable.Exists(namespace), "LFC: The namespace {0} does not exist".format(namespace))
        st = symboltable.GetOrCreate(namespace)
        if st.RowExistsWhere("name", entityName):
            return namespace
        level += 1

    return None