from helper import *

def preorder_lvalue_component(node):
    if not node.Contains("function_call"):
        return

    fc = node["function_call"]

    identifier, loc = GetValue(node["identifier"])
    dynamic_stid = node["dynamic_pstid"] + "::" + identifier
    st = symboltable.GetOrCreate(dynamic_stid)

    arguments = fc.AsArray("function_argument") if fc.Contains("function_argument") else []
    parameters = st.GetRowsWhere("is_parameter", True)

    ErrorIfNot(len(arguments) == len(parameters), "Expected {0} parameters at {1}. Got {2}".format(len(parameters), loc, len(arguments)))

    fullParameters = []
    fullArguments = []
    for i in range(len(arguments)):
        fullArguments.append(arguments[i]["type"])
        fullParameters.append(parameters[i]["type"])

    fullParameters = "({0})".format(", ".join(fullParameters))
    fullArguments = "({0})".format(", ".join(fullArguments))

    ErrorIfNot(fullParameters == fullArguments, "Expected {3}{0} at {2}. Got {3}{1}".format(fullParameters, fullArguments, loc, identifier))