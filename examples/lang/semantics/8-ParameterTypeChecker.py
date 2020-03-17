from NamespaceHelper import *

def postorder_lvalue_component(node):
    if node.Contains("function_identifier"):
        function_name, loc = getNodeValues(node, "function_identifier")
        function_parent_stid = node["referencing_type"]
        function_parent_st = symboltable.GetOrCreate(function_parent_stid)

        # The prefix for the function symbol table is the class that actually implements it.
        # If a member is inherited, the function_parent_stid will be located in the inherited_from column
        # for the row in the inheriting class.
        log.WriteLineVerbose("Function_parent_stid = " + function_parent_stid)
        function_row = function_parent_st.GetRowWhere("name", function_name, "entity_type", "function")
        if function_row.Contains("inherited_from"):
            function_parent_stid = function_row["inherited_from"]

        function_stid = function_parent_stid + "::" + function_name
        function_st = symboltable.GetOrCreate(function_stid)
        expectedArguments = function_st.GetRowsWhere("is_parameter", True)
        actualArguments = []

        if node.Contains("function_argument"):
            actualArguments = node.AsArray("function_argument")

        if len(expectedArguments) != len(actualArguments):
            Error("The function {5}::{0} at {1}:{2} expects {3} arguments. Got {4}".format(function_name, loc[0], loc[1], str(len(expectedArguments)), str(len(actualArguments)), function_parent_stid))
            return

        actualTypes = []
        expectedTypes = []

        for i in range(len(expectedArguments)):
            expectedTypes.append(expectedArguments[i]["type"])
            actualTypes.append(actualArguments[i]["type"])

        actualTypes = ",".join(actualTypes)
        expectedTypes= ",".join(expectedTypes)

        if expectedTypes != actualTypes:
            Error("Expected {4}({0}) at {1}:{2}, got {4}({3})".format(expectedTypes, loc[0], loc[1], actualTypes, function_name))
            return