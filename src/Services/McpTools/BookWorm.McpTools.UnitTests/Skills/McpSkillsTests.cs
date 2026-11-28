using System.IO.Pipelines;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Skills;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Extensions.Skills;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.UnitTests.Skills;

public sealed class McpSkillsTests
{
    private static string SkillsDirectory => Path.Combine(AppContext.BaseDirectory, "Skills");

    [Test]
    public async Task GivenMcpServerWithSkills_WhenListingSkills_ThenShouldReturnBookCatalogAndStorePolicies()
    {
        // Arrange
        await using var harness = await McpSkillsTestHarness.CreateAsync(SkillsDirectory);

        // Act
        var supportsSkills = harness.Client.ServerCapabilities.SupportsSkills();
        var skills = await harness.Client.ListSkillsAsync();

        // Assert
        supportsSkills.ShouldBeTrue();
        skills.Count.ShouldBe(3);
        skills
            .Select(s => s.Name)
            .ShouldBe(["book-catalog", "review-analysis", "store-policies"], true);

        var bookCatalog = skills.Single(s => s.Name == "book-catalog");
        bookCatalog.Uri.ShouldBe("skill://book-catalog/SKILL.md");
        bookCatalog.Description.ShouldNotBeNullOrWhiteSpace();
        bookCatalog.License.ShouldBe("MIT");
        bookCatalog.Resources.IsDynamic.ShouldBeFalse();
        bookCatalog.Resources.Entries.ShouldNotBeNull();
        bookCatalog
            .Resources.Entries.Select(e => e.Uri)
            .ShouldBe(
                [
                    "skill://book-catalog/SKILL.md",
                    "skill://book-catalog/assets/book-listing-template.md",
                    "skill://book-catalog/references/CATALOG_GUIDE.md",
                ],
                true
            );

        var storePolicies = skills.Single(s => s.Name == "store-policies");
        storePolicies.Uri.ShouldBe("skill://store-policies/SKILL.md");
        storePolicies.Description.ShouldNotBeNullOrWhiteSpace();
        storePolicies.License.ShouldBe("MIT");
        storePolicies.Resources.IsDynamic.ShouldBeFalse();
        storePolicies.Resources.Entries.ShouldNotBeNull();
        storePolicies
            .Resources.Entries.Select(e => e.Uri)
            .ShouldBe(
                [
                    "skill://store-policies/SKILL.md",
                    "skill://store-policies/references/STORE_FAQ.md",
                ],
                true
            );

        var reviewAnalysis = skills.Single(s => s.Name == "review-analysis");
        reviewAnalysis.Uri.ShouldBe("skill://review-analysis/SKILL.md");
        reviewAnalysis.Description.ShouldNotBeNullOrWhiteSpace();
        reviewAnalysis.License.ShouldBe("MIT");
        reviewAnalysis.Resources.IsDynamic.ShouldBeFalse();
        reviewAnalysis.Resources.Entries.ShouldNotBeNull();
        reviewAnalysis
            .Resources.Entries.Select(e => e.Uri)
            .ShouldBe(
                [
                    "skill://review-analysis/SKILL.md",
                    "skill://review-analysis/references/RATING_GUIDE.md",
                ],
                true
            );
    }

    [Test]
    public async Task GivenMcpServerWithSkills_WhenGettingSkillAndReadingResources_ThenShouldVerifyManifestAndReturnContent()
    {
        // Arrange
        await using var harness = await McpSkillsTestHarness.CreateAsync(SkillsDirectory);

        // Act
        var skill = await harness.Client.GetSkillAsync("skill://book-catalog/SKILL.md");
        var skillDoc = await harness.Client.ReadSkillResourceAsync(skill, skill.Uri);
        var guideDoc = await harness.Client.ReadSkillResourceAsync(
            skill,
            "skill://book-catalog/references/CATALOG_GUIDE.md"
        );
        var templateDoc = await harness.Client.ReadSkillResourceAsync(
            skill,
            "skill://book-catalog/assets/book-listing-template.md"
        );

        // Assert
        var skillText = skillDoc.Contents.OfType<TextResourceContents>().Single();
        skillText.Text.ShouldContain("# Book Catalog");

        var guideText = guideDoc.Contents.OfType<TextResourceContents>().Single();
        guideText.Text.ShouldContain("# Book Catalog Guide");

        var templateText = templateDoc.Contents.OfType<TextResourceContents>().Single();
        templateText.Text.ShouldContain("# Book Listing Template");

        await Should.ThrowAsync<InvalidDataException>(async () =>
            await harness.Client.ReadSkillResourceAsync(
                skill,
                "skill://book-catalog/references/UNLISTED.md"
            )
        );
    }

