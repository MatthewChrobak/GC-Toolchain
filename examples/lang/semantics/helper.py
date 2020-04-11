current_namespace = ""
namespace_stack = []

def currentNamespaceID():
    global current_namespace
    return current_namespace

def enterNamespace(id):
    global current_namespace, namespace_stack

    id = "::" + id
    current_namespace = current_namespace + id
    namespace_stack.append(id)
    return current_namespace

def leaveNamespace():
    global current_namespace, namespace_stack
    id = namespace_stack.pop()
    current_namespace = current_namespace[:-(len(id))]

def getParentNamespaceID(level, namespace):
    for i in range(level):
        index = namespace.rindex("::")
        namespace = namespace[0:index]
    return namespace
    
def Error(message):
    raise Exception(message)

def ErrorIf(cond, message):
    if cond:
        Error(message)

def ErrorIfNot(cond, message):
    ErrorIf(not cond, message)

def GetValue(node):
    ErrorIfNot(node.Contains("value"), "Cannot get 'value' from node. Node only contains {0}".format(node.Keys))
    ErrorIfNot(node.Contains("row"), "Cannot get 'row' from node. Node only contains {0}".format(node.Keys))
    ErrorIfNot(node.Contains("column"), "Cannot get 'column' from node. Node only contains {0}".format(node.Keys))
    return node["value"], (node["row"], node["column"])


valid_types = ["int"]
def isValidType(type):
    global valid_types
    return type in valid_types

def isValidReturnType(type):
    return isValidType(type) or type == "void"