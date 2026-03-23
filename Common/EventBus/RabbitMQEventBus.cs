using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Common.Events;
using Microsoft.Extensions.DependencyInjection;

// Mikroservislerin birbiriyle RabbitMQ üzerinden konuşabilmesi için IEventBus interface'indeki kurallara uyan gerçek koddur
namespace Common.EventBus
{
    public class RabbitMQEventBus : IEventBus
    {
        //Dependency Injection servisler başladığında Program.cs RabbitMQ bağlantısı IConnection ve 
        // servis sağlayıcı IServiceProvider buraya otomatik olarak enjekte edilir
        private readonly IConnection _connection;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMQEventBus(IConnection connection, IServiceProvider serviceProvider)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        // Bir servis Örn: Order.API Sipariş Oluşturuldu dediğinde bu metot çalışır
        public async Task PublishAsync(IntegrationEvent @event)
        {
            // Event'in türünden yola çıkarak kuyruğa vereceğimiz ismi EventName
            var eventName = @event.GetType().Name;

            // RabbitMQ ile konuşmak için bir kanal açılır.
            using var channel = await _connection.CreateChannelAsync();

            // RabbitMQ Böyle bir kuyruk var yoksa sen oluştur
            await channel.QueueDeclareAsync(queue: eventName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // C# dilindeki nesneyi JSON ayani saf metne çevirme
            var message = JsonSerializer.Serialize(@event, @event.GetType());
            
            // Bilgisayar ağında veriler metin değil Byte olarak akar Bytea dönüştür
            var body = Encoding.UTF8.GetBytes(message);

            // Kanal üzerinden mesajımızı RabbitMQ kuyruğuna ateşliyoruz.
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: eventName, body: body);
        }

        //Subscribe Metodu
        // Bir servis Biri sipariş verdiğinde beni haberdar et demek için burayı çalıştırır.
        public async Task SubscribeAsync<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;

            // RabbitMQ kanalımızı sadece dinleme modunda kullanmak için kanal aç using kullanma çünkü dinleme hep açık kalmalı
            var channel = await _connection.CreateChannelAsync();
            
            await channel.QueueDeclareAsync(queue: eventName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Dinleyici nesnesi oluştur Kuyruktan mesaj gelince tetiklenecek nesne
            var consumer = new AsyncEventingBasicConsumer(channel);

            // Kuyruğa bir şey düştüğünde "Received" event tetiklendiğinde ne yapılacağı
            consumer.ReceivedAsync += async (model, ea) =>
            {
                // Bilgisayar ağından gelen Byte dizisini oku
                var body = ea.Body.ToArray();
                // Byte dizisini string JSON metnine çevir
                var message = Encoding.UTF8.GetString(body);
                // JSON metnini bizim C# nesnemiz Event türüne geri kalıpla
                var @event = JsonSerializer.Deserialize<T>(message);

                if (@event != null)
                {
                    // Dependency Injection içinden bu Eventi işleyecek asıl sınıfı bulup getiriyoruz.
                    // Örn: TH = OrderCreatedEventHandler
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TH>();
                    
                    // Handler metodunu çalıştırıyoruz. Sipariş burada işlenir!
                    await handler.Handle(@event);
                }
            };

            // Program.csye Ben bu kuyruğu dinlemeye başladım komutunu ver
            await channel.BasicConsumeAsync(queue: eventName, autoAck: true, consumer: consumer);
        }

        public Task UnsubscribeAsync<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            return Task.CompletedTask;
        }
    }
}
