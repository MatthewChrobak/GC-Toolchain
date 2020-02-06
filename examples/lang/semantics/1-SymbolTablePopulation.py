cnsp = ""
nsp_stk = []

def currentNamespace():
    return symboltable.GetOrCreate(currentNamespaceID())

def currentNamespaceID():
    global cnsp
    return cnsp

def enterNamespace(id):
    global cnsp, nsp_stk
    id = id + "::"
    cnsp = cnsp + id
    nsp_stk.append(id)
    return cnsp

def leaveNamespace():
    global cnsp, nsp_stk
    id = nsp_stk.pop()
    cnsp = cnsp[0:-(len(id))]


def preorder_global(node):
    enterNamespace("global")

def postorder_global(node):
    leaveNamespace()

def preorder_namespace(node):
    if node.Contains("namespace_name"):
        id = node["namespace_name"]["value"]
        enterNamespace(id)
        symboltable.GetOrCreate(currentNamespaceID())

def postorder_namespace(node):
    if node.Contains("namespace_name"):
        leaveNamespace()

def preorder_class(node):   
    classname_node = node["class_name"]
    parent_namespace_id = currentNamespaceID()
    parent_namespace = currentNamespace()

    class_name = classname_node["value"]
    class_row = str(classname_node["row"])
    class_column = str(classname_node["column"])

    enterNamespace(class_name)
    class_namespace_id = parent_namespace_id + class_name
    
    if symboltable.Exists(class_namespace_id):
        raise Exception("Symbol table " + class_name + " at " + class_row + ":" + class_column + " already exists")

    class_namespace = symboltable.GetOrCreate(class_namespace_id)
    log.WriteLineVerbose("Creating symbol table: " + class_namespace_id)

    #namespace symboltable
    row = class_namespace.CreateRow()
    row["name"] = class_name
    row["type"] = "class"

    access_modifier_key = "access_modifier"
    if node.Contains(access_modifier_key):
        row[access_modifier_key] = node[access_modifier_key]["value"]
    else:
        row[access_modifier_key] = "default"

    static_key = "static"
    if node.Contains(static_key):
        row[static_key] = True
    else:
        row[static_key] = False

def preorder_field(node):
    fieldname_node = node["field_name"]
    field_name = fieldname_node["value"]

    fieldtype_node = node["field_type"]
    field_type = fieldtype_node["value"]

    namespace = currentNamespace()

    if namespace.RowExistsWhere("name", field_name, "type", field_type):
        raise Exception("Property " + field_name + ":" + field_type + " already exists in " + currentNamespaceID())

    row = namespace.CreateRow()
    row["name"] = field_name
    row["type"] = field_type
    row["member_type"] = "field"

    static_key = "static"
    if node.Contains(static_key):
        row[static_key] = True
    else:
        row[static_key] = False
    
    access_modifier_key = "access_modifier"
    if node.Contains(access_modifier_key):
        row[access_modifier_key] = node[access_modifier_key]["value"]
    else:
        row[access_modifier_key] = "default"