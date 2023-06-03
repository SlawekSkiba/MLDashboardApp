namespace MLD.Application.Acceptance_Tests;

[Binding]
public class TrainingSessionsSteps
{
    private Mock<ILogger<TrainingSessionsService>> _loggerMock;
    private SqliteConnection _connection;
    private object _configuration;
    private DbContextOptions<AppDbContext> _dbContextOptions;
    private TrainingSessionsService _service;
    private string _githash;
    private AddTrainingSessionRequest _request;

    public TrainingSessionsSteps()
    {
        _loggerMock = new Mock<ILogger<TrainingSessionsService>>();
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();


        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "ConnectionStrings:AppDatabase", "DataSource=:memory:" }
            })
            .Build();

        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new AppDbContext(_dbContextOptions);
        context.Database.EnsureCreated();
        //context.Database.Migrate();

        // Arrange      

        _service = new TrainingSessionsService(context, _loggerMock.Object);

    }
        
    [Given(@"I want to register new session with name (.+)")]
    public async Task RegisterSessionWithName(string sessionName)
    {                
        _request = new AddTrainingSessionRequest(
            Name: sessionName,
            Description: "New session description",
            GitHash: "abc123"
        );                
    }

    [When(@"GitHash is (.+)")]
    public async Task SetGitHash(string githash)
    {
        _githash = githash;
        _request = _request with { GitHash = githash };
    }

    [Then("Session should be registered with name (.+)")]
    public async Task CheckIfSessionIsRegisteredWithTheName(string sessionName)
    {
        var result = await _service.AddTrainingSession(_request);        

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.AsT0.Should().BeOfType<TrainingSessionDto>();
            result.AsT0.Name.Should().Be(sessionName);
            result.AsT0.GitHash.Should().Be(_githash);
            
        }
    }

}