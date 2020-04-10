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
    error = "The variable '{0}' at {1} already exists in {2}".format(identifier, loc, node["pstid"])
    row = CreateRow(node, "variable", identifier, ("name", identifier, error))
    row["name"] = identifier

def postorder_lvalue(node):
    labels = []
    for component in node.AsArray("lvalue_component"):
        labels.append(component["label"])
    CreateRow(node, "lvalue", ".".join(labels))

def postorder_lvalue_component(node):
    CreateRow(node, "lvalue_component", node["identifier"]["value"])

def postorder_integer(node):
    CreateRow(node, "integer", node["value"])

def postorder_rvalue(node):
    label = ""
    possibleChildren = ["lvalue", "integer"]
    for child in possibleChildren:
        if node.Contains(child):
            label = node[child]["label"]
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