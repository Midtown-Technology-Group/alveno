// Copyright (c) 2026 Midtown Technology Group
// SPDX-License-Identifier: AGPL-3.0-only

using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Bifrost.DeviceLifecycle;

namespace Bifrost.Bootstrap;

public sealed class Client(Journal journal, Checkpoint state, ECDsa key, HttpClient http)
{
    public const string Version = "bifrost.device-lifecycle/v1";
    private Session Session => JsonSerializer.Deserialize(state.SessionJson, ProtocolJson.Default.Session)
        ?? throw new InvalidDataException("invalid_session");
    private string BaseUrl => state.Endpoint.TrimEnd('/');
    private async Task<string> Request(string path, string method, string body, bool registration, CancellationToken token)
    {
        string nonce;
        if (registration) nonce = Guid.NewGuid().ToString();
        else
        {
            using var challengeRequest = new HttpRequestMessage(HttpMethod.Post, BaseUrl + $"/v1/sessions/{Session.SessionId}/challenge");
            using var challenge = await http.SendAsync(challengeRequest, HttpCompletionOption.ResponseHeadersRead, token);
            if (!challenge.IsSuccessStatusCode) throw new HttpRequestException("challenge_unavailable", null, challenge.StatusCode);
            using var document = JsonDocument.Parse(await ReadBounded(challenge, token));
            nonce = document.RootElement.GetProperty("challenge").GetString()!;
        }
        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
        var frame = string.Join('\n', Version, method, path, state.InstallationId, time, nonce, hash);
        var signature = key.SignData(Encoding.UTF8.GetBytes(frame), HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        using var request = new HttpRequestMessage(new HttpMethod(method), BaseUrl + path);
        if (method != "GET") request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        request.Headers.Add("X-Lifecycle-Installation", state.InstallationId); request.Headers.Add("X-Lifecycle-Timestamp", time);
        request.Headers.Add("X-Lifecycle-Nonce", nonce); request.Headers.Add("X-Lifecycle-Signature", Convert.ToBase64String(signature));
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException("session_request_rejected", null, response.StatusCode);
        return await ReadBounded(response, token);
    }
    private static async Task<string> ReadBounded(HttpResponseMessage response, CancellationToken token)
    {
        if (response.Content.Headers.ContentLength > 4 * 1048576) throw new InvalidDataException("session_response_too_large");
        await using var stream = await response.Content.ReadAsStreamAsync(token);
        using var memory = new MemoryStream(); var buffer = new byte[8192];
        int count;
        while ((count = await stream.ReadAsync(buffer, token)) != 0)
        {
            if (memory.Length + count > 4 * 1048576) throw new InvalidDataException("session_response_too_large");
            memory.Write(buffer, 0, count);
        }
        return Encoding.UTF8.GetString(memory.ToArray());
    }
    public void Enqueue(string checkpoint, string diagnostic = "")
    {
        if (state.PendingEvents.Count >= 128) throw new InvalidOperationException("local_journal_full");
        var next = state.LastAcknowledgedSequence + state.PendingEvents.Count + 1;
        var value = new ClientEvent
        {
            ContractVersion = Version,
            SessionId = Session.SessionId,
            InstallationId = state.InstallationId,
            Sequence = next,
            EventId = Guid.NewGuid().ToString(),
            EventType = diagnostic.Length == 0 ? "checkpoint" : "diagnostic",
            Timestamp = DateTimeOffset.UtcNow.ToString("O"),
            Payload = new EventPayload
            {
                BootId = state.BootId,
                Checkpoint = checkpoint,
                Result = "ok",
                Diagnostic = diagnostic.Length == 0 ? null : diagnostic
            }
        };
        value.Validate();
        if (checkpoint is "registered" or "reconnected") state.LastReportedBootId = state.BootId;
        if (checkpoint == "approval_observed") state.ApprovalReported = true;
        state.PendingEvents.Add(JsonSerializer.Serialize(value, ProtocolJson.Default.ClientEvent)); journal.Save(state);
    }
    public async Task Run(CancellationToken token)
    {
        if (state.RegistrationJson.Length == 0)
        {
            var identity = Hardware.Observe(state, Convert.ToBase64String(key.ExportSubjectPublicKeyInfo())); identity.Validate();
            state.RegistrationJson = JsonSerializer.Serialize(identity, ProtocolJson.Default.Registration); journal.Save(state);
        }
        if (state.SessionJson.Length > 0 && state.LastReportedBootId != state.BootId) Enqueue("reconnected");
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                if (state.SessionJson.Length == 0)
                {
                    state.SessionJson = await Request("/v1/sessions", "POST", state.RegistrationJson, true, token);
                    Session.Validate(); journal.Save(state);
                    Enqueue("registered");
                }
                var remote = await Request($"/v1/sessions/{Session.SessionId}", "GET", "", false, token);
                Reconcile(remote);
                if (Session.State == "approved" && !state.ApprovalReported) Enqueue("approval_observed");
                while (state.PendingEvents.Count > 0)
                {
                    var response = await Request($"/v1/sessions/{Session.SessionId}/events", "POST", state.PendingEvents[0], false, token);
                    Reconcile(response);
                }
                var heartbeat = new Heartbeat { ContractVersion = Version, InstallationId = state.InstallationId, BootId = state.BootId };
                var seen = await Request($"/v1/sessions/{Session.SessionId}/heartbeat", "POST", JsonSerializer.Serialize(heartbeat, ProtocolJson.Default.Heartbeat), false, token);
                Reconcile(seen); state.LastOutcome = "connected"; journal.Save(state); return;
            }
            catch (HttpRequestException error) when (error.StatusCode is null or HttpStatusCode.TooManyRequests || (int?)error.StatusCode >= 500)
            {
                state.LastOutcome = "network_unavailable"; journal.Save(state);
                if (attempt == 2) return;
                await Task.Delay(TimeSpan.FromMilliseconds((1 << attempt) * 1000 + Random.Shared.Next(100, 800)), token);
            }
        }
    }
    public void Reconcile(string response)
    {
        var remote = JsonSerializer.Deserialize(response, ProtocolJson.Default.Session) ?? throw new InvalidDataException("invalid_session");
        remote.Validate();
        if (remote.Registration.InstallationId != state.InstallationId || remote.Registration.PublicKey != Convert.ToBase64String(key.ExportSubjectPublicKeyInfo())) throw new InvalidDataException("session_identity_conflict");
        if (state.SessionJson.Length != 0 && remote.SessionId != Session.SessionId) throw new InvalidDataException("session_identity_conflict");
        while (state.PendingEvents.Count > 0)
        {
            var pending = JsonSerializer.Deserialize(state.PendingEvents[0], ProtocolJson.Default.ClientEvent)!;
            if (pending.Sequence > remote.LastSequence) break;
            var acknowledged = remote.Events.SingleOrDefault(e => e.Sequence == pending.Sequence);
            if (acknowledged is null || JsonSerializer.Serialize(acknowledged, ProtocolJson.Default.ClientEvent) != state.PendingEvents[0]) throw new InvalidDataException("event_reconciliation_conflict");
            state.LastAcknowledgedSequence = pending.Sequence; state.PendingEvents.RemoveAt(0);
        }
        if (remote.LastSequence != state.LastAcknowledgedSequence) throw new InvalidDataException("event_journal_conflict");
        state.SessionJson = response; journal.Save(state);
    }
}
