using DAL.Models.Groups;
using DAL.Models.UserGroups;
using DAL.Models.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API;

public sealed class MoneyMinderDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public MoneyMinderDbContext(DbContextOptions<MoneyMinderDbContext> options) : base(options)
    {
        Database.EnsureCreated();

        if (!Roles.Any())
        {
            Roles.Add(new AppRole
            {
                Name = "Admin",
                NormalizedName = "ADMIN"
            });
            Roles.Add(new AppRole
            {
                Name = "User",
                NormalizedName = "USER"
            });
            SaveChanges();
        }
    }

    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<UserGroup> UserGroups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();

        builder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

        builder.Entity<UserGroup>()
            .Property(ug => ug.JoinedAt)
            .HasDefaultValueSql("NOW()");

        builder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId);

        builder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId);

        builder.Entity<Group>()
            .HasOne(g => g.Owner)
            .WithMany(u => u.OwnedGroups)
            .HasForeignKey(g => g.OwnerId);
    }
}