using API.Mutations;
using API.Queries;
using API.Repositories;
using API.Types;
using DAL.Models.Users;
using DAL.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Postgres identity db context
        builder.Services.AddDbContext<MoneyMinderDbContext>(options =>
        {
            options.UseNpgsql(DockerEnv.ConnectionString);
        });

        // Add Identity
        builder.Services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<MoneyMinderDbContext>()
            .AddDefaultTokenProviders();

        // GraphQL
        builder.Services.AddGraphQLServer()
            // Add db context
            .AddQueryType(d => d.Name("Query"))
            .AddTypeExtension<UserQueries>()
            .AddTypeExtension<GroupQueries>()
            
            .AddMutationType(d => d.Name("Mutation"))
            .AddTypeExtension<UserMutations>()
            .AddTypeExtension<GroupMutations>()
            .AddTypeExtension<MessageMutations>()
            
            .AddType<AppUserType>()
            ;

        // Dependency Injection
        builder.Services
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IGroupRepository, GroupRepository>()
            .AddScoped<IUserGroupRepository, UserGroupRepository>()
            .AddScoped<IMessageRepository, MessageRepository>()
            .AddScoped<IGroupMessageRepository, GroupMessageRepository>()
            ;

        // Add services to the container.
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

        app.UseStaticFiles();

        app.UseAuthorization();

        app.MapGraphQL(DockerEnv.ApiEndpoint);

        app.Run();
    }
}