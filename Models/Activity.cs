using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace to_dogether_api.Models;

public class Activity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public ActivityType ActivityType { get; set; }

    [Required]
    public EntityType EntityType { get; set; }

    [Required]
    public int EntityId { get; set; }

    [Required]
    [MaxLength(200)]
    public string EntityTitle { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}

public enum ActivityType
{
    Created,
    Updated,
    Deleted,
    Completed,
    Reopened,
    ItemAdded,
    ItemUpdated,
    ItemDeleted,
    ItemCompleted,
    ItemReopened
}

public enum EntityType
{
    TodoList,
    TodoItem
} 