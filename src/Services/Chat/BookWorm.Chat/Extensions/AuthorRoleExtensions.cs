using Microsoft.SemanticKernel.ChatCompletion;

namespace BookWorm.Chat.Extensions;

public static class AuthorRoleExtensions
{
    public static AuthorRole ToAuthorRole(this ChatRole role)
    {
        return role switch
        {
            _ when role == ChatRole.Assistant => AuthorRole.Assistant,
            _ when role == ChatRole.System => AuthorRole.System,
            _ when role == ChatRole.User => AuthorRole.User,
            _ when role == ChatRole.Tool => AuthorRole.Tool,
            _ => throw new InvalidOperationException($"Unexpected role '{role}'"),
        };
    }
}
