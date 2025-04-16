using Elearning.Modules.Users.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Elearning.Common.Infrastructure.Outbox;
using Elearning.Modules.Users.Application.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Elearning.Modules.Users.Infrastructure.Outbox;
using Elearning.Common.Application.EventBus;
using Elearning.Modules.Users.Infrastructure.Inbox;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Modules.Users.Domain.Users;
using Elearning.Modules.Users.Infrastructure.Users;
namespace Elearning.Modules.Users.Infrastructure;


public static class UsersModule
{
  public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDomainEventHandlers();

    services.AddIntegrationEventHandlers();
    services.AddInfrastructure(configuration);
    services.AddEndpoints(User.Presentation.AssemblyReference.Assembly);
    return services;
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddScoped<IUserRepository, UsersRepository>();
    services.AddDbContext<UsersDbContext>((sp, options) =>
      options
             .UseNpgsql(
              configuration.GetConnectionString("Database"),
               npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Users))
                        .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                        .UseSnakeCaseNamingConvention()
             );
    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());
  }

  private static void AddDomainEventHandlers(this IServiceCollection services)
  {
    Type[] domainEventHandlers = Application.AssemblyReference.Assembly
        .GetTypes()
        .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))
        .ToArray();

    foreach (Type domainEventHandler in domainEventHandlers)
    {
      services.TryAddScoped(domainEventHandler);

      Type domainEvent = domainEventHandler
          .GetInterfaces()
          .Single(i => i.IsGenericType)
          .GetGenericArguments()
          .Single();

      Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent);

      services.Decorate(domainEventHandler, closedIdempotentHandler);
    }
  }
  private static void AddIntegrationEventHandlers(this IServiceCollection services)
  {
    Type[] integrationEventHandlers = User.Presentation.AssemblyReference.Assembly
        .GetTypes()
        .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))
        .ToArray();

    foreach (Type integrationEventHandler in integrationEventHandlers)
    {
      services.TryAddScoped(integrationEventHandler);

      Type integrationEvent = integrationEventHandler
          .GetInterfaces()
          .Single(i => i.IsGenericType)
          .GetGenericArguments()
          .Single();

      Type closedIdempotentHandler =
          typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEvent);

      services.Decorate(integrationEventHandler, closedIdempotentHandler);
    }
  }
}