[CmdletBinding()]
param([string]$OutputPath = (Join-Path $PSScriptRoot '..\dist\win-x64'))
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    & dotnet run --project tests/Bifrost.Bootstrap.Tests -p:PublishAot=false
    if ($LASTEXITCODE -ne 0) { throw 'Contract and interruption tests failed.' }
    & dotnet publish src/Bifrost.Bootstrap -c Release -r win-x64 --self-contained true -o $OutputPath
    if ($LASTEXITCODE -ne 0) { throw 'Windows Native AOT publication failed.' }
    # Output is unsigned. Protected Authenticode signing and installer readback
    # are mandatory before this artifact becomes a release or OOBE candidate.
    Get-FileHash -LiteralPath (Join-Path $OutputPath 'bifrost-bootstrap.exe') -Algorithm SHA256
} finally { Pop-Location }
