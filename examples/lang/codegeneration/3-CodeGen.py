from NamespaceHelper import *

# def preorder_global(node):
#     instructionstream.Append("""
# @.str = private unnamed_addr constant [12 x i8] c"hello world\0"
# declare i32 @print_str(i8* nocapture) nounwind

# define i32 @main() {
#   %str = getelementptr [12 x i8], [12 x i8]* @.str, i64 0, i64 0
#   call i32 @print_str(i8* %str)
#   ret i32 0
# }

# """)

def bubbleupRegister_err(node, possibleChildren, comment = ""):
  if not bubbleupRegister(node, possibleChildren, comment):
    Error("Unable to find children in node")

def bubbleupRegister(node, possibleChildren, comment = ""):
  for possibleChild in possibleChildren:
    if node.Contains(possibleChild):
      child = node[possibleChild]
      myRegister = node["register"]
      childRegister = child["register"]
      instructionstream.AppendLine("{0} = {1} ; {2}".format(myRegister, childRegister, comment))
      return True
  return False

def preorder_function(node):
  handlePreFunction(node)
def postorder_function(node):
  handlePostFunction(node)

def preorder_free_function(node):
  handlePreFunction(node)
def postorder_free_function(node):
  handlePostFunction(node)

def handlePreFunction(node):
  functionName, loc = getNodeValues(node, "function_name")
  instructionstream.Append("define i32 @{0}() {{".format(functionName))

def handlePostFunction(node):
  instructionstream.AppendLine("""
  ret i32 0
}""")

def preorder_global(node):
  instructionstream.AppendLine("declare i32 @print_int(i32 nocapture) nounwind")

def preorder_lvalue_component(node):
  if node.Contains("function_identifier"):
    function_name, loc = getNodeValues(node, "function_identifier")
    instructionstream.Append("call i32 @{0}(".format(function_name))

    if node.Contains("function_argument"):
      for argument in node.AsArray("function_argument"):
        instructionstream.Append("i32 1")

    instructionstream.AppendLine(")")

def postorder_integer(node):
  instructionstream.AppendLine("{0} = {1} ; integer".format(node["register"], node["value"]))

def postorder_rvalue(node):
  bubbleupRegister_err(node, ["integer", "lvalue"], "rvalue")

def postorder_expression(node):
  if node.Contains("operator"):
    print("Can't do operators yet")
  else:
    bubbleupRegister(node, ["rvalue"], "expression")

def postorder_declaration_statement(node):
  bubbleupRegister(node, ["expression"], "declaration statement")