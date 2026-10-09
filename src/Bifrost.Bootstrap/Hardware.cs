using System.Runtime.InteropServices;
using Bifrost.DeviceLifecycle;

namespace Bifrost.Bootstrap;

public static partial class Hardware
{
    [LibraryImport("kernel32.dll", EntryPoint = "GetSystemFirmwareTable")]
    private static unsafe partial uint GetFirmware(uint provider, uint table, byte* buffer, uint size);

    public static Registration Observe(Checkpoint checkpoint, string publicKey)
    {
        var value = new Registration
        {
            ContractVersion = Client.Version, InstallationId = checkpoint.InstallationId, ClientKind = "windows_bootstrap",
            PublicKey = publicKey, Architecture = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "x64",
            BootEnvironment = "windows11", ClientVersion = "0.1.0", ObservedAt = DateTimeOffset.UtcNow.ToString("O")
        };
        if (OperatingSystem.IsWindows()) ReadSmbios(value);
        return value;
    }
    private static unsafe void ReadSmbios(Registration registration)
    {
        const uint rsmb = 0x52534D42;
        var size = GetFirmware(rsmb, 0, null, 0);
        if (size < 8 || size > 1048576) return;
        var bytes = new byte[size];
        fixed (byte* pointer = bytes) if (GetFirmware(rsmb, 0, pointer, size) != size) return;
        for (int offset = 8; offset + 4 < bytes.Length;)
        {
            int length = bytes[offset + 1], end = offset + length;
            if (length < 4 || end >= bytes.Length) break;
            var strings = new List<string>(); int current = end;
            while (current < bytes.Length && bytes[current] != 0)
            {
                int stop = Array.IndexOf(bytes, (byte)0, current); if (stop < 0) return;
                strings.Add(System.Text.Encoding.UTF8.GetString(bytes, current, stop - current)); current = stop + 1;
            }
            string Field(int index) => index > 0 && index <= strings.Count ? strings[index - 1] : "";
            if (bytes[offset] == 1 && length >= 24)
            {
                registration.Manufacturer = Field(bytes[offset + 4]); registration.Model = Field(bytes[offset + 5]);
                registration.SerialNumber = Field(bytes[offset + 7]); registration.SmbiosUuid = new Guid(bytes.AsSpan(offset + 8, 16)).ToString();
                return;
            }
            offset = strings.Count == 0 ? current + 2 : current + 1;
        }
    }
}
