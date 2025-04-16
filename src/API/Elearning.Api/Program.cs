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
using Elearning.Modules.Program.Infrastructure;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

Assembly[] moduleApplicationAssemblies = [
    Elearning.Modules.Users.Application.AssemblyReference.Assembly,
    Elearning.Modules.Program.Application.AssemblyReference.Assembly,
];

builder.Services.AddApplication(moduleApplicationAssemblies);
// builder.Services.AddSwaggerDocumentation();
string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");
Console.WriteLine(databaseConnectionString);
string redisConnectionString = builder.Configuration.GetConnectionStringOrThrow("Cache");
Console.WriteLine(redisConnectionString);
string rabbitmqConnectionString = builder.Configuration.GetConnectionStringOrThrow("RabbitMQ");
Console.WriteLine(rabbitmqConnectionString);

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
// builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
    .AddNpgSql(databaseConnectionString)
    .AddRedis(redisConnectionString)
    .AddRabbitMQ();
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
Console.WriteLine(app.Environment.IsDevelopment());
if (app.Environment.IsDevelopment())
{
  app.ApplyMigrations();
  // app.MapOpenApi();
}
app.MapHealthChecks("health", new HealthCheckOptions
{
  ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
// build
app.UseLogContext();
app.UseSerilogRequestLogging();

app.UseExceptionHandler();
app.UseCors("AllowFrontend");
app.MapEndpoints();

Console.WriteLine("Running");
app.Run();

public partial class Program;