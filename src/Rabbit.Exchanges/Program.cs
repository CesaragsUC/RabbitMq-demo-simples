using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rabbit.Exchanges.Integration;
using Rabbit.Exchanges.RabbitMQConfig.Basic;
using Rabbit.Subscription.Interfaces;
using RabbitAppContext;
using RabbitAppContext.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RabbitContext>(options => 
{
    options.UseInMemoryDatabase("MyDemoRabbitMq");
    options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

builder.Services.AddHostedService<ClienteIntegration>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddRabbit(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
