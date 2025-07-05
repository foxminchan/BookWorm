using BookWorm.ArchTests.Abstractions;
using BookWorm.ArchTests.TUnit;
using BookWorm.Chassis.Command;
using BookWorm.Chassis.Endpoints;
using BookWorm.Chassis.EventBus;
using BookWorm.Chassis.Query;
using BookWorm.Chassis.Repository;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Dependencies;

public sealed class DependenciesTests : ArchUnitBaseTest
{
    private const string ExtensionNamespace = $"{nameof(BookWorm)}.*.Extensions";
    private const string FeatureNamespace = $"{nameof(BookWorm)}.*.Features.*";
    private const string IntegrationNamespace = $"{nameof(BookWorm)}.Contracts";
    private const string InfrastructureNamespace = $"{nameof(BookWorm)}.*.Infrastructure.*";

    [Test]
    public void GivenCommands_WhenCommandHandler_ThenShouldNotHaveListOrGet()
    {
        Types()
            .That()
            .ResideInNamespace(FeatureNamespace, true)
            .And()
            .HaveNameEndingWith("Command")
            .Should()
            .NotHaveNameContaining("List")
            .OrShould()
            .NotHaveNameContaining("Get")
            .AndShould()
            .ImplementInterface(typeof(ICommand))
            .OrShould()
            .ImplementInterface(typeof(ICommand<>))
            .Because(
                "Command handlers should not have List or Get methods, as they are responsible for handling commands and not for querying data."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenQueries_WhenQueryHandler_ThenShouldContainListOrGetOrSummarize()
    {
        Types()
            .That()
            .ResideInNamespace(FeatureNamespace, true)
            .And()
            .HaveNameEndingWith("Query")
            .Should()
            .HaveNameContaining("List")
            .OrShould()
            .HaveNameContaining("Get")
            .OrShould()
            .HaveNameContaining("Summarize")
            .AndShould()
            .ImplementInterface(typeof(IQuery<>))
            .Because(
                "Query handlers should contain List, Get, or Summarize methods, as they are responsible for querying data and not for modifying it."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenEndpoints_WhenChecking_ThenShouldImplementIEndpoint()
    {
        Classes()
            .That()
            .ResideInNamespace(FeatureNamespace, true)
            .And()
            .HaveNameEndingWith("Endpoint")
            .Should()
            .ImplementInterface(typeof(IEndpoint))
            .Because(
                $"Endpoints should implement the {nameof(IEndpoint)} interface to ensure they follow the required structure and behavior."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenValidators_WhenChecking_ThenShouldImplementIValidator()
    {
        Classes()
            .That()
            .ResideInNamespace(FeatureNamespace, true)
            .And()
            .HaveNameEndingWith("Validator")
            .Should()
            .ImplementInterface(typeof(IValidator<>))
            .Because(
                $"Validators should implement the {nameof(IValidator)} interface to ensure they follow the required structure and behavior."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenIntegrationEvents_WhenChecking_ThenShouldExtendIntegrationEvent()
    {
        Classes()
            .That()
            .ResideInNamespace(IntegrationNamespace)
            .And()
            .HaveNameEndingWith("IntegrationEvent")
            .Should()
            .BeRecord()
            .AndShould()
            .BeSealed()
            .AndShould()
            .NotImplementInterface(typeof(ICommand))
            .AndShould()
            .NotImplementInterface(typeof(IQuery<>))
            .AndShould()
            .NotImplementInterface(typeof(IEndpoint))
            .AndShould()
            .NotImplementInterface(typeof(IValidator<>))
            .AndShould()
            .BeAssignableTo(typeof(IntegrationEvent))
            .Because(
                $"Integration events should extend the {nameof(IntegrationEvent)} class to ensure they follow the required structure and behavior."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenRepositories_WhenChecking_ThenShouldNotImplementIQueryOrICommand()
    {
        Classes()
            .That()
            .ResideInNamespace(InfrastructureNamespace, true)
            .And()
            .HaveNameEndingWith("Repository")
            .Should()
            .NotImplementInterface(typeof(IQuery<>))
            .AndShould()
            .NotImplementInterface(typeof(ICommand))
            .AndShould()
            .NotImplementInterface(typeof(ICommand<>))
            .OrShould()
            .ImplementInterface(typeof(IRepository<>))
            .OrShould()
            .ImplementInterface(typeof(IUnitOfWork))
            .Because(
                $"Repositories should not implement the IQuery or {nameof(ICommand)} interfaces to ensure they follow the required structure and behavior."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenExtensions_WhenChecking_ThenShouldBePublic()
    {
        Classes()
            .That()
            .ResideInNamespace(ExtensionNamespace, true)
            .And()
            .HaveName("Extensions")
            .Should()
            .BeInternal()
            .Because(
                "Extensions should not be abstract or sealed to ensure they can be extended and used as intended."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenEntityConfigurations_WhenChecking_ThenShouldImplementIEntityTypeConfiguration()
    {
        Classes()
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}", true)
            .And()
            .HaveNameEndingWith("Configuration")
            .And()
            .DoNotResideInNamespace($"{nameof(BookWorm)}.*.Infrastructure.Migrations", true)
            .And()
            .DoNotResideInAssembly(FinanceAssembly)
            .Should()
            .BeInternal()
            .AndShould()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Because(
                "Entity configurations should implement the IEntityTypeConfiguration interface to ensure they follow the required structure and behavior."
            )
            .Check(Architecture);
    }
}
