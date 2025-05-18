using Elearning.Api.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using HealthChecks.UI.Client;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Infrastructure.Configuration;
using Elearning.Common.Infrastructure;
using Elearning.Common.Application;
using Elearning.Api;
using System.Reflection;
using Elearning.Api.Extensions;
using Elearning.Modules.Users.Infrastructure;
using Serilog.Events;
using Elearning.Modules.Program.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
  loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Information)
    .MinimumLevel.Information() // Set minimum level to Information
    .WriteTo.Console());

// Redirect logs to console
Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers(); // Sử dụng cách đơn giản để đăng ký controllers

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");
Console.WriteLine($"Database connection: {databaseConnectionString}");
string redisConnectionString = builder.Configuration.GetConnectionStringOrThrow("Cache");
Console.WriteLine($"Redis connection: {redisConnectionString}");
string rabbitmqConnectionString = builder.Configuration.GetConnectionStringOrThrow("RabbitMQ");
Console.WriteLine($"RabbitMQ connection: {rabbitmqConnectionString}");

IConfigurationSection jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"];

builder.Services.AddInfrastructure(
    DiagnosticsConfig.ServiceName,
    jwtSettings,
    secret,
    // [
    //     EventsModule.ConfigureConsumers(redisConnectionString),
    //     TicketingModule.ConfigureConsumers,
    //     AttendanceModule.ConfigureConsumers
    // ],
    databaseConnectionString,
    redisConnectionString,
    rabbitmqConnectionString);
Assembly[] moduleApplicationAssemblies = [
    Elearning.Modules.Users.Application.AssemblyReference.Assembly,
    Elearning.Modules.Program.Application.AssemblyReference.Assembly
];

builder.Services.AddApplication(moduleApplicationAssemblies);
// builder.Services.AddSwaggerDocumentation();

// builder.Services.AddOpenApi();

// Tạm thời comment health check RabbitMQ vì đang có lỗi
builder.Services.AddHealthChecks()
    .AddNpgSql(databaseConnectionString)
    .AddRedis(redisConnectionString);
// .AddRabbitMQ();
// .AddKeyCloak(keyCloakHealthUrl);

builder.Configuration.AddModuleConfiguration(["users"]);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddProgramsModule(builder.Configuration);


builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowFrontend", builder =>
  {
    builder
          .WithOrigins("http://localhost:3000") // Frontend URL
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials();
  });
});

WebApplication app = builder.Build();
Console.WriteLine($"Development environment: {app.Environment.IsDevelopment()}");
if (app.Environment.IsDevelopment())
{
  app.ApplyMigrations();
  // app.MapOpenApi();
}
app.MapHealthChecks("health", new HealthCheckOptions
{
  ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Thêm endpoint debug để kiểm tra tất cả các endpoint có sẵn
app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
{
  var endpoints = endpointSources
      .SelectMany(source => source.Endpoints)
      .OfType<RouteEndpoint>()
      .Select(endpoint => new
      {
        Method = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault(),
        Route = endpoint.RoutePattern.RawText,
        DisplayName = endpoint.DisplayName
      })
      .ToList();

  return endpoints;
});

// build
app.UseLogContext();
app.UseSerilogRequestLogging();
app.UseAuthentication(); // Add this
app.UseAuthorization(); // Add this
app.UseExceptionHandler();
app.UseCors("AllowFrontend");
app.MapEndpoints();
app.MapControllers();

Console.WriteLine("Running");
app.Run();

public partial class Program;