# Copyright (c) 2026 Midtown Technology Group
# SPDX-License-Identifier: AGPL-3.0-only

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Executable,
    [Parameter(Mandatory)][uri]$Endpoint,
    [Parameter(Mandatory)][string]$PublisherSubject,
    [Parameter(Mandatory)][string]$IcdPath,
    [Parameter(Mandatory)][string]$OutputPath,
    [switch]$AllowSoftwareKey
)
$ErrorActionPreference = 'Stop'
if ($Endpoint.Scheme -ne 'https' -or $Endpoint.AbsolutePath -ne '/' -or $Endpoint.Query -or $Endpoint.Fragment -or $Endpoint.UserInfo) { throw 'HTTPS enrollment origin required.' }
$signature = Get-AuthenticodeSignature -LiteralPath $Executable
if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -ne $PublisherSubject -or -not $signature.TimeStamperCertificate) { throw 'An independently verified, timestamped bootstrap release is required.' }
if (-not (Test-Path -LiteralPath $IcdPath)) { throw 'ICD.exe is required.' }
$store = Join-Path (Split-Path $IcdPath) 'Microsoft-Desktop-Provisioning.dat'
if (-not (Test-Path -LiteralPath $store)) { throw 'ICD desktop store is required.' }
$stage = Join-Path $env:TEMP ('bifrost-enrollment-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
try {
    Copy-Item -LiteralPath $Executable -Destination (Join-Path $stage 'bifrost-bootstrap.exe')
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Install-Bootstrap.ps1') -Destination $stage
    @{ endpoint = $Endpoint.AbsoluteUri; publisher = $PublisherSubject; sha256 = (Get-FileHash -LiteralPath $Executable -Algorithm SHA256).Hash; software_key = [bool]$AllowSoftwareKey } |
        ConvertTo-Json | Set-Content -LiteralPath (Join-Path $stage 'enrollment.json') -Encoding UTF8
    @'
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$metadata = Get-Content -LiteralPath (Join-Path $root 'enrollment.json') -Raw | ConvertFrom-Json
& (Join-Path $root 'Install-Bootstrap.ps1') -Executable (Join-Path $root 'bifrost-bootstrap.exe') -Endpoint $metadata.endpoint -PublisherSubject $metadata.publisher -Sha256 $metadata.sha256 -AllowSoftwareKey:$metadata.software_key
'@ | Set-Content -LiteralPath (Join-Path $stage 'Invoke-Enrollment.ps1') -Encoding UTF8
    $files = @('bifrost-bootstrap.exe', 'Install-Bootstrap.ps1', 'Invoke-Enrollment.ps1', 'enrollment.json')
    $assets = ($files | ForEach-Object { '<CommandFile Name="' + [Security.SecurityElement]::Escape($_) + '">' + [Security.SecurityElement]::Escape((Join-Path $stage $_)) + '</CommandFile>' }) -join "`n"
    $xml = @"
<?xml version="1.0" encoding="utf-8"?>
<WindowsCustomizations>
<PackageConfig xmlns="urn:schemas-Microsoft-com:Windows-ICD-Package-Config.v1.0">
<ID>{$([guid]::NewGuid())}</ID><Name>BiFrost Device Enrollment</Name><Version>1.0</Version><OwnerType>ITAdmin</OwnerType><Rank>0</Rank><Notes>Generic credential-free enrollment</Notes>
</PackageConfig>
<Settings xmlns="urn:schemas-microsoft-com:windows-provisioning"><Customizations><Common><ProvisioningCommands><DeviceContext>
<CommandFiles>$assets</CommandFiles><CommandLine>powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File Invoke-Enrollment.ps1</CommandLine>
</DeviceContext></ProvisioningCommands></Common></Customizations></Settings>
</WindowsCustomizations>
"@
    $xmlPath = Join-Path $stage 'customizations.xml'
    $xml | Set-Content -LiteralPath $xmlPath -Encoding UTF8
    $OutputPath = [IO.Path]::GetFullPath($OutputPath)
    New-Item -ItemType Directory -Path (Split-Path $OutputPath) -Force | Out-Null
    $arguments = '/Build-ProvisioningPackage /CustomizationXML:"' + $xmlPath + '" /PackagePath:"' + $OutputPath + '" /StoreFile:"' + $store + '" +Overwrite'
    $process = Start-Process -FilePath $IcdPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    if ($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $OutputPath)) { throw 'Provisioning package build failed.' }
    Get-Item -LiteralPath $OutputPath
} finally { Remove-Item -LiteralPath $stage -Recurse -Force }
