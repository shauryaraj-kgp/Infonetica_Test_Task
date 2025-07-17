namespace WorkflowEngine.Models
{
    public class Action {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool Enabled { get; set; } = true;
        public List<string> FromStates { get; set; } = new();
        public string ToState { get; set; } = null!;
    }
} 