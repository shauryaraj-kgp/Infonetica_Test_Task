# Workflow Engine – Infonetica Assignment

## Quick Start

1. **Clone the repository:**
   ```bash
   git clone https://github.com/shauryaraj-kgp/Infonetica_Test_Task.git
   cd Infonetica_Test_Task
   ```
2. **Build:**
   ```bash
   dotnet build
   ```
3. **Run:**
   ```bash
   dotnet run
   ```
   Default API URL: `http://localhost:5000`

---

## API Reference

### Workflow Definitions
- **POST `/workflow-definitions`**
  - Create a new workflow definition. If `id` is omitted, one is auto-generated. Returns the created workflow or error.
  - Body: WorkflowDefinition JSON (see example below)
- **GET `/workflow-definitions`**
  - List all workflow definitions.
- **GET `/workflow-definitions/{id}`**
  - Get a workflow definition by ID.
- **GET `/workflow-definitions/{id}/states`**
  - List all states for a workflow definition.
- **GET `/workflow-definitions/{id}/actions`**
  - List all actions for a workflow definition.
- **POST `/workflow-definitions/{id}/states`**
  - Add a state to an existing workflow definition. Returns the added state or error.
  - Body: State JSON
- **POST `/workflow-definitions/{id}/actions`**
  - Add an action to an existing workflow definition. Returns the added action or error.
  - Body: Action JSON

### Workflow Instances
- **POST `/workflow-instances?definitionId={id}`**
  - Start a new workflow instance for the given workflow definition ID. Returns the new instance or error.
- **GET `/workflow-instances`**
  - List all workflow instances.
- **GET `/workflow-instances/{id}`**
  - Get a workflow instance by ID, including its current state and history.
- **POST `/workflow-instances/{id}/actions/{actionId}`**
  - Execute an action on a workflow instance. Returns the updated instance or error.

---

## Example Workflow: Pizza Order

A sample workflow for tracking a pizza order lifecycle.

### States
- `ordered` (initial)
- `preparing`
- `baking`
- `out_for_delivery`
- `delivered` (final)

### Actions
- `start_preparing`: `ordered` → `preparing`
- `start_baking`: `preparing` → `baking`
- `dispatch`: `baking` → `out_for_delivery`
- `deliver`: `out_for_delivery` → `delivered`

### Create Workflow Definition
```json
{
  "states": [
    { "id": "ordered", "name": "Ordered", "isInitial": true, "isFinal": false, "enabled": true, "description": "Order placed" },
    { "id": "preparing", "name": "Preparing", "isInitial": false, "isFinal": false, "enabled": true, "description": "Preparing pizza" },
    { "id": "baking", "name": "Baking", "isInitial": false, "isFinal": false, "enabled": true, "description": "In the oven" },
    { "id": "out_for_delivery", "name": "Out for Delivery", "isInitial": false, "isFinal": false, "enabled": true, "description": "On the way" },
    { "id": "delivered", "name": "Delivered", "isInitial": false, "isFinal": true, "enabled": true, "description": "Delivered to customer" }
  ],
  "actions": [
    { "id": "start_preparing", "name": "Start Preparing", "enabled": true, "fromStates": ["ordered"], "toState": "preparing", "description": "Begin prep" },
    { "id": "start_baking", "name": "Start Baking", "enabled": true, "fromStates": ["preparing"], "toState": "baking", "description": "Bake pizza" },
    { "id": "dispatch", "name": "Dispatch", "enabled": true, "fromStates": ["baking"], "toState": "out_for_delivery", "description": "Send out" },
    { "id": "deliver", "name": "Deliver", "enabled": true, "fromStates": ["out_for_delivery"], "toState": "delivered", "description": "Complete delivery" }
  ]
}
```

### API Usage Example
- `POST /workflow-definitions` — Create workflow
- `POST /workflow-instances?definitionId={id}` — Start instance
- `POST /workflow-instances/{instanceId}/actions/start_preparing` — Move to "preparing"
- `POST /workflow-instances/{instanceId}/actions/start_baking` — Move to "baking"
- `POST /workflow-instances/{instanceId}/actions/dispatch` — Move to "out_for_delivery"
- `POST /workflow-instances/{instanceId}/actions/deliver` — Move to "delivered"
- `GET /workflow-instances/{instanceId}` — Get instance status/history

---

## Notes
- All data is in-memory; restarting the app clears all workflows and instances.
- No authentication or external storage.
- Runs on port 5000 by default.
- Use any HTTP client (curl, Postman, etc.).

## Design
- Minimal, in-memory workflow engine.
- Core logic in `WorkflowService`.
- Models, storage, and endpoints are separated for clarity.

