using System.Threading.Tasks;
using Common.Events;

// Bir serviste Basket.API kuyruktan gelen bir mesaj okunduğunda OrderCreatedEvent o mesajla ne yapılacağına karar vermek için bir metodun tetiklenmesi gerekir 
// Bu interface dinleyici sınıflarının standartlaşmasını sağlar
// Her Event Handler bu interfacei uygulayarak zorunlu olarak Handle metodunu barındırmak durumunda kalır.
namespace Common.EventBus
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> 
        where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }
}
