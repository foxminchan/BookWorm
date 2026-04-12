namespace BookWorm.Chassis.AI.Governance.Detectors;

public sealed record AnomalyScore(
    double ZScore,
    double Entropy,
    double CapabilityDeviation,
    bool IsAnomalous,
    bool Quarantine
);
