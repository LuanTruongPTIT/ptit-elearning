
using System.Text;
using Common.Elearning.Infrastructure.Clock;
using Dapper;
using Elearning.Common.Application.Clock;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.EventBus;
using Elearning.Common.Application.Jwt;
using Elearning.Common.Domain;
using Elearning.Common.Infrastructure.Data;
using Elearning.Common.Infrastructure.Jwt;
using Elearning.Common.Infrastructure.Outbox;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using StackExchange.Redis;

namespace Elearning.Common.Infrastructure;

public static class InfrastructureConfiguration
{
  public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    string serviceName,
    IConfigurationSection jwtConfig,
    string secret,
    // Action<IRegistrationConfigurator>[] moduleConfigureConsumers,
    string databaseConnectionString,
    string redisConnectionString,
  string rabbitmqConnectionString
  )
  {
    services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

    services.TryAddSingleton<IEventBus, EventBus.EventBus>();
    services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddAuthentication();
    services.AddAuthorization();
    NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
    services.TryAddSingleton(npgsqlDataSource);

    services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

    SqlMapper.AddTypeHandler(new GenericArrayHandler<string>());
    services.Configure<JwtSettings>(jwtConfig);


    // Đăng ký TokenService
    services.AddScoped<ITokenService, TokenService>();

    // Cấu hình Authentication
    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
      };

      options.Events = new JwtBearerEvents
      {
        OnChallenge = async context =>
            {
              // Prevent default response
              context.HandleResponse();

              // Custom response
              context.Response.StatusCode = StatusCodes.Status401Unauthorized;
              context.Response.ContentType = "application/json";
              var result = System.Text.Json.JsonSerializer.Serialize(new
              {
                status = StatusCodes.Status401Unauthorized,
                title = "Unauthorized",
                detail = "You are not authorized to access this resource",
                type = "https://tools.ietf.org/html/rfc7235#section-3.1"
              });

              await context.Response.WriteAsync(result);
            }
      };
    });
    services.AddAuthorization(options =>
    {
      options.AddPolicy("RequireRole_Teacher", policy => policy.RequireRole("Teacher"));
      options.AddPolicy("RequireRole_Admin", policy => policy.RequireRole("Administrator"));
      options.AddPolicy("RequireRole_Student", policy => policy.RequireRole("Student"));
    });
    services.AddQuartz(configurator =>
    {
      var scheduler = Guid.NewGuid();
      configurator.SchedulerId = $"default-id-{scheduler}";
      configurator.SchedulerName = $"default-name-{scheduler}";
    });

    services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

    try
    {
      IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
      services.AddSingleton(connectionMultiplexer);
      services.AddStackExchangeRedisCache(options =>
         options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer)
      );
    }
    catch
    {
      services.AddDistributedMemoryCache();
    }

    services.AddMassTransit(configure =>
    {
      // foreach (Action<IRegistrationConfigurator> configureConsumers in moduleConfigureConsumers)
      // {
      //   configureConsumers(configure);
      // }
      configure.SetKebabCaseEndpointNameFormatter();

      configure.UsingRabbitMq((context, cfg) =>
      {
        cfg.Host(rabbitmqConnectionString, host =>
        {
          host.Username("user");
          host.Password("user");
        });
        cfg.ConfigureEndpoints(context);
      });
    });

    services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing =>
    {
      tracing
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddEntityFrameworkCoreInstrumentation()
          .AddRedisInstrumentation()
          // .AddMassTransitInstrumentation()
          .AddNpgsql()
      .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);

      tracing.AddOtlpExporter();
    });
    return services;
  }
}