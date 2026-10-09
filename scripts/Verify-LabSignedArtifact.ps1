[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Executable,
    [Parameter(Mandatory)][string]$PublisherSubject,
    [Parameter(Mandatory)][ValidatePattern('^[a-fA-F0-9]{64}$')][string]$UnsignedSha256,
    [Parameter(Mandatory)][ValidatePattern('^[a-f0-9]{40}$')][string]$SourceCommit,
    [Parameter(Mandatory)][string]$OutputPath
)
$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT' -or [string]::IsNullOrWhiteSpace($PublisherSubject)) { throw 'Windows and an independently approved publisher are required.' }
$signature = Get-AuthenticodeSignature -LiteralPath $Executable
if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -cne $PublisherSubject -or -not $signature.TimeStamperCertificate) {
    throw 'Authenticode publisher, timestamp or signature verification failed.'
}
$ekus = @($signature.SignerCertificate.Extensions | Where-Object { $_ -is [Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension] } | ForEach-Object { $_.EnhancedKeyUsages } | ForEach-Object { $_.Value })
if ('1.3.6.1.5.5.7.3.3' -notin $ekus -or -not ($ekus | Where-Object { $_.StartsWith('1.3.6.1.4.1.311.97.1.3.1.') })) {
    throw 'Private Trust code-signing certificate required; CI Policy certificates are not accepted.'
}
$chain = [Security.Cryptography.X509Certificates.X509Chain]::new()
try {
    $chain.ChainPolicy.RevocationMode = [Security.Cryptography.X509Certificates.X509RevocationMode]::Online
    $chain.ChainPolicy.UrlRetrievalTimeout = [TimeSpan]::FromSeconds(30)
    # Authenticode already verifies signing-time validity through the timestamp.
    $chain.ChainPolicy.VerificationTime = $signature.SignerCertificate.NotBefore.AddMinutes(1)
    if (-not $chain.Build($signature.SignerCertificate)) { throw 'Independent certificate-chain verification failed.' }
    $root = $chain.ChainElements[$chain.ChainElements.Count - 1].Certificate
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $rootHash = [BitConverter]::ToString($sha.ComputeHash($root.RawData)).Replace('-', '') } finally { $sha.Dispose() }
    if ($rootHash -ne 'D549DC2314F7A16E496A515491B273BC9C098E40A070D61EF1602870F0C402D8') { throw 'Unexpected Private Trust root.' }
} finally { $chain.Dispose() }
$signedHash = (Get-FileHash -LiteralPath $Executable -Algorithm SHA256).Hash
if ($signedHash -eq $UnsignedSha256) { throw 'Signing did not change the artifact.' }
$receipt = [ordered]@{
    scope = 'disposable-lab-only'; source_commit = $SourceCommit; unsigned_sha256 = $UnsignedSha256; signed_sha256 = $signedHash
    signature_status = [string]$signature.Status; publisher = $signature.SignerCertificate.Subject
    signer_thumbprint = $signature.SignerCertificate.Thumbprint; timestamp_thumbprint = $signature.TimeStamperCertificate.Thumbprint
    private_root_sha256 = $rootHash; account = 'mtg-sopdet-signing'; profile = 'mtg-lab-profile'
    producer_run = $env:GITHUB_RUN_ID; producer_attempt = $env:GITHUB_RUN_ATTEMPT; observed_at = [DateTimeOffset]::UtcNow.ToString('o')
}
$receipt | ConvertTo-Json | Set-Content -LiteralPath $OutputPath -Encoding utf8
