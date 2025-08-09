var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddDefaultOpenApi();

builder.Services.AddKernel();

builder.AddSkTelemetry();

builder.AddChatCompletion();

var app = builder.Build();

app.UseOutputCache();

var tag = AgentFactory.GetAgentName();

app.MapPost(
        "/api/sentiment/evaluate",
        async (Kernel kernel, SentimentRequest sentimentRequest) =>
        {
            var summaryAgent = AgentFactory.CreateAgent(kernel);

            var message = new ChatMessageContent(AuthorRole.User, sentimentRequest.TextToEvaluate);

            var responseBuilder = new StringBuilder();

            await foreach (
                ChatMessageContent response in summaryAgent
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
