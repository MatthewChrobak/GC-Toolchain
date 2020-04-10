from helper import *

registerMap = {}

def resetRegisterMap():
    global registerMap
    registerMap = {}

def registerRegister(register):
    global registerMap

    if register.startsWith("%"):
        return register

    # Does the mapping already exist?
    if register in registerMap:
        return registerMap[register]

    # It doesn't. Make an entry
    n = len(registerMap) + 1
    registerMap[register] = "%{0}".format(str(n))
    return registerMap[register]

def getRegisters(node):
    r = node["register"]
    r = registerRegister(r)

    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    row["register"] = r

    a = []
    if node.Contains("additional_registers"):
        for r in node["additional_register"]:
            a.append(registerRegister(r))
    row["additional_registers"] = ", ".join(a)

    node["register"] = r
    node["additional_registes"] = a
    return r, a
    

def typeMorpher(type):
    if type == "int":
        return "i32"
    return type

def getAdditionalRegisters(node):
    return node["additional_registers"]

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
    instructionstream.AppendLine("; integer")
    value, loc = GetValue(node)
    instructionstream.AppendLine("{0} = alloca i32".format(node["register"]))
    instructionstream.AppendLine("store i32 {1}, i32* {0}".format(node["register"], value))

def postorder_rvalue(node):
    instructionstream.AppendLine("; rvalue")
    possibleChildren = ["integer", "lvalue"]
    for child in possibleChildren:
        if node.Contains(child):
            r = getAdditionalRegisters(node)
            instructionstream.AppendLine("{0} = load i32, i32* {1}".format(r[0], node[child]["register"]))
            instructionstream.AppendLine("{0} = alloca i32".format(node["register"]))
            instructionstream.AppendLine("store i32 {1}, i32* {0}".format(node["register"], r[0]))
            break

def postorder_lvalue_statement(node):
    instructionstream.AppendLine("; lvalue statement")


def postorder_lvalue(node):
    instructionstream.AppendLine("; lvalue")
    components = node.AsArray("lvalue_component")
    last_register = None

    for component in components:
        identifier, loc = GetValue(component["identifier"])
        r = getAdditionalRegisters(component)

        if component.Contains("function_call"):
            instructionstream.AppendLine("; function call")
            instructionstream.AppendLine("; function arguments")
            fc = component["function_call"]
            arguments = fc.AsArray("function_argument") if fc.Contains("function_argument") else []
            argumentRegisters = []
            for i in range(len(arguments)):
                instructionstream.AppendLine("{0} = load i32, i32* {1}".format(r[i], arguments[i]["register"]))
                argumentRegisters.append("i32 {0}".format(r[i]))
            instructionstream.AppendLine("call void @{0}({1})".format(identifier, ", ".join(argumentRegisters)))
        else:
            instructionstream.AppendLine("; non-function call")
            row = symboltable.GetOrCreate(component["pstid"]).GetRowWhere("name", identifier)
            instructionstream.AppendLine("{0} = load i32, i32* {1}".format(r[0], row["register"]))
            instructionstream.AppendLine("{0} = alloca i32".format(component["register"]))
            instructionstream.AppendLine("store i32 {1}, i32* {0}".format(component["register"], r[0]))
            last_register = component["register"]

    instructionstream.AppendLine("; lvalue end")
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    if not row.Contains("do_not_allocate_register"):
        r = getAdditionalRegisters(node)

        instructionstream.AppendLine("{0} = load i32, i32* {1}".format(r[0], last_register))
        instructionstream.AppendLine("{0} = alloca i32".format(node["register"]))
        instructionstream.AppendLine("store i32 {1}, i32* {0}".format(node["register"], r[0]))

# def postorder_lvalue(node):
#     components = node.AsArray("lvalue_component")
#     previous_register = None

#     for component in components:
#         identifier, loc = GetValue(component["identifier"])
#         if component.Contains("function_call"):
#             instructionstream.Append("{0} = call void @{1}(".format(component["register"], identifier))

#             fc = component["function_call"]
#             arguments = fc.AsArray("function_argument") if fc.Contains("function_argument") else []
#             argumentRegisters = []
#             for argument in arguments:
#                 argumentRegisters.append("i32 {0}".format(argument["register"]))
#             instructionstream.Append(", ".join(argumentRegisters))
#             instructionstream.AppendLine(")")

#             previous_register = component["register"]
#         else:
#             row = symboltable.GetOrCreate(component["pstid"]).GetRowWhere("name", identifier)
#             instructionstream.AppendLine("{0} = alloca i32".format(node["register"]))
#             instructionstream.AppendLine("store i32 {1}, i32* {0}".format(component["register"], row["register"]))
#             previous_register = row["register"]
    
#     instructionstream.AppendLine("{0} = alloca i32".format(node["register"]))
#     instructionstream.AppendLine("store i32 {1}, i32* {0}".format(node["register"], previous_register))

def postorder_declaration_statement(node):
    instructionstream.AppendLine("; declaration statement")
    r = getAdditionalRegisters(node)

    if node.Contains("assignment"):
        instructionstream.AppendLine("{0} = load i32, i32* {1}".format(r[0], node["rvalue"]["register"]))
    instructionstream.AppendLine("{0} = alloca i32".format(node["register"]))
    if node.Contains("assignment"):
        instructionstream.AppendLine("store i32 {1}, i32* {0}".format(node["register"], r[0]))

def postorder_function_argument(node):
    r = getAdditionalRegisters(node)
    instructionstream.AppendLine("{0} = load i32, i32* {1}".format(r[0], node["rvalue"]["register"]))
    instructionstream.AppendLine("{0} = alloca i32".format(node["register"]))
    instructionstream.AppendLine("store i32 {1}, i32* {0}".format(node["register"], r[0]))

def postorder_return_statement(node):
    instructionstream.AppendLine("; return statement")

    instructionstream.AppendLine("{0} = load i32, i32* {1}".format(node["register"], node["rvalue"]["register"]))
    type = typeMorpher(node["rvalue"]["type"])
    instructionstream.AppendLine("ret {0} {1}".format(type, node["register"]))