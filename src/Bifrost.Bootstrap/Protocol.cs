// Generated from schema.json SHA-256 76de2248d578cf6ccf77247e591a62f05164558a8e26d9d95ed5c200cb275d7b; do not edit.
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
namespace Bifrost.DeviceLifecycle;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class Registration {
  [JsonRequired]
  [JsonPropertyName("contract_version")] public string ContractVersion { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("installation_id")] public string InstallationId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("client_kind")] public string ClientKind { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("public_key")] public string PublicKey { get; set; } = default!;
  [JsonPropertyName("serial_number")] public string? SerialNumber { get; set; } = default!;
  [JsonPropertyName("smbios_uuid")] public string? SmbiosUuid { get; set; } = default!;
  [JsonPropertyName("manufacturer")] public string? Manufacturer { get; set; } = default!;
  [JsonPropertyName("model")] public string? Model { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("architecture")] public string Architecture { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("boot_environment")] public string BootEnvironment { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("client_version")] public string ClientVersion { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("observed_at")] public string ObservedAt { get; set; } = default!;
  public void Validate() {
    if (ContractVersion is null) throw new ArgumentException("contract_version required");
    if (!new string[] { "bifrost.device-lifecycle/v1" }.Contains(ContractVersion)) throw new ArgumentException("contract_version value");
    if (InstallationId is null) throw new ArgumentException("installation_id required");
    if (!Guid.TryParseExact(InstallationId, "D", out _)) throw new ArgumentException("installation_id uuid");
    if (ClientKind is null) throw new ArgumentException("client_kind required");
    if (!new string[] { "windows_bootstrap", "manx_recovery" }.Contains(ClientKind)) throw new ArgumentException("client_kind value");
    if (PublicKey is null) throw new ArgumentException("public_key required");
    if (PublicKey!.Length < 100) throw new ArgumentException("public_key length");
    if (PublicKey!.Length > 160) throw new ArgumentException("public_key length");
    if (!Regex.IsMatch(PublicKey!, "^[A-Za-z0-9+/]+={0,2}$")) throw new ArgumentException("public_key pattern");
    if (SerialNumber != null && SerialNumber!.Length > 128) throw new ArgumentException("serial_number length");
    if (SmbiosUuid != null && SmbiosUuid!.Length > 128) throw new ArgumentException("smbios_uuid length");
    if (Manufacturer != null && Manufacturer!.Length > 128) throw new ArgumentException("manufacturer length");
    if (Model != null && Model!.Length > 128) throw new ArgumentException("model length");
    if (Architecture is null) throw new ArgumentException("architecture required");
    if (!new string[] { "x64", "arm64", "x86", "unknown" }.Contains(Architecture)) throw new ArgumentException("architecture value");
    if (BootEnvironment is null) throw new ArgumentException("boot_environment required");
    if (!new string[] { "windows11", "linux_rescue" }.Contains(BootEnvironment)) throw new ArgumentException("boot_environment value");
    if (ClientVersion is null) throw new ArgumentException("client_version required");
    if (ClientVersion!.Length < 1) throw new ArgumentException("client_version length");
    if (ClientVersion!.Length > 40) throw new ArgumentException("client_version length");
    if (ObservedAt is null) throw new ArgumentException("observed_at required");
    if (!DateTimeOffset.TryParse(ObservedAt, out _)) throw new ArgumentException("observed_at timestamp");
  }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EventPayload {
  [JsonRequired]
  [JsonPropertyName("boot_id")] public string BootId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("checkpoint")] public string Checkpoint { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("result")] public string Result { get; set; } = default!;
  [JsonPropertyName("diagnostic")] public string? Diagnostic { get; set; } = default!;
  public void Validate() {
    if (BootId is null) throw new ArgumentException("boot_id required");
    if (!Guid.TryParseExact(BootId, "D", out _)) throw new ArgumentException("boot_id uuid");
    if (Checkpoint is null) throw new ArgumentException("checkpoint required");
    if (!new string[] { "started", "registered", "approval_observed", "diagnostic_collected", "reconnected", "handoff_verified" }.Contains(Checkpoint)) throw new ArgumentException("checkpoint value");
    if (Result is null) throw new ArgumentException("result required");
    if (!new string[] { "ok", "unavailable", "intervention_required" }.Contains(Result)) throw new ArgumentException("result value");
    if (Diagnostic != null && !new string[] { "hardware_inventory", "network_connectivity", "session_recovery" }.Contains(Diagnostic)) throw new ArgumentException("diagnostic value");
  }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ClientEvent {
  [JsonRequired]
  [JsonPropertyName("contract_version")] public string ContractVersion { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("session_id")] public string SessionId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("installation_id")] public string InstallationId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("sequence")] public long Sequence { get; set; }
  [JsonRequired]
  [JsonPropertyName("event_id")] public string EventId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("event_type")] public string EventType { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("timestamp")] public string Timestamp { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("payload")] public EventPayload Payload { get; set; } = default!;
  public void Validate() {
    if (ContractVersion is null) throw new ArgumentException("contract_version required");
    if (!new string[] { "bifrost.device-lifecycle/v1" }.Contains(ContractVersion)) throw new ArgumentException("contract_version value");
    if (SessionId is null) throw new ArgumentException("session_id required");
    if (!Regex.IsMatch(SessionId!, "^[a-f0-9]{64}$")) throw new ArgumentException("session_id pattern");
    if (InstallationId is null) throw new ArgumentException("installation_id required");
    if (!Guid.TryParseExact(InstallationId, "D", out _)) throw new ArgumentException("installation_id uuid");
    if (Sequence < 1) throw new ArgumentException("sequence range");
    if (Sequence > 4096) throw new ArgumentException("sequence range");
    if (EventId is null) throw new ArgumentException("event_id required");
    if (!Guid.TryParseExact(EventId, "D", out _)) throw new ArgumentException("event_id uuid");
    if (EventType is null) throw new ArgumentException("event_type required");
    if (!new string[] { "checkpoint", "diagnostic", "intervention_requested", "handoff_ready", "handoff_verified" }.Contains(EventType)) throw new ArgumentException("event_type value");
    if (Timestamp is null) throw new ArgumentException("timestamp required");
    if (!DateTimeOffset.TryParse(Timestamp, out _)) throw new ArgumentException("timestamp timestamp");
    Payload.Validate();
  }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class Heartbeat {
  [JsonRequired]
  [JsonPropertyName("contract_version")] public string ContractVersion { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("installation_id")] public string InstallationId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("boot_id")] public string BootId { get; set; } = default!;
  public void Validate() {
    if (ContractVersion is null) throw new ArgumentException("contract_version required");
    if (!new string[] { "bifrost.device-lifecycle/v1" }.Contains(ContractVersion)) throw new ArgumentException("contract_version value");
    if (InstallationId is null) throw new ArgumentException("installation_id required");
    if (!Guid.TryParseExact(InstallationId, "D", out _)) throw new ArgumentException("installation_id uuid");
    if (BootId is null) throw new ArgumentException("boot_id required");
    if (!Guid.TryParseExact(BootId, "D", out _)) throw new ArgumentException("boot_id uuid");
  }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class Assignment {
  [JsonRequired]
  [JsonPropertyName("organization_id")] public string OrganizationId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("organization_name")] public string OrganizationName { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("halo_ticket_id")] public long HaloTicketId { get; set; }
  [JsonRequired]
  [JsonPropertyName("device_id")] public string DeviceId { get; set; } = default!;
  public void Validate() {
    if (OrganizationId is null) throw new ArgumentException("organization_id required");
    if (!Guid.TryParseExact(OrganizationId, "D", out _)) throw new ArgumentException("organization_id uuid");
    if (OrganizationName is null) throw new ArgumentException("organization_name required");
    if (OrganizationName!.Length < 1) throw new ArgumentException("organization_name length");
    if (OrganizationName!.Length > 128) throw new ArgumentException("organization_name length");
    if (HaloTicketId < 1) throw new ArgumentException("halo_ticket_id range");
    if (DeviceId is null) throw new ArgumentException("device_id required");
    if (DeviceId!.Length < 1) throw new ArgumentException("device_id length");
    if (DeviceId!.Length > 200) throw new ArgumentException("device_id length");
  }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class OperatorMutation {
  [JsonRequired]
  [JsonPropertyName("contract_version")] public string ContractVersion { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("request_id")] public string RequestId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("actor_id")] public string ActorId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("session_id")] public string SessionId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("expected_revision")] public long ExpectedRevision { get; set; }
  [JsonRequired]
  [JsonPropertyName("identity_sha256")] public string IdentitySha256 { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("pairing_code")] public string PairingCode { get; set; } = default!;
  [JsonPropertyName("assignment")] public Assignment? Assignment { get; set; } = default!;
  [JsonPropertyName("reason")] public string? Reason { get; set; } = default!;
  public void Validate() {
    if (ContractVersion is null) throw new ArgumentException("contract_version required");
    if (!new string[] { "bifrost.device-lifecycle/v1" }.Contains(ContractVersion)) throw new ArgumentException("contract_version value");
    if (RequestId is null) throw new ArgumentException("request_id required");
    if (!Guid.TryParseExact(RequestId, "D", out _)) throw new ArgumentException("request_id uuid");
    if (ActorId is null) throw new ArgumentException("actor_id required");
    if (!Guid.TryParseExact(ActorId, "D", out _)) throw new ArgumentException("actor_id uuid");
    if (SessionId is null) throw new ArgumentException("session_id required");
    if (!Regex.IsMatch(SessionId!, "^[a-f0-9]{64}$")) throw new ArgumentException("session_id pattern");
    if (ExpectedRevision < 1) throw new ArgumentException("expected_revision range");
    if (IdentitySha256 is null) throw new ArgumentException("identity_sha256 required");
    if (!Regex.IsMatch(IdentitySha256!, "^[a-f0-9]{64}$")) throw new ArgumentException("identity_sha256 pattern");
    if (PairingCode is null) throw new ArgumentException("pairing_code required");
    if (!Regex.IsMatch(PairingCode!, "^[A-Z2-9]{8}$")) throw new ArgumentException("pairing_code pattern");
    Assignment?.Validate();
    if (Reason != null && !new string[] { "technician_approved", "technician_rejected", "identity_collision", "technician_cancelled" }.Contains(Reason)) throw new ArgumentException("reason value");
  }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class Session {
  [JsonRequired]
  [JsonPropertyName("contract_version")] public string ContractVersion { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("session_id")] public string SessionId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("registration")] public Registration Registration { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("identity_sha256")] public string IdentitySha256 { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("pairing_code")] public string PairingCode { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("state")] public string State { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("revision")] public long Revision { get; set; }
  [JsonRequired]
  [JsonPropertyName("last_sequence")] public long LastSequence { get; set; }
  [JsonRequired]
  [JsonPropertyName("created_at")] public string CreatedAt { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("expires_at")] public string ExpiresAt { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("last_seen_at")] public string LastSeenAt { get; set; } = default!;
  [JsonRequired]
  [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
  [JsonPropertyName("assignment")] public Assignment? Assignment { get; set; } = default!;
  [JsonRequired]
  [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
  [JsonPropertyName("claimed_by")] public string? ClaimedBy { get; set; } = default!;
  [JsonRequired]
  [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
  [JsonPropertyName("approved_by")] public string? ApprovedBy { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("events")] public List<ClientEvent> Events { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("operator_events")] public List<OperatorEvent> OperatorEvents { get; set; } = default!;
  [JsonPropertyName("resume_state")] public string? ResumeState { get; set; } = default!;
  public void Validate() {
    if (ContractVersion is null) throw new ArgumentException("contract_version required");
    if (!new string[] { "bifrost.device-lifecycle/v1" }.Contains(ContractVersion)) throw new ArgumentException("contract_version value");
    if (SessionId is null) throw new ArgumentException("session_id required");
    if (!Regex.IsMatch(SessionId!, "^[a-f0-9]{64}$")) throw new ArgumentException("session_id pattern");
    Registration.Validate();
    if (IdentitySha256 is null) throw new ArgumentException("identity_sha256 required");
    if (!Regex.IsMatch(IdentitySha256!, "^[a-f0-9]{64}$")) throw new ArgumentException("identity_sha256 pattern");
    if (PairingCode is null) throw new ArgumentException("pairing_code required");
    if (!Regex.IsMatch(PairingCode!, "^[A-Z2-9]{8}$")) throw new ArgumentException("pairing_code pattern");
    if (State is null) throw new ArgumentException("state required");
    if (!new string[] { "registered", "awaiting_claim", "claimed", "approved", "handoff_ready", "completed", "intervention_required", "rejected", "expired", "cancelled", "failed" }.Contains(State)) throw new ArgumentException("state value");
    if (Revision < 1) throw new ArgumentException("revision range");
    if (LastSequence < 0) throw new ArgumentException("last_sequence range");
    if (CreatedAt is null) throw new ArgumentException("created_at required");
    if (!DateTimeOffset.TryParse(CreatedAt, out _)) throw new ArgumentException("created_at timestamp");
    if (ExpiresAt is null) throw new ArgumentException("expires_at required");
    if (!DateTimeOffset.TryParse(ExpiresAt, out _)) throw new ArgumentException("expires_at timestamp");
    if (LastSeenAt is null) throw new ArgumentException("last_seen_at required");
    if (!DateTimeOffset.TryParse(LastSeenAt, out _)) throw new ArgumentException("last_seen_at timestamp");
    Assignment?.Validate();
    if (ClaimedBy != null && !Guid.TryParseExact(ClaimedBy, "D", out _)) throw new ArgumentException("claimed_by uuid");
    if (ApprovedBy != null && !Guid.TryParseExact(ApprovedBy, "D", out _)) throw new ArgumentException("approved_by uuid");
    if (Events is null) throw new ArgumentException("events required");
    if (Events!.Count > 4096) throw new ArgumentException("events limit");
    foreach (var item in Events!) item.Validate();
    if (OperatorEvents is null) throw new ArgumentException("operator_events required");
    if (OperatorEvents!.Count > 256) throw new ArgumentException("operator_events limit");
    foreach (var item in OperatorEvents!) item.Validate();
    if (ResumeState != null && !new string[] { "registered", "awaiting_claim", "claimed", "approved", "handoff_ready", "completed", "intervention_required", "rejected", "expired", "cancelled", "failed" }.Contains(ResumeState)) throw new ArgumentException("resume_state value");
  }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class OperatorEvent {
  [JsonRequired]
  [JsonPropertyName("event_id")] public string EventId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("revision")] public long Revision { get; set; }
  [JsonRequired]
  [JsonPropertyName("actor_id")] public string ActorId { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("operation")] public string Operation { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("timestamp")] public string Timestamp { get; set; } = default!;
  [JsonRequired]
  [JsonPropertyName("state")] public string State { get; set; } = default!;
  public void Validate() {
    if (EventId is null) throw new ArgumentException("event_id required");
    if (!Guid.TryParseExact(EventId, "D", out _)) throw new ArgumentException("event_id uuid");
    if (Revision < 1) throw new ArgumentException("revision range");
    if (ActorId is null) throw new ArgumentException("actor_id required");
    if (!Guid.TryParseExact(ActorId, "D", out _)) throw new ArgumentException("actor_id uuid");
    if (Operation is null) throw new ArgumentException("operation required");
    if (!new string[] { "claim", "approve", "reject", "cancel", "resume" }.Contains(Operation)) throw new ArgumentException("operation value");
    if (Timestamp is null) throw new ArgumentException("timestamp required");
    if (!DateTimeOffset.TryParse(Timestamp, out _)) throw new ArgumentException("timestamp timestamp");
    if (State is null) throw new ArgumentException("state required");
    if (!new string[] { "registered", "awaiting_claim", "claimed", "approved", "handoff_ready", "completed", "intervention_required", "rejected", "expired", "cancelled", "failed" }.Contains(State)) throw new ArgumentException("state value");
  }
}

[JsonSerializable(typeof(Registration))]
[JsonSerializable(typeof(EventPayload))]
[JsonSerializable(typeof(ClientEvent))]
[JsonSerializable(typeof(Heartbeat))]
[JsonSerializable(typeof(Assignment))]
[JsonSerializable(typeof(OperatorMutation))]
[JsonSerializable(typeof(Session))]
[JsonSerializable(typeof(OperatorEvent))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ProtocolJson : JsonSerializerContext {}
