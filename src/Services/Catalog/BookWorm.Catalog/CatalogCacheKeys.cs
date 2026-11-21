using System.Security.Cryptography;
using System.Text;

namespace BookWorm.Catalog;

internal static class CatalogCacheKeys
{
    public const string BookTag = "book";
    public const string BookListTag = "books:list";
    public const string BookSearchTag = "books:search";

    public const string Categories = "catalog:categories:v1";
    public const string Authors = "catalog:authors:v1";
    public const string Publishers = "catalog:publishers:v1";
    public const string Dashboard = "catalog:dashboard:v1";

    public static string BookDetail(BookId id)
    {
        return $"{BookTag}:{id}";
    }

    public static string BookDetail(Guid id)
    {
        return $"{BookTag}:{id}";
    }

    public static string BookSummary(BookId id)
    {
        return $"{BookTag}:summary:{id}";
    }

    public static string BookSummary(Guid id)
    {
        return $"{BookTag}:summary:{id}";
    }

    public static string BookSearch(string normalizedSearch, int pageSize)
    {
        return $"catalog:books:search:v1:{Hash(normalizedSearch)}:{pageSize}";
    }

    public static string BookList(string canonicalKey)
    {
        return $"catalog:books:list:v1:{Hash(canonicalKey)}";
    }

    private static string Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }
}
