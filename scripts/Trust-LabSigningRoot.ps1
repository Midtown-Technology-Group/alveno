# Copyright (c) 2026 Midtown Technology Group
# SPDX-License-Identifier: AGPL-3.0-only

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CertificatePath,
    [ValidateSet('CurrentUser', 'LocalMachine')][string]$StoreLocation = 'LocalMachine',
    [switch]$DisposableLab
)
$ErrorActionPreference = 'Stop'
if (-not $DisposableLab -or $env:OS -ne 'Windows_NT') { throw 'Explicit disposable Windows lab authorization required.' }
$expected = 'D549DC2314F7A16E496A515491B273BC9C098E40A070D61EF1602870F0C402D8'
if ((Get-FileHash -LiteralPath $CertificatePath -Algorithm SHA256).Hash -ne $expected) {
    throw 'Private Trust root differs from the reviewed Microsoft certificate.'
}
$root = [Security.Cryptography.X509Certificates.X509Certificate2]::new([IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $CertificatePath)))
if ($root.HasPrivateKey -or $root.NotAfter.ToUniversalTime() -le [DateTime]::UtcNow) { throw 'Invalid public root certificate.' }
$path = "Cert:\$StoreLocation\Root\$($root.Thumbprint)"
$existed = Test-Path -LiteralPath $path
if (-not $existed) { Import-Certificate -FilePath $CertificatePath -CertStoreLocation "Cert:\$StoreLocation\Root" | Out-Null }
if (-not (Test-Path -LiteralPath $path)) { throw 'Lab root trust readback failed.' }
[pscustomobject]@{ thumbprint = $root.Thumbprint; sha256 = $expected; store = $StoreLocation; newly_imported = -not $existed }
