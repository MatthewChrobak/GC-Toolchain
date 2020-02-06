# Build GCT
$gct = "../../source/GCT/bin/Debug/netcoreapp3.1/GCT.exe";
if (-Not (Test-Path $gct)) {
    Write-Host "Building GCT...";
    $csproj = "../../source/GCT/GCT.csproj";
    $res = Invoke-MsBuild $csproj
    
    if ($res.BuildSucceeded -eq $false) {
        Write-Host $res.Message;
        Write-Host "Build did not succeed. Terminating.";
        exit;
    }
}


$currentFolder = (Get-Location).Path;
# Invoke GCT
Write-Host $currentFolder;
Start-Process $gct -ArgumentList "-f '$currentFolder' -v -all";