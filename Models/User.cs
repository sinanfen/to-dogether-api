namespace to_dogether_api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ColorCode { get; set; } = "#3B82F6"; // Default mavi
    public int? CoupleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<TodoList> TodoLists { get; set; } = new List<TodoList>();
    public Couple? Couple { get; set; }
} 