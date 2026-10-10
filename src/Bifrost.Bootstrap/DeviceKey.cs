// Copyright (c) 2026 Midtown Technology Group
// SPDX-License-Identifier: AGPL-3.0-only

using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace Bifrost.Bootstrap;

[SupportedOSPlatform("windows")]
public static class DeviceKey
{
    public static ECDsa Open(Checkpoint checkpoint, bool allowSoftware)
    {
        var name = "BiFrostDeviceLifecycle-" + checkpoint.InstallationId;
        var providers = checkpoint.KeyProvider.Length > 0 ? new[] { checkpoint.KeyProvider } :
            allowSoftware ? new[] { CngProvider.MicrosoftPlatformCryptoProvider.Provider, CngProvider.MicrosoftSoftwareKeyStorageProvider.Provider } :
            new[] { CngProvider.MicrosoftPlatformCryptoProvider.Provider };
        foreach (var providerName in providers)
        {
            var provider = new CngProvider(providerName);
            try
            {
                if (checkpoint.RegistrationJson.Length > 0 && !CngKey.Exists(name, provider, CngKeyOpenOptions.MachineKey))
                    throw new CryptographicException("registered_device_key_missing");
                var key = CngKey.Exists(name, provider, CngKeyOpenOptions.MachineKey)
                    ? CngKey.Open(name, provider, CngKeyOpenOptions.MachineKey)
                    : CngKey.Create(CngAlgorithm.ECDsaP256, name, new CngKeyCreationParameters
                    {
                        Provider = provider,
                        KeyCreationOptions = CngKeyCreationOptions.MachineKey,
                        ExportPolicy = CngExportPolicies.None,
                        KeyUsage = CngKeyUsages.Signing
                    });
                checkpoint.KeyProvider = providerName;
                return new ECDsaCng(key);
            }
            catch (CryptographicException) when (checkpoint.KeyProvider.Length == 0 && providerName == CngProvider.MicrosoftPlatformCryptoProvider.Provider && allowSoftware) { }
        }
        throw new CryptographicException("device_key_unavailable");
    }
}
