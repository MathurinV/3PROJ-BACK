using DAL.Models.Expenses;
using DAL.Models.Groups;
using DAL.Models.Invitations;
using DAL.Models.Messages;
using DAL.Models.PaymentDetails;
using DAL.Models.UserExpenses;
using DAL.Models.UserGroups;
using DAL.Models.Users;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
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
    public DbSet<Invitation> Invitations { get; set; } = null!;

    public DbSet<PayDueTo> PayDueTos { get; set; } = null!;

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Change the name of the identity tables
        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

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

        builder.Entity<Expense>()
            .Property(e => e.JustificationExtension)
            .HasDefaultValue(null);

        builder.Entity<AppUser>()
            .Property(au => au.AvatarExtension)
            .HasDefaultValue(null);

        builder.Entity<Invitation>()
            .Property(i => i.InvitedAt)
            .HasDefaultValueSql("NOW()");

        builder.Entity<UserExpense>()
            .Property(ue => ue.PaidAt)
            .HasDefaultValue(null);

        builder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Group>()
            .HasOne(g => g.Owner)
            .WithMany(u => u.OwnedGroups)
            .HasForeignKey(g => g.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

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
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupMessage>()
            .HasOne(gm => gm.Sender)
            .WithMany(u => u.SentGroupMessages)
            .HasForeignKey(gm => gm.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupMessage>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.ReceivedGroupMessages)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Expense>()
            .HasOne(e => e.Group)
            .WithMany(g => g.Expenses)
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Expense>()
            .HasOne(e => e.CreatedBy)
            .WithMany(u => u.CreatedExpenses)
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserExpense>()
            .HasOne(ue => ue.User)
            .WithMany(u => u.UserExpenses)
            .HasForeignKey(ue => ue.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserExpense>()
            .HasOne(ue => ue.Expense)
            .WithMany(e => e.UserExpenses)
            .HasForeignKey(ue => ue.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Invitation>()
            .HasKey(i => new { i.UserId, i.GroupId });

        builder.Entity<Invitation>()
            .HasOne(i => i.Group)
            .WithMany(g => g.Invitations)
            .HasForeignKey(i => i.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Invitation>()
            .HasOne(i => i.User)
            .WithMany(u => u.Invitations)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PayDueTo>()
            .HasKey(pd => new { pd.UserId, pd.GroupId });

        builder.Entity<AppUser>()
            .HasMany(u => u.PaymentsToBeReceived)
            .WithOne(pd => pd.PayToUser)
            .HasForeignKey(pd => pd.PayToUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PayDueTo>()
            .HasOne(pd => pd.UserGroup)
            .WithOne(ug => ug.PayTo)
            .HasForeignKey<PayDueTo>(pd => new { pd.UserId, pd.GroupId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserGroup>()
            .HasOne(ug => ug.PayTo)
            .WithOne(pd => pd.UserGroup)
            .HasForeignKey<PayDueTo>(pd => new { pd.UserId, pd.GroupId })
            .OnDelete(DeleteBehavior.Cascade);
    }
}