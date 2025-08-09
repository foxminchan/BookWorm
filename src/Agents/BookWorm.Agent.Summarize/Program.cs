using System.Buffers;
using System.Text;
using A2A.AspNetCore;
using BookWorm.Chassis.Endpoints;
using BookWorm.Chassis.RAG;
using BookWorm.Chassis.RAG.A2A;
using BookWorm.ServiceDefaults;
using BookWorm.SharedKernel.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Agent = BookWorm.Agent.Summarize.Agent;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddKernel();

builder.AddSkTelemetry();

builder.AddChatCompletion();

builder.Services.AddVersioning();

var app = builder.Build();

app.MapPost(
        "/api/summary",
        async (Kernel kernel, SummarizeRequest summarizeRequest) =>
        {
            var summaryAgent = Agent.CreateAgent(kernel);

            // Add a user message to the conversation
            var message = new ChatMessageContent(AuthorRole.User, summarizeRequest.TextToSummarize);

            // Generate the agent response(s)
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
    .WithTags("SummarizeAgent");

var hostAgent = new A2AHostAgent(
    Agent.CreateAgent(app.Services.GetRequiredService<Kernel>()),
    Agent.GetAgentCard()
);

app.MapA2A(hostAgent.TaskManager!, "/").WithTags("SummarizeAgent");

app.MapHttpA2A(hostAgent.TaskManager!, "/").WithTags("SummarizeAgent");

app.MapDefaultEndpoints();

app.Run();
