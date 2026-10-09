using System.Diagnostics;
using System.Text.Json;
using Bifrost.DeviceLifecycle;

namespace Bifrost.Bootstrap;

public static class Program
{
    public static int Main(string[] args)
    {
        if (!OperatingSystem.IsWindows()) { Console.Error.WriteLine("windows_required"); return 2; }
        using var mutex = new Mutex(false, @"Global\BiFrostDeviceLifecycle");
        try { if (!mutex.WaitOne(0)) return 0; } catch (AbandonedMutexException) { }
        try
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MTG", "BiFrostBootstrap");
            var journal = new Journal(directory);
            var command = args.FirstOrDefault() ?? "status";
            if (command == "install")
            {
                var endpoint = Argument(args, "--endpoint");
                if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != "https" || uri.AbsolutePath != "/" || uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0) throw new ArgumentException("https_endpoint_required");
                Directory.CreateDirectory(directory);
                ProcessFixed("icacls.exe", directory, "/inheritance:r", "/grant:r", "*S-1-5-18:(OI)(CI)F", "*S-1-5-32-544:(OI)(CI)F");
                var state = File.Exists(Path.Combine(directory, "checkpoint.json")) ? journal.Load() : new Checkpoint { Endpoint = endpoint };
                if (state.Endpoint != endpoint) throw new InvalidOperationException("endpoint_change_requires_review");
                journal.Save(state); // Installation ID must exist before any key/network operation.
                using var key = DeviceKey.Open(state, args.Contains("--allow-software-key")); journal.Save(state);
                var source = Environment.ProcessPath!; var target = Path.Combine(directory, "bifrost-bootstrap.exe");
                if (!StringComparer.OrdinalIgnoreCase.Equals(source, target)) File.Copy(source, target, true);
                var xml = Path.Combine(directory, "startup-task.xml");
                File.WriteAllText(xml, $"""
                <Task version="1.4" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
                  <Triggers><BootTrigger><Enabled>true</Enabled></BootTrigger><TimeTrigger><Repetition><Interval>PT1M</Interval><StopAtDurationEnd>false</StopAtDurationEnd></Repetition><StartBoundary>{DateTime.Now:yyyy-MM-ddTHH:mm:ss}</StartBoundary><Enabled>true</Enabled></TimeTrigger></Triggers>
                  <Principals><Principal id="System"><UserId>S-1-5-18</UserId><LogonType>ServiceAccount</LogonType><RunLevel>HighestAvailable</RunLevel></Principal></Principals>
                  <Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy><DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries><StopIfGoingOnBatteries>false</StopIfGoingOnBatteries><ExecutionTimeLimit>PT2M</ExecutionTimeLimit></Settings>
                  <Actions Context="System"><Exec><Command>{System.Security.SecurityElement.Escape(target)}</Command><Arguments>run</Arguments></Exec></Actions>
                </Task>
                """);
                ProcessFixed("schtasks.exe", "/Create", "/TN", @"MTG\BiFrostBootstrap", "/XML", xml, "/F");
                ProcessFixed("schtasks.exe", "/Run", "/TN", @"MTG\BiFrostBootstrap"); return 0;
            }
            var checkpoint = journal.Load();
            if (command == "status")
            {
                var session = checkpoint.SessionJson.Length > 0 ? JsonSerializer.Deserialize(checkpoint.SessionJson, ProtocolJson.Default.Session) : null;
                using var output = new Utf8JsonWriter(Console.OpenStandardOutput());
                output.WriteStartObject(); output.WriteString("installation_id", checkpoint.InstallationId);
                output.WriteString("session_id", session?.SessionId); output.WriteString("pairing_code", session?.PairingCode);
                output.WriteString("state", session?.State ?? "not_registered"); output.WriteString("last_outcome", checkpoint.LastOutcome);
                output.WriteString("last_seen_at", session?.LastSeenAt); output.WriteString("boot_id", checkpoint.BootId);
                output.WriteNumber("acknowledged_sequence", checkpoint.LastAcknowledgedSequence); output.WriteNumber("pending_events", checkpoint.PendingEvents.Count);
                output.WriteBoolean("software_key_fallback", checkpoint.KeyProvider == "Microsoft Software Key Storage Provider"); output.WriteEndObject(); return 0;
            }
            if (command == "uninstall")
            {
                var session = checkpoint.SessionJson.Length > 0 ? JsonSerializer.Deserialize(checkpoint.SessionJson, ProtocolJson.Default.Session) : null;
                if (session?.State != "completed" && !args.Contains("--rollback")) throw new InvalidOperationException("verified_handoff_required_or_explicit_rollback");
                ProcessFixed("schtasks.exe", "/Delete", "/TN", @"MTG\BiFrostBootstrap", "/F");
                checkpoint.LastOutcome = "uninstalled_state_retained"; journal.Save(checkpoint); return 0;
            }
            if (command != "run") throw new ArgumentException("unknown_command");
            checkpoint.BootId = BootIdentity.Read();
            checkpoint.LastUptime = Environment.TickCount64; journal.Save(checkpoint);
            using var signingKey = DeviceKey.Open(checkpoint, false);
            using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(10) };
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(90));
            new Client(journal, checkpoint, signingKey, http).Run(cancellation.Token).GetAwaiter().GetResult(); return 0;
        }
        catch (Exception error)
        {
            // Exception messages may contain paths, server bodies or credential metadata.
            Console.Error.WriteLine(error is OperationCanceledException ? "bounded_run_timeout" : "bootstrap_intervention_required"); return 1;
        }
        finally { mutex.ReleaseMutex(); }
    }
    private static string Argument(string[] args, string flag)
    {
        var index = Array.IndexOf(args, flag);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : "";
    }
    private static void ProcessFixed(string executable, params string[] arguments)
    {
        var start = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), executable)) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("local_setup_failed");
        var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(30000)) { process.Kill(); throw new TimeoutException("local_setup_timeout"); }
        Task.WaitAll(stdout, stderr);
        if (process.ExitCode != 0) throw new InvalidOperationException("local_setup_failed");
    }
}
