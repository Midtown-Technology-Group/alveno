[CmdletBinding()]
param(
    [Parameter(Mandatory)][switch]$DisposableVm,
    [string]$Executable = 'C:\ProgramData\MTG\BiFrostBootstrap\bifrost-bootstrap.exe'
)
$ErrorActionPreference = 'Stop'
if (-not $DisposableVm) { throw 'Only disposable test VMs are authorized.' }
$before = & $Executable status | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or -not $before.installation_id) { throw 'An installed test fixture is required.' }
$mutex = [Threading.Mutex]::new($false, 'Global\BiFrostDeviceLifecycle')
$owned = $false
try {
    try { $owned = $mutex.WaitOne(10000) } catch [Threading.AbandonedMutexException] { $owned = $true }
    if (-not $owned) { throw 'The execution lock could not be acquired within the test bound.' }
    $result = & $Executable status
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($result)) { throw 'Status failed while execution was locked.' }
    $status = $result | ConvertFrom-Json
    if ($status.installation_id -ne $before.installation_id -or $status.session_id -ne $before.session_id) { throw 'Read-only status changed identity.' }
    [ordered]@{ status_during_execution_lock = 'passed'; installation_id = $status.installation_id; session_id = $status.session_id } | ConvertTo-Json
} finally {
    if ($owned) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
