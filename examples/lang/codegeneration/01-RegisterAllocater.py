registerCount = 1

def reset():
    global registerCount
    registerCount = 1

def getAdditionalRegisters(node, count):
    registers = []
    for i in range(count):
        registers.append(get())
    node["additional_registers"] = registers

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
    st = symboltable.GetOrCreate(node["pstid"])
    row = st.RowAt(node["rowid"])
    row["register"] = register
    node["register"] = register

def postorder_function(node):
    reset()

def postorder_integer(node):
    allocate(node)

def postorder_lvalue_component(node):
    if node.Contains("function_call"):
        fc = node["function_call"]
        arguments = fc.AsArray("function_argument") if fc.Contains("function_argument") else []
        getAdditionalRegisters(node, len(arguments))

def postorder_lvalue_statement(node):
    if node.Contains("assignment"):
        node["register"] = get()

def postorder_lvalue(node):
    allocate(node)

def postorder_rvalue(node):
    allocate(node)

def postorder_declaration_statement(node):
    getAdditionalRegisters(node, 1)
    allocate(node)

def postorder_function_argument(node):
    getAdditionalRegisters(node, 1)
    allocate(node)

def postorder_return_statement(node):
    node["register"] = get()