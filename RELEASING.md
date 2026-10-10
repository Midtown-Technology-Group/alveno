# Release process

There is currently no approved public binary release. Source availability and
green CI do not establish signed OOBE/PPKG or full phase-1 acceptance.

For a candidate, use reviewed `main`, the pinned SDK and Windows x64 Native AOT
toolchain. All required checks, signed-commit and conversation gates must pass.
Record the exact source, producer run/attempt, unsigned and signed digests, signer,
timestamp and independently verified chain. Never sign code from pull requests.

The manually dispatched lab signing workflow is restricted to the lab environment
and exact Private Trust profile; it produces a seven-day lab artifact with binary,
public certificate, verification evidence, AGPL license/notice and the exact
corresponding source archive. It cannot substitute for Public Trust on generic
customer machines. See [lab signing](docs/signing/private-lab.md).

Before a public binary release, complete Windows 11 disposable OOBE/provisioning-
package acceptance, interrupted-operation/reboot evidence, independent enrollment
and technician approval through Workspace, and the approved publisher/signing path.
Review applicable .NET Native AOT runtime and third-party redistribution notices
alongside this project's AGPL obligations. Version the client and build metadata
together, document compatibility and rollback, and archive the source/provenance
with the artifact. Never label an unsigned build or incomplete acceptance as stable.

The repository rename does not change installed file, task, key or state identity.
Future migrations of those names need explicit compatibility and recovery evidence.
ARM64, customer provisioning and production rollout require separate authorization.
