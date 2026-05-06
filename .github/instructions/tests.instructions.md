---
applyTo: "**/tests/**/*.cs,**/*.Tests/**/*.cs"
---

# FirstBank Nigeria — Test Standards

Every test file in this repository must comply with the following rules.
These apply to all test projects under the `/tests/` directory.

---

## Test Project Structure

Each solution must contain these five test projects under `/tests/`:

```
tests/
├── FirstBankNigeria.<SolutionName>.Domain.Tests/
├── FirstBankNigeria.<SolutionName>.Application.Tests/
├── FirstBankNigeria.<SolutionName>.Infrastructure.Tests/
├── FirstBankNigeria.<SolutionName>.Api.Tests/
└── FirstBankNigeria.<SolutionName>.Architecture.Tests/
```

---

## Coverage Requirements

- Every **command handler** and **query handler** must have unit tests
- Every **API endpoint** must have an integration test
- Every **repository method** must have an infrastructure test
- Every **FluentValidation validator** must have tests for both valid and invalid inputs
- Architecture tests must enforce ALL layer dependency rules on every build

---

## Architecture Test Pattern

The Architecture.Tests project uses `NetArchTest.Rules` to enforce layer boundaries.
Every solution must include tests that assert:

```csharp
// API must not reference Infrastructure
var result = Types.InAssembly(apiAssembly)
    .ShouldNot()
    .HaveDependencyOn("FirstBankNigeria.<SolutionName>.Infrastructure")
    .GetResult();
Assert.True(result.IsSuccessful);

// Application must not reference Infrastructure
var result2 = Types.InAssembly(applicationAssembly)
    .ShouldNot()
    .HaveDependencyOn("FirstBankNigeria.<SolutionName>.Infrastructure")
    .GetResult();
Assert.True(result2.IsSuccessful);

// Domain must have no external dependencies
var result3 = Types.InAssembly(domainAssembly)
    .ShouldNot()
    .HaveDependencyOnAny("MediatR", "Dapper", "FluentValidation")
    .GetResult();
Assert.True(result3.IsSuccessful);
```

---

## Unit Test Conventions

- Use xUnit as the test framework
- Use Moq for mocking
- Test method naming: `MethodName_StateUnderTest_ExpectedBehaviour`
  - Example: `Handle_ValidCommand_ReturnsSuccessResponse`
  - Example: `Handle_DuplicateEmail_ThrowsValidationException`
- Each test must follow Arrange / Act / Assert structure with clear comments
- No test should depend on external state (database, file system, network)
- Mock all repository and service interfaces — never call real infrastructure

---

## Handler Unit Test Pattern

```csharp
public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly Mock<ILogger<CreateCustomerCommandHandler>> _loggerMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _loggerMock = new Mock<ILogger<CreateCustomerCommandHandler>>();
        _handler = new CreateCustomerCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCustomerId()
    {
        // Arrange
        var command = new CreateCustomerCommand { /* valid data */ };
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<CustomerEntity>()))
                       .ReturnsAsync(42);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(42, result.CustomerId);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<CustomerEntity>()), Times.Once);
    }
}
```

---

## Integration Test Requirements

- API integration tests must use `WebApplicationFactory<Program>`
- Every endpoint test must cover: happy path, validation failure (400), unauthorised (401)
- Do not test against a real Oracle database in CI — use a mock or in-memory substitute
- Integration tests must not leave test data in any shared environment

---

## Mandatory NuGet Packages for Test Projects

| Package | Purpose |
|---------|---------|
| `xunit` | Test framework |
| `xunit.runner.visualstudio` | VS Test Explorer integration |
| `Moq` | Mocking framework |
| `FluentAssertions` | Readable assertions |
| `Microsoft.AspNetCore.Mvc.Testing` | API integration tests |
| `NetArchTest.Rules` | Architecture enforcement (Architecture.Tests only) |
