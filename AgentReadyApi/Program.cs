using AgentReadyApi.Data;
using AgentReadyApi.Data.Services;
using AgentReadyApi.Exceptions;
using AgentReadyApi.Middlewares;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new Exception("Missing connection string");

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        "agent-policy",
        limiterOptions =>
        {
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueLimit = 20;
        });
});

builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority =
            "https://auth.example.com";

        options.Audience =
            "agent-api";

        options.RequireHttpsMetadata = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "InvoicesRead",
        policy => policy.RequireClaim(
            "scope",
            "invoices:read"));

    options.AddPolicy(
        "InvoicesWrite",
        policy => policy.RequireClaim(
            "scope",
            "invoices:write"));
});

builder.Services.AddControllers();
var app = builder.Build();
app.UseStaticFiles();
app.MapControllers();

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseHttpsRedirection();

app.Run();