    [Test]
    public async Task GivenMcpAgentSkillsSource_WhenGettingSkills_ThenShouldLoadVerifiedContentAndResources()
    {
        // Arrange
        await using var harness = await McpSkillsTestHarness.CreateAsync(SkillsDirectory);
        using var source = new McpAgentSkillsSource(harness.Client);

        // Act
        var skills = await source.GetSkillsAsync(new(Mock.Of<AIAgent>(), null));

        // Assert
        skills.Count.ShouldBe(3);

        var bookCatalog = skills.Single(s => s.Frontmatter.Name == "book-catalog");
        bookCatalog.Frontmatter.Description.ShouldNotBeNullOrWhiteSpace();
        bookCatalog.Frontmatter.License.ShouldBe("MIT");
        bookCatalog.Frontmatter.AllowedTools.ShouldBeNull();

        var content = await bookCatalog.GetContentAsync();
        content.ShouldContain("# Book Catalog");

        var guideResource = await bookCatalog.GetResourceAsync("references/CATALOG_GUIDE.md");
        guideResource.ShouldNotBeNull();
        var guideContent = await guideResource.ReadAsync();
        guideContent.ShouldBeOfType<string>().ShouldContain("# Book Catalog Guide");

        var missingResource = await bookCatalog.GetResourceAsync("references/missing.md");
        missingResource.ShouldBeNull();
    }

    [Test]
    public async Task GivenMcpServerWithoutSkills_WhenGettingSkillsFromSource_ThenShouldReturnEmptyList()
    {
        // Arrange
        await using var harness = await McpSkillsTestHarness.CreateAsync(null);
        using var source = new McpAgentSkillsSource(harness.Client);

        // Act
        var supportsSkills = harness.Client.ServerCapabilities.SupportsSkills();
        var skills = await source.GetSkillsAsync(new(Mock.Of<AIAgent>(), null));

        // Assert
        supportsSkills.ShouldBeFalse();
        skills.ShouldBeEmpty();
    }

    [Test]
    public async Task GivenMcpClient_WhenCreatingSkillsProviderWithFilter_ThenShouldBuildProvider()
    {
        // Arrange
        await using var harness = await McpSkillsTestHarness.CreateAsync(SkillsDirectory);

        // Act
        using var provider = harness.Client.CreateSkillsProvider(
            new HashSet<string>(["book-catalog"], StringComparer.OrdinalIgnoreCase)
        );

        // Assert
        provider.ShouldNotBeNull();
    }

    private sealed class McpSkillsTestHarness : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts;
        private readonly McpServer _server;
        private readonly Task _serverTask;
        private readonly ServiceProvider _serviceProvider;

        private McpSkillsTestHarness(
            McpClient client,
            McpServer server,
            Task serverTask,
            ServiceProvider serviceProvider,
            CancellationTokenSource cts
        )
        {
            Client = client;
            _server = server;
            _serverTask = serverTask;
            _serviceProvider = serviceProvider;
            _cts = cts;
        }

        public McpClient Client { get; }

        public async ValueTask DisposeAsync()
        {
            await Client.DisposeAsync();
            await _cts.CancelAsync();

            await _serverTask.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

            if (_serverTask.IsFaulted)
            {
                await _serverTask;
            }

            await _server.DisposeAsync();
            await _serviceProvider.DisposeAsync();
            _cts.Dispose();
        }

        public static async Task<McpSkillsTestHarness> CreateAsync(string? skillsDirectory)
        {
            var clientToServer = new Pipe();
            var serverToClient = new Pipe();

            var services = new ServiceCollection();
            var builder = services
                .AddMcpServer()
                .WithStreamServerTransport(
                    clientToServer.Reader.AsStream(),
                    serverToClient.Writer.AsStream()
                );

            if (skillsDirectory is not null)
            {
                builder.WithSkillsFromSubdirectories(skillsDirectory);
            }

            var serviceProvider = services.BuildServiceProvider();
            var server = serviceProvider.GetRequiredService<McpServer>();

            var cts = new CancellationTokenSource();
            var serverTask = server.RunAsync(cts.Token);

            var client = await McpClient.CreateAsync(
                new StreamClientTransport(
                    clientToServer.Writer.AsStream(),
                    serverToClient.Reader.AsStream()
                ),
                cancellationToken: cts.Token
            );

            return new(client, server, serverTask, serviceProvider, cts);
        }
    }
}
