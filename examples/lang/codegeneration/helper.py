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

def ConvertType(type):
    if type == "void":
        return "void"
    if type == "int":
        return "i32"
    Error("Unknown type {0}".format(type))