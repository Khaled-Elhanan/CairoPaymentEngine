using CairoPaymentEngine.Api.Middleware;
using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Application.Service;
using CairoPaymentEngine.Infrastructure.Gateways;
using CairoPaymentEngine.Infrastructure.Repository;
using CairoPaymentEngine.Infrastructure.Settings;
using CairoPaymentEngine.Persistence.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var portValue = Environment.GetEnvironmentVariable("PORT");
var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 10000;
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        var originsFromLegacyEnv = builder.Configuration["CORS_ALLOWED_ORIGINS"];
        var allowedOrigins = configuredOrigins?.Length > 0
            ? configuredOrigins
            : !string.IsNullOrWhiteSpace(originsFromLegacyEnv)
                ? originsFromLegacyEnv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>();

        if (allowedOrigins.Length == 0)
        {
            allowedOrigins = ["http://localhost:5173", "https://localhost:5173"];
        }

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "CairoPaymentEngine API",
        Version = "v1",
        Description = "A clean payment engine supporting Stripe, Paymob — built for Egypt."
    });
});



builder.Services.AddDbContext<CairoPaymentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

    options.UseSqlServer(connectionString);
});

builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection(StripeSettings.SectionName));

builder.Services.Configure<PaymobSettings>(
   builder.Configuration.GetSection(PaymobSettings.SectionName));
builder.Services.AddHealthChecks();




//── Repositories 

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();


// ── Payment Gateways
builder.Services.AddHttpClient<IPaymentGateway, StripeGateway>();
builder.Services.AddHttpClient<IPaymentGateway, PaymobGateway>();

// ── Application Services
builder.Services.AddScoped<PaymentOrchestrator>();
builder.Services.AddScoped<IPaymentService, PaymentService>();



var app = builder.Build();

var runMigrationsOnStartup =
    app.Configuration.GetValue<bool?>("Database:RunMigrationsOnStartup") ?? true;
if (runMigrationsOnStartup)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CairoPaymentDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("FrontendPolicy");
var useHttpsRedirection =
    app.Configuration.GetValue<bool?>("UseHttpsRedirection") ?? !app.Environment.IsProduction();
if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}
app.MapControllers();
app.MapHealthChecks("/healthz");
app.Run();


