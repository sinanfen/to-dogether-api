using Microsoft.EntityFrameworkCore;
using to_dogether_api.Models;

namespace to_dogether_api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<TodoList> TodoLists { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Couple> Couples { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // TodoList configuration
        modelBuilder.Entity<TodoList>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.TodoLists)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TodoItem configuration
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Severity).HasConversion<string>();
            
            entity.HasOne(e => e.TodoList)
                .WithMany(tl => tl.TodoItems)
                .HasForeignKey(e => e.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Token).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Couple configuration
        modelBuilder.Entity<Couple>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InviteToken).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.InviteToken).IsUnique();
        });

        // User configuration update
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ColorCode).IsRequired().HasMaxLength(7); // #RRGGBB format
            entity.HasIndex(e => e.Username).IsUnique();
            
            entity.HasOne(e => e.Couple)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CoupleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

    }
} 