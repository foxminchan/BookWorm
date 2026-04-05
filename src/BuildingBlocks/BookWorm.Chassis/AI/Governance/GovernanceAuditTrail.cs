using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace BookWorm.Chassis.AI.Governance;

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

public sealed class GovernanceAuditTrail
{
    private readonly Lock _appendLock = new();
    private readonly ConcurrentBag<GovernanceAuditEntry> _entries = [];
    private string _lastHash = new('0', 64);
    private int _nextIndex;

    private IReadOnlyList<GovernanceAuditEntry> Entries => [.. _entries.OrderBy(e => e.Index)];

    public int Count => _entries.Count;

    /// <summary>
    ///     Appends a new governance event to the Merkle-chained audit trail.
    /// </summary>
    /// <param name="agentId">The agent identifier (DID or logical name).</param>
    /// <param name="eventType">The governance event category.</param>
    /// <param name="action">The governance decision ("allow", "deny", "anomaly_check").</param>
    /// <param name="detail">Human-readable detail about the event.</param>
    /// <returns>The newly created <see cref="GovernanceAuditEntry" />.</returns>
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

    /// <summary>
    ///     Verifies the integrity of the entire Merkle chain.
    ///     Re-computes each hash from the payload and checks it matches the stored hash.
    /// </summary>
    /// <returns>
    ///     A tuple where <c>IsValid</c> indicates whether the chain is intact, and
    ///     <c>VerifiedCount</c> is the number of entries successfully verified (or the
    ///     index of the first corrupted entry).
    /// </returns>
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

    /// <summary>
    ///     Generates a cryptographic proof for a specific audit entry,
    ///     including chain position, hashes, and overall integrity status.
    /// </summary>
    /// <param name="index">The zero-based index of the entry to prove.</param>
    /// <returns>A dictionary containing proof details.</returns>
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
