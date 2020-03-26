# Build the stdio lib
Write-Host "Compiling stdio lib..."
clang -emit-llvm -S stdio.c

# Remove the previous .ll
if (Test-Path output.ir) {
    if (Test-Path output.ll) {
        Remove-Item output.ll
    }
    Rename-Item output.ir output.ll
}

Write-Host "Compiling output.ir..."
clang output.ll stdio.ll -o program.exe -v