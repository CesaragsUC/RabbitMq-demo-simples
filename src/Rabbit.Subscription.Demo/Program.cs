using Microsoft.EntityFrameworkCore;
using Rabbit.EventBus.Config.Bus;
using Rabbit.EventBus.Config.Eventos;
using Rabbit.EventBus.Infra.Config;
using Rabbit.Subscription.Demo.IntegrationEvents.EventHandlers;
using Rabbit.Subscription.Demo.IntegrationEvents.Events;
using Rabbit.Subscription.Interfaces;
using RabbitAppContext;
using RabbitAppContext.Repository;
using System;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<RabbitContext>(options =>
{
    options.UseInMemoryDatabase("MyDemoRabbitMqSubscription");
    options.EnableSensitiveDataLogging();
});


var rabbitMQSection = builder.Configuration.GetSection("RabbitMQ");

builder.Services.AddRabbitMQEventBus
(
    connectionUrl: rabbitMQSection["ConnectionUrl"],
    brokerName: "netCoreEventBusBroker2",
    queueName: "netCoreEventBusQueue2",
    timeoutBeforeReconnecting: 15
);


builder.Services.AddTransient<ProdutoventHandler>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.\
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var eventBus = app.Services.GetRequiredService<IEventBus>();//registrado como Singlenton

// Aqui você adiciona os manipuladores de eventos para cada evento de integração.
eventBus.Subscribe<ProdutoCadastradoEvent, ProdutoventHandler>();
eventBus.Subscribe<ProdutoAtualizadoEvent, ProdutoventHandler>();
eventBus.Subscribe<ProdutoExcluidoEvent, ProdutoventHandler>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


