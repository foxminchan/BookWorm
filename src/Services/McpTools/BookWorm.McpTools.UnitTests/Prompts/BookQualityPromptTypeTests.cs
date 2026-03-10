using BookWorm.McpTools.Prompts;
using Microsoft.Extensions.AI;

namespace BookWorm.McpTools.UnitTests.Prompts;

public sealed class BookQualityPromptTypeTests
{
    [Test]
    public void GivenValidMetrics_WhenAnalyzeBookQuality_ThenShouldReturnUserRoleChatMessage()
    {
        // Act
        var result = BookQualityPromptType.AnalyzeBookQualityPrompt(4.5, 100, "Great book");

        // Assert
        result.Role.ShouldBe(ChatRole.User);
    }

    [Test]
    public void GivenValidMetrics_WhenAnalyzeBookQuality_ThenShouldContainAverageRating()
    {
        // Arrange
        const double averageRating = 4.5;

        // Act
        var result = BookQualityPromptType.AnalyzeBookQualityPrompt(averageRating, 100, "Good");

        // Assert
        result.Text.ShouldContain("4.5");
    }

    [Test]
    public void GivenValidMetrics_WhenAnalyzeBookQuality_ThenShouldContainTotalReviews()
    {
        // Arrange
        const int totalReviews = 250;

        // Act
        var result = BookQualityPromptType.AnalyzeBookQualityPrompt(3.8, totalReviews, "Mixed");

        // Assert
        result.Text.ShouldContain("250");
    }

    [Test]
    public void GivenValidMetrics_WhenAnalyzeBookQuality_ThenShouldContainReviewSummary()
    {
        // Arrange
        const string reviewSummary = "Readers praised the writing style";

        // Act
        var result = BookQualityPromptType.AnalyzeBookQualityPrompt(4.0, 50, reviewSummary);

        // Assert
        result.Text.ShouldContain(reviewSummary);
    }

    [Test]
    public void GivenZeroReviews_WhenAnalyzeBookQuality_ThenShouldStillReturnValidMessage()
    {
        // Act
        var result = BookQualityPromptType.AnalyzeBookQualityPrompt(0.0, 0, string.Empty);

        // Assert
        result.Role.ShouldBe(ChatRole.User);
        result.Text.ShouldNotBeNullOrWhiteSpace();
    }
}
