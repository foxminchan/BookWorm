using BookWorm.McpTools.Prompts;
using Microsoft.Extensions.AI;

namespace BookWorm.McpTools.UnitTests.Prompts;

public sealed class BookRecommendationPromptTypeTests
{
    [Test]
    public void GivenNoParameters_WhenRecommendBooks_ThenShouldReturnUserRoleChatMessage()
    {
        // Act
        var result = BookRecommendationPromptType.RecommendBooksPrompt();

        // Assert
        result.Role.ShouldBe(ChatRole.User);
    }

    [Test]
    public void GivenNoParameters_WhenRecommendBooks_ThenShouldIndicateBroadRecommendation()
    {
        // Act
        var result = BookRecommendationPromptType.RecommendBooksPrompt();

        // Assert
        result.Text.ShouldContain("No specific constraints (recommend broadly)");
    }

    [Test]
    public void GivenGenre_WhenRecommendBooks_ThenShouldIncludeGenreConstraint()
    {
        // Arrange
        const string genre = "science fiction";

        // Act
        var result = BookRecommendationPromptType.RecommendBooksPrompt(genre: genre);

        // Assert
        result.Text.ShouldContain("Genre/Topic: science fiction");
    }

    [Test]
    public void GivenPriceRange_WhenRecommendBooks_ThenShouldIncludePriceRangeConstraint()
    {
        // Arrange
        const string priceRange = "10-30";

        // Act
        var result = BookRecommendationPromptType.RecommendBooksPrompt(priceRange: priceRange);

        // Assert
        result.Text.ShouldContain("Price range: $10-30");
    }

    [Test]
    public void GivenPreferredAuthors_WhenRecommendBooks_ThenShouldIncludeAuthorsConstraint()
    {
        // Arrange
        const string authors = "Brandon Sanderson, Patrick Rothfuss";

        // Act
        var result = BookRecommendationPromptType.RecommendBooksPrompt(preferredAuthors: authors);

        // Assert
        result.Text.ShouldContain("Preferred authors: Brandon Sanderson, Patrick Rothfuss");
    }

    [Test]
    public void GivenReaderProfile_WhenRecommendBooks_ThenShouldIncludeReaderProfileConstraint()
    {
        // Arrange
        const string profile = "beginner programmer";

        // Act
        var result = BookRecommendationPromptType.RecommendBooksPrompt(readerProfile: profile);

        // Assert
        result.Text.ShouldContain("Reader profile: beginner programmer");
    }

    [Test]
    public void GivenAllParameters_WhenRecommendBooks_ThenShouldIncludeAllConstraints()
    {
        // Act
        var result = BookRecommendationPromptType.RecommendBooksPrompt(
            genre: "fantasy",
            priceRange: "15-40",
            preferredAuthors: "Tolkien",
            readerProfile: "adult reader"
        );

        // Assert
        result.Text.ShouldContain("Genre/Topic: fantasy");
        result.Text.ShouldContain("Price range: $15-40");
        result.Text.ShouldContain("Preferred authors: Tolkien");
        result.Text.ShouldContain("Reader profile: adult reader");
        result.Text.ShouldNotContain("No specific constraints");
    }

    [Test]
    [Arguments(null, null, null, null)]
    [Arguments("  ", "  ", "  ", "  ")]
    [Arguments("", "", "", "")]
    public void GivenBlankParameters_WhenRecommendBooks_ThenShouldFallBackToBroadRecommendation(
        string? genre,
        string? priceRange,
        string? preferredAuthors,
        string? readerProfile
    )
    {
        // Act
        var result = BookRecommendationPromptType.RecommendBooksPrompt(
            genre,
            priceRange,
            preferredAuthors,
            readerProfile
        );

        // Assert
        result.Text.ShouldContain("No specific constraints (recommend broadly)");
    }
}
