def preorder_global(node):
    instructionstream.Append("""
@.str = private unnamed_addr constant [12 x i8] c"hello world\0"
declare i32 @print_str(i8* nocapture) nounwind

define i32 @main() {
  %str = getelementptr [12 x i8], [12 x i8]* @.str, i64 0, i64 0
  call i32 @print_str(i8* %str)
  ret i32 0
}

""")