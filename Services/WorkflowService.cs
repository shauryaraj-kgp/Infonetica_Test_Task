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

        public bool IsValidDefinition(WorkflowDefinition def, out string error) {
            var stateIds = def.States.Select(s => s.Id).ToHashSet();
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
    }
} 