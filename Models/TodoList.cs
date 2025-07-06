namespace to_dogether_api.Models;

public class TodoList
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
} 