using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Common.EventBus
{
    public class RabbitMQEventBus : IEventBus
    {
        private readonly IConnection _connection;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMQEventBus(IConnection connection, IServiceProvider serviceProvider)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        // Publisher: fanout exchange'e gönderir, tüm subscriber'lar alır
        public async Task PublishAsync(IntegrationEvent @event)
        {
            var exchangeName = @event.GetType().Name;

            using var channel = await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);

            var message = JsonSerializer.Serialize(@event, @event.GetType());
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: exchangeName, routingKey: string.Empty, body: body);
        }

        // Subscriber: her consumer kendi geçici kuyruğunu alır → aynı event'i herkes ayrı ayrı alır
        public async Task SubscribeAsync<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var exchangeName = typeof(T).Name;

            var channel = await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);

            // Her consumer'a özel, otomatik silinen kuyruk
            var queueDeclare = await channel.QueueDeclareAsync(queue: string.Empty, durable: false, exclusive: true, autoDelete: true);
            var queueName = queueDeclare.QueueName;

            await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<T>(message);

                if (@event != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TH>();
                    await handler.Handle(@event);
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
        }

        public Task UnsubscribeAsync<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            return Task.CompletedTask;
        }
    }
}
