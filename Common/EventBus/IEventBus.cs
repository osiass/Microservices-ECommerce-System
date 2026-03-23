using System.Threading.Tasks;
using Common.Events;

// Projemizin herhangi bir mesajlaşma aracıyla RabbitMQ sıkı sıkıya bağlı olmasın Bu yüzden bir Interface oluştur
// doğrudan RabbitMQ kodlarına dokunmak yerine bu Interface'i kullanacaklar 
namespace Common.EventBus
{
    public interface IEventBus
    {
        Task PublishAsync(IntegrationEvent @event);

        Task SubscribeAsync<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        Task UnsubscribeAsync<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
    }
}
