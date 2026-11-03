# Bootstrap repository rules

Preserve contract ownership in BiFrost Workspace. Generated DTOs come only from
the canonical schema and carry its digest; do not maintain a separate model.

Phase 1 supports typed enrollment and diagnostics only. No remote command or
script interpreter, reusable media credential, customer provisioning, Ninja
installation, domain join, imaging, or destructive disk action.

Run the console contract/recovery tests and Windows build before publication.
Native AOT win-x64 builds require a Windows toolchain. Signed release/VM proof
is a separate acceptance gate; do not label an unsigned binary release-ready.
Keep authenticated signing in a reviewed protected environment. No keys in PR
jobs. Use signed Git commits and ready-for-review PRs; preserve unrelated work.

## Secret gate before publication

Before every push and PR creation/update, commit the complete candidate and run
`python3 scripts/secret_gate.py` (native Windows: `python`). Require exit 0;
missing tooling/authentication, scan errors and skipped scans block publication.
Then run the existing repository gates. Repeat after changing outgoing commits.
See [secret scanning](SECRET-SCANNING.md) for base selection, new branches,
pre-push input and prerequisites. Preserve GitGuardian App, CI and Gitleaks checks.
