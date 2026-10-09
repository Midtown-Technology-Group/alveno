# Windows Native AOT lab evidence

Partial Phase 1 evidence, recorded October 9, 2026. **Not a signed release, fresh
OOBE, provisioning-package or technician-approval acceptance result.**

[Sanitized evidence](evidence.json) binds the real Windows x64 Native AOT binary
to code commit `5a965c5708d551e90f8b71e4899c521f5bc26179`, its source archive and
SHA-256, toolchain provenance, lab Worker, hardware observation, session and
ordered events. No credentials, private keys or complete checkpoints are included.

VM127 is a disposable full current-image copy of stopped build VM104. Source
VM104 and its protected snapshots were preserved. The network was disconnected
until an inherited Ninja agent was disabled and verified stopped after reboot on
the copy. No Ninja installer or customer provisioning ran. Authorized vault-backed
console authentication was used without exporting credentials to source or media.
VM125's historical snapshot booted to an existing login, so neither specimen is
claimed as fresh OOBE. Build tools were installed only on the copy after verifying
Microsoft's SDK SHA-512 and executable publisher signatures.

Actual results:

- Windows SDK 10.0.401 and Visual Studio Build Tools 2022 produced a self-contained
  win-x64 Native AOT executable. It remains unsigned.
- All 25 Windows contract/checkpoint/security checks passed. ACL tests replace
  pre-existing grants and reject ancestor/descendant reparse paths. Machine-key
  tests used the TPM provider, checked non-exportability and reopened the key.
- The native executable registered with the real lab Worker. Its startup task
  runs as SYSTEM with highest privileges and returns zero. State ACL and TPM key
  readback passed. The session remains `awaiting_claim`.
- A real reboot while disconnected preserved installation/session identity,
  changed physical and client boot identities, and durably buffered one event.
  Reconnection acknowledged that event once. Repeated synchronization advanced
  freshness without changing the three logical events accumulated across two
  actual lab reboots (registration plus one event for each reboot).
- Structured status remains available while the execution mutex is held. The
  focused regression procedure is [Test-WindowsStatus.ps1](../../../scripts/Test-WindowsStatus.ps1).

Initial registration occurred with candidate `2514a7d` after the lab corrected
its generated task XML for diagnosis. The corrected native installers at
`eba3907` and `5a965c5` preserved that installation/key/session and authenticated
again. The evidence does not claim a new fresh installation at each code revision.

The first native installation exposed invalid Task Scheduler XML:
`LogonType:ServiceAccount`. The native installer now omits that unsupported XML
field while keeping the SYSTEM SID and highest privilege; corrected installation
was verified. A held-mutex probe also reproduced empty successful status output;
status now reads the atomic checkpoint independently, with delete sharing so it
cannot block the writer's replacement. The native regression check passed.

The existing protected signing observer37932331548 returned zero certificate
profiles. Pending identity-validation status is unknown. Approved Public Trust
signing, signed generic PPKG/OOBE, reviewed Workspace backend publication and real
technician claim/approval remain required. No production customer endpoints were
affected. Continue through [Workspace acceptance issue 1238](https://github.com/MTG-Thomas/bifrost-workspace/issues/1238).
