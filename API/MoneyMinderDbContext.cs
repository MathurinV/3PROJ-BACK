using DAL.Models.Expenses;
using DAL.Models.Groups;
using DAL.Models.Messages;
using DAL.Models.UserExpenses;
using DAL.Models.UserGroups;
using DAL.Models.Users;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API;

public sealed class MoneyMinderDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IDataProtectionKeyContext
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
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<GroupMessage> GroupMessages { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<UserExpense> UserExpenses { get; set; } = null!;

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();
        builder.Entity<AppUser>().HasIndex(u => u.UserName).IsUnique();

        builder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

        builder.Entity<UserExpense>()
            .HasKey(ue => new { ue.UserId, ue.ExpenseId });

        builder.Entity<UserGroup>()
            .Property(ug => ug.JoinedAt)
            .HasDefaultValueSql("NOW()");

        builder.Entity<Expense>()
            .Property(e => e.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Entity<UserExpense>()
            .Property(ue => ue.PaidAt)
            .HasDefaultValue(null);

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

        builder.Entity<Message>()
            .Property(m => m.SentAt)
            .HasDefaultValueSql("NOW()");

        builder.Entity<GroupMessage>()
            .Property(gm => gm.SentAt)
            .HasDefaultValueSql("NOW()");

        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroupMessage>()
            .HasOne(gm => gm.Sender)
            .WithMany(u => u.SentGroupMessages)
            .HasForeignKey(gm => gm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroupMessage>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.ReceivedGroupMessages)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Expense>()
            .HasOne(e => e.Group)
            .WithMany(g => g.Expenses)
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Expense>()
            .HasOne(e => e.CreatedBy)
            .WithMany(u => u.CreatedExpenses)
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserExpense>()
            .HasOne(ue => ue.User)
            .WithMany(u => u.UserExpenses)
            .HasForeignKey(ue => ue.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserExpense>()
            .HasOne(ue => ue.Expense)
            .WithMany(e => e.UserExpenses)
            .HasForeignKey(ue => ue.ExpenseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}