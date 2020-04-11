from helper import *

registerMap = {}

def GetRow(node):
    return symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])

def resetRegisterMap():
    global registerMap
    registerMap = {}

def registerRegister(register):
    global registerMap

    if register[0] == "%":
        return register

    # Does the mapping already exist?
    if register in registerMap:
        return registerMap[register]

    # It doesn't. Make an entry
    n = len(registerMap) + 1
    registerMap[register] = "%{0}".format(str(n))
    return registerMap[register]

def getAdditionalRegisters(node, row=None):
    if row is None:
        row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])

    a = []
    if node.Contains("additional_registers"):
        for r in node["additional_registers"]:
            a.append(registerRegister(r))
    row["additional_registers"] = ", ".join(a)
    node["additional_registes"] = a
    return a

def getRegister(node, row=None):
    if row is None:
        row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    r = node["register"]
    r = registerRegister(r)
    row["register"] = r
    node["register"] = r
    return r

def getRegisters(node):
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    a = getAdditionalRegisters(node)
    r = getRegister(node)
    return r, a

def preorder_global(node):
    instructionstream.AppendLine("declare void @print_int(i32)")

def preorder_function(node):
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    returnType = ConvertType(row["return_type"], True)
    instructionstream.AppendLine("define {1} @{0}() {{".format(node["function_name"], returnType))
    instructionstream.IncrementTab(1)

def postorder_function(node):
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    returnType = ConvertType(row["return_type"], True)
    if returnType == "void":
        instructionstream.AppendLine("ret void")
    instructionstream.IncrementTab(-1)
    instructionstream.AppendLine("}")

def postorder_integer(node):
    r = getRegister(node)
    instructionstream.AppendLine("; integer")
    value, loc = GetValue(node)
    type = ConvertType(node["type"])
    instructionstream.AppendLine("{0} = alloca {1}".format(r, type))
    instructionstream.AppendLine("store {2} {1}, {2}* {0}".format(r, value, type))

def postorder_rvalue(node):
    instructionstream.AppendLine("; rvalue")
    possibleChildren = ["integer", "lvalue"]
    for child in possibleChildren:
        if node.Contains(child):
            row = GetRow(node)
            node["register"] = node[child]["register"]
            row["register"] = node["register"]
            break

def postorder_lvalue_statement(node):
    if not node.Contains("assignment"):
        return

    instructionstream.AppendLine("; lvalue statement")
    r = registerRegister(node["register"])

    type = ConvertType(node["rvalue"]["type"])

    instructionstream.AppendLine("{0} = load {2}, {2}* {1}".format(r, node["rvalue"]["register"], type))
    instructionstream.AppendLine("store {2} {0}, {2}* {1}".format(r, node["lvalue"]["register"], type))


def postorder_lvalue(node):
    instructionstream.AppendLine("; lvalue")
    components = node.AsArray("lvalue_component")
    last_register = None

    if len(components) == 1:
        component = components[0]
        identifier, loc = GetValue(component["identifier"])

        if component.Contains("function_call"):
            instructionstream.AppendLine("; function call")
            instructionstream.AppendLine("; function arguments")
            ca = getAdditionalRegisters(component)
            fc = component["function_call"]
            arguments = fc.AsArray("function_argument") if fc.Contains("function_argument") else []
            argumentRegisters = []
            for i in range(len(arguments)):
                argument = arguments[i]
                argument_type = ConvertType(argument["type"])
                ar = getRegister(argument)
                instructionstream.AppendLine("{0} = load {2}, {2}* {1}".format(ca[i], ar, argument_type))
                argumentRegisters.append("{1} {0}".format(ca[i], argument_type))
                # TODO: Return type for function.
            instructionstream.AppendLine("call void @{0}({1})".format(identifier, ", ".join(argumentRegisters)))
        else:
            instructionstream.AppendLine("; non-function call")
            r = symboltable.GetOrCreate(component["pstid"]).GetRowWhere("name", identifier)["register"]
            row = GetRow(node)
            node["register"] = r
            row["register"] = r

def postorder_declaration_statement(node):
    instructionstream.AppendLine("; declaration statement")
    type = ConvertType(GetRow(node)["type"])
    if node.Contains("assignment"):
        a = getAdditionalRegisters(node)
        rr = getRegister(node["rvalue"])
        instructionstream.AppendLine("{0} = load {2}, {2}* {1}".format(a[0], rr, type))
    r = getRegister(node)
    instructionstream.AppendLine("{0} = alloca {1}".format(r, type))
    if node.Contains("assignment"):
        instructionstream.AppendLine("store {2} {1}, {2}* {0}".format(r, a[0], type))

def postorder_function_argument(node):
    r, a = getRegisters(node)
    rr = getRegister(node["rvalue"])
    type = ConvertType(node["type"])
    instructionstream.AppendLine("{0} = load {2}, {2}* {1}".format(a[0], rr, type))
    instructionstream.AppendLine("{0} = alloca {1}".format(r, type))
    instructionstream.AppendLine("store {2} {1}, {2}* {0}".format(r, a[0], type))

def postorder_return_statement(node):
    r = getRegister(node)
    rr = getRegister(node["rvalue"])

    type = ConvertType(node["rvalue"]["type"])

    instructionstream.AppendLine("; return statement")
    instructionstream.AppendLine("{0} = load {2}, {2}* {1}".format(r, rr, type))
    type = ConvertType(node["rvalue"]["type"])
    instructionstream.AppendLine("ret {0} {1}".format(type, r))