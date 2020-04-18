from helper import *

def CreateRow(node, entity_type, label, existsRestriction = None):
    st = symboltable.GetOrCreate(node["pstid"])

    if existsRestriction is not None:
        key, value, message = existsRestriction
        ErrorIf(st.RowExistsWhere(key, value), message)

    row, id = st.CreateRow()
    row["entity_type"] = entity_type
    row["label"] = label
    node["label"] = label
    node["rowid"] = id

    return row

def preorder_global(node):
    ErrorIfNot(symboltable.Exists("::global::main"), "Program must have a main")

def preorder_function(node):
    global parameterIndex
    parameterIndex = 0
    row = CreateRow(node, "function", "")
    identifier, loc = GetValue(node["identifier"])
    row["name"] = identifier

def preorder_declaration_statement(node):
    identifier, loc = GetValue(node["identifier"])
    pstid = node["pstid"]
    st = symboltable.GetOrCreate(pstid)

    while st.GetMetaData("st_type") in ["local", "function"]:
        ErrorIf(st.RowExistsWhere("name", identifier), "The variable '{0}' at {1} already exists in {2}".format(identifier, loc, pstid))
        pstid = getParentNamespaceID(1, pstid)
        st = symboltable.GetOrCreate(pstid)

    row = CreateRow(node, "variable", identifier)
    row["name"] = identifier

def postorder_lvalue(node):
    labels = []
    for component in node.AsArray("lvalue_component"):
        labels.append(component["label"])
    CreateRow(node, "lvalue", ".".join(labels))

def postorder_expression(node):
    if node.Contains("operator"):
        expressions = node.AsArray("expression")
        lhs = expressions[0]
        rhs = expressions[1]

        op = None
        loc = None
        operator = GetPossibleChild(node["operator"], ["arithmetic", "logical", "comparison"])
        op, loc = GetValue(operator)

        label = "{0} {1} {2}".format(lhs["label"], op, rhs["label"])
    else:
        child = GetPossibleChild(node, ["expression", "lvalue", "integer", "rvalue", "float"])
        label = child["label"]

    if node.Contains("sign"):
        sign, loc = GetValue(node["sign"])
        label = sign + label

    row = CreateRow(node, "expression", label)


def postorder_lvalue_component(node):
    CreateRow(node, "lvalue_component", node["identifier"]["value"])

def postorder_integer(node):
    CreateRow(node, "integer", node["value"])

def postorder_float(node):
    CreateRow(node, "float", node["value"])

def postorder_rvalue(node):
    label = ""
    child = GetPossibleChild(node, ["expression"])
    label = child["label"]
    CreateRow(node, "rvalue", label)

def postorder_function_argument(node):
    CreateRow(node, "function_argument", node["rvalue"]["label"])

parameterIndex = 0
def postorder_function_parameter(node):
    global parameterIndex
    identifier, loc = GetValue(node["identifier"])
    error = "The parameter '{0}' at {1} already exists in {2}".format(identifier, loc, node["pstid"])
    row = CreateRow(node, "variable", identifier, ("name", identifier, error))
    row["name"] = identifier
    row["is_parameter"] = True
    row["parameter_index"] = parameterIndex
    parameterIndex += 1

def postorder_return_statement(node):
    if node.Contains("rvalue"):
        CreateRow(node, "return_statement", "return " + node["rvalue"]["label"])
    else:
        CreateRow(node, "return_statement", "return void")

def postorder_while_condition(node):
    CreateRow(node, "while_condition", node["rvalue"]["label"])

def postorder_if_condition(node):
    CreateRow(node, "if_condition", node["rvalue"]["label"])

def postorder_for_condition(node):
    if not node.Contains("rvalue"):
        return
    CreateRow(node, "for_condition", node["rvalue"]["label"])