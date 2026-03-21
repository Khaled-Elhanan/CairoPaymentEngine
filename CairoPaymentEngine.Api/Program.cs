using CairoPaymentEngine.Api.Middleware;
using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Application.Service;
using CairoPaymentEngine.Infrastructure.Gateways;
using CairoPaymentEngine.Infrastructure.Repository;
using CairoPaymentEngine.Infrastructure.Settings;
using CairoPaymentEngine.Persistence.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection(StripeSettings.SectionName));

builder.Services.Configure<PaymobSettings>(
   builder.Configuration.GetSection(PaymobSettings.SectionName));




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

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.UseHttpsRedirection();
app.Run();


