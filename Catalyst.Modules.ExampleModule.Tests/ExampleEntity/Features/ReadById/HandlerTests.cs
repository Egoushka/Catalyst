using Catalyst.Common.Errors.Definitions;
using Catalyst.ExampleModule;
using Catalyst.ExampleModule.Example.Application.Features.ReadById;
using Catalyst.ExampleModule.Example.Domain.Enums;
using Catalyst.ExampleModule.Example.Domain.Models;
using Catalyst.Tests.Unit.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Type = Catalyst.ExampleModule.Example.Domain.Models.Type;

namespace Catalyst.Modules.ExampleModule.Tests.ExampleEntity.Features.ReadById;

public class HandlerTests : BaseTest
{
    private readonly ExampleModuleDbContext _dbContext; // Using InMemory for this example
    private readonly Handler _handler;
    private readonly Mock<ILogger<Handler>> _mockLogger;

    public HandlerTests()
    {
        _mockLogger = new Mock<ILogger<Handler>>();

        // Setup InMemory DbContext
        var options = new DbContextOptionsBuilder<ExampleModuleDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name per test run
            .Options;
        _dbContext = new ExampleModuleDbContext(options);

        _handler = new Handler(_mockLogger.Object, _dbContext);
    }

    private Catalyst.ExampleModule.Example.Domain.Models.ExampleEntity SeedExampleEntity(int id)
    {
        var stop = new Stop { Name = Faker.Address.StreetName() };
        var model = new Model { Name = Faker.Vehicle.Model() };
        var type = new Type { Name = Faker.Commerce.Categories(1)[0] };
        var serviceOrg = new ServiceOrganization { Name = Faker.Company.CompanyName() };
        var manufacturer = new Manufacturer { Name = Faker.Company.CompanyName() };

        _dbContext.Types.Add(type);
        _dbContext.SaveChanges();

        var dashboard = new Catalyst.ExampleModule.Example.Domain.Models.ExampleEntity
        {
            // Id is auto-generated for BaseEntity, but we can't set it directly.
            // For InMemory, it's tricky to control Id without saving and querying.
            // Let's assume Id is set by the database upon insertion.
            // For this test, we'll retrieve it after saving.
            TypeId = type.Id,
            SerialNumber = Faker.Random.AlphaNumeric(10),
            CreatedDateTime = Faker.Date.Past(),
            UpdatedDateTime = Faker.Date.Recent(),
            GeneralStatus = Faker.PickRandom<ExampleEntityStatus>(),
            // Navigation properties will be populated by EF Core if includes are correct
        };
        _dbContext.ExampleEntities.Add(dashboard);
        _dbContext.SaveChanges();

        // Manually load related entities if using InMemory and not relying on lazy loading or complex Include setup in test
        _dbContext.Entry(dashboard).Reference(d => d.Type).Load();

        return dashboard; // Return the entity with its generated ID
    }


    [Fact]
    public async Task ExecuteHandlerLogic_WhenExampleEntityExists_ReturnsSuccessResultWithData()
    {
        // Arrange
        var seededExampleEntity = SeedExampleEntity(1);
        var request = new Request { Id = seededExampleEntity.Id };

        // Act
        var result = await _handler.ExecuteHandlerLogic(request, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Data.Should().NotBeNull();
        result.Value.Data.Id.Should().Be(seededExampleEntity.Id);
        result.Value.Data.SerialNumber.Should().Be(seededExampleEntity.SerialNumber);
    }

    [Fact]
    public async Task ExecuteHandlerLogic_WhenExampleEntityDoesNotExist_ReturnsFailResultWithNotFoundError()
    {
        // Arrange
        var nonExistentId = Faker.Random.Int(999, 10000);
        var request = new Request { Id = nonExistentId };

        // Act
        var result = await _handler.ExecuteHandlerLogic(request, CancellationToken.None);

        // Assert
        result.Errors.Should()
            .ContainSingle(e => e is NotFoundError && e.Message.Contains($"ExampleEntity {nonExistentId}"));
    }

    [Fact(Explicit = true)]
    public async Task ExecuteHandlerLogic_WhenExceptionOccurs_ReturnsFailResult()
    {
        // Arrange
        // To simulate an exception, we can dispose the DbContext before the handler uses it,
        // or mock the DbContext to throw an exception on a specific call.
        // For simplicity with InMemory, let's test the general exception path of BaseHandler.
        // The provided handler's ExecuteHandlerLogic catches general Exception.
        var request = new Request { Id = 1 };

        // Create a new handler with a DbContext that will throw
        var mockDbContext =
            new Mock<ExampleModuleDbContext>(
                new DbContextOptionsBuilder<ExampleModuleDbContext>().Options);
        // to simulate mocking, just remove sealed from the DbContext class and add virtual to the DbSet properties
        mockDbContext.Setup(db => db.ExampleEntities).Throws(new InvalidOperationException("DB error simulation"));

        var handlerWithMockedDb = new Handler(_mockLogger.Object, mockDbContext.Object);

        // Act
        var result = await handlerWithMockedDb.ExecuteHandlerLogic(request, CancellationToken.None);

        // Assert
        result.Errors.First().Message.Should().Be("DB error simulation"); // The handler's catch block wraps this
    }


    public override void Dispose()
    {
        _dbContext.Database.EnsureDeleted(); // Clean up InMemory database
        _dbContext.Dispose();
        base.Dispose();
    }
}