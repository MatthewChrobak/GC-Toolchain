current_namespace = ""
namespace_stack = []

def currentNamespaceID():
    global current_namespace
    return current_namespace

def enterNamespace(node, id):
    global current_namespace, namespace_stack

    setPSTID(node)
    id = "::" + id
    current_namespace = current_namespace + id
    namespace_stack.append(id)
    setSTID(node)
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

def getNodeValue(node, key):
    return node[key]["value"]

def getNodeValues(node, key):
    node = node[key]
    return node["value"], (node["row"], node["column"])

def Error(str):
    raise Exception(str)

def Warning(str):
    print(str)

def setPSTID(node):
    node["pstid"] = currentNamespaceID()

def setSTID(node):
    node["stid"] = currentNamespaceID()

def getPSTID(node):
    return node["pstid"]

def getSTID(node):
    return node["stid"]