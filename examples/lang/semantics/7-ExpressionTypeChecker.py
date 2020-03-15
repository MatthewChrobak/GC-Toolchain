from NamespaceHelper import *

def postorder_expression(node):

    if node.Contains("rvalue"):
        node["type"] = node["rvalue"]["type"]
    else:
        lhs = node["lhs"]
        rhs = node["rhs"]
        operator, loc = getNodeValues(node, "operator")

        if lhs["type"] != rhs["type"]:
            Error("The operator {0} cannot be used for types {1} and {2} at {3}:{4}".format(operator, lhs["type"], rhs["type"], loc[0], loc[1]))
            return
        
        node["type"] = lhs["type"]

def postorder_declaration_statement(node):
    if node.Contains("expression"):
        if node["variable_type"]["type"] != node["expression"]["type"]:
            _, loc = getNodeValues(node, "variable_name")
            Error("Unable to assign {0} to a {1} at {2}:{3}".format(node["expression"]["type"], node["variable_type"]["type"], loc[0], loc[1]))
            return

def postorder_lvalue_statement(node):
    if node.Contains("expression"):
        if node["lvalue"]["type"] != node["expression"]["type"]:
            _, loc = getNodeValues(node, "operator")
            Error("Unable to assign {0} to a {1} at {2}:{3}".format(node["expression"]["type"], node["lvalue"]["type"], loc[0], loc[1]))
            return