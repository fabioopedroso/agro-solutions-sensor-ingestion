using Application.DTOs;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class SensorIngestionAppService
{
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SensorIngestionAppService> _logger;

    public SensorIngestionAppService(
        IRabbitMqPublisher rabbitMqPublisher,
        IConfiguration configuration,
        ILogger<SensorIngestionAppService> logger)
    {
        _rabbitMqPublisher = rabbitMqPublisher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishAsync(SensorDataDto sensorData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Publicando dados do sensor: FieldId={FieldId}, SensorType={SensorType}, Value={Value}, Timestamp={Timestamp}",
            sensorData.FieldId, sensorData.SensorType, sensorData.Value, sensorData.Timestamp);

        // Validação adicional de regras de negócio
        sensorData.Validate();

        var queueName = _configuration["RabbitMQ:QueueName"] 
            ?? throw new InvalidOperationException("RabbitMQ:QueueName não configurado");

        await _rabbitMqPublisher.PublishAsync(sensorData, queueName, cancellationToken);

        _logger.LogInformation(
            "Dados do sensor publicados com sucesso na fila {QueueName}",
            queueName);
    }
}
