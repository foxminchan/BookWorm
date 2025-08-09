var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

var tag = AgentFactory.GetAgentName();

app.UseOutputCache();

app.MapPost(
        "/api/book/interact",
        async (Kernel kernel, BookInteractionRequest bookInteractionRequest) =>
        {
            var bookAgent = await AgentFactory.CreateAgentAsync(kernel);

            var message = new ChatMessageContent(AuthorRole.User, bookInteractionRequest.Query);

            var responseBuilder = new StringBuilder();

            await foreach (
                ChatMessageContent response in bookAgent.InvokeAsync(message).ConfigureAwait(false)
            )
            {
                if (response.Items.Count > 0)
                {
                    responseBuilder.Append(response.Items[^1].ToString() ?? string.Empty);
                }
            }

            return responseBuilder.Length > 0 ? responseBuilder.ToString() : null;
        }
    )
    .Produces<string>(contentType: MediaTypeNames.Text.Plain)
    .WithTags(tag);

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.Run();
