def preorder_global(node):
    instructionstream.AppendLine("""
declare void @print_int(i32)
declare void @print_float(float)
declare i8 @getkey()
    """)

def GetRow(node):
  return symboltable.GetOrCreate(node["pstid"]).RowAt(node["rowid"])

def preorder_function(node):
  row = GetRow(node)

  instructionstream.Append("define ")
  instructionstream.Append("void ")
  instructionstream.Append("@{0} ".format(row["name"]))
  instructionstream.AppendLine("() {")
  instructionstream.AppendLine("entry:")
  instructionstream.AppendLine("ret void")
  instructionstream.Append("}")
  