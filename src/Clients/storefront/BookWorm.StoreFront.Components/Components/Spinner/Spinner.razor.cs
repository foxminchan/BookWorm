using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Spinner;

public sealed partial class Spinner
{
    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public string SpinnerSize { get; set; } = "loading-lg";

    [Parameter]
    public string ContainerClass { get; set; } = "flex justify-center items-center py-20";

    [Parameter]
    public bool Inline { get; set; }
}
