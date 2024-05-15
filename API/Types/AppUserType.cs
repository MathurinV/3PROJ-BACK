using DAL.Models.Users;

namespace API.Types;

public class AppUserType : ObjectType<AppUser>
{
    protected override void Configure(IObjectTypeDescriptor<AppUser> descriptor)
    {
        descriptor.Field(u => u.NormalizedEmail).Ignore();
        descriptor.Field(u => u.NormalizedUserName).Ignore();
        descriptor.Field(u => u.EmailConfirmed).Ignore();
        descriptor.Field(u => u.PasswordHash).Ignore();
        descriptor.Field(u => u.SecurityStamp).Ignore();
        descriptor.Field(u => u.ConcurrencyStamp).Ignore();
        descriptor.Field(u => u.PhoneNumber).Ignore();
        descriptor.Field(u => u.PhoneNumberConfirmed).Ignore();
        descriptor.Field(u => u.TwoFactorEnabled).Ignore();
        descriptor.Field(u => u.LockoutEnabled).Ignore();
        descriptor.Field(u => u.LockoutEnd).Ignore();
        descriptor.Field(u => u.AccessFailedCount).Ignore();
        descriptor.Field(f => f.Id).IsProjected();
        descriptor.Field(u => u.AvatarExtension).IsProjected();
        descriptor.Field(u => u.RibExtension).IsProjected();
    }
}