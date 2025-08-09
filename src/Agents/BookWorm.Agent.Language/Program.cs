var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddDefaultOpenApi();

builder.Services.AddKernel();

builder.AddSkTelemetry();

builder.AddChatCompletion();

var app = builder.Build();

var tag = AgentFactory.GetAgentName();

app.UseOutputCache();

app.MapPost(
        "/api/language/translate",
        async (Kernel kernel, LanguageRequest languageRequest) =>
        {
            var languageAgent = AgentFactory.CreateAgent(kernel);

            var message = new ChatMessageContent(AuthorRole.User, languageRequest.TextToTranslate);

            var responseBuilder = new StringBuilder();

            await foreach (
                ChatMessageContent response in languageAgent
                    .InvokeAsync(message)
                    .ConfigureAwait(false)
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
