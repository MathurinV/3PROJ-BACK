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
            options.UseNpgsql(DockerEnv.ConnectionString,
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });

        // Add Identity
        builder.Services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<MoneyMinderDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthentication();
        
        builder.Services.AddAuthorization();
        
        // GraphQL
        builder.Services.AddGraphQLServer()
            .AddAuthorization()
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
            .AddScoped<IExpenseRepository, ExpenseRepository>()
            .AddScoped<IUserExpenseRepository, UserExpenseRepository>()
            ;

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

        app.UseStaticFiles();

        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGraphQL(DockerEnv.ApiEndpoint);

        app.Run();
    }
}