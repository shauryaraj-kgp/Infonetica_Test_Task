using WorkflowEngine.Models;
using WorkflowEngine.Storage;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<IRepository<WorkflowDefinition>, InMemoryRepository<WorkflowDefinition>>();
builder.Services.AddSingleton<IRepository<WorkflowInstance>, InMemoryRepository<WorkflowInstance>>();
builder.Services.AddSingleton<WorkflowService>();

var app = builder.Build();

// Create a workflow definition
app.MapPost("/workflow-definitions", (WorkflowDefinition def, WorkflowService service) => {
    try {
        service.CreateWorkflowDefinition(def);
        return Results.Ok(def);
    } catch (Exception e) {
        return Results.BadRequest(e.Message);
    }
});

// Get a workflow definition by id
app.MapGet("/workflow-definitions/{id}", (string id, IRepository<WorkflowDefinition> repo) => {
    var def = repo.Get(id);
    if (def == null) return Results.NotFound();
    return Results.Ok(def);
});

// Get all workflow definitions
app.MapGet("/workflow-definitions", (IRepository<WorkflowDefinition> repo) => {
    return Results.Ok(repo.GetAll());
});

// Start a new workflow instance
app.MapPost("/workflow-instances", (string definitionId, WorkflowService service) => {
    try {
        var instance = service.StartInstance(definitionId);
        return Results.Ok(instance);
    } catch (Exception e) {
        return Results.BadRequest(e.Message);
    }
});

// Execute an action on a workflow instance
app.MapPost("/workflow-instances/{id}/actions/{actionId}", (string id, string actionId, WorkflowService service) => {
    try {
        var updated = service.ExecuteAction(id, actionId);
        return Results.Ok(updated);
    } catch (Exception e) {
        return Results.BadRequest(e.Message);
    }
});

// Get a workflow instance by id
app.MapGet("/workflow-instances/{id}", (string id, IRepository<WorkflowInstance> repo) => {
    var inst = repo.Get(id);
    if (inst == null) return Results.NotFound();
    return Results.Ok(inst);
});

// Get all workflow instances
app.MapGet("/workflow-instances", (IRepository<WorkflowInstance> repo) => {
    return Results.Ok(repo.GetAll());
});

// Get all states for a workflow definition
app.MapGet("/workflow-definitions/{id}/states", (string id, IRepository<WorkflowDefinition> repo) => {
    var def = repo.Get(id);
    if (def == null) return Results.NotFound();
    return Results.Ok(def.States);
});

// Get all actions for a workflow definition
app.MapGet("/workflow-definitions/{id}/actions", (string id, IRepository<WorkflowDefinition> repo) => {
    var def = repo.Get(id);
    if (def == null) return Results.NotFound();
    return Results.Ok(def.Actions);
});

// Add a state to a workflow definition
app.MapPost("/workflow-definitions/{id}/states", (string id, State state, WorkflowService service) => {
    try {
        service.AddStateToWorkflow(id, state);
        return Results.Ok(state);
    } catch (Exception e) {
        return Results.BadRequest(e.Message);
    }
});

// Add an action to a workflow definition
app.MapPost("/workflow-definitions/{id}/actions", (string id, WorkflowEngine.Models.Action action, WorkflowService service) => {
    try {
        service.AddActionToWorkflow(id, action);
        return Results.Ok(action);
    } catch (Exception e) {
        return Results.BadRequest(e.Message);
    }
});

app.Run();
