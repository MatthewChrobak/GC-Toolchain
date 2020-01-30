current_namespace = ""

def preorder_global(node):
    global current_namespace
    current_namespace = current_namespace + "global::"

def postorder_global(node):
    global current_namespace
    current_namespace = current_namespace[0:-len("global::")]

def preorder_namespace(node):
    global current_namespace
    id = str(node["namespace_name"]["value"]) + "::"
    current_namespace = current_namespace + id

def postorder_namespace(node):
    global current_namespace
    id = str(node["namespace_name"]["value"]) + "::"
    current_namespace = current_namespace[0:-len(id)]

def preorder_class(node):   
    global current_namespace
    classname = node["class_name"]
    id = current_namespace + classname["value"]
    row = str(classname["row"])
    column = str(classname["column"])
    
    if symboltable.Exists(id):
        raise Exception("Symbol table " + id + " at " + row + ":" + column + " already exists")

    st = symboltable.GetOrCreate(id)
    log.WriteLineVerbose("Creating symbol table: " + id)