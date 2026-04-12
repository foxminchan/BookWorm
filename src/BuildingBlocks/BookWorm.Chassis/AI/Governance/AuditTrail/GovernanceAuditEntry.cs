namespace BookWorm.Chassis.AI.Governance.AuditTrail;

public sealed record GovernanceAuditEntry(
    int Index,
    string Timestamp,
    string AgentId,
    string EventType,
    string Action,
    string Detail,
    string Hash,
    string PreviousHash
);
