choco install -y asmspy
$output = AsmSpy.exe .\bin\Release --all --nonsystem | Out-String
if ($output.Contains("Microsoft.Web.Administration, Version=7.9")) {
    Write-Error "Wrong assembly reference."
    exit 1
}

if (!(Test-Path("PHPManagerForIIS_x64.msi"))) {
    Write-Error "x64 installer misisng."
    exit 1
}

if (!(Test-Path("PHPManagerForIIS_x86.msi"))) {
    Write-Error "x86 installer missing."
    exit 1
}

Write-Host "Verify ended."
