var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

var tag = AgentFactory.GetAgentName();

app.UseOutputCache();

app.MapPost(
        "/api/feedbacks/rate",
        async (Kernel kernel, FeedbackSummaryRequest feedbackSummaryRequest) =>
        {
            var ratingAgent = await AgentFactory.CreateAgentAsync(kernel);

            var message = new ChatMessageContent(AuthorRole.User, feedbackSummaryRequest.Query);

            var responseBuilder = new StringBuilder();

            await foreach (
                ChatMessageContent response in ratingAgent
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

app.MapA2AEndpoints(tag);

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.Run();
