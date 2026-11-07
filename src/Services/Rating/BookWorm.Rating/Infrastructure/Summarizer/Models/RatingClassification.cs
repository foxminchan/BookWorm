using System.Text.Json.Serialization;

namespace BookWorm.Rating.Infrastructure.Summarizer.Models;

[JsonConverter(typeof(JsonStringEnumConverter<RatingClassification>))]
public enum RatingClassification : byte
{
    [Description("No Data")]
    NoData = 0,

    [Description("Bad")]
    Bad = 1,

    [Description("Good")]
    Good = 2,

    [Description("Top Rated")]
    TopRated = 3,
}

public static class RatingClassifier
{
    public static RatingClassification Classify(
        double averageRating,
        int reviewCount,
        bool dominantNegativePattern
    )
    {
        if (reviewCount < 3 || !double.IsFinite(averageRating) || averageRating is < 0 or > 5)
        {
            return RatingClassification.NoData;
        }

        return (averageRating, reviewCount, dominantNegativePattern) switch
        {
            (< 3.5, _, _) or (_, _, true) => RatingClassification.Bad,
            (>= 4.5, >= 50, _) or (>= 4.0, >= 200, _) => RatingClassification.TopRated,
            _ => RatingClassification.Good,
        };
    }
}
