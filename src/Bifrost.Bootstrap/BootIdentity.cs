using Microsoft.Win32;
using System.Runtime.Versioning;

namespace Bifrost.Bootstrap;

[SupportedOSPlatform("windows")]
public static class BootIdentity
{
    // A volatile HKLM key survives process termination but is discarded on full
    // restart. Fast Startup resumes the same kernel and therefore the same boot.
    // https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regcreatekeyexw
    public static string Read()
    {
        using var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var key = machine.CreateSubKey(@"SOFTWARE\MTG\BiFrostBootstrap\CurrentBoot", true, RegistryOptions.Volatile)
            ?? throw new InvalidOperationException("boot_identity_unavailable");
        if (key.GetValue("Identity") is string existing && Guid.TryParseExact(existing, "D", out _)) return existing;
        var identity = Guid.NewGuid().ToString();
        key.SetValue("Identity", identity, RegistryValueKind.String);
        return identity;
    }
}
