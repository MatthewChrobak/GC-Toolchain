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
    else:
        getAdditionalRegisters(node, 1)
        allocate(node)

def preorder_lvalue_statement(node):
    if not node.Contains("assignment"):
        row = symboltable.GetOrCreate(node["lvalue"]["pstid"]).RowAt(node["lvalue"]["rowid"])
        row["do_not_allocate_register"] = True

def postorder_lvalue(node):
    row = symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])
    if not row.Contains("do_not_allocate_register"):
        getAdditionalRegisters(node, 1)
        allocate(node)

def postorder_rvalue(node):
    getAdditionalRegisters(node, 1)
    allocate(node)

def postorder_declaration_statement(node):
    getAdditionalRegisters(node, 1)
    allocate(node)

def postorder_function_argument(node):
    getAdditionalRegisters(node, 1)
    allocate(node)

def postorder_return_statement(node):
    node["register"] = get()