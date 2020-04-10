from helper import *

registerMap = {}

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
    
def typeMorpher(type):
    if type == "int":
        return "i32"
    return type

def preorder_global(node):
    instructionstream.AppendLine("declare void @print_int(i32)")

def preorder_function(node):
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    returnType = typeMorpher(row["return_type"])
    instructionstream.AppendLine("define {1} @{0}() {{".format(node["function_name"], returnType))
    instructionstream.IncrementTab(1)

def postorder_function(node):
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    returnType = typeMorpher(row["return_type"])
    if returnType == "void":
        instructionstream.AppendLine("ret void")
    instructionstream.IncrementTab(-1)
    instructionstream.AppendLine("}")

def postorder_integer(node):
    r = getRegister(node)
    instructionstream.AppendLine("; integer")
    value, loc = GetValue(node)
    instructionstream.AppendLine("{0} = alloca i32".format(r))
    instructionstream.AppendLine("store i32 {1}, i32* {0}".format(r, value))

def postorder_rvalue(node):
    instructionstream.AppendLine("; rvalue")
    possibleChildren = ["integer", "lvalue"]
    r, a = getRegisters(node)
    for child in possibleChildren:
        if node.Contains(child):
            cr = getRegister(node[child])
            instructionstream.AppendLine("{0} = load i32, i32* {1}".format(a[0], cr))
            instructionstream.AppendLine("{0} = alloca i32".format(r))
            instructionstream.AppendLine("store i32 {1}, i32* {0}".format(r, a[0]))
            break

def postorder_lvalue_statement(node):
    instructionstream.AppendLine("; lvalue statement")

def postorder_lvalue(node):
    instructionstream.AppendLine("; lvalue")
    components = node.AsArray("lvalue_component")
    last_register = None

    for component in components:
        identifier, loc = GetValue(component["identifier"])

        if component.Contains("function_call"):
            instructionstream.AppendLine("; function call")
            instructionstream.AppendLine("; function arguments")
            ca = getAdditionalRegisters(component)
            fc = component["function_call"]
            arguments = fc.AsArray("function_argument") if fc.Contains("function_argument") else []
            argumentRegisters = []
            for i in range(len(arguments)):
                ar = getRegister(arguments[i])
                instructionstream.AppendLine("{0} = load i32, i32* {1}".format(ca[i], ar))
                argumentRegisters.append("i32 {0}".format(ca[i]))
            instructionstream.AppendLine("call void @{0}({1})".format(identifier, ", ".join(argumentRegisters)))
        else:
            cr, ca = getRegisters(component)
            instructionstream.AppendLine("; non-function call")
            row = symboltable.GetOrCreate(component["pstid"]).GetRowWhere("name", identifier)
            instructionstream.AppendLine("{0} = load i32, i32* {1}".format(ca[0], row["register"]))
            instructionstream.AppendLine("{0} = alloca i32".format(cr))
            instructionstream.AppendLine("store i32 {1}, i32* {0}".format(cr, ca[0]))
            last_register = cr

    instructionstream.AppendLine("; lvalue end")
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    if not row.Contains("do_not_allocate_register"):
        r, a = getRegisters(node)
        instructionstream.AppendLine("{0} = load i32, i32* {1}".format(a[0], last_register))
        instructionstream.AppendLine("{0} = alloca i32".format(r))
        instructionstream.AppendLine("store i32 {1}, i32* {0}".format(r, a[0]))

def postorder_declaration_statement(node):
    instructionstream.AppendLine("; declaration statement")
    r, a = getRegisters(node)

    if node.Contains("assignment"):
        rr = getRegister(node["rvalue"])
        instructionstream.AppendLine("{0} = load i32, i32* {1}".format(a[0], rr))
    instructionstream.AppendLine("{0} = alloca i32".format(r))
    if node.Contains("assignment"):
        instructionstream.AppendLine("store i32 {1}, i32* {0}".format(r, a[0]))

def postorder_function_argument(node):
    r, a = getRegisters(node)
    rr = getRegister(node["rvalue"])
    instructionstream.AppendLine("{0} = load i32, i32* {1}".format(a[0], rr))
    instructionstream.AppendLine("{0} = alloca i32".format(r))
    instructionstream.AppendLine("store i32 {1}, i32* {0}".format(r, a[0]))

def postorder_return_statement(node):
    r = getRegister(node)
    rr = getRegister(node["rvalue"])

    instructionstream.AppendLine("; return statement")
    instructionstream.AppendLine("{0} = load i32, i32* {1}".format(r, rr))
    type = typeMorpher(node["rvalue"]["type"])
    instructionstream.AppendLine("ret {0} {1}".format(type, r))