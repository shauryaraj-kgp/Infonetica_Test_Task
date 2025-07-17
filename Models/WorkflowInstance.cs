namespace WorkflowEngine.Models
{
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