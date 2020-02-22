# Checks that types are properly built lvalues. Does not actually apply any naming checking. Only syntax.
def preorder_declaration_statement(node):
    handle_type(node, "variable_type", "variable_name", "variable")

def preorder_function_parameter(node):
    handle_type(node, "parameter_type", "parameter_name", "parameter")

def preorder_field(node):
    handle_type(node, "field_type", "field_name", "field")

def preorder_function(node):
    row = handle_type(node, "return_type", "function_name", "function")
    row["is_free"] = False

def preorder_free_function(node):
    row = handle_type(node, "return_type", "function_name", "function")
    row["is_free"] = True


def handle_type(parent, type_key, name_key, entity_type):
    type = parent[type_key]
    name = parent[name_key]["value"]
    handle_lvalue_type(type)
    row = symboltable.GetOrCreate(parent["pstid"]).GetRowWhere("name", name, "entity_type", entity_type)
    row["label"] = type["label"] + " " + name
    row["pointer"] = type["pointer"]
    row["reference"] = type["reference"]
    return row

def handle_lvalue_type(node):
    lvalue = node["lvalue"]
    is_pointer = node.Contains("pointer")
    is_reference = node.Contains("reference")

    lvalue_components = lvalue.AsArray("lvalue_component")
    label = ""

    for component in lvalue_components:
        identifier = component["identifier"]
        id = identifier["value"]
        row = identifier["row"]
        column = identifier["column"]
        if component.Contains("arrow") or component.Contains("dot"):
            raise Exception("LValue at {0}:{1} must be a type".format(row, column))

        if component.Contains("scope"):
            label += "::"
        label += id
    
    node["pointer"] = is_pointer
    node["reference"] = is_reference
    node["label"] = label