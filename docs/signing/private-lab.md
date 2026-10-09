# Private Trust lab signing

Scope: disposable Windows lab acceptance only. No customer trust-store changes,
public release, new signing account, reusable signing key or enrollment secret.

The protected read-only [observer run 37950775438](https://github.com/MTG-Thomas/bifrost-infra/actions/runs/37950775438)
confirmed `mtg-sopdet-signing`, `eastus`, existing Basic SKU, and profile
`mtg-lab-profile`, `PrivateTrust`, `Active` at 2026-10-09 15:19 UTC. Identity
validation and certificate profiles are separate resources. Earlier empty
profile inventories are historical evidence, not the current lab blocker.

## Administrator setup

Use a dedicated release identity; do not grant signing to the read-only Prowler
observer or the production infrastructure deployer. An administrator owns the
identity, federation and role assignment; source authoring does not grant access.

Configure the repository environment `lab-artifact-signing` with access limited
to reviewed `main`, retaining required human approvals wherever configured.
If the GitHub plan cannot enforce the environment, do not enable signing:
resolve that control through the repository owner before dispatch. No protection
or billing changes are made by this implementation.

Federation configuration for the release identity:

- Issuer: `https://token.actions.githubusercontent.com`.
- Subject: `repo:Midtown-Technology-Group/bifrost-device-bootstrap:environment:lab-artifact-signing`.
- Audience: `api://AzureADTokenExchange`.
- Subscription: `a1d63b24-1202-4bfa-9086-cf32d1d352fc`.
- Assign **Artifact Signing Certificate Profile Signer** only at
  `/subscriptions/a1d63b24-1202-4bfa-9086-cf32d1d352fc/resourceGroups/rg-sopdet-signing/providers/Microsoft.CodeSigning/codeSigningAccounts/mtg-sopdet-signing/certificateProfiles/mtg-lab-profile`.
- The workflow also reads this exact profile; provide metadata read access at
  the same profile if the signer role does not include it. No account-wide
  write or permission-management rights are needed.

Set environment variables `AZURE_SIGNING_CLIENT_ID`, `AZURE_TENANT_ID`, and
`LAB_PUBLISHER_SUBJECT`. Obtain the exact full publisher subject from the approved
Azure profile. These are public identifiers, not credentials. No client secret,
password or signing private key is required. Do not put credentials in chat.

## Build, sign and verify

After reviewed merge and administrator configuration, dispatch **Build and sign
disposable lab bootstrap** on `main`. It tests and builds Windows x64 Native AOT,
records the unsigned digest, authenticates through OIDC, verifies the exact
active profile, signs only `bifrost-bootstrap.exe` with SHA-256 and RFC3161
timestamping, and independently verifies the resulting signature and chain.
The seven-day artifact contains the executable, public root and source/run-bound
verification receipt; no secrets. It is not a public release.

The reviewed root certificate is downloaded from [Microsoft PKI](https://www.microsoft.com/pkiops/certs/Microsoft%20Enterprise%20Identity%20Verification%20Root%20Certificate%20Authority%202020.crt),
with SHA-256 `d549dc2314f7a16e496a515491b273bc9c098e40a070d61ef1602870f0c402d8`.
Trust does not come from a certificate supplied by the executable being checked.
The verifier requires that root, approved exact publisher, Private Trust
code-signing EKUs, valid Authenticode and timestamp. CI Policy certificates fail.

On an explicitly disposable VM, verify the downloaded artifact digest against
the producer receipt and source/run before staging it. Import the pinned public
root with elevated Windows PowerShell:

```powershell
./scripts/Trust-LabSigningRoot.ps1 -CertificatePath ./docs/signing/microsoft-private-trust-root-2020.cer -DisposableLab
```

Record the returned trust-store receipt. Then run the existing signed installer,
generic package builder and Windows acceptance script. SYSTEM execution needs
LocalMachine trust; the ephemeral build runner imports only CurrentUser trust.
No private key is imported. No verification switch is disabled. A signed build
alone does not prove fresh OOBE, generic PPKG delivery, technician approval or
successful handoff. Those still require disposable VM evidence and the governed
Workspace publication path.

## Rollback and current verification

Keep existing tasks, journal and identity intact when replacing the unsigned lab
candidate. A failed signature or publisher check must stop before installation.
Remove lab artifacts after evidence retention and retire the disposable VM.
If retaining the VM, remove the root only if the trust-store receipt says this
task newly imported it; never remove pre-existing trust. Disabling the lab workflow
does not revoke signatures; use Azure certificate revocation only under separately
authorized incident scope. Do not delete the account or profile for rollback.

Windows PR validation has no OIDC or signing environment access. It runs the
existing contract/recovery/security tests and Native AOT build, then rejects a
tampered root and an unsigned artifact. Authenticated signing, administrator
configuration and VM signature verification remain unverified until recorded.

References: [signing integrations](https://learn.microsoft.com/en-us/azure/artifact-signing/how-to-signing-integrations),
[Private Trust](https://learn.microsoft.com/en-us/azure/artifact-signing/concept-trust-models).
