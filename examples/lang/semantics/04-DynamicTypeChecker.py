from helper import *

def GetRow(node):
    return symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])

def preorder_lvalue(node):
    stid = node["pstid"]
    lvalueType = None

    for component in node.AsArray("lvalue_component"):
        identifier, loc = GetValue(component["identifier"])
        parent = LFC(identifier, stid)
        ErrorIf(parent is None, "LFC was unable to find an entity in {0} for {1}".format(stid, identifier))


        row = symboltable.GetOrCreate(parent).GetRowWhere("name", identifier)
        type = row["return_type"] if component.Contains("function_call") else row["type"]

        row = GetRow(component)
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
    row = GetRow(node)
    node["dynamic_pstid"] = row["dynamic_pstid"]
    node["dynamic_type"] = row["dynamic_type"]

def postorder_rvalue(node):
    child = GetPossibleChild(node, ["expression"])
    node["type"] = child["type"]

def postorder_function_argument(node):
    row = GetRow(node)
    node["type"] = node["rvalue"]["type"]
    row["type"] = node["type"]

def postorder_function_parameter(node):
    identifier, loc = GetValue(node["type"]["identifier"])
    row = GetRow(node)
    row["type"] = identifier

def LFC(entityName, startingNamespace):
    level = 0
    namespace = startingNamespace
    while True:
        print(namespace + " " + entityName)
        namespace = getParentNamespaceID(level, namespace)
        if namespace is None:
            break
        ErrorIfNot(symboltable.Exists(namespace), "LFC: The namespace {0} does not exist".format(namespace))
        st = symboltable.GetOrCreate(namespace)
        if st.RowExistsWhere("name", entityName):
            return namespace
        level += 1

    return None

def postorder_expression(node):
    if node.Contains("operator"):
        expressions = node.AsArray("expression")
        lhs = expressions[0]
        rhs = expressions[1]

        operator = GetPossibleChild(node["operator"], ["arithmetic", "logical", "comparison"])
        op, loc = GetValue(operator)

        rhs_type = rhs["type"]
        lhs_type = lhs["type"]

        ErrorIf(lhs_type != rhs_type, "LHS and RHS at {0} have incompatible types for operator '{2}': {1}{2}{3}".format(loc, lhs_type, op, rhs_type))

        type = lhs_type
    else:
        child = GetPossibleChild(node, ["expression", "lvalue", "integer", "rvalue"])
        type = child["type"]

    if node.Contains("sign"):
        _, loc = GetValue(node["sign"])
        ErrorIf(type not in ["int"], "{0} at {1} cannot be signed".format(type, loc))

    row = GetRow(node)
    node["type"] = type
    row["type"] = type

def postorder_while_condition(node):
    row = GetRow(node)
    type = node["rvalue"]["type"]
    node["type"] = type
    row["type"] = type

    _, loc = GetValue(node["start_marker"])
    ErrorIf(type != "int", "While-loop condition at {0} needs to be of type int".format(loc))

def postorder_if_condition(node):
    row = GetRow(node)
    type = node["rvalue"]["type"]
    node["type"] = type
    row["type"] = type

    _, loc = GetValue(node["start_marker"])
    ErrorIf(type != "int", "If-statement condition at {0} needs to be of type int".format(loc))
