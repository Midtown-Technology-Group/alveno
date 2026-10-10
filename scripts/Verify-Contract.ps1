# Copyright (c) 2026 Midtown Technology Group
# SPDX-License-Identifier: AGPL-3.0-only

[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot
$provenance = Get-Content -LiteralPath (Join-Path $repo 'contract/provenance.json') -Raw | ConvertFrom-Json
foreach ($entry in @(
    @{ file = 'schema.json'; expected = $provenance.schema_sha256 },
    @{ file = 'fixtures.json'; expected = $provenance.fixtures_sha256 }
)) {
    if ((Get-FileHash -LiteralPath (Join-Path $repo ('contract/' + $entry.file)) -Algorithm SHA256).Hash -ne $entry.expected) {
        throw "Canonical contract digest mismatch: $($entry.file)"
    }
}
$header = Get-Content -LiteralPath (Join-Path $repo 'src/Bifrost.Bootstrap/Protocol.cs') -TotalCount 1
if (-not $header.Contains($provenance.schema_sha256)) { throw 'Generated DTO schema provenance differs.' }
Write-Output 'Canonical schema, fixtures and generated DTO provenance verified.'
