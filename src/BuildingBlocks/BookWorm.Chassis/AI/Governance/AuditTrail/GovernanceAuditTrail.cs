using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace BookWorm.Chassis.AI.Governance.AuditTrail;

internal sealed class GovernanceAuditTrail : IGovernanceAuditTrail
{
    private readonly Lock _appendLock = new();
    private readonly ConcurrentBag<GovernanceAuditEntry> _entries = [];
    private string _lastHash = new('0', 64);
    private int _nextIndex;

    private IReadOnlyList<GovernanceAuditEntry> Entries => [.. _entries.OrderBy(e => e.Index)];

    public GovernanceAuditEntry Log(string agentId, string eventType, string action, string detail)
    {
        lock (_appendLock)
        {
            var ts = DateTime.UtcNow.ToString("O");
            var payload = $"{_nextIndex}|{ts}|{agentId}|{eventType}|{action}|{detail}|{_lastHash}";
            var hash = ComputeSha256(payload);

            var entry = new GovernanceAuditEntry(
                _nextIndex,
                ts,
                agentId,
                eventType,
                action,
                detail,
                hash,
                _lastHash
            );

            _entries.Add(entry);
            _lastHash = hash;
            _nextIndex++;
            return entry;
        }
    }

    public (bool IsValid, int VerifiedCount) VerifyIntegrity()
    {
        var ordered = Entries;
        var prevHash = new string('0', 64);

        foreach (var entry in ordered)
        {
            var payload =
                $"{entry.Index}|{entry.Timestamp}|{entry.AgentId}|{entry.EventType}|{entry.Action}|{entry.Detail}|{prevHash}";
            var expected = ComputeSha256(payload);

            if (expected != entry.Hash)
            {
                return (false, entry.Index);
            }

            prevHash = entry.Hash;
        }

        return (true, ordered.Count);
    }

    public Dictionary<string, object> GenerateProof(int index)
    {
        var ordered = Entries;

        if (index < 0 || index >= ordered.Count)
        {
            return new() { ["error"] = "Index out of range" };
        }

        var entry = ordered[index];

        return new()
        {
            ["entry_index"] = index,
            ["entry_hash"] = entry.Hash,
            ["previous_hash"] = entry.PreviousHash,
            ["chain_length"] = ordered.Count,
            ["chain_head"] = ordered[^1].Hash,
            ["verified"] = VerifyIntegrity().IsValid,
        };
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
