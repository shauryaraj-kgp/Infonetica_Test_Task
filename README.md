# Workflow Engine – Infonetica Assignment

## 🔧 Quick Start

1. **Build the project (optional):**
   ```bash
   dotnet build
   ```
2. **Run the API:**
   ```bash
   dotnet run
   ```
   The API will start and listen on:  
   `http://localhost:5000`

## API Routes

* `POST /workflow-definitions` – Create workflow
* `GET /workflow-definitions` – List all workflows
* `GET /workflow-definitions/{id}` – Get workflow
* `GET /workflow-definitions/{id}/states` – List states for a workflow
* `GET /workflow-definitions/{id}/actions` – List actions for a workflow
* `POST /workflow-definitions/{id}/states` – Add a state to an existing workflow
* `POST /workflow-definitions/{id}/actions` – Add an action to an existing workflow
* `POST /workflow-instances?definitionId={id}` – Start new instance
* `GET /workflow-instances` – List all instances
* `GET /workflow-instances/{id}` – Get instance status + history
* `POST /workflow-instances/{id}/actions/{actionId}` – Execute action

## Sample Payloads

### Create Workflow Definition
```json
{
  "id": "work1",
  "states": [
    { "id": "draft", "name": "Draft", "isInitial": true, "isFinal": false, "enabled": true, "description": "Initial State"},
    { "id": "review", "name": "Review", "isInitial": false, "isFinal": false, "enabled": true },
    { "id": "approved", "name": "Approved", "isInitial": false, "isFinal": true, "enabled": true }
  ],
  "actions": [
    { "id": "submit", "name": "Submit for Review", "enabled": true, "fromStates": ["draft"], "toState": "review" },
    { "id": "approve", "name": "Approve", "enabled": true, "fromStates": ["review"], "toState": "approved" }
  ]
}
```

### Start Workflow Instance
`POST /workflow-instances?definitionId=work1`

### Execute Action
`POST /workflow-instances/{instanceId}/actions/submit`

### State and Action with Description
```json
{
  "id": "draft",
  "name": "Draft",
  "description": "This is the draft state.",
  "isInitial": true,
  "isFinal": false,
  "enabled": true
}
```

```json
{
  "id": "submit",
  "name": "Submit for Review",
  "description": "Submit the document for review.",
  "enabled": true,
  "fromStates": ["draft"],
  "toState": "review"
}
```

### Add State to Workflow
`POST /workflow-definitions/{id}/states`

Body:
```json
{
  "id": "newstate",
  "name": "New State",
  "description": "A new state added to the workflow.",
  "isInitial": false,
  "isFinal": false,
  "enabled": true
}
```

### Add Action to Workflow
`POST /workflow-definitions/{id}/actions`

Body:
```json
{
  "id": "newaction",
  "name": "New Action",
  "description": "A new action added to the workflow.",
  "enabled": true,
  "fromStates": ["someStateId"],
  "toState": "anotherStateId"
}
```

## Notes & Assumptions

- **All data is in-memory only.** When the app stops, all workflows and instances are lost.
- **No database, no file persistence, no extra config.**
- **No authentication or authorization.**
- **No bin/**, **obj/**, or **Properties/** folders are needed in source control.
- **Runs on port 5000** by default (see console output for confirmation).
- **Test with curl, Postman, or any HTTP client.**

## Design Notes
- The codebase is as minimal as possible, with only the files needed for a working, in-memory workflow engine.
- All business logic is in `WorkflowService`.
- Models, storage, and endpoints are clearly separated for maintainability.

