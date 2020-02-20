cnsp = ""
nsp_stk = []

def currentNamespaceID():
    global cnsp
    return cnsp

def enterNamespace(id):
    global cnsp, nsp_stk
    id = "::" + id
    cnsp = cnsp + id
    nsp_stk.append(id)
    return cnsp

def leaveNamespace():
    global cnsp, nsp_stk
    id = nsp_stk.pop()
    cnsp = cnsp[0:-(len(id))]

def getParentNamespaceID(level, namespace):
    for i in range(level):
        index = namespace.rindex("::")
        namespace = namespace[0:index]

    return namespace