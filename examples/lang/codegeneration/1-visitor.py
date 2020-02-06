def preorder_global(node):
    instructionstream.AppendLine("""
declare void @print_int(i32)
declare i8 @getkey()

define i32 @main() {
  call void @print_int(i32 5)
  call i8 @getkey()
  ret i32 0
}
    """)