var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TodoSystem_API>("todosystem-api");

builder.Build().Run();
