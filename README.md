# Alveno

[![Windows validation](https://github.com/Midtown-Technology-Group/alveno/actions/workflows/windows-validation.yml/badge.svg)](https://github.com/Midtown-Technology-Group/alveno/actions/workflows/windows-validation.yml)
[![Linux validation](https://github.com/Midtown-Technology-Group/alveno/actions/workflows/linux-validation.yml/badge.svg)](https://github.com/Midtown-Technology-Group/alveno/actions/workflows/linux-validation.yml)

A small Windows 11 enrollment client for the BiFrost device lifecycle, written in
C# and published as a self-contained Native AOT executable. It registers an
unassigned device, proves possession of a machine-local key, reports typed
checkpoints, and observes technician claim/approval through the coordination service.

**Status: experimental phase-1 foundation.** Native AOT, TPM, restricted ACLs,
SYSTEM task execution, live registration and reboot/offline replay have disposable
lab evidence. Signed generic provisioning-package/OOBE delivery, Workspace claim
and end-to-end approval remain unverified. There is no approved production release.

## Build and validate

Install .NET SDK **10.0.402**, pinned in `global.json`:

```sh
dotnet build Alveno.slnx -p:PublishAot=false
dotnet run --project tests/Bifrost.Bootstrap.Tests -p:PublishAot=false
dotnet format whitespace Alveno.slnx --verify-no-changes --exclude src/Bifrost.Bootstrap/Protocol.cs
```

Windows Native AOT additionally requires Visual C++ Build Tools and the Windows
SDK. `scripts/Build-Windows.ps1` runs the checks and produces an **unsigned**
`dist/win-x64/bifrost-bootstrap.exe`. Windows adds TPM and filesystem security
checks that Linux cannot verify. ARM64 is deferred until separately validated.

CI checks Linux compilation, shared fixtures/reconciliation, formatting, Windows
Native AOT and security guards, and secret history. GitHub’s existing extended
CodeQL setup scans C# and workflow Actions; it remains the single CodeQL producer. All compiler/analyzer
warnings are errors. The application has no external NuGet package dependencies
and no separately installed .NET runtime requirement.

## Operate

```text
bifrost-bootstrap.exe install --endpoint https://your-enrollment-origin.example/
bifrost-bootstrap.exe run
bifrost-bootstrap.exe status
bifrost-bootstrap.exe uninstall
```

Use `scripts/Install-Bootstrap.ps1` with an independently verified, timestamped
release, approved exact publisher and SHA-256. It is the same installer used by
`scripts/Build-ProvisioningPackage.ps1`, which requires Windows Configuration
Designer. Generic media contains only public endpoint/trust metadata and signed
artifacts; it never contains customer credentials or reusable enrollment tokens.

The startup Scheduled Task runs as SYSTEM, repeats every minute, does bounded work
and exits. State under ProgramData is restricted to SYSTEM/Administrators, uses
atomic checkpoints and rejects reparse points. TPM keys are machine-local and
non-exportable; software fallback requires explicit lab opt-in. Read-only status
remains available during execution. Uninstall removes the task while retaining
state for investigation; phase 1 never automatically removes onboarding evidence.

The repository is named Alveno. Existing `bifrost-bootstrap.exe`, task, namespace
and ProgramData names remain stable so branding cannot reset installed identities.

## Boundaries and evidence

Alveno supports enrollment and typed diagnostics. Imaging, repair, destructive
recovery, arbitrary shell/scripts, NinjaOne installation, domain joining and
customer-specific provisioning belong to separate authorized phases/runtimes.
Hardware identifiers and pairing codes are correlation evidence, not credentials.

- [Actual Windows lab evidence](docs/acceptance/windows-native-aot-lab-2026-10-09/README.md).
- [Private Trust lab signing and rollback](docs/signing/private-lab.md).
- [Contribution/build guidance](CONTRIBUTING.md) and [security reporting](SECURITY.md).
- [Development changelog](CHANGELOG.md).

The versioned protocol's schema and generated models are owned by BiFrost Workspace.
Bundled schema, fixtures and DTO provenance are pinned in `contract/provenance.json`;
`scripts/Verify-Contract.ps1` checks their digests without requiring private upstream
access. Do not maintain a second protocol or device/customer identity registry here.

Source is **AGPL-3.0-only**; see [LICENSE](LICENSE) and [NOTICE](NOTICE).
