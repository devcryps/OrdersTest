using OrdersManagement.Application.Interfaces;
using OrdersManagement.Application.Services;
using OrdersManagement.Infrastructure.DependencyInjection;
using OrdersManagement.WebApi.BackgroundServices;
using OrdersManagement.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<OrderCreatedConsumerService>();

var app = builder.Build();

app.UseExceptionHandler();
app.MapControllers();

app.Run();

