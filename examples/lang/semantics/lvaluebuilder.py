def preorder_lvalue(node):
    lvalue_components = node.AsArray("lvalue_component")

    full_lvalue = ""

    # for lvalue_component in lvalue_components: