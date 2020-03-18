def preorder_global(node):
  instructionstream.Append("""
declare i32 @print_int(i32 nocapture) nounwind

define i32 @main() {
  call i32 @print_int(i32 1)
  ret i32 0
}
  """)