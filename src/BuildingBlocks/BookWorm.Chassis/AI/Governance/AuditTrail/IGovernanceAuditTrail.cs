namespace BookWorm.Chassis.AI.Governance.AuditTrail;

public interface IGovernanceAuditTrail
{
    /// <summary>
    ///     Appends a new governance event to the Merkle-chained audit trail.
    /// </summary>
    /// <param name="agentId">The agent identifier (DID or logical name).</param>
    /// <param name="eventType">The governance event category.</param>
    /// <param name="action">The governance decision ("allow", "deny", "anomaly_check").</param>
    /// <param name="detail">Human-readable detail about the event.</param>
    /// <returns>The newly created <see cref="GovernanceAuditEntry" />.</returns>
    GovernanceAuditEntry Log(string agentId, string eventType, string action, string detail);

    /// <summary>
    ///     Verifies the integrity of the entire Merkle chain.
    ///     Re-computes each hash from the payload and checks it matches the stored hash.
    /// </summary>
    /// <returns>
    ///     A tuple where <c>IsValid</c> indicates whether the chain is intact, and
    ///     <c>VerifiedCount</c> is the number of entries successfully verified (or the
    ///     index of the first corrupted entry).
    /// </returns>
    (bool IsValid, int VerifiedCount) VerifyIntegrity();

    /// <summary>
    ///     Generates a cryptographic proof for a specific audit entry,
    ///     including chain position, hashes, and overall integrity status.
    /// </summary>
    /// <param name="index">The zero-based index of the entry to prove.</param>
    /// <returns>A dictionary containing proof details.</returns>
    Dictionary<string, object> GenerateProof(int index);
}
