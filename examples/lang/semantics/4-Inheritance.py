from NamespaceHelper import *
from TypeHelper import *

def resolveType(type, namespace):
    global internal_types

    if type in internal_types:
        return type

    separator =  "" if type.startswith("::") else "::"

    level = 0
    while True:
        stid = getParentNamespaceID(level, namespace)
        potentialType = stid + separator + type
        print(potentialType)
        if symboltable.Exists(potentialType):
            if symboltable.GetOrCreate(potentialType).GetMetaData("symboltable_type") == "class":
                return potentialType
        
        if len(stid) == 0:
            return None

        level += 1

pending = []

def postorder_global(node):
    global pending
    finished = []

    while len(pending) != 0:
        count = len(pending)

        for i in range(len(pending)):
            classNode = pending.pop(0)
            className, name_loc = getNodeValues(classNode, "class_name")
            classNameResolved = resolveType(className, classNode["pstid"])

            if classNode.Contains("parent_class"):
                parentName = classNode["parent_class"]["lvalue_type"]["type"]
                parentNameResolved = resolveType(parentName, classNode["pstid"])

                if parentNameResolved not in finished:
                    pending.append(classNode)
                else:
                    #TODO: Add members to the class
                    finished.append(classNameResolved)
            else:
                finished.append(classNameResolved)
            

        if len(pending) == count:
            classNames = ""
            for classNode in pending:
                className, nameLoc = getNodeValues(classNode, "class_name")
                classNames += className + ", "
            Error("Unable to process classes " + classNames[0:(len(classNames) - 2)] + ". They either do not exist, or have a form of circular inheritance")

def preorder_class(node):
    global pending
    pending.append(node)