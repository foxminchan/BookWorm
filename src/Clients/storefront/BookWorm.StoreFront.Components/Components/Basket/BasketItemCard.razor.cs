using BookWorm.StoreFront.Components.Models;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Basket;

public partial class BasketItemCard
{
	[Parameter, EditorRequired]
	public required BasketItem Item { get; set; }

	[Parameter]
	public EventCallback OnIncrease { get; set; }

	[Parameter]
	public EventCallback OnDecrease { get; set; }

	[Parameter]
	public EventCallback OnRemove { get; set; }
}
