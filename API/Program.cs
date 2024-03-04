using API.ErrorsHandling;
using API.Mutations;
using API.Queries;
using API.Repositories;
using API.Types;
using DAL.Models.Users;
using DAL.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        // Cors
        services.AddCors(options =>
        {
            options.AddPolicy("AllowVueApp", builder => builder
                .AllowAnyMethod()
                .AllowAnyHeader()
            );
        });

        // Postgres identity db context
        services.AddDbContext<MoneyMinderDbContext>(options =>
        {
            options.UseNpgsql(DockerEnv.ConnectionString
                , o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            );
        });

        // Add Identity
        services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<MoneyMinderDbContext>()
            .AddDefaultTokenProviders();

        services.AddDataProtection()
            .PersistKeysToDbContext<MoneyMinderDbContext>()
            .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
            });

        services.AddAuthentication()
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = DockerEnv.GoogleClientId;
                googleOptions.ClientSecret = DockerEnv.GoogleClientSecret;
            })
            ;

        services.AddAuthorization();

        // GraphQL
        services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddErrorFilter<GraphQlErrorFilter>()
            .AddQueryType(d => d.Name("Query"))
            .AddTypeExtension<UserQueries>()
            .AddTypeExtension<GroupQueries>()
            .AddMutationType(d => d.Name("Mutation"))
            .AddTypeExtension<UserMutations>()
            .AddTypeExtension<GroupMutations>()
            .AddTypeExtension<MessageMutations>()
            .AddType<AppUserType>()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            ;

        // Dependency Injection
        services
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IGroupRepository, GroupRepository>()
            .AddScoped<IUserGroupRepository, UserGroupRepository>()
            .AddScoped<IMessageRepository, MessageRepository>()
            .AddScoped<IGroupMessageRepository, GroupMessageRepository>()
            .AddScoped<IExpenseRepository, ExpenseRepository>()
            .AddScoped<IUserExpenseRepository, UserExpenseRepository>()
            .AddScoped<IInvitationRepository, InvitationRepository>()
            ;

        // Health checks
        services.AddHealthChecks()
            .AddNpgSql(DockerEnv.ConnectionString);

        var app = builder.Build();

        app.UseCors("AllowVueApp");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGraphQL(DockerEnv.ApiEndpoint);
        app.MapHealthChecks("/health")
            .RequireHost("localhost");

        app.Run();
    }
}