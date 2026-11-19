using BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;

namespace BookWorm.Chat.Domain.Services;

public interface IPromptSafetyEvaluator
{
    PromptSafetyResult Evaluate(string content);
}
