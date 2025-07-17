namespace WorkflowEngine.Models
{
    public class State {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsInitial { get; set; }
        public bool IsFinal { get; set; }
        public bool Enabled { get; set; } = true;
    }

    public class Action {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool Enabled { get; set; } = true;
        public List<string> FromStates { get; set; } = new();
        public string ToState { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
    }

    public class WorkflowDefinition {
        public string Id { get; set; } = null!;
        public List<State> States { get; set; } = new();
        public List<Action> Actions { get; set; } = new();
    }

    public class WorkflowInstance {
        public string Id { get; set; } = null!;
        public string DefinitionId { get; set; } = null!;
        public string CurrentState { get; set; } = null!;
        public List<HistoryEntry> History { get; set; } = new();
    }

    public class HistoryEntry {
        public string ActionId { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
} 