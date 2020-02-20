def preorder_declaration_statement(node):
    variable_type = node["variable_type"]
    variable_name = node["variable_name"]["value"]
    lvalue = variable_type["lvalue"]
    is_pointer = variable_type.Contains("pointer")
    is_reference = variable_type.Contains("reference")

    lvalue_components = lvalue.AsArray("lvalue_component")
    type = ""

    for component in lvalue_components:
        identifier = component["identifier"]
        id = identifier["value"]
        row = identifier["row"]
        column = identifier["column"]
        if component.Contains("arrow") or component.Contains("dot"):
            raise Exception("LValue at {0}:{1} must be a type".format(row, column))

        if component.Contains("scope"):
            type += "::"
        type += id
    
    row = symboltable.GetOrCreate(node["pstid"]).GetRowWhere("name", variable_name, "entity_type", "variable")
    row["type"] = type
    row["pointer"] = is_pointer
    row["reference"] = is_reference

def preorder_function_parameter(node):
    parameter_type = node["parameter_type"]
    parameter_name = node["parameter_name"]["value"]
    lvalue = parameter_type["lvalue"]
    is_pointer = parameter_type.Contains("pointer")
    is_reference = parameter_type.Contains("reference")

    lvalue_components = lvalue.AsArray("lvalue_component")
    type = ""

    for component in lvalue_components:
        identifier = component["identifier"]
        id = identifier["value"]
        row = identifier["row"]
        column = identifier["column"]
        if component.Contains("arrow") or component.Contains("dot"):
            raise Exception("LValue at {0}:{1} must be a type".format(row, column))

        if component.Contains("scope"):
            type += "::"
        type += id

    row = symboltable.GetOrCreate(node["pstid"]).GetRowWhere("name", parameter_name, "entity_type", "parameter")
    row["type"] = type
    row["pointer"] = is_pointer
    row["reference"] = is_reference