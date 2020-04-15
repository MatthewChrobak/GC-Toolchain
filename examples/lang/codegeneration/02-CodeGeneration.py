from helper import *

registerMap = {}

def GetRow(node):
    st = symboltable.GetOrCreate(node["pstid"])
    row = st.RowAt(node["rowid"])
    return row

def resetRegisterMap():
    global registerMap
    registerMap = {}

def RegisterMapLength():
    global registerMap
    return len(registerMap)

def registerRegister(register):
    global registerMap

    if register[0] == "%":
        return register

    # Does the mapping already exist?
    if register in registerMap:
        return registerMap[register]

    # It doesn't. Make an entry
    n = len(registerMap)
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
        row = GetRow(node)
    r = node["register"]
    r = registerRegister(r)
    row["register"] = r
    node["register"] = r
    return r

def getRegisters(node):
    row = GetRow(node)
    a = getAdditionalRegisters(node)
    r = getRegister(node)
    return r, a

def preorder_global(node):
    instructionstream.AppendLine("declare void @print_int(i32)")

def preorder_function(node):
    row = GetRow(node)
    returnType = ConvertType(row["return_type"], True)

    resetRegisterMap()

    parameters = node.AsArray("function_parameter") if node.Contains("function_parameter") else []
    p_types = [] # Types
    p_ar = [] # Raw value
    p_r = [] # Pointer value
    for parameter in parameters:
        row = GetRow(parameter)
        ar = parameter["additional_registers"][0]
        ar = registerRegister(ar)
        row["additional_registers"] = ar
        p_ar.append(ar)
   
    for parameter in parameters:
        row = GetRow(parameter)
        r = registerRegister(row["register"])
        row["register"] = r
        type = ConvertType(row["type"])
        p_types.append(type)
        p_r.append(r)

    instructionstream.AppendLine("define {1} @{0}({2}) {{".format(node["function_name"], returnType, ",".join(p_types)))
    instructionstream.IncrementTab(1)

    instructionstream.AppendLineNoIndent("{0}: ; entrypoint".format(registerRegister("unused")[1:]))

    for (t, r, a) in zip(p_types, p_r, p_ar):
        instructionstream.AppendLine("{0} = alloca {1}".format(r, t))
        instructionstream.AppendLine("store {0} {1}, {0}* {2}".format(t, a, r))

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
    child = GetPossibleChild(node, ["expression"])
    row = GetRow(node)
    node["register"] = child["register"]
    row["register"] = node["register"]

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
            row = GetRow(component)
            returnType = ConvertType(row["dynamic_type"], True)
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

            prefix = ""
            if returnType != "void":
                realRegister = getRegister(component)
                returnRegister = ca[len(ca) - 1]
                prefix = "{0} = ".format(returnRegister)

                row = GetRow(node)
                node["register"] = realRegister
                row["register"] = realRegister

            instructionstream.AppendLine("{3}call {2} @{0}({1})".format(identifier, ", ".join(argumentRegisters), returnType, prefix))

            if returnType != "void":
                instructionstream.AppendLine("{0} = alloca {1}".format(realRegister, returnType))
                instructionstream.AppendLine("store {0} {1}, {0}* {2}".format(returnType, returnRegister, realRegister))
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

def postorder_function_parameter(node):
    row = GetRow(node)
    r = getRegister(node)
    row["register"] = r

def postorder_expression(node):
    instructionstream.AppendLine("; expression")
    if node.Contains("operator"):
        operator = node["operator"]
        expressions = node.AsArray("expression")
        lhs = expressions[0]
        rhs = expressions[1]

        op, loc = GetValue(GetPossibleChild(operator, ["arithmetic", "logical", "comparison"]))
        r, a = getRegisters(node)
        s = a[2]
        t = ConvertType(node["type"])
        instructionstream.AppendLine("{0} = load {1}, {1}* {2}".format(a[0], t, lhs["register"]))
        instructionstream.AppendLine("{0} = load {1}, {1}* {2}".format(a[1], t, rhs["register"]))
        
        if t == "i32":
            op = {
                "+":"add", 
                "-":"sub", 
                "*":"mul", 
                "/":"sdiv", 
                "&&":"and", 
                "||":"or",
                ">":"icmp sgt",
                ">=":"icmp sge",
                "<":"icmp slt",
                "<=":"icmp sle",
                "==":"icmp eq",
                "!=":"icmp ne"
            }[op]

        instructionstream.AppendLine("{0} = {1} {2} {3}, {4}".format(s, op, t, a[0], a[1]))

        if operator.Contains("comparison"):
            instructionstream.AppendLine("{0} = zext i1 {1} to i32".format(a[3], s))
            s = a[3]
                
        instructionstream.AppendLine("{0} = alloca {1}".format(r, t))
        instructionstream.AppendLine("store {0} {1}, {0}* {2}".format(t, s, r))
    else:
        child = GetPossibleChild(node, ["expression", "lvalue", "integer", "rvalue"])
        r = getRegister(child)
        row = GetRow(node)
        row["register"] = r
        node["register"] = r

        if node.Contains("sign"):
            instructionstream.AppendLine("; sign")
            a = getAdditionalRegisters(node)
            sign, _ = GetValue(node["sign"])
            multiplier = "1" if sign == "+" else "-1"

            instructionstream.AppendLine("{0} = load i32, i32* {1}".format(a[0], node["register"]))
            instructionstream.AppendLine("{0} = mul i32 {1}, {2}".format(a[1], multiplier, a[0]))
            instructionstream.AppendLine("store i32 {0}, i32* {1}".format(a[1], node["register"]))

def postorder_while_loop(node):
    while_condition = node["while_condition"]
    compare_marker = while_condition["compare_marker"]
    false_marker = registerRegister(node["false_marker"])[1:]
    true_marker = while_condition["true_marker"]
    r = while_condition["register"]
    index = while_condition["branch_instruction_index"]

    instructionstream.AppendLine("br label %{0}".format(compare_marker))
    instructionstream.AppendLineNoIndent("{0}: ; While - false_marker".format(false_marker))
    instructionstream.AppendLineAt(index, "br i1 {0}, label %{1}, label %{2}".format(r, true_marker, false_marker))

def preorder_while_condition(node):
    r = registerRegister(node["compare_marker"])[1:]
    node["compare_marker"] = r

    instructionstream.AppendLine("br label %{0}".format(r))
    instructionstream.AppendLineNoIndent("{0}: ; While - compare_marker".format(r))

def postorder_while_condition(node):
    r, a = getRegisters(node)
    true_marker = registerRegister(node["true_marker"])[1:]
    node["true_marker"] = true_marker

    instructionstream.AppendLine("{0} = load i32, i32* {1}".format(a[0], node["rvalue"]["register"]))
    instructionstream.AppendLine("{0} = icmp ne i32 {1}, 0".format(r, a[0]))

    node["branch_instruction_index"] = instructionstream.CreateEmptyInstruction()

    instructionstream.AppendLineNoIndent("{0}: ; While - true_marker".format(true_marker))