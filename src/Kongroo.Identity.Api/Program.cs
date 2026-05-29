using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using HealthChecks.UI.Client;
using Kongroo.BuildingBlocks;
using Kongroo.BuildingBlocks.Presentation.Authorization;
using Kongroo.Identity;
using Kongroo.Identity.Api;
using Kongroo.Identity.Api.OpenApi;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Kongroo.Identity.Presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
    )
);

builder.Services.AddSerilog(configuration =>
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithEnvironmentUserName()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .Enrich.WithProcessName()
        .Enrich.WithThreadId()
        .Enrich.WithThreadName()
        .Enrich.WithProperty("Application", AppDomain.CurrentDomain.FriendlyName)
);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecurityRequirementTransformer>();
});

builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder
    .Services.AddHealthChecks()
    .AddApplicationLifecycleHealthCheck()
    .AddResourceUtilizationHealthCheck()
    .AddNpgSql(builder.Configuration.GetRequiredConnectionString("Database"))
    .AddDbContextCheck<IdentityDbContext>();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions =
            builder.Configuration.GetRequiredSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Configuration section '{JwtOptions.SectionName}' is missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtOptions.CreateSigningKey(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(nameof(UserRole.Admin)));

builder.Services.AddBuildingBlocks(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("health", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.MapIdentityEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}
