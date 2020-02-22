def preorder_declaration_statement(node):
    handle_type(node, "variable_type", "variable_name", "variable")

def preorder_function_parameter(node):
    handle_type(node, "parameter_type", "parameter_name", "parameter")

def preorder_field(node):
    handle_type(node, "field_type", "field_name", "field")

def preorder_function(node):
    row = handle_type(node, "return_type", "function_name", "function")

def preorder_free_function(node):
    row = handle_type(node, "return_type", "function_name", "function")

def handle_type(parent, type_key, name_key, entity_type):
    type = parent[type_key]
    name = parent[name_key]["value"]
    handle_lvalue_type(type)
    row = symboltable.GetOrCreate(parent["pstid"]).GetRowWhere("name", name, "entity_type", entity_type)
    row["type"] = type["type"]
    return row

def handle_lvalue_type(node):
    lvalue = node["lvalue"]
    internal_types = ["int", "string", "char", "void"]
    lvalue_components = lvalue.AsArray("lvalue_component")

    known_type = None
    if len(lvalue_components) == 1:
        id = lvalue_components[0]["identifier"]["value"]
        if id in internal_types:
            known_type = id

    if known_type is None:
        st_id = None
        for component in lvalue_components:
            id = component["identifier"]
            id_value = id["value"]
            row = id["row"]
            column = id["column"]

            if st_id is None:
                st_id = id_value
            else:
                st_id += "::" + id_value

        valid_type = isClassType(st_id)
        if not valid_type:
            st_id = "::" + st_id
            valid_type = isClassType(st_id)
        if not valid_type:
            st_id = "::global" + st_id
            valid_type = isClassType(st_id)

        if valid_type:
            known_type = st_id
        else:
            raise Exception("Uknown type {0} at {1}:{2}".format(st_id, row, column))



    node["type"] = known_type

def isClassType(id):
    if symboltable.Exists(id):
        st = symboltable.GetOrCreate(id)
        if st.GetMetaData("sttype") == "class":
            return True
    return False