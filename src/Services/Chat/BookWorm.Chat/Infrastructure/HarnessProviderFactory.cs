using Azure.AI.Projects;
using BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Identity.Web;

namespace BookWorm.Chat.Infrastructure;

internal static class HarnessProviderFactory
{
    private const string FoundryMemoryStoreNameConfigKey = "Chat:Harness:FoundryMemory:StoreName";
    private const string DefaultFoundryMemoryStoreName = "bookworm-book-agent-session-memories";

    private static readonly string[] _sharedSkillAndHandoffTools =
    [
        "load_skill",
        "read_skill_resource",
    ];

    public static DecisionToolSelectionOptions CreateBookAgentToolSelectionOptions()
    {
        return new()
        {
            AlwaysIncludedToolNames = new HashSet<string>(
                [
                    .. _sharedSkillAndHandoffTools,
                    "mode_get",
                    "mode_set",
                    "todos_add",
                    "todos_complete",
                    "todos_get_remaining",
                    "todos_get_all",
                    "todos_remove",
                    "file_memory_write",
                    "file_memory_read",
                    "file_memory_delete",
                    "file_memory_ls",
                    "file_memory_grep",
                    "file_memory_replace",
                    "file_memory_replace_lines",
                    $"handoff_to_{Constants.Other.Agents.RatingAgent}",
                    $"handoff_to_{Constants.Other.Agents.QAAgent}",
                ],
                StringComparer.OrdinalIgnoreCase
            ),
        };
    }

    public static DecisionToolSelectionOptions CreateQAAgentToolSelectionOptions()
    {
        return new()
        {
            AlwaysIncludedToolNames = new HashSet<string>(
                [.. _sharedSkillAndHandoffTools, $"handoff_to_{Constants.Other.Agents.BookAgent}"],
                StringComparer.OrdinalIgnoreCase
            ),
        };
    }

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

                You have access to a session-scoped, file-based memory system via the `file_memory_*` tools
                for storing and retrieving information across interactions.
                These files act as your working memory for the current session and are isolated from other sessions.

                - Before starting new research tasks, call `file_memory_ls` and `file_memory_grep`
                  to check for relevant existing memories (plans, findings, downloaded data).
                - Use `file_memory_read`, `file_memory_write`, `file_memory_replace`, `file_memory_replace_lines`,
                  and `file_memory_delete` to manage memory files.
                - Use descriptive file names (e.g., "plan.md", "findings-sci-fi.md").
                - Include a brief description when saving a file to aid future discovery.
                - Save the research plan to a file in plan mode and update it as it changes.
                - Save final findings to a file at the end of execute mode so they survive compaction.
                - Keep memories up-to-date by overwriting or updating files when information changes.
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
            _ => new(new(httpContextAccessor.ResolveUserScope())),
            new()
            {
                ContextPrompt = """
                ## User Reading Preferences & Memories
                Consider the following remembered user preferences, constraints, and reading history when recommending books or answering questions:
                """,
            },
            loggerFactory
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

            return UserScope.FromUserIdOrAnonymous(userId).Value;
        }
    }
}
