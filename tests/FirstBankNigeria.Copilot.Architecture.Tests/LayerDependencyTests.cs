using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace FirstBankNigeria.Copilot.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private const string DomainNamespace = "FirstBankNigeria.Copilot.Domain";
    private const string ApplicationNamespace = "FirstBankNigeria.Copilot.Application";
    private const string InfrastructureNamespace = "FirstBankNigeria.Copilot.Infrastructure";
    private const string ApiNamespace = "FirstBankNigeria.Copilot.Api";

    [Fact]
    public void Api_ShouldNot_HaveDependencyOn_Infrastructure()
    {
        // Arrange
        var apiAssembly = typeof(FirstBankNigeria.Copilot.Api.Controllers.BaseController).Assembly;

        // Act
        var result = Types.InAssembly(apiAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "the API layer must not reference Infrastructure directly");
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOn_Infrastructure()
    {
        // Arrange
        var applicationAssembly = typeof(FirstBankNigeria.Copilot.Application.DependencyInjection).Assembly;

        // Act
        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "the Application layer must not reference Infrastructure");
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOn_MediatR()
    {
        // Arrange
        var domainAssembly = typeof(FirstBankNigeria.Copilot.Domain.AssemblyMarker).Assembly;

        // Act
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("MediatR", "Dapper", "FluentValidation")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "the Domain layer must have zero external dependencies");
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOn_Application()
    {
        // Arrange
        var domainAssembly = typeof(FirstBankNigeria.Copilot.Domain.AssemblyMarker).Assembly;

        // Act
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "the Domain layer must not reference the Application layer");
    }
}
