using BookWorm.StoreFront.Components.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BookWorm.StoreFront.Components.Components.Button;

public sealed partial class Button
{
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    [Parameter]
    public ButtonType Type { get; set; } = ButtonType.Button;

    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;

    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.Medium;

    [Parameter]
    public string? IconUrl { get; set; }

    [Parameter]
    public string? IconAlt { get; set; }

    [Parameter]
    public IconPosition IconPosition { get; set; } = IconPosition.Left;

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? AdditionalClasses { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    private async Task HandleClick(MouseEventArgs e)
    {
        if (!Disabled && !IsLoading && OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(e);
        }
    }

    private string GetButtonClasses()
    {
        var classes = new List<string>
        {
            "inline-flex",
            "items-center",
            "justify-center",
            "font-medium",
            "rounded-lg",
            "transition-all",
            "duration-200",
            "focus:outline-none",
            "focus:ring-2",
            "focus:ring-offset-2",
            Size switch
            {
                ButtonSize.Small => "px-3 py-1.5 text-sm",
                ButtonSize.Medium => "px-4 py-2 text-base",
                ButtonSize.Large => "px-6 py-3 text-lg",
                _ => "px-4 py-2 text-base",
            },
        };

        if (Disabled || IsLoading)
        {
            classes.Add("opacity-50 cursor-not-allowed pointer-events-none");
        }
        else
        {
            classes.Add("active:scale-95");
            classes.Add(
                Variant switch
                {
                    ButtonVariant.Primary =>
                        "bg-primary text-white hover:opacity-90 hover:shadow-lg active:shadow-md focus:ring-primary",
                    ButtonVariant.Secondary =>
                        "bg-secondary text-white hover:opacity-90 hover:shadow-lg active:shadow-md focus:ring-secondary",
                    ButtonVariant.Outline =>
                        "border-2 border-primary text-primary hover:bg-primary hover:text-white hover:opacity-90 hover:shadow-lg active:shadow-md focus:ring-primary",
                    ButtonVariant.Ghost =>
                        "text-primary hover:bg-primary/10 hover:opacity-80 active:bg-primary/20 focus:ring-primary",
                    ButtonVariant.Danger =>
                        "bg-error text-white hover:opacity-90 hover:shadow-lg active:shadow-md focus:ring-error",
                    _ =>
                        "bg-primary text-white hover:opacity-90 hover:shadow-lg active:shadow-md focus:ring-primary",
                }
            );
        }

        if (!string.IsNullOrEmpty(AdditionalClasses))
        {
            classes.Add(AdditionalClasses);
        }

        return string.Join(" ", classes);
    }

    private string GetHtmlButtonType() =>
        Type switch
        {
            ButtonType.Submit => "submit",
            ButtonType.Reset => "reset",
            _ => "button",
        };

    private string GetIconSize() =>
        Size switch
        {
            ButtonSize.Small => "h-4 w-4",
            ButtonSize.Medium => "h-5 w-5",
            ButtonSize.Large => "h-6 w-6",
            _ => "h-5 w-5",
        };

    private string GetSpinnerSize() =>
        Size switch
        {
            ButtonSize.Small => "loading-sm",
            ButtonSize.Medium => "loading-md",
            ButtonSize.Large => "loading-lg",
            _ => "loading-md",
        };
}
