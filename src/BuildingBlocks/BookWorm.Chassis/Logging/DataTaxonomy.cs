using Microsoft.Extensions.Compliance.Classification;

namespace BookWorm.Chassis.Logging;

public static class DataTaxonomy
{
    private static string TaxonomyName { get; } = typeof(DataTaxonomy).FullName!;

    public static DataClassification SensitiveData { get; } =
        new(TaxonomyName, nameof(SensitiveData));
    public static DataClassification PiiData { get; } = new(TaxonomyName, nameof(PiiData));
}

[AttributeUsage(AttributeTargets.All)]
public sealed class SensitiveDataAttribute()
    : DataClassificationAttribute(DataTaxonomy.SensitiveData);

[AttributeUsage(AttributeTargets.All)]
public sealed class PiiDataAttribute() : DataClassificationAttribute(DataTaxonomy.PiiData);
