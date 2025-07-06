namespace to_dogether_api.Models;

public class Couple
{
    public int Id { get; set; }
    public string InviteToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
} 