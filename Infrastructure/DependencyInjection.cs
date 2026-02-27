using Application.Services;
using Core.Interfaces;
using Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar RabbitMQ Settings
        services.Configure<RabbitMqSettings>(options =>
        {
            options.Host = configuration["RabbitMQ:Host"] ?? string.Empty;
            options.Port = int.TryParse(configuration["RabbitMQ:Port"], out var port) ? port : 5672;
            options.Username = configuration["RabbitMQ:Username"] ?? string.Empty;
            options.Password = configuration["RabbitMQ:Password"] ?? string.Empty;
            options.QueueName = configuration["RabbitMQ:QueueName"] ?? string.Empty;
        });

        // Registrar RabbitMQ Publisher como Singleton
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        // Registrar Application Services
        services.AddScoped<SensorIngestionAppService>();

        return services;
    }
}
