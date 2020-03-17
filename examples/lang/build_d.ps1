# Build the stdio lib
Write-Host "Compiling stdio lib..."
clang -emit-llvm -S stdio_d.c

# Remove the previous .ll
if (Test-Path output.ll) {
    Remove-Item output.ll
}
Rename-Item output.ir output.ll

Write-Host "Compiling output.ir..."
clang output.ll stdio_d.ll -o program.exe -v