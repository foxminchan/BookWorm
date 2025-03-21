using System.Diagnostics.CodeAnalysis;

namespace BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;

[ExcludeFromCodeCoverage]
public sealed class AuthorData : List<Author>
{
    public AuthorData()
    {
        AddRange(
            [
                new("J.K. Rowling"),
                new("George R.R. Martin"),
                new("Agatha Christie"),
                new("Stephen King"),
                new("J.R.R. Tolkien"),
                new("Mark Twain"),
                new("Jane Austen"),
                new("Charles Dickens"),
                new("F. Scott Fitzgerald"),
                new("Ernest Hemingway"),
            ]
        );
    }
}
