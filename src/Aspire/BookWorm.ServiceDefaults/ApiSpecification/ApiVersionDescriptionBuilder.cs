using System.Net;
using System.Net.Mime;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Primitives;

namespace BookWorm.ServiceDefaults.ApiSpecification;

public static class ApiVersionDescriptionBuilder
{
    public static string BuildDescription(ApiVersionDescription? api, string baseDescription)
    {
        var text = new StringBuilder(baseDescription);

        if (api is null)
        {
            return text.ToString();
        }

        if (api.IsDeprecated)
        {
            text.AppendLine();
            text.AppendLine("<b>⚠️ This API version has been deprecated.</b>");
        }

        if (api.SunsetPolicy is not { } policy)
        {
            return text.ToString();
        }

        text.AppendLine();

        if (policy.Date is { } when)
        {
            text.AppendLine($"<b>Sunset date:</b> {when:yyyy-MM-dd}");
        }

        if (!policy.HasLinks)
        {
            return text.ToString();
        }

        text.AppendLine();

        var rendered = false;

        foreach (var link in policy.Links.Where(l => l.Type == MediaTypeNames.Text.Html))
        {
            if (!rendered)
            {
                text.Append("<h4>Links</h4><ul>");
                rendered = true;
            }

            text.Append("<li><a href=\"");
            text.Append(WebUtility.HtmlEncode(link.LinkTarget.OriginalString));
            text.Append("\">");
            text.Append(
                StringSegment.IsNullOrEmpty(link.Title)
                    ? WebUtility.HtmlEncode(link.LinkTarget.OriginalString)
                    : WebUtility.HtmlEncode(link.Title.ToString())
            );
            text.Append("</a></li>");
        }

        if (rendered)
        {
            text.Append("</ul>");
        }

        return text.ToString();
    }

    public static IReadOnlyList<ApiVersionDescription> GetApiVersionDescription(
        this IServiceProvider provider
    )
    {
        ArgumentNullException.ThrowIfNull(provider);

        return provider.GetService<IApiVersionDescriptionProvider>()?.ApiVersionDescriptions
            ?? [new(new(1, 0), "v1")];
    }
}
