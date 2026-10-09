# BiFrost device bootstrap

Windows 11 enrollment client for the BiFrost device lifecycle. Phase 1 registers
an unassigned session, reports typed diagnostic checkpoints, and observes
technician approval. It performs no provisioning or remote commands.

Contract and architecture live in
[BiFrost Workspace](https://github.com/MTG-Thomas/bifrost-workspace/issues/1236).
Track implementation in [issue 1](https://github.com/Midtown-Technology-Group/bifrost-device-bootstrap/issues/1).

Windows x64 release artifacts require Native AOT, an approved Authenticode
publisher, timestamp verification and recorded build provenance. A Linux test
build cannot substitute for Windows acceptance. Keep signing credentials out of
source, media, state files and logs.

```sh
dotnet build src/Bifrost.Bootstrap -p:PublishAot=false
dotnet run --project tests/Bifrost.Bootstrap.Tests -p:PublishAot=false
```

On a Windows host with the Visual C++/Windows SDK Native AOT prerequisites:

```powershell
dotnet publish src/Bifrost.Bootstrap -r win-x64 -c Release -o dist\win-x64
```

Use `scripts/Install-Bootstrap.ps1` for both manual lab installation and package
installation. It verifies publisher and binary hash before invoking `install`.
Public endpoint and publisher metadata are the only package configuration.
Software CNG key fallback requires explicit lab opt-in. Private keys remain
non-exportable machine CNG keys. The status command reports the fallback.

The startup task also repeats every minute, performs bounded work, and exits.
Checkpoint/event writes are atomic; pending events stay durable until matched
against server acknowledgment. No automatic uninstall occurs in Phase 1.
Explicit rollback unregisters the task while retaining state for investigation.

`scripts/Build-Windows.ps1` runs the shared fixtures and publishes win-x64 Native
AOT with the Windows Visual C++ toolchain. Its output is an unsigned build,
requiring protected signing before installer or OOBE acceptance. The private
repository has no approved Windows CI runner; do not add a GitHub-hosted fallback
around MTG's private CI policy. A disposable Windows build VM is the lab path.

`scripts/Build-ProvisioningPackage.ps1` uses Windows Configuration Designer and
the same verified installer as manual installation. The package contains only
the signed executable, finite local installer and public trust metadata. It
does not create users, bypass OOBE, embed passwords or contain an enrollment key.
Run `Test-WindowsAcceptance.ps1` in a disposable VM before and after a real
reboot, after technician claim/approval. Capture its structured JSON plus the
Workspace timeline; never include key material. This script is an acceptance
procedure, not recorded evidence that the VM test has run.

The existing signing observer found no certificate profiles in the MTG signing
account on October 8, 2026. Public Trust profile readiness, a test signature and
signed VM acceptance remain unverified. Phase 2 may add artifact grants and
typed provisioning only after this foundation's live evidence is complete.
