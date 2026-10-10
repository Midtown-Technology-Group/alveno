# Copyright (c) 2026 Midtown Technology Group
# SPDX-License-Identifier: AGPL-3.0-only

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Executable,
    [Parameter(Mandatory)][uri]$Endpoint,
    [Parameter(Mandatory)][string]$PublisherSubject,
    [Parameter(Mandatory)][ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$Sha256,
    [switch]$AllowSoftwareKey
)
$ErrorActionPreference = 'Stop'
if ($Endpoint.Scheme -ne 'https' -or $Endpoint.AbsolutePath -ne '/' -or $Endpoint.Query -or $Endpoint.Fragment -or $Endpoint.UserInfo) { throw 'A public HTTPS enrollment origin is required.' }
$signature = Get-AuthenticodeSignature -LiteralPath $Executable
if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -ne $PublisherSubject -or -not $signature.TimeStamperCertificate) { throw 'Bootstrap publisher or timestamp verification failed.' }
if ((Get-FileHash -LiteralPath $Executable -Algorithm SHA256).Hash -ne $Sha256) { throw 'Bootstrap hash verification failed.' }
$arguments = @('install', '--endpoint', $Endpoint.AbsoluteUri)
if ($AllowSoftwareKey) { $arguments += '--allow-software-key' }
& $Executable @arguments
if ($LASTEXITCODE -ne 0) { throw 'Bootstrap installation failed.' }
