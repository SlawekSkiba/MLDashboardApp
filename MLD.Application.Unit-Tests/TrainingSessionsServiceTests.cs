using Microsoft.EntityFrameworkCore;
using MLD.Application.TrainingSessions.Models;
using MLD.Application.TrainingSessions.Services;

using MLD.Persistence;
using MLD.Application.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Data.Sqlite;

namespace MLD.TrainingSession.Unit_Tests;

public class TrainingSessionsServiceTests : IDisposable
{
    private readonly Mock<ILogger<TrainingSessionsService>> _loggerMock;
    private readonly SqliteConnection _connection;
    private readonly IConfiguration _configuration;
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public TrainingSessionsServiceTests()
    {
        _loggerMock = new Mock<ILogger<TrainingSessionsService>>();
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();


        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "ConnectionStrings:AppDatabase", "DataSource=:memory:" }
            })
            //.AddJsonFile("appSettings.json")
            .Build();

        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(_dbContextOptions);
        context.Database.EnsureCreated();
        //context.Database.Migrate();
    }

    [Fact]
    public async Task GetTrainingSessionsAsync_ShouldReturnTrainingSessionDtos()
    {
        // Arrange, _configuration
        using var context = new AppDbContext(_dbContextOptions);        

        var service = new TrainingSessionsService(context, _loggerMock.Object);
        await SeedTestDataAsync(context);

        // Act
        var result = await service.GetTrainingSessionsAsync();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(dto => dto is TrainingSessionDto);
        }
    }

    [Fact]
    public async Task AddTrainingSession_ShouldReturnTrainingSessionDto()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);

        var service = new TrainingSessionsService(context, _loggerMock.Object);
        var request = new AddTrainingSessionRequest(
            Name: "New Session",
            Description: "New session description",
            GitHash: "abc123"
        );

        // Act
        var result = await service.AddTrainingSession(request);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.IsT0.Should().BeTrue();
            result.AsT0.Name.Should().Be(request.Name);
            result.AsT0.Description.Should().Be(request.Description);
            result.AsT0.GitHash.Should().Be(request.GitHash);
        }
    }

    [Fact]
    public async Task UpdateAddTrainingSession_ShouldReturnTrainingSessionDto()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);

        var service = new TrainingSessionsService(context, _loggerMock.Object);
        var session = new MLD.Persistence.Entities.TrainingSession
        {
            Name = "Existing Session",
            Description = "Existing session description",
            GitHash = "abc123",
            Iteration = 10,
            Epoch = 100,
            Loss = 0.5f
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();
        var request = new UpdateTrainingSessionRequest
        (
            Name: "Updated Session",
            Description: "Updated session description",
            GitHash: "def456",
            Iteration: 20,
            Epoch: 200,
            Loss: 0.7f
        );

        // Act
        var result = await service.UpdateAddTrainingSession(session.Id, request);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.AsT0.Should().BeOfType<TrainingSessionDto>();
            result.AsT0.Name.Should().Be(request.Name);
            result.AsT0.Description.Should().Be(request.Description);
            result.AsT0.GitHash.Should().Be(request.GitHash);
            result.AsT0.Iteration.Should().Be(request.Iteration);
            result.AsT0.Epoch.Should().Be(request.Epoch);
            result.AsT0.Loss.Should().Be(request.Loss);
        }
    }

    [Fact]
    public async Task UpdateAddTrainingSession_ShouldReturnNotFoundForNonExistingSession()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);

        var service = new TrainingSessionsService(context, _loggerMock.Object);
        var request = new UpdateTrainingSessionRequest
        (
            Name: "Updated Session",
            Description: "Updated session description",
            GitHash: "def456",
            Iteration: 20,
            Epoch: 200,
            Loss: 0.7f
        );

        // Act
        var result = await service.UpdateAddTrainingSession("nonexisting-id", request);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Should().BeOfType<NotFound>();
    }

    [Fact]
    public async Task DeleteTrainingSession_ShouldReturnDeletedCount()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);

        var service = new TrainingSessionsService(context, _loggerMock.Object);
        var session = new MLD.Persistence.Entities.TrainingSession
        {
            Name = "Session to delete",
            Description = "Session to delete description",
            GitHash = "abc123"
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteTrainingSession(session.Id);

        // Assert
        result.AsT0.Should().Be(1);
    }

    [Fact]
    public async Task DeleteTrainingSession_ShouldReturnErrorForNonExistingSession()
    {
        // Arrange
        using var context = new AppDbContext(_dbContextOptions);

        var service = new TrainingSessionsService(context, _loggerMock.Object);

        // Act
        var result = await service.DeleteTrainingSession("nonexisting-id");

        // Assert
        result.AsT0.Should().Be(0);
    }

    // Helper method to seed test data
    private async Task SeedTestDataAsync(AppDbContext context)
    {
        var session1 = new MLD.Persistence.Entities.TrainingSession
        {
            Name = "Session 1",
            Description = "Description 1",
            GitHash = "hash1"
        };

        var session2 = new MLD.Persistence.Entities.TrainingSession
        {
            Name = "Session 2",
            Description = "Description 2",
            GitHash = "hash2"
        };

        context.Sessions.AddRange(session1, session2);
        await context.SaveChangesAsync();
    }

    // Dispose the resources after all the tests have run
    public void Dispose()
    {

    }
}
