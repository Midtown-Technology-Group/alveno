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
dotnet run --project tests/Bifrost.Bootstrap.Tests
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
