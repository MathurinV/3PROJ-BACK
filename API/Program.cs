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

        // Logging
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter("Microsoft", LogLevel.Debug);
            loggingBuilder.AddFilter("Microsoft.AspNetCore.DataProtection", LogLevel.Warning);
            loggingBuilder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            loggingBuilder.AddFilter("System", LogLevel.Debug);
        });

        // Cors
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", configurePolicy =>
                configurePolicy
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithMethods("GET", "POST")
                    .WithOrigins(DockerEnv.ClientUrl)
            );
        });

        services.AddHttpContextAccessor();

        services.AddStackExchangeRedisCache(options => { options.Configuration = "cache:6379"; });

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
            .AddTypeExtension<MessageQueries>()
            .AddMutationType(d => d.Name("Mutation"))
            .AddTypeExtension<UserMutations>()
            .AddTypeExtension<GroupMutations>()
            .AddTypeExtension<MessageMutations>()
            .AddTypeExtension<ExpenseMutations>()
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

        services.AddControllers();

        var app = builder.Build();

        app.UseCors("DefaultPolicy");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapGraphQL(DockerEnv.ApiEndpoint);
        app.MapHealthChecks("/health")
            .RequireHost("localhost");

        app.Run();
    }
}