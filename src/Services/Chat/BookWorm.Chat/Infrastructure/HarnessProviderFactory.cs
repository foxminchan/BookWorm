using Azure.AI.Projects;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Identity.Web;

namespace BookWorm.Chat.Infrastructure;

internal static class HarnessProviderFactory
{
    private const string FoundryMemoryStoreNameConfigKey = "Chat:Harness:FoundryMemory:StoreName";
    private const string DefaultFoundryMemoryStoreName = "bookworm-book-agent-session-memories";

    public static (TodoProvider Todo, AgentModeProvider Mode) CreatePlanningProviders()
    {
        return (new(), new());
    }

    public static FileMemoryProvider CreateFileMemoryProvider(string rootFolder)
    {
        return new(
            new FileSystemAgentFileStore(rootFolder),
            _ =>
                new()
                {
                    WorkingFolder = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.CreateVersion7()}",
                },
            new()
            {
                Instructions = """
                ## File Based Memory

                You have access to a session-scoped, file-based memory system via the `FileMemory_*` tools
                for storing and retrieving information across interactions.
                These files act as your working memory for the current session and are isolated from other sessions.

                - Before starting new research tasks, call FileMemory_ListFiles and FileMemory_SearchFiles
                  to check for relevant existing memories (plans, findings, downloaded data).
                - Use descriptive file names (e.g., "plan.md", "findings-sci-fi.md").
                - Include a brief description when saving a file to aid future discovery.
                - Save the research plan to a file in plan mode and update it as it changes.
                - Save final findings to a file at the end of execute mode so they survive compaction.
                - Keep memories up-to-date by overwriting files when information changes.
                """,
            }
        );
    }

    public static FoundryMemoryProvider CreateFoundryMemoryProvider(
        AIProjectClient projectClient,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILoggerFactory? loggerFactory = null
    )
    {
        ArgumentNullException.ThrowIfNull(projectClient);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        var memoryStoreName = configuration.GetValue(
            FoundryMemoryStoreNameConfigKey,
            DefaultFoundryMemoryStoreName
        );

        return new(
            projectClient,
            memoryStoreName,
            stateInitializer: _ => new(new(httpContextAccessor.ResolveUserScope())),
            options: new()
            {
                ContextPrompt = """
                ## User Reading Preferences & Memories
                Consider the following remembered user preferences, constraints, and reading history when recommending books or answering questions:
                """,
            },
            loggerFactory: loggerFactory
        );
    }

    extension(IHttpContextAccessor httpContextAccessor)
    {
        private string ResolveUserScope()
        {
            var user = httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst(ClaimConstants.NameIdentifierId)?.Value
                ?? user?.FindFirst(ClaimConstants.Oid)?.Value
                ?? user?.FindFirst(ClaimConstants.ObjectId)?.Value
                ?? user?.FindFirst(ClaimConstants.Sub)?.Value;

            return string.IsNullOrWhiteSpace(userId)
                ? $"anonymous-{Guid.CreateVersion7()}"
                : userId;
        }
    }
}
