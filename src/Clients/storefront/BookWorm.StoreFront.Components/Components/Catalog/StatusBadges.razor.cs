using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Catalog;

public partial class StatusBadges
{
    [Parameter]
    public bool IsOutOfStock { get; set; }
}
