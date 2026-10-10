# Contributing to Alveno

Use .NET SDK 10.0.401. `global.json` deliberately rejects unreviewed SDK drift.
The Windows x64 Native AOT build also needs the Visual C++ toolchain and Windows
SDK described in [Microsoft's prerequisites](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

```sh
dotnet build Alveno.slnx -p:PublishAot=false
dotnet run --project tests/Bifrost.Bootstrap.Tests -p:PublishAot=false
dotnet format whitespace Alveno.slnx --verify-no-changes --exclude src/Bifrost.Bootstrap/Protocol.cs
```

On Windows, `scripts/Build-Windows.ps1` runs the same console checks and publishes
Native AOT x64. Windows adds filesystem ACL/reparse-point and non-exportable TPM
key tests; Linux cannot establish those behaviors. Keep the current lightweight
console test runner: it exercises shared fixtures and interruption/reconciliation
behavior without adding a separately maintained test framework or runtime.

Submit focused pull requests with the problem, resulting behavior and actual
verification. CI builds on Linux and Windows, checks formatting and signatures'
rejection guards, audits history for secrets, and scans C# with CodeQL. PRs have
no signing access. Signing is a separate main-only, protected, manually dispatched
lab workflow. Never include machine credentials, private keys, customer data,
raw checkpoints or diagnostic bundles in issues or logs.

The JSON schema and generated C# DTOs are owned upstream. See
`contract/provenance.json`; update schema, fixtures and DTOs from that same pinned
source together. Do not hand-edit or locally reformat `Protocol.cs`. Product
identity remains separate from hardware observations, operating-system installation
and enrollment sessions.

Changes must preserve phase-1 limits: typed enrollment and local diagnostics only.
No arbitrary commands/scripts, destructive recovery, imaging, customer deployment,
NinjaOne installation or AD join. Repository branding must not reset ProgramData
state, task identity or the installed `bifrost-bootstrap.exe` filename.

Source contributions are licensed AGPL-3.0-only. The Microsoft public root
certificate is an unmodified third-party trust artifact, not software authored by
this project; its origin and pinned digest are documented in the signing runbook.
