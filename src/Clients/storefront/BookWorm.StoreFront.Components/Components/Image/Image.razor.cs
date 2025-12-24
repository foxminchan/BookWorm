using BookWorm.StoreFront.Components.Enums;
using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Components.Image;

public sealed partial class Image
{
    [Parameter, EditorRequired]
    public required string Src { get; set; }

    [Parameter, EditorRequired]
    public required string Alt { get; set; }

    [Parameter]
    public string Loading { get; set; } = "lazy";

    [Parameter]
    public string Decoding { get; set; } = "async";

    [Parameter]
    public ImageFit ObjectFit { get; set; } = ImageFit.Cover;

    [Parameter]
    public string? Width { get; set; }

    [Parameter]
    public string? Height { get; set; }

    [Parameter]
    public string? AspectRatio { get; set; }

    [Parameter]
    public bool Rounded { get; set; }

    [Parameter]
    public string? AdditionalClasses { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public string? FallbackSrc { get; set; }

    [Parameter]
    public EventCallback OnLoad { get; set; }

    private string GetImageClasses()
    {
        var classes = new List<string>
        {
            ObjectFit switch
            {
                ImageFit.Cover => "object-cover",
                ImageFit.Contain => "object-contain",
                ImageFit.Fill => "object-fill",
                ImageFit.None => "object-none",
                ImageFit.ScaleDown => "object-scale-down",
                _ => "object-cover",
            },
        };

        if (Rounded)
        {
            classes.Add("rounded-lg");
        }

        if (!string.IsNullOrEmpty(AspectRatio))
        {
            classes.Add($"aspect-{AspectRatio}");
        }

        if (!string.IsNullOrEmpty(AdditionalClasses))
        {
            classes.Add(AdditionalClasses);
        }

        return string.Join(" ", classes);
    }

    private async Task HandleLoad()
    {
        if (OnLoad.HasDelegate)
        {
            await OnLoad.InvokeAsync();
        }
    }
}
