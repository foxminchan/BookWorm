using BookWorm.StoreFront.Components.Mocks;
using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Home;

public sealed partial class NewArrivals : ComponentBase
{
    private List<Book> _books = [];
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        await Task.Delay(500);
        _books = MockDataProvider.GetNewArrivals();
        _isLoading = false;
    }
}
