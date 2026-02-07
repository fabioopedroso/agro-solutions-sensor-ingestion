namespace Core.Interfaces;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default) where T : class;
}
