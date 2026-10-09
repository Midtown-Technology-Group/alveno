[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('BeforeReboot', 'AfterReboot')][string]$Phase,
    [Parameter(Mandatory)][switch]$DisposableVm,
    [string]$OutputPath = 'C:\ProgramData\MTG\BiFrostAcceptance',
    [switch]$Reboot
)
$ErrorActionPreference = 'Stop'
if (-not $DisposableVm) { throw 'Only disposable acceptance VMs are authorized.' }
$executable = 'C:\ProgramData\MTG\BiFrostBootstrap\bifrost-bootstrap.exe'
if (-not (Test-Path -LiteralPath $executable)) { throw 'Apply the generic provisioning package at OOBE first.' }
$signature = Get-AuthenticodeSignature -LiteralPath $executable
if ($signature.Status -ne 'Valid' -or -not $signature.TimeStamperCertificate) { throw 'Signed release acceptance is required.' }
$task = Get-ScheduledTask -TaskPath '\MTG\' -TaskName 'BiFrostBootstrap'
if ($task.Principal.UserId -notin @('SYSTEM', 'S-1-5-18') -or $task.Principal.RunLevel -ne 'Highest') { throw 'The startup task must run as SYSTEM with highest privileges.' }
& $executable run
if ($LASTEXITCODE -ne 0) { throw 'Bootstrap could not reconnect.' }
$status = (& $executable status | ConvertFrom-Json)
if ($LASTEXITCODE -ne 0 -or -not $status.session_id) { throw 'Registration has not been verified.' }
if ($status.last_outcome -ne 'connected') { throw 'Fresh authenticated connection has not been verified.' }
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$evidence = @{ phase = $Phase; observed_at = [DateTimeOffset]::UtcNow.ToString('o'); status = $status; boot = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime.ToUniversalTime().ToString('o'); task_user = $task.Principal.UserId; signature_status = [string]$signature.Status; publisher = $signature.SignerCertificate.Subject }
if ($Phase -eq 'AfterReboot') {
    $previous = Get-Content -LiteralPath (Join-Path $OutputPath 'BeforeReboot.json') -Raw | ConvertFrom-Json
    if ($previous.status.session_id -ne $status.session_id -or $previous.status.installation_id -ne $status.installation_id) { throw 'Session or installation identity was lost across reboot.' }
    if ($previous.boot -eq $evidence.boot) { throw 'A real reboot has not occurred.' }
    if ($previous.status.boot_id -eq $status.boot_id) { throw 'The bootstrapper did not establish a new boot identity.' }
    if ($status.state -notin @('approved', 'handoff_ready')) { throw 'Technician approval was not preserved.' }
}
$evidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $OutputPath ($Phase + '.json')) -Encoding UTF8
if ($Reboot) {
    if ($Phase -ne 'BeforeReboot') { throw 'Reboot is only available after the initial evidence checkpoint.' }
    Restart-Computer -Force
}
