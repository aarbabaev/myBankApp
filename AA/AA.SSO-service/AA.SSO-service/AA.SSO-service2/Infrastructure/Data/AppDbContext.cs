using AA.SSO_service2.Entities;
using Microsoft.EntityFrameworkCore;


namespace AA.SSO_service2.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.Property(u => u.PasswordHash).IsRequired();
            
            builder.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.HasKey(rt => rt.Id);
            builder.HasIndex(rt => rt.TokenHash).IsUnique();
            builder.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(200);
        });
    }

}