using System.Globalization;
using BookWorm.StoreFront.Components.Components.Base;
using BookWorm.StoreFront.Components.Mocks;
using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace BookWorm.StoreFront.Components.Pages.Catalog;

public sealed partial class Catalog
{
    private readonly List<Breadcrumb.BreadcrumbItem> _breadcrumbItems =
    [
        new("Home", "/"),
        new("Catalog"),
    ];

    private CatalogQueryParams _filters = new();
    private List<Book> _allBooks = [];
    private List<Book> _filteredBooks = [];
    private int _totalBooks;
    private int _totalPages;
    private bool _isLoading = true;
    private bool _showFilters;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        InitializeMockData();
        ParseUrlParameters();
        ApplyFilters();
        _isLoading = false;
        await base.OnInitializedAsync();
    }

    private void InitializeMockData()
    {
        _allBooks = MockDataProvider.GetAllBooks();
        _totalBooks = _allBooks.Count;
    }

    private void ParseUrlParameters()
    {
        var uri = new Uri(Navigation.Uri);
        var queryParams = QueryHelpers.ParseQuery(uri.Query);

        if (queryParams.TryGetValue("category", out var categoryValue))
        {
            var categories = categoryValue
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => Guid.TryParse(c, out var guid) ? guid : (Guid?)null)
                .Where(c => c.HasValue)
                .Select(c => c!.Value)
                .ToArray();

            if (categories.Length > 0)
            {
                _filters.CategoryId = categories;
            }
        }

        if (queryParams.TryGetValue("publisher", out var publisherValue))
        {
            var publishers = publisherValue
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => Guid.TryParse(p, out var guid) ? guid : (Guid?)null)
                .Where(p => p.HasValue)
                .Select(p => p!.Value)
                .ToArray();

            if (publishers.Length > 0)
            {
                _filters.PublisherId = publishers;
            }
        }

        if (
            queryParams.TryGetValue("minPrice", out var minPriceValue)
            && decimal.TryParse(minPriceValue, out var minPrice)
        )
        {
            _filters.MinPrice = minPrice;
        }

        if (
            queryParams.TryGetValue("maxPrice", out var maxPriceValue)
            && decimal.TryParse(maxPriceValue, out var maxPrice)
        )
        {
            _filters.MaxPrice = maxPrice;
        }
    }

    private void ApplyFilters()
    {
        var query = _allBooks.AsEnumerable();

        // Apply price filter
        if (_filters.MinPrice.HasValue)
        {
            query = query.Where(b => b.Price >= _filters.MinPrice.Value);
        }

        if (_filters.MaxPrice.HasValue)
        {
            query = query.Where(b => b.Price <= _filters.MaxPrice.Value);
        }

        // Apply category filter
        if (_filters.CategoryId is { Length: > 0 })
        {
            query = query.Where(b =>
                b.Category is not null && _filters.CategoryId.Contains(b.Category.Id)
            );
        }

        // Apply publisher filter
        if (_filters.PublisherId is { Length: > 0 })
        {
            query = query.Where(b =>
                b.Publisher is not null && _filters.PublisherId.Contains(b.Publisher.Id)
            );
        }

        // Apply sorting
        query = _filters.OrderBy switch
        {
            nameof(Book.Price) => _filters.IsDescending
                ? query.OrderByDescending(b => b.Price)
                : query.OrderBy(b => b.Price),
            nameof(Book.Name) => _filters.IsDescending
                ? query.OrderByDescending(b => b.Name)
                : query.OrderBy(b => b.Name),
            nameof(Book.AverageRating) => query.OrderByDescending(b => b.AverageRating),
            nameof(Book.TotalReviews) => query.OrderByDescending(b => b.TotalReviews),
            _ => query,
        };

        // Materialize query to avoid multiple enumerations
        var filteredBooksList = query.ToList();

        // Calculate pagination on full filtered results
        var filteredCount = filteredBooksList.Count;
        _totalPages = (int)Math.Ceiling(filteredCount / (double)_filters.PageSize);

        // Apply pagination
        var skip = (_filters.PageIndex - 1) * _filters.PageSize;
        _filteredBooks = filteredBooksList.Skip(skip).Take(_filters.PageSize).ToList();
    }

    private void OnCategoryToggled((Guid CategoryId, bool IsChecked) args)
    {
        var categoryIds = _filters.CategoryId?.ToHashSet() ?? [];

        if (args.IsChecked)
        {
            categoryIds.Add(args.CategoryId);
        }
        else
        {
            categoryIds.Remove(args.CategoryId);
        }

        _filters.CategoryId = categoryIds.Count > 0 ? [.. categoryIds] : null;
        _filters.PageIndex = 1;
        UpdateUrl();
        ApplyFilters();
        StateHasChanged();
    }

    private void OnPublisherToggled((Guid PublisherId, bool IsChecked) args)
    {
        var publisherIds = _filters.PublisherId?.ToHashSet() ?? [];

        if (args.IsChecked)
        {
            publisherIds.Add(args.PublisherId);
        }
        else
        {
            publisherIds.Remove(args.PublisherId);
        }

        _filters.PublisherId = publisherIds.Count > 0 ? [.. publisherIds] : null;
        _filters.PageIndex = 1;
        UpdateUrl();
        ApplyFilters();
        StateHasChanged();
    }

    private void OnMinPriceChange(decimal? value)
    {
        if (value.HasValue && value.Value > (_filters.MaxPrice ?? 9999))
            return;
        _filters.MinPrice = value;
        _filters.PageIndex = 1;
        UpdateUrl();
        ApplyFilters();
        StateHasChanged();
    }

    private void OnMaxPriceChange(decimal? value)
    {
        if (value.HasValue && value.Value < (_filters.MinPrice ?? 0))
            return;
        _filters.MaxPrice = value;
        _filters.PageIndex = 1;
        UpdateUrl();
        ApplyFilters();
        StateHasChanged();
    }

    private void OnPriceRangeSelected((decimal? Min, decimal? Max) range)
    {
        _filters.MinPrice = range.Min;
        _filters.MaxPrice = range.Max;
        _filters.PageIndex = 1;
        UpdateUrl();
        ApplyFilters();
        StateHasChanged();
    }

    private void ClearFilters()
    {
        _filters = new();
        UpdateUrl();
        ApplyFilters();
        _showFilters = false;
        StateHasChanged();
    }

    private void OnSortChanged(string sortBy)
    {
        switch (sortBy)
        {
            case "price-low":
                _filters.OrderBy = nameof(Book.Price);
                _filters.IsDescending = false;
                break;
            case "price-high":
                _filters.OrderBy = nameof(Book.Price);
                _filters.IsDescending = true;
                break;
            case "name-asc":
                _filters.OrderBy = nameof(Book.Name);
                _filters.IsDescending = false;
                break;
            case "name-desc":
                _filters.OrderBy = nameof(Book.Name);
                _filters.IsDescending = true;
                break;
            case "rating":
                _filters.OrderBy = nameof(Book.AverageRating);
                _filters.IsDescending = true;
                break;
            case "reviews":
                _filters.OrderBy = nameof(Book.TotalReviews);
                _filters.IsDescending = true;
                break;
            default:
                _filters.OrderBy = nameof(Book.Price);
                _filters.IsDescending = false;
                break;
        }

        ApplyFilters();
    }

    private string GetSortByValue()
    {
        return _filters.OrderBy switch
        {
            nameof(Book.Price) when !_filters.IsDescending => "price-low",
            nameof(Book.Price) when _filters.IsDescending => "price-high",
            nameof(Book.Name) when !_filters.IsDescending => "name-asc",
            nameof(Book.Name) when _filters.IsDescending => "name-desc",
            nameof(Book.AverageRating) => "rating",
            nameof(Book.TotalReviews) => "reviews",
            _ => "price-low",
        };
    }

    private void ChangePage(int page)
    {
        if (page < 1 || page > _totalPages)
            return;
        _filters.PageIndex = page;
        ApplyFilters();
    }

    private void AddToCart(Guid bookId)
    {
        // TODO: Implement add to cart functionality
        Console.WriteLine($"Added book {bookId} to cart");
    }

    private void UpdateUrl()
    {
        var queryParams = new Dictionary<string, object?>();

        if (_filters.CategoryId is { Length: > 0 })
        {
            queryParams["category"] = string.Join(",", _filters.CategoryId);
        }
        else
        {
            queryParams["category"] = null;
        }

        if (_filters.PublisherId is { Length: > 0 })
        {
            queryParams["publisher"] = string.Join(",", _filters.PublisherId);
        }
        else
        {
            queryParams["publisher"] = null;
        }

        if (_filters.MinPrice.HasValue)
        {
            queryParams["minPrice"] = _filters.MinPrice.Value.ToString(
                CultureInfo.InvariantCulture
            );
        }
        else
        {
            queryParams["minPrice"] = null;
        }

        if (_filters.MaxPrice.HasValue)
        {
            queryParams["maxPrice"] = _filters.MaxPrice.Value.ToString(
                CultureInfo.InvariantCulture
            );
        }
        else
        {
            queryParams["maxPrice"] = null;
        }

        var newUri = Navigation.GetUriWithQueryParameters(queryParams);
        Navigation.NavigateTo(newUri, forceLoad: false, replace: true);
    }

    private void ToggleFilters()
    {
        _showFilters = !_showFilters;
    }
}
