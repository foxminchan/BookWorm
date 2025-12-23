using BookWorm.StoreFront.Components.Components.Base;
using BookWorm.StoreFront.Components.Mocks;
using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Pages.Catalog;

public partial class CatalogItem
{
    [Parameter]
    public Guid Id { get; set; }

    private Book? _book;
    private bool _isLoading = true;
    private int _quantity = 1;
    private string _activeTab = "description";
    private readonly List<Breadcrumb.BreadcrumbItem> _breadcrumbItems = [];

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        await LoadBook();
        _isLoading = false;
    }

    private Task LoadBook()
    {
        // Get book from mock data
        _book = MockDataProvider.GetAllBooks().FirstOrDefault(b => b.Id == Id);

        if (_book is null)
            return Task.CompletedTask;
        _breadcrumbItems.Clear();
        _breadcrumbItems.Add(new("Home", "/"));
        _breadcrumbItems.Add(new("Catalog", "/catalog"));
        if (_book.Category is not null)
        {
            _breadcrumbItems.Add(
                new(_book.Category.Name, $"/catalog?category={_book.Category.Id}")
            );
        }
        _breadcrumbItems.Add(new(_book.Name ?? "Product"));

        return Task.CompletedTask;
    }

    private void AddToCart(int quantity)
    {
        if (_book is null)
            return;

        // TODO: Implement add to cart functionality
        Console.WriteLine($"Added {quantity} of book {_book.Name} (ID: {Id}) to cart");
    }
}
