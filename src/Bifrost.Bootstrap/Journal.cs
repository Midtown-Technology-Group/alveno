// Copyright (c) 2026 Midtown Technology Group
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bifrost.Bootstrap;

public sealed class Checkpoint
{
    [JsonRequired]
    public long JournalVersion { get; set; } = 1;
    public string InstallationId { get; set; } = Guid.NewGuid().ToString();
    public string Endpoint { get; set; } = "";
    public string KeyProvider { get; set; } = "";
    public string RegistrationJson { get; set; } = "";
    public string SessionJson { get; set; } = "";
    public string BootId { get; set; } = Guid.NewGuid().ToString();
    public string LastReportedBootId { get; set; } = "";
    public bool ApprovalReported { get; set; }
    public long LastUptime { get; set; }
    public long LastAcknowledgedSequence { get; set; }
    public List<string> PendingEvents { get; set; } = [];
    public string LastOutcome { get; set; } = "not_started";
}

[JsonSerializable(typeof(Checkpoint))]
public partial class CheckpointJson : JsonSerializerContext { }

public sealed class Journal(string directory)
{
    public string DirectoryPath => directory;
    public Checkpoint Load()
    {
        var path = Path.Combine(directory, "checkpoint.json");
        if (!File.Exists(path)) throw new InvalidOperationException("installation_required");
        // An open status reader must not prevent the writer's atomic replacement.
        using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
        var checkpoint = JsonSerializer.Deserialize(file, CheckpointJson.Default.Checkpoint)
            ?? throw new InvalidDataException("invalid_checkpoint");
        if (checkpoint.JournalVersion != 1) throw new InvalidDataException("unsupported_checkpoint_version");
        return checkpoint;
    }
    public void Save(Checkpoint state)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "checkpoint.json");
        var temporary = path + ".pending";
        using (var file = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
        {
            JsonSerializer.Serialize(file, state, CheckpointJson.Default.Checkpoint);
            file.Flush(true);
        }
        File.Move(temporary, path, true);
    }
}
