using to_dogether_api.Models;

namespace to_dogether_api.DTOs;

public record CreateTodoListRequest(string Title, string Description);
public record UpdateTodoListRequest(string Title, string Description);
public record TodoListResponse(int Id, string Title, string Description, int OwnerId, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateTodoItemRequest(string Title, string Description, TodoSeverity Severity);
public record UpdateTodoItemRequest(string Title, string Description, TodoStatus Status, TodoSeverity Severity, int Order);
public record TodoItemResponse(int Id, string Title, string Description, TodoStatus Status, TodoSeverity Severity, int Order, DateTime CreatedAt, DateTime UpdatedAt);

public record UserResponse(int Id, string Username, string ColorCode, DateTime CreatedAt);

public enum TodoStatus
{
    Pending,
    Done
}

public enum TodoSeverity
{
    Low,
    Medium,
    High
} 