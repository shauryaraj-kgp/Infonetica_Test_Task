using WorkflowEngine.Models;
using WorkflowEngine.Storage;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRepository<WorkflowDefinition>, InMemoryRepository<WorkflowDefinition>>();
builder.Services.AddSingleton<IRepository<WorkflowInstance>, InMemoryRepository<WorkflowInstance>>();
builder.Services.AddSingleton<WorkflowService>();

var app = builder.Build();

app.MapPost("/workflow-definitions", (WorkflowDefinition def, WorkflowService service, IRepository<WorkflowDefinition> repo) => {
    if (!service.IsValidDefinition(def, out var error))
        return Results.BadRequest(error);
    repo.Add(def);
    return Results.Ok(def);
});

app.MapGet("/workflow-definitions/{id}", (string id, IRepository<WorkflowDefinition> repo) =>
    repo.Get(id) is { } def ? Results.Ok(def) : Results.NotFound());

app.MapGet("/workflow-definitions", (IRepository<WorkflowDefinition> repo) =>
    Results.Ok(repo.GetAll()));

app.MapPost("/workflow-instances", (string definitionId, WorkflowService service) => {
    try {
        var instance = service.StartInstance(definitionId);
        return Results.Ok(instance);
    } catch (Exception e) {
        return Results.BadRequest(e.Message);
    }
});

app.MapPost("/workflow-instances/{id}/actions/{actionId}", (string id, string actionId, WorkflowService service) => {
    try {
        var updated = service.ExecuteAction(id, actionId);
        return Results.Ok(updated);
    } catch (Exception e) {
        return Results.BadRequest(e.Message);
    }
});

app.MapGet("/workflow-instances/{id}", (string id, IRepository<WorkflowInstance> repo) =>
    repo.Get(id) is { } inst ? Results.Ok(inst) : Results.NotFound());

app.MapGet("/workflow-instances", (IRepository<WorkflowInstance> repo) =>
    Results.Ok(repo.GetAll()));

app.MapGet("/workflow-definitions/{id}/states", (string id, IRepository<WorkflowDefinition> repo) => {
    var def = repo.Get(id);
    return def is null ? Results.NotFound() : Results.Ok(def.States);
});

app.MapGet("/workflow-definitions/{id}/actions", (string id, IRepository<WorkflowDefinition> repo) => {
    var def = repo.Get(id);
    return def is null ? Results.NotFound() : Results.Ok(def.Actions);
});

app.Run();
