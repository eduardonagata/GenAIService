using System.Reflection;
using GenAIService.EndpointHandlers;
using GenAIService.Plugins;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Implementations;
using GenAIService.Infrastructure.Persistence.Repositories;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte ao Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GenAI Service API",
        Version = "v1",
        Description = "API para interação com o Semantic Kernel via linguagem natural."
    });
});

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:3002")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configura a conexão com o Redis.
var redisConfiguration = new RedisConfiguration
{
    ConnectionString = builder.Configuration.GetConnectionString("Redis"),
    AbortOnConnectFail = false
};
// Registrar Redis no DI com System.Text.Json
builder.Services.AddSingleton(redisConfiguration);

// Registrar o Connection Pool Manager.
builder.Services.AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConfiguration.ConnectionString!));

// Adicionar suporte ao Redis com System.Text.Json
builder.Services.AddSingleton<ISerializer, SystemTextJsonSerializer>();
builder.Services.AddSingleton<IRedisClient, RedisClient>();
builder.Services.AddSingleton<IRedisDatabase>(sp =>
    sp.GetRequiredService<IRedisClient>().Db7);

// Registrar repositories.
builder.Services.AddSingleton<ChatHistoryRedisRepository>();

builder.Services.AddKernel();
builder.Services.AddOpenAIChatCompletion("gpt-4o-mini", builder.Configuration["OPENAI_API_KEY"]!);
builder.Services.AddSingleton<TimePlugin>();

var app = builder.Build();

// Habilita CORS.
app.UseCors("AllowLocalhost");

// Habilita Swagger e a UI do Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GenAI Service API v1");
        c.RoutePrefix = string.Empty; // Para acessar Swagger diretamente na raiz: http://localhost:5000/
    });
}

// Mapeia os endpoints
app.MapEndpoints(Assembly.GetExecutingAssembly());

app.Run();
