using to_dogether_api.Models;

namespace to_dogether_api.DTOs;

public record CreateTodoListRequest(string Title, string Description, bool IsShared = false, string ColorCode = "#3B82F6");
public record UpdateTodoListRequest(string Title, string Description, bool IsShared, string ColorCode);
public record TodoListResponse(int Id, string Title, string Description, int OwnerId, bool IsShared, string ColorCode, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateTodoItemRequest(string Title, string? Description, TodoSeverity Severity);
public record UpdateTodoItemRequest(string Title, string? Description, TodoStatus Status, TodoSeverity Severity, int Order);
public record TodoItemResponse(int Id, string Title, string? Description, TodoStatus Status, TodoSeverity Severity, int Order, DateTime CreatedAt, DateTime UpdatedAt);

public record UserResponse(int Id, string Username, string ColorCode, DateTime CreatedAt);

// Partner overview için yeni DTO'lar
public record TodoListWithItemsResponse(
    int Id, 
    string Title, 
    string Description, 
    int OwnerId, 
    DateTime CreatedAt, 
    DateTime UpdatedAt, 
    List<TodoItemResponse> Items);

public record PartnerOverviewResponse(
    int Id, 
    string Username, 
    string ColorCode, 
    DateTime CreatedAt, 
    List<TodoListWithItemsResponse> TodoLists);

// Dashboard istatistikleri için DTO
public record CoupleDashboardStatsResponse(
    int TotalTasks,
    int CompletedToday, 
    int PendingTasks,
    int HighPriorityTasks,
    int MyTasks,
    int PartnerTasks,
    string? PartnerUsername);

// Activity DTOs
public record ActivityResponse(
    int Id,
    int UserId,
    string Username,
    string UserColorCode,
    ActivityType ActivityType,
    EntityType EntityType,
    int EntityId,
    string EntityTitle,
    string Message,
    DateTime CreatedAt);

public record RecentActivitiesResponse(
    List<ActivityResponse> Activities,
    int TotalCount);

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