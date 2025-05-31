using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Elearning.Common.Infrastructure.Outbox;
using Elearning.Common.Presentation.Endpoints;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Elearning.Common.Application.EventBus;
using Elearning.Modules.Program.Infrastructure.Database;
using Elearning.Modules.Program.Application.Data;
using System.Reflection;


namespace Elearning.Modules.Program.Infrastructure;


public static class ProgramsModule
{
  public static IServiceCollection AddProgramsModule(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddInfrastructure(configuration);

    // Skip loading the Presentation assembly for now
    // This is a temporary workaround to avoid the duplicate attribute error
    // Console.WriteLine("Skipping loading Presentation assembly to avoid duplicate attribute error.");

    // We'll manually register the endpoints here

    services.AddEndpoints(Elearning.Modules.Program.Presentation.AssemblyReference.Assembly);

    return services;
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    // services.AddScoped<IUserRepository, UsersRepository>();

    // Add HttpClient for file download functionality
    services.AddHttpClient();

    services.AddDbContext<ProgramDbContext>((sp, options) =>
      options
             .UseNpgsql(
              configuration.GetConnectionString("Database"),
               npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Program))
                        .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                        .UseSnakeCaseNamingConvention()
             );
    services.AddScoped<IProgramUnitOfWork>(sp => sp.GetRequiredService<ProgramDbContext>());
  }

}
