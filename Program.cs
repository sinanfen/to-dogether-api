using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using to_dogether_api.Data;
using to_dogether_api.DTOs;
using to_dogether_api.Middleware;
using to_dogether_api.Models;
using to_dogether_api.Services;
using Serilog;
using TodoStatus = to_dogether_api.DTOs.TodoStatus;
using TodoSeverity = to_dogether_api.DTOs.TodoSeverity;

var builder = WebApplication.CreateBuilder(args);

// Serilog konfigürasyonu
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<HashService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "To-Dogether API", Version = "v1" });
    options.UseInlineDefinitionsForEnums();

    // JWT Bearer Authentication için Swagger konfigürasyonu
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "To-Dogether API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "To-Dogether API Documentation";
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<JwtMiddleware>();

// Root URL'yi Swagger'a yönlendir
app.MapGet("/", () => Results.Redirect("/swagger"));

// Helper function to get current user ID from JWT token
int GetCurrentUserId(ClaimsPrincipal user)
{
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
    return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
}

// Helper function to check if user is authenticated
bool IsAuthenticated(ClaimsPrincipal user)
{
    return user.Identity?.IsAuthenticated == true && GetCurrentUserId(user) > 0;
}

// Auth endpoints
app.MapPost("/auth/login", async (LoginRequest request, AppDbContext db, JwtService jwtService, HashService hashService, HttpContext context) =>
{
    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    var requestPath = context.Request.Path;
    var requestMethod = context.Request.Method;

    Log.Information("Login attempt for username: {Username} from IP: {IpAddress}", request.Username, ipAddress);

    try
    {
        // Önce kullanıcıyı username'e göre bul
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            Log.Warning("Login failed - User not found: {Username} from IP: {IpAddress}", request.Username, ipAddress);
            return Results.BadRequest(new { message = "Kullanıcı bulunamadı." });
        }

        // Password kontrolü
        if (!hashService.VerifyPassword(request.Password, user.Password))
        {
            Log.Warning("Login failed - Invalid password for user: {Username} from IP: {IpAddress}", request.Username, ipAddress);
            return Results.BadRequest(new { message = "Hatalı şifre." });
        }

        // Eski refresh token'ları temizle
        var oldTokens = await db.RefreshTokens.Where(rt => rt.UserId == user.Id && rt.IsRevoked == false).ToListAsync();
        db.RefreshTokens.RemoveRange(oldTokens);

        // Yeni token'ları oluştur
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Refresh token'ı veritabanına kaydet
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMonths(6), // 6 ay
            CreatedAt = DateTime.UtcNow
        };

        db.RefreshTokens.Add(refreshTokenEntity);
        await db.SaveChangesAsync();

        Log.Information("Login successful for user: {Username} (ID: {UserId}) from IP: {IpAddress}",
            user.Username, user.Id, ipAddress);

        return Results.Ok(new AuthResponse(accessToken, refreshToken, user.Username, user.Id));
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Login error for username: {Username} from IP: {IpAddress}", request.Username, ipAddress);
        return Results.StatusCode(500);
    }
})
.WithName("Login");

app.MapPost("/auth/register", async (RegisterRequest request, AppDbContext db, JwtService jwtService, HashService hashService, HttpContext context) =>
{
    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    var requestPath = context.Request.Path;
    var requestMethod = context.Request.Method;

    Log.Information("Register attempt for username: {Username} from IP: {IpAddress}, InviteToken: {InviteToken}",
        request.Username, ipAddress, request.InviteToken ?? "None");

    try
    {
        // Kullanıcı adı zaten var mı kontrol et
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (existingUser != null)
        {
            Log.Warning("Register failed - Username already exists: {Username} from IP: {IpAddress}",
                request.Username, ipAddress);
            return Results.BadRequest("Bu kullanıcı adı zaten kullanılıyor.");
        }

        User user;
        string? inviteToken = null;

        if (string.IsNullOrEmpty(request.InviteToken))
        {
            // Yeni couple oluştur
            var couple = new Couple
            {
                InviteToken = Guid.NewGuid().ToString("N")[..16], // 16 karakterlik token
                CreatedAt = DateTime.UtcNow
            };

            db.Couples.Add(couple);
            await db.SaveChangesAsync();

            user = new User
            {
                Username = request.Username,
                Password = hashService.HashPassword(request.Password),
                ColorCode = request.ColorCode ?? "#3B82F6", // Default mavi
                CoupleId = couple.Id,
                CreatedAt = DateTime.UtcNow
            };

            inviteToken = couple.InviteToken; // İlk kullanıcıya invite token ver

            Log.Information("New couple created for user: {Username} (ID: {UserId}), Couple ID: {CoupleId}, InviteToken: {InviteToken}",
                user.Username, user.Id, couple.Id, inviteToken);
        }
        else
        {
            // Mevcut couple'a katıl
            var couple = await db.Couples.FirstOrDefaultAsync(c => c.InviteToken == request.InviteToken && c.IsActive);
            if (couple == null)
            {
                Log.Warning("Register failed - Invalid invite token: {InviteToken} for username: {Username} from IP: {IpAddress}",
                    request.InviteToken, request.Username, ipAddress);
                return Results.BadRequest("Geçersiz invite token.");
            }

            // Couple'da kaç kişi var kontrol et
            var coupleUserCount = await db.Users.CountAsync(u => u.CoupleId == couple.Id);
            if (coupleUserCount >= 2)
            {
                Log.Warning("Register failed - Couple is full: {InviteToken} for username: {Username} from IP: {IpAddress}",
                    request.InviteToken, request.Username, ipAddress);
                return Results.BadRequest("Bu couple zaten 2 kişiye sahip.");
            }

            user = new User
            {
                Username = request.Username,
                Password = hashService.HashPassword(request.Password),
                ColorCode = request.ColorCode ?? "#EF4444", // Default kırmızı
                CoupleId = couple.Id,
                CreatedAt = DateTime.UtcNow
            };

            Log.Information("User joined existing couple: {Username} (ID: {UserId}), Couple ID: {CoupleId}",
                user.Username, user.Id, couple.Id);
        }

        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Token'ları oluştur
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Refresh token'ı veritabanına kaydet
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMonths(6), // 6 ay
            CreatedAt = DateTime.UtcNow
        };

        db.RefreshTokens.Add(refreshTokenEntity);
        await db.SaveChangesAsync();

        Log.Information("Register successful for user: {Username} (ID: {UserId}) from IP: {IpAddress}",
            user.Username, user.Id, ipAddress);

        return Results.Ok(new AuthResponse(accessToken, refreshToken, user.Username, user.Id, inviteToken));
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Register error for username: {Username} from IP: {IpAddress}", request.Username, ipAddress);
        return Results.StatusCode(500);
    }
})
.WithName("Register");

app.MapPost("/auth/refresh", async (RefreshTokenRequest request, AppDbContext db, JwtService jwtService) =>
{
    // Refresh token'ı veritabanından bul
    var refreshTokenEntity = await db.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.IsRevoked == false);

    if (refreshTokenEntity == null)
        return Results.BadRequest(new { message = "Geçersiz refresh token." });

    // Token süresi dolmuş mu kontrol et
    if (refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
    {
        refreshTokenEntity.IsRevoked = true;
        await db.SaveChangesAsync();
        return Results.BadRequest(new { message = "Refresh token süresi dolmuş." });
    }

    // Kullanıcıyı bul
    var user = await db.Users.FindAsync(refreshTokenEntity.UserId);
    if (user == null)
        return Results.BadRequest(new { message = "Kullanıcı bulunamadı." });

    // Eski refresh token'ı iptal et
    refreshTokenEntity.IsRevoked = true;

    // Yeni token'ları oluştur
    var newAccessToken = jwtService.GenerateAccessToken(user);
    var newRefreshToken = jwtService.GenerateRefreshToken();

    // Yeni refresh token'ı kaydet
    var newRefreshTokenEntity = new RefreshToken
    {
        UserId = user.Id,
        Token = newRefreshToken,
        ExpiresAt = DateTime.UtcNow.AddMonths(6), // 6 ay
        CreatedAt = DateTime.UtcNow
    };

    db.RefreshTokens.Add(newRefreshTokenEntity);
    await db.SaveChangesAsync();

    return Results.Ok(new RefreshTokenResponse(newAccessToken, newRefreshToken));
})
.WithName("RefreshToken");

app.MapPost("/auth/logout", async (RefreshTokenRequest request, AppDbContext db) =>
{
    // Refresh token'ı bul ve iptal et
    var refreshTokenEntity = await db.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

    if (refreshTokenEntity != null)
    {
        refreshTokenEntity.IsRevoked = true;
        await db.SaveChangesAsync();
    }

    return Results.Ok(new { message = "Başarıyla çıkış yapıldı" });
})
.WithName("Logout");

// Protected endpoints - JWT token required
app.MapGet("/users/me", (ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.Unauthorized();

    var userId = GetCurrentUserId(user);
    var currentUser = db.Users.FirstOrDefault(u => u.Id == userId);

    if (currentUser == null)
        return Results.NotFound();

    return Results.Ok(new UserResponse(currentUser.Id, currentUser.Username, currentUser.ColorCode, currentUser.CreatedAt));
})
.WithName("GetCurrentUser");

// TodoList endpoints
app.MapGet("/todolists", async (ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);
    var todoLists = await db.TodoLists
        .Where(tl => tl.OwnerId == userId)
        .OrderBy(tl => tl.CreatedAt)
        .Select(tl => new TodoListResponse(tl.Id, tl.Title, tl.Description, tl.OwnerId, tl.CreatedAt, tl.UpdatedAt))
        .ToListAsync();

    return Results.Ok(todoLists);
})
.WithName("GetMyTodoLists");

app.MapGet("/todolists/partner", async (ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);
    var currentUser = await db.Users.Include(u => u.Couple).FirstOrDefaultAsync(u => u.Id == userId);
    
    if (currentUser?.CoupleId == null)
        return Results.BadRequest("Henüz bir couple'a ait değilsiniz.");

    var partnerTodoLists = await db.TodoLists
        .Where(tl => tl.Owner.CoupleId == currentUser.CoupleId && tl.OwnerId != userId)
        .OrderBy(tl => tl.CreatedAt)
        .Select(tl => new TodoListResponse(tl.Id, tl.Title, tl.Description, tl.OwnerId, tl.CreatedAt, tl.UpdatedAt))
        .ToListAsync();

    return Results.Ok(partnerTodoLists);
})
.WithName("GetPartnerTodoLists");

app.MapPost("/todolists", async (CreateTodoListRequest request, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);

    var todoList = new TodoList
    {
        OwnerId = userId,
        Title = request.Title,
        Description = request.Description,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.TodoLists.Add(todoList);
    await db.SaveChangesAsync();

    var response = new TodoListResponse(todoList.Id, todoList.Title, todoList.Description, todoList.OwnerId, todoList.CreatedAt, todoList.UpdatedAt);
    return Results.Created($"/todolists/{todoList.Id}", response);
})
.WithName("CreateTodoList");

app.MapPut("/todolists/{id}", async (int id, UpdateTodoListRequest request, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);
    
    // TodoList'in kullanıcıya ait olduğunu veya partner'ın olduğunu kontrol et
    var todoList = await db.TodoLists
        .Include(tl => tl.Owner)
        .FirstOrDefaultAsync(tl => tl.Id == id);
    
    if (todoList == null)
        return Results.NotFound();

    var currentUser = await db.Users.Include(u => u.Couple).FirstOrDefaultAsync(u => u.Id == userId);
    if (currentUser?.CoupleId == null || todoList.Owner.CoupleId != currentUser.CoupleId)
        return Results.BadRequest("Bu todo list'e erişim yetkiniz yok.");

    todoList.Title = request.Title;
    todoList.Description = request.Description;
    todoList.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    var response = new TodoListResponse(todoList.Id, todoList.Title, todoList.Description, todoList.OwnerId, todoList.CreatedAt, todoList.UpdatedAt);
    return Results.Ok(response);
})
.WithName("UpdateTodoList");

app.MapDelete("/todolists/{id}", async (int id, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);
    
    // TodoList'in kullanıcıya ait olduğunu veya partner'ın olduğunu kontrol et
    var todoList = await db.TodoLists
        .Include(tl => tl.Owner)
        .FirstOrDefaultAsync(tl => tl.Id == id);
    
    if (todoList == null)
        return Results.NotFound();

    var currentUser = await db.Users.Include(u => u.Couple).FirstOrDefaultAsync(u => u.Id == userId);
    if (currentUser?.CoupleId == null || todoList.Owner.CoupleId != currentUser.CoupleId)
        return Results.BadRequest("Bu todo list'e erişim yetkiniz yok.");

    db.TodoLists.Remove(todoList);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteTodoList");

// TodoItem endpoints
app.MapGet("/todolists/{todoListId}/items", async (int todoListId, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);
    
    // TodoList'in kullanıcıya ait olduğunu veya partner'ın olduğunu kontrol et
    var todoList = await db.TodoLists
        .Include(tl => tl.Owner)
        .FirstOrDefaultAsync(tl => tl.Id == todoListId);
    
    if (todoList == null)
        return Results.NotFound();

    var currentUser = await db.Users.Include(u => u.Couple).FirstOrDefaultAsync(u => u.Id == userId);
    if (currentUser?.CoupleId == null || todoList.Owner.CoupleId != currentUser.CoupleId)
        return Results.BadRequest("Bu todo list'e erişim yetkiniz yok.");

    var todoItems = await db.TodoItems
        .Where(ti => ti.TodoListId == todoListId)
        .OrderBy(ti => ti.Order)
        .Select(ti => new TodoItemResponse(ti.Id, ti.Title, ti.Description, ti.Status, ti.Severity, ti.Order, ti.CreatedAt, ti.UpdatedAt))
        .ToListAsync();

    return Results.Ok(todoItems);
})
.WithName("GetTodoItems");

app.MapPost("/todolists/{todoListId}/items", async (int todoListId, CreateTodoItemRequest request, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);
    
    // TodoList'in kullanıcıya ait olduğunu veya partner'ın olduğunu kontrol et
    var todoList = await db.TodoLists
        .Include(tl => tl.Owner)
        .FirstOrDefaultAsync(tl => tl.Id == todoListId);
    
    if (todoList == null)
        return Results.NotFound();

    var currentUser = await db.Users.Include(u => u.Couple).FirstOrDefaultAsync(u => u.Id == userId);
    if (currentUser?.CoupleId == null || todoList.Owner.CoupleId != currentUser.CoupleId)
        return Results.BadRequest("Bu todo list'e erişim yetkiniz yok.");

    // En yüksek order'ı bul
    var maxOrder = await db.TodoItems
        .Where(ti => ti.TodoListId == todoListId)
        .MaxAsync(ti => (int?)ti.Order) ?? 0;

    var todoItem = new TodoItem
    {
        TodoListId = todoListId,
        Title = request.Title,
        Description = request.Description,
        Status = TodoStatus.Pending,
        Severity = request.Severity,
        Order = maxOrder + 1,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.TodoItems.Add(todoItem);
    await db.SaveChangesAsync();

    var response = new TodoItemResponse(todoItem.Id, todoItem.Title, todoItem.Description, todoItem.Status, todoItem.Severity, todoItem.Order, todoItem.CreatedAt, todoItem.UpdatedAt);
    return Results.Created($"/todolists/{todoListId}/items/{todoItem.Id}", response);
})
.WithName("CreateTodoItem");

app.MapPut("/todolists/{todoListId}/items/{itemId}", async (int todoListId, int itemId, UpdateTodoItemRequest request, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);
    
    // TodoList'in kullanıcıya ait olduğunu veya partner'ın olduğunu kontrol et
    var todoList = await db.TodoLists
        .Include(tl => tl.Owner)
        .FirstOrDefaultAsync(tl => tl.Id == todoListId);
    
    if (todoList == null)
        return Results.NotFound();

    var currentUser = await db.Users.Include(u => u.Couple).FirstOrDefaultAsync(u => u.Id == userId);
    if (currentUser?.CoupleId == null || todoList.Owner.CoupleId != currentUser.CoupleId)
        return Results.BadRequest("Bu todo list'e erişim yetkiniz yok.");

    var todoItem = await db.TodoItems.FirstOrDefaultAsync(ti => ti.Id == itemId && ti.TodoListId == todoListId);
    
    if (todoItem == null)
        return Results.NotFound();

    todoItem.Title = request.Title;
    todoItem.Description = request.Description;
    todoItem.Status = request.Status;
    todoItem.Severity = request.Severity;
    todoItem.Order = request.Order;
    todoItem.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    var response = new TodoItemResponse(todoItem.Id, todoItem.Title, todoItem.Description, todoItem.Status, todoItem.Severity, todoItem.Order, todoItem.CreatedAt, todoItem.UpdatedAt);
    return Results.Ok(response);
})
.WithName("UpdateTodoItem");

app.MapDelete("/todolists/{todoListId}/items/{itemId}", async (int todoListId, int itemId, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);
    
    // TodoList'in kullanıcıya ait olduğunu veya partner'ın olduğunu kontrol et
    var todoList = await db.TodoLists
        .Include(tl => tl.Owner)
        .FirstOrDefaultAsync(tl => tl.Id == todoListId);
    
    if (todoList == null)
        return Results.NotFound();

    var currentUser = await db.Users.Include(u => u.Couple).FirstOrDefaultAsync(u => u.Id == userId);
    if (currentUser?.CoupleId == null || todoList.Owner.CoupleId != currentUser.CoupleId)
        return Results.BadRequest("Bu todo list'e erişim yetkiniz yok.");

    var todoItem = await db.TodoItems.FirstOrDefaultAsync(ti => ti.Id == itemId && ti.TodoListId == todoListId);
    
    if (todoItem == null)
        return Results.NotFound();

    db.TodoItems.Remove(todoItem);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteTodoItem");

// Profile management
app.MapPut("/users/profile", async (UpdateProfileRequest request, ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);
    var currentUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

    if (currentUser == null)
        return Results.NotFound();

    // Username değişikliği varsa benzersizlik kontrolü
    if (currentUser.Username != request.Username)
    {
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (existingUser != null)
            return Results.BadRequest("Bu kullanıcı adı zaten kullanılıyor.");
    }

    // Color code format kontrolü (#RRGGBB)
    if (!System.Text.RegularExpressions.Regex.IsMatch(request.ColorCode, @"^#[0-9A-Fa-f]{6}$"))
        return Results.BadRequest("Geçersiz renk kodu formatı. #RRGGBB formatında olmalı.");

    currentUser.Username = request.Username;
    currentUser.ColorCode = request.ColorCode;

    await db.SaveChangesAsync();

    return Results.Ok(new UserResponse(currentUser.Id, currentUser.Username, currentUser.ColorCode, currentUser.CreatedAt));
})
.WithName("UpdateProfile");

// Partner overview - tüm partner bilgilerini, todo listelerini ve itemlerini tek seferde getir
app.MapGet("/partner/overview", async (ClaimsPrincipal user, AppDbContext db) =>
{
    if (!IsAuthenticated(user))
        return Results.BadRequest(new { message = "Geçersiz veya eksik JWT token." });

    var userId = GetCurrentUserId(user);
    var currentUser = await db.Users.Include(u => u.Couple).FirstOrDefaultAsync(u => u.Id == userId);
    
    if (currentUser?.CoupleId == null)
        return Results.BadRequest("Henüz bir couple'a ait değilsiniz.");

    // Partner'ı bul (aynı couple'da olan diğer user)
    var partner = await db.Users
        .FirstOrDefaultAsync(u => u.CoupleId == currentUser.CoupleId && u.Id != userId);

    if (partner == null)
        return Results.BadRequest("Partner bulunamadı. Couple'da henüz başka bir kullanıcı yok.");

    // Partner'ın todo listelerini ve itemlerini getir
    var todoLists = await db.TodoLists
        .Where(tl => tl.OwnerId == partner.Id)
        .Include(tl => tl.TodoItems)
        .OrderBy(tl => tl.CreatedAt)
        .ToListAsync();

    // Response oluştur
    var todoListsWithItems = todoLists.Select(tl => new TodoListWithItemsResponse(
        tl.Id,
        tl.Title,
        tl.Description,
        tl.OwnerId,
        tl.CreatedAt,
        tl.UpdatedAt,
        tl.TodoItems
            .OrderBy(ti => ti.Order)
            .Select(ti => new TodoItemResponse(
                ti.Id,
                ti.Title,
                ti.Description,
                ti.Status,
                ti.Severity,
                ti.Order,
                ti.CreatedAt,
                ti.UpdatedAt))
            .ToList()
    )).ToList();

    var response = new PartnerOverviewResponse(
        partner.Id,
        partner.Username,
        partner.ColorCode,
        partner.CreatedAt,
        todoListsWithItems
    );

    return Results.Ok(response);
})
.WithName("GetPartnerOverview");

// Database migration
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.Run();