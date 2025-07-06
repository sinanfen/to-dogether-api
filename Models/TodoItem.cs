using to_dogether_api.DTOs;

namespace to_dogether_api.Models;

public class TodoItem
{
    public int Id { get; set; }
    public int TodoListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TodoStatus Status { get; set; } = TodoStatus.Pending;
    public TodoSeverity Severity { get; set; } = TodoSeverity.Medium;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public TodoList TodoList { get; set; } = null!;
}

 