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

  instructionstream.Append("""
define i32 @{0}() {{
""".format(functionName))


def handlePostFunction(node):
  instructionstream.Append("""
  ret i32 0
}

""")

def preorder_global(node):
  instructionstream.Append("""

  """)