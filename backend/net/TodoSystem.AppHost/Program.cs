var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL as a resource using AddContainer
var postgres = builder.AddContainer("postgres", "postgres:16")
    .WithEnvironment("POSTGRES_PASSWORD", "postgres")
    .WithPort(5432, 5432);

// Add the API project with database connection
var api = builder.AddProject<Projects.TodoSystem_API>("api")
    .WithReference(postgres);

// Add the dashboard for monitoring
builder.AddAspireDashboard();

// Build and run the application
await builder.BuildAsync().RunAsync();
