using BookWorm.McpTools.Prompts;
using Microsoft.Extensions.AI;

namespace BookWorm.McpTools.UnitTests.Prompts;

public sealed class SummarizeRatingPromptTypeTests
{
    [Test]
    public void GivenContent_WhenSummarizeRating_ThenShouldReturnUserRoleChatMessage()
    {
        // Act
        var result = SummarizeRatingPromptType.SummarizeRatingPrompt("Great book overall.");

        // Assert
        result.Role.ShouldBe(ChatRole.User);
    }

    [Test]
    public void GivenContent_WhenSummarizeRating_ThenShouldEmbedContentInMessage()
    {
        // Arrange
        const string content = "The plot was engaging and characters were well-developed.";

        // Act
        var result = SummarizeRatingPromptType.SummarizeRatingPrompt(content);

        // Assert
        result.Text.ShouldContain(content);
    }

    [Test]
    public void GivenContent_WhenSummarizeRating_ThenShouldIncludeAnalysisInstructions()
    {
        // Act
        var result = SummarizeRatingPromptType.SummarizeRatingPrompt("Some review text.");

        // Assert
        result.Text.ShouldContain("Overall sentiment");
        result.Text.ShouldContain("Key themes");
    }

    [Test]
    public void GivenEmptyContent_WhenSummarizeRating_ThenShouldStillReturnValidMessage()
    {
        // Act
        var result = SummarizeRatingPromptType.SummarizeRatingPrompt(string.Empty);

        // Assert
        result.Role.ShouldBe(ChatRole.User);
        result.Text.ShouldNotBeNullOrWhiteSpace();
    }
}
