# Security policy

Alveno is currently an experimental Windows enrollment client, not a generally
available production release. No signed public release is approved yet.

Report vulnerabilities through GitHub's private **Report a vulnerability** action
or email `dev@midtowntg.com`. Do not file a public issue containing an exploit,
credentials, private keys, customer data, hardware inventory or a raw execution
journal. Include affected source/version, impact and a minimal sanitized reproducer.

Security fixes target the current `main` branch until a supported release policy
is announced. There is no promised response SLA or support for historical lab
artifacts. Private Trust lab signing is not public publisher trust; signing
certificates and release authorization must be independently verified.

Public session IDs, pairing codes and hardware serials are correlation evidence,
not credentials. Devices cannot approve themselves. Credentials remain machine
local and non-exportable where supported. Preserve signature, publisher, hash,
timestamp, replay and checkpoint checks when reproducing a fault.
