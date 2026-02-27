using Core.Interfaces;
using Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public RabbitMqPublisher(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqPublisher> logger)
    {
        _settings = settings.Value;
        _settings.Validate();
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default) where T : class
    {
        EnsureConnection();

        var body = SerializeMessage(message);

        try
        {
            _channel!.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Mensagem publicada com sucesso na fila {QueueName}",
                queueName);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem na fila {QueueName}", queueName);
            throw;
        }
    }

    private void EnsureConnection()
    {
        if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
            return;

        lock (_lock)
        {
            if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
                return;

            try
            {
                _logger.LogInformation(
                    "Conectando ao RabbitMQ em {Host}:{Port}",
                    _settings.Host,
                    _settings.Port);

                var factory = new ConnectionFactory
                {
                    HostName = _settings.Host,
                    Port = _settings.Port,
                    UserName = _settings.Username,
                    Password = _settings.Password,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation("Conectado ao RabbitMQ com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao conectar ao RabbitMQ");
                throw;
            }
        }
    }

    private byte[] SerializeMessage<T>(T message)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        return Encoding.UTF8.GetBytes(json);
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("Conexão com RabbitMQ fechada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fechar conexão com RabbitMQ");
        }
    }
}
