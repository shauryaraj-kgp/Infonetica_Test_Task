using WorkflowEngine.Models;
using WorkflowEngine.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkflowEngine.Services
{
    public class WorkflowService
    {
        // Fields
        private readonly IRepository<WorkflowDefinition> _definitions;
        private readonly IRepository<WorkflowInstance> _instances;

        // Constructor
        public WorkflowService(IRepository<WorkflowDefinition> definitions, IRepository<WorkflowInstance> instances)
        {
            _definitions = definitions;
            _instances = instances;
        }

        // Creates a new workflow definition. Generates an ID if not provided and checks for duplicates.
        public void CreateWorkflowDefinition(WorkflowDefinition def)
        {
            if (string.IsNullOrWhiteSpace(def.Id))
                def.Id = Guid.NewGuid().ToString();

            if (_definitions.Get(def.Id) != null)
                throw new InvalidOperationException("Workflow with id " + def.Id + " already exists");

            IsValidDefinition(def);
            _definitions.Add(def);
        }

        // Validates a workflow definition for required states, actions, and structure. Throws if invalid.
        public void IsValidDefinition(WorkflowDefinition def)
        {
            var stateIds = def.States.Select(s => s.Id).ToHashSet();
            if (def.States.Count < 2)
                throw new InvalidOperationException("Workflow must have at least two states");
            if (def.Actions.Count < 1)
                throw new InvalidOperationException("Workflow must have at least one action");
            if (def.States.Count(s => s.IsInitial) != 1)
                throw new InvalidOperationException("Workflow must have exactly one initial state");
            if (def.States.GroupBy(s => s.Id).Any(g => g.Count() > 1))
                throw new InvalidOperationException("Duplicate state IDs found");
            if (def.Actions.Any(a => !stateIds.Contains(a.ToState) || a.FromStates.Any(f => !stateIds.Contains(f))))
                throw new InvalidOperationException("Action refers to unknown state");
        }

        // Starts a new workflow instance for a given workflow definition ID.
        public WorkflowInstance StartInstance(string definitionId)
        {
            var def = _definitions.Get(definitionId) ?? throw new InvalidOperationException("Definition not found");
            var init = def.States.First(s => s.IsInitial);
            var instance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                DefinitionId = definitionId,
                CurrentState = init.Id
            };
            _instances.Add(instance);
            return instance;
        }

        // Executes an action on a workflow instance, updating its state and history.
        public WorkflowInstance ExecuteAction(string instanceId, string actionId)
        {
            var instance = _instances.Get(instanceId) ?? throw new InvalidOperationException("Instance not found");
            var def = _definitions.Get(instance.DefinitionId)!;
            var action = def.Actions.FirstOrDefault(a => a.Id == actionId)
                ?? throw new InvalidOperationException("Action not found in workflow definition");

            if (!action.Enabled)
                throw new InvalidOperationException("Action is disabled");

            if (!action.FromStates.Contains(instance.CurrentState))
                throw new InvalidOperationException("Invalid transition: current state not in action's source states");

            var current = def.States.First(s => s.Id == instance.CurrentState);
            if (current.IsFinal)
                throw new InvalidOperationException("No actions allowed from final state");

            instance.CurrentState = action.ToState;
            instance.History.Add(new HistoryEntry
            {
                ActionId = action.Id,
                Timestamp = DateTime.UtcNow
            });

            return instance;
        }

        // Adds a new state to an existing workflow definition.
        public void AddStateToWorkflow(string workflowId, State state)
        {
            var def = _definitions.Get(workflowId);
            if (def == null)
                throw new InvalidOperationException("Workflow not found");
            if (def.States.Any(s => s.Id == state.Id))
                throw new InvalidOperationException("State with this ID already exists");
            if (state.IsInitial)
                throw new InvalidOperationException("There is already an initial state in this workflow");
            def.States.Add(state);
            try {
                IsValidDefinition(def);
            } catch {
                def.States.Remove(state);
                throw;
            }
            _definitions.Add(def);
        }

        // Adds a new action to an existing workflow definition.
        public void AddActionToWorkflow(string workflowId, WorkflowEngine.Models.Action action)
        {
            var def = _definitions.Get(workflowId);
            if (def == null)
                throw new InvalidOperationException("Workflow not found");
            if (def.Actions.Any(a => a.Id == action.Id))
                throw new InvalidOperationException("Action with this ID already exists");
            def.Actions.Add(action);
            try {
                IsValidDefinition(def);
            } catch {
                def.Actions.Remove(action);
                throw;
            }
            _definitions.Add(def);
        }
    }
} 