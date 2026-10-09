using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Bifrost.Bootstrap;

[SupportedOSPlatform("windows")]
public static class StateDirectory
{
    public static void Protect(string path)
    {
        // Validate ancestors before creating or following a privileged state path.
        for (var parent = new DirectoryInfo(path); parent != null; parent = parent.Parent)
            if (parent.Exists && (parent.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new IOException("state_reparse_point");
        Directory.CreateDirectory(path);
        ProtectDirectory(new DirectoryInfo(path));
        var pending = new Stack<DirectoryInfo>(); pending.Push(new DirectoryInfo(path));
        var count = 0;
        while (pending.TryPop(out var directory))
            foreach (var item in directory.EnumerateFileSystemInfos())
            {
                if (++count > 4096) throw new IOException("state_directory_limit");
                if ((item.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("state_reparse_point");
                if (item is DirectoryInfo child) { ProtectDirectory(child); pending.Push(child); }
                else if (item is FileInfo file)
                {
                    var security = new FileSecurity();
                    security.SetAccessRuleProtection(true, false);
                    foreach (var sid in TrustedSids())
                        security.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.FullControl, AccessControlType.Allow));
                    file.SetAccessControl(security);
                }
            }
    }

    private static void ProtectDirectory(DirectoryInfo directory)
    {
        var security = new DirectorySecurity();
        security.SetAccessRuleProtection(true, false);
        foreach (var sid in TrustedSids())
            security.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
        directory.SetAccessControl(security);
    }

    private static IEnumerable<SecurityIdentifier> TrustedSids()
    {
        yield return new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        yield return new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
    }
}
