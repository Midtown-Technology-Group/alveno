using System.Security.Cryptography;
using System.Text.Json;
using Bifrost.Bootstrap;
using Bifrost.DeviceLifecycle;

static void Require(bool value, string label) { if (!value) throw new Exception(label); }
using var fixtures = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures.json")));
int passed = 0;
foreach (var fixture in fixtures.RootElement.EnumerateArray())
{
    bool valid = true;
    try
    {
        string input = fixture.GetProperty("value").GetRawText();
        switch (fixture.GetProperty("model").GetString())
        {
            case "Registration": JsonSerializer.Deserialize(input, ProtocolJson.Default.Registration)!.Validate(); break;
            case "ClientEvent": JsonSerializer.Deserialize(input, ProtocolJson.Default.ClientEvent)!.Validate(); break;
            case "Session": JsonSerializer.Deserialize(input, ProtocolJson.Default.Session)!.Validate(); break;
            default: throw new Exception("unsupported fixture model");
        }
    }
    catch (Exception) { valid = false; }
    Require(valid == fixture.GetProperty("valid").GetBoolean(), fixture.GetProperty("name").GetString()!); passed++;
}
var directory = Path.Combine(Path.GetTempPath(), "bifrost-bootstrap-test-" + Guid.NewGuid());
try
{
    var journal = new Journal(directory); var state = new Checkpoint { Endpoint = "https://synthetic.invalid" };
    journal.Save(state); File.WriteAllText(Path.Combine(directory, "checkpoint.json.pending"), "{interrupted");
    Require(journal.Load().InstallationId == state.InstallationId, "partial write changed identity"); passed++;
    using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    var identity = Hardware.Observe(state, Convert.ToBase64String(key.ExportSubjectPublicKeyInfo()));
    var session = new Session { ContractVersion = Client.Version, SessionId = new string('a', 64), Registration = identity,
        IdentitySha256 = new string('b', 64), PairingCode = "ABCDEFGH", State = "awaiting_claim", Revision = 1,
        CreatedAt = DateTimeOffset.UtcNow.ToString("O"), ExpiresAt = DateTimeOffset.UtcNow.AddDays(1).ToString("O"), LastSeenAt = DateTimeOffset.UtcNow.ToString("O"), Events = [], OperatorEvents = [] };
    state.SessionJson = JsonSerializer.Serialize(session, ProtocolJson.Default.Session);
    using var http = new HttpClient(); var client = new Client(journal, state, key, http);
    client.Enqueue("diagnostic_collected", "hardware_inventory");
    var resumed = journal.Load(); Require(resumed.PendingEvents.Count == 1, "event not durable before network"); passed++;
    var pending = JsonSerializer.Deserialize(resumed.PendingEvents[0], ProtocolJson.Default.ClientEvent)!;
    session.LastSequence = 1; session.Events.Add(pending);
    client.Reconcile(JsonSerializer.Serialize(session, ProtocolJson.Default.Session));
    Require(journal.Load().PendingEvents.Count == 0 && journal.Load().LastAcknowledgedSequence == 1, "uncertain ack did not reconcile"); passed++;
    client.Reconcile(JsonSerializer.Serialize(session, ProtocolJson.Default.Session));
    Require(journal.Load().LastAcknowledgedSequence == 1, "duplicate ack changed sequence"); passed++;
    client.Enqueue("reconnected");
    var conflict = JsonSerializer.Deserialize(state.PendingEvents[0], ProtocolJson.Default.ClientEvent)!;
    conflict.EventId = Guid.NewGuid().ToString(); session.Events.Add(conflict); session.LastSequence = 2;
    bool refused = false;
    try { client.Reconcile(JsonSerializer.Serialize(session, ProtocolJson.Default.Session)); } catch (InvalidDataException) { refused = true; }
    Require(refused && journal.Load().PendingEvents.Count == 1, "conflict erased pending event"); passed++;
}
finally { Directory.Delete(directory, true); }
Console.WriteLine($"{passed} contract and checkpoint/reconciliation checks passed.");
