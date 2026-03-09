using BookWorm.AI.Evaluation.Setup;
using Microsoft.Extensions.AI.Evaluation.Quality;

namespace BookWorm.AI.Evaluation;

public sealed partial class WorkflowEvaluationTests
{
    [Test]
    public async Task GivenReturnPolicyQuestion_WhenResponding_ThenReturnsAccurateDetails()
    {
        await using var scenarioRun = await s_groundednessReportingConfig.CreateScenarioRunAsync(
            "CustomerSupport.ReturnPolicy"
        );

        var chatClient = scenarioRun.ChatConfiguration!.ChatClient;

        var (messages, response) = await TestSetup.GetBookstoreConversationAsync(
            chatClient,
            "What is your return policy? Can I return a book I started reading?"
        );

        var result = await scenarioRun.EvaluateAsync(
            messages,
            response,
            [
                new GroundednessEvaluatorContext(
                    """
                    Return window: 30 days from delivery for unused items in original condition.
                    Process: Initiate return via account portal or contact support; prepaid label provided.
                    Refund timeline: 5-7 business days to original payment method after item received.
                    Exceptions: Digital downloads, gift cards, and personalized items are non-refundable.
                    Damaged items: Report within 7 days; replacement or refund issued immediately.
                    Physical books in resalable condition (no markings, intact spine) qualify for return within 30 days.
                    """
                ),
            ]
        );

        ValidateNoWarnings(result);
    }

    [Test]
    public async Task GivenShippingInquiry_WhenResponding_ThenReturnsAccurateInfo()
    {
        await using var scenarioRun = await s_groundednessReportingConfig.CreateScenarioRunAsync(
            "CustomerSupport.ShippingInfo"
        );

        var chatClient = scenarioRun.ChatConfiguration!.ChatClient;

        var (messages, response) = await TestSetup.GetBookstoreConversationAsync(
            chatClient,
            "What are your shipping options and how long does delivery take?"
        );

        var result = await scenarioRun.EvaluateAsync(
            messages,
            response,
            [
                new GroundednessEvaluatorContext(
                    """
                    Standard shipping: 5-7 business days, free over $35.
                    Express shipping: 2-3 business days, $7.99.
                    Overnight shipping: Next business day, $19.99.
                    International shipping: 10-21 business days, varies by country.
                    Orders placed before 2 PM EST ship same day.
                    Tracking number emailed within 24 hours of dispatch.
                    """
                ),
            ]
        );

        ValidateNoWarnings(result);
    }

    [Test]
    public async Task GivenLoyaltyProgramQuestion_WhenResponding_ThenReturnsAccurateDetails()
    {
        await using var scenarioRun = await s_groundednessReportingConfig.CreateScenarioRunAsync(
            "CustomerSupport.LoyaltyProgram"
        );

        var chatClient = scenarioRun.ChatConfiguration!.ChatClient;

        var (messages, response) = await TestSetup.GetBookstoreConversationAsync(
            chatClient,
            "How does the BookPoints loyalty program work? Do my points expire?"
        );

        var result = await scenarioRun.EvaluateAsync(
            messages,
            response,
            [
                new GroundednessEvaluatorContext(
                    """
                    BookPoints Loyalty Program: Earn 1 point per $1 spent; 100 points = $1 reward.
                    Points expire after 12 months of account inactivity.
                    Points are non-transferable but can be gifted as a gift card.
                    Gift cards and discount codes can be combined at checkout.
                    """
                ),
            ]
        );

        ValidateNoWarnings(result);
    }
}
