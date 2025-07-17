using WorkflowEngine.Models;
using WorkflowEngine.Storage;
using System;
using System.Linq;

namespace WorkflowEngine.Services
{
    public class WorkflowService {
        private readonly IRepository<WorkflowDefinition> _definitions;
        private readonly IRepository<WorkflowInstance> _instances;

        public WorkflowService(IRepository<WorkflowDefinition> definitions, IRepository<WorkflowInstance> instances) {
            _definitions = definitions;
            _instances = instances;
        }

        public bool CreateWorkflowDefinition(WorkflowDefinition def, out string error) {
            if (string.IsNullOrWhiteSpace(def.Id))
                def.Id = Guid.NewGuid().ToString();

            if (_definitions.Get(def.Id) != null) {
                error = $"Workflow with id '{def.Id}' already exists.";
                return false;
            }

            if (!IsValidDefinition(def, out error))
                return false;

            _definitions.Add(def);
            error = string.Empty;
            return true;
        }

        public bool IsValidDefinition(WorkflowDefinition def, out string error) {
            var stateIds = def.States.Select(s => s.Id).ToHashSet();
            if (def.States.Count < 2) {
                error = "Workflow must have at least two states.";
                return false;
            }
            if (def.Actions.Count < 1) {
                error = "Workflow must have at least one action.";
                return false;
            }
            if (def.States.Count(s => s.IsInitial) != 1) {
                error = "Workflow must have exactly one initial state.";
                return false;
            }
            if (def.States.GroupBy(s => s.Id).Any(g => g.Count() > 1)) {
                error = "Duplicate state IDs found.";
                return false;
            }
            if (def.Actions.Any(a => !stateIds.Contains(a.ToState) || a.FromStates.Any(f => !stateIds.Contains(f)))) {
                error = "Action refers to unknown state.";
                return false;
            }
            error = "";
            return true;
        }

        public WorkflowInstance StartInstance(string definitionId) {
            var def = _definitions.Get(definitionId) ?? throw new Exception("Definition not found.");
            var init = def.States.First(s => s.IsInitial);
            var instance = new WorkflowInstance {
                Id = Guid.NewGuid().ToString(),
                DefinitionId = definitionId,
                CurrentState = init.Id
            };
            _instances.Add(instance);
            return instance;
        }

        public WorkflowInstance ExecuteAction(string instanceId, string actionId) {
            var instance = _instances.Get(instanceId) ?? throw new Exception("Instance not found.");
            var def = _definitions.Get(instance.DefinitionId)!;
            var action = def.Actions.FirstOrDefault(a => a.Id == actionId)
                ?? throw new Exception("Action not found in workflow definition.");

            if (!action.Enabled)
                throw new Exception("Action is disabled.");

            if (!action.FromStates.Contains(instance.CurrentState))
                throw new Exception("Invalid transition: current state not in action's source states.");

            var current = def.States.First(s => s.Id == instance.CurrentState);
            if (current.IsFinal)
                throw new Exception("No actions allowed from final state.");

            instance.CurrentState = action.ToState;
            instance.History.Add(new HistoryEntry {
                ActionId = action.Id,
                Timestamp = DateTime.UtcNow
            });

            return instance;
        }

        public bool AddStateToWorkflow(string workflowId, State state, out string error) {
            var def = _definitions.Get(workflowId);
            if (def == null) {
                error = "Workflow not found.";
                return false;
            }
            if (def.States.Any(s => s.Id == state.Id)) {
                error = "State with this ID already exists.";
                return false;
            }
            if (state.IsInitial) {
                error = "There is already an initial state in this workflow. Only one initial state is allowed.";
                return false;
            }
            def.States.Add(state);
            if (!IsValidDefinition(def, out error)) {
                def.States.Remove(state);
                return false;
            }
            _definitions.Add(def);
            return true;
        }

        public bool AddActionToWorkflow(string workflowId, WorkflowEngine.Models.Action action, out string error) {
            var def = _definitions.Get(workflowId);
            if (def == null) {
                error = "Workflow not found.";
                return false;
            }
            if (def.Actions.Any(a => a.Id == action.Id)) {
                error = "Action with this ID already exists.";
                return false;
            }
            def.Actions.Add(action);
            if (!IsValidDefinition(def, out error)) {
                def.Actions.Remove(action);
                return false;
            }
            _definitions.Add(def);
            return true;
        }
    }
} 