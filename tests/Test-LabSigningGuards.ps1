[CmdletBinding()]
param([Parameter(Mandatory)][string]$UnsignedExecutable)
$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'Windows signature validation required.' }
$repo = Split-Path $PSScriptRoot
$temp = Join-Path ([IO.Path]::GetTempPath()) ('bifrost-signing-guards-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
    $root = Join-Path $repo 'docs/signing/microsoft-private-trust-root-2020.cer'
    $bytes = [IO.File]::ReadAllBytes($root)
    $bytes[0] = $bytes[0] -bxor 1
    $badRoot = Join-Path $temp 'tampered.cer'
    [IO.File]::WriteAllBytes($badRoot, $bytes)
    $refused = $false
    try { & (Join-Path $repo 'scripts/Trust-LabSigningRoot.ps1') -CertificatePath $badRoot -StoreLocation CurrentUser -DisposableLab } catch {
        if ($_.Exception.Message -ne 'Private Trust root differs from the reviewed Microsoft certificate.') { throw }
        $refused = $true
    }
    if (-not $refused) { throw 'Tampered root was accepted.' }
    $receipt = Join-Path $temp 'receipt.json'
    $refused = $false
    try {
        & (Join-Path $repo 'scripts/Verify-LabSignedArtifact.ps1') -Executable $UnsignedExecutable -PublisherSubject 'CN=Unapproved test publisher' -UnsignedSha256 (Get-FileHash $UnsignedExecutable).Hash -SourceCommit ('0' * 40) -OutputPath $receipt
    } catch {
        if ($_.Exception.Message -ne 'Authenticode publisher, timestamp or signature verification failed.') { throw }
        $refused = $true
    }
    if (-not $refused -or (Test-Path $receipt)) { throw 'Unsigned executable produced an acceptance receipt.' }
    Write-Output '2 Windows lab-signing rejection checks passed.'
} finally { Remove-Item -LiteralPath $temp -Recurse -Force }
