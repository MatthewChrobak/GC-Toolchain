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
    
    row = symboltable.GetOrCreate(node["stid"]).GetRowWhere("name", variable_name, "entity_type", "variable")
    row["type"] = type
    row["pointer"] = is_pointer
    row["reference"] = is_reference