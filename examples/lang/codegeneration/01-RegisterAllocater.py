registerCount = 1

def GetRow(node):
    return symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])

def reset():
    global registerCount
    registerCount = 1

def getAdditionalRegisters(node, count):
    registers = []
    for i in range(count):
        registers.append(get())
    node["additional_registers"] = registers

    if not node.Contains("pstid"):
        return

    st = symboltable.GetOrCreate(node["pstid"])
    row = st.RowAt(node["rowid"])
    row["additional_registers"] = ", ".join(registers)

def get():
    global registerCount
    register = "u{0}".format(str(registerCount))
    registerCount += 1
    return register

def allocate(node):
    register = get()
    node["register"] = register

    if not node.Contains("pstid"):
        return

    row = GetRow(node)
    row["register"] = register

def postorder_function(node):
    reset()

def postorder_integer(node):
    allocate(node)

def postorder_float(node):
    allocate(node)

def postorder_lvalue_component(node):
    if node.Contains("function_call"):
        row = GetRow(node)
        fc = node["function_call"]
        arguments = fc.AsArray("function_argument") if fc.Contains("function_argument") else []
        numArguments = len(arguments)

        if row["dynamic_type"] != "void":
            numArguments += 1

        getAdditionalRegisters(node, numArguments)

        if row["dynamic_type"] != "void":
            allocate(node)

def postorder_lvalue_statement(node):
    if node.Contains("assignment"):
        node["register"] = get()

def postorder_lvalue(node):
    allocate(node)

def postorder_rvalue(node):
    allocate(node)

def postorder_expression(node):
    if node.Contains("operator"):
        operator = node["operator"]
        if operator.Contains("comparison"):
            getAdditionalRegisters(node, 4)
        else:
            getAdditionalRegisters(node, 3)
    else:
        if node.Contains("sign"):
            getAdditionalRegisters(node, 2)

    allocate(node)

def postorder_declaration_statement(node):
    getAdditionalRegisters(node, 1)
    allocate(node)

def postorder_function_argument(node):
    getAdditionalRegisters(node, 1)
    allocate(node)

def postorder_function_parameter(node):
    getAdditionalRegisters(node, 1)
    allocate(node)

def postorder_return_statement(node):
    getAdditionalRegisters(node, 1)
    allocate(node)

def postorder_while_loop(node):
    node["false_marker"] = get()

def postorder_while_condition(node):
    getAdditionalRegisters(node, 1)
    allocate(node)
    node["compare_marker"] = get()
    node["true_marker"] = get()

def postorder_if_statement(node):
    node["end_marker"] = get()
    
def postorder_if_condition(node):
    getAdditionalRegisters(node, 1)
    allocate(node)
    node["true_marker"] = get()

def preorder_else(node):
    node["else_marker"] = get()

def postorder_for_loop(node):
    node["end_marker"] = get()

def preorder_for_condition(node):
    if node.Contains("rvalue"):
        getAdditionalRegisters(node, 1)
        allocate(node)
    node["compare_marker"] = get()

def preorder_for_update(node):
    node["update_marker"] = get()

def preorder_for_body(node):
    node["body_marker"] = get()