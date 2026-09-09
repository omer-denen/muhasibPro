using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;

namespace MuhasibPro.Business.Services.CommonServices
{
    /// <summary>
    /// Tipli olay hattı — ham <see cref="IMessageService"/> transportunun üstünde
    /// idempotent abonelik sunar. Yeni kod burayı kullanır.
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly IMessageService _messages;

        public EventBus(IMessageService messages)
        {
            _messages = messages;
        }

        public void Publish<TEvent>(object sender, TEvent @event) where TEvent : DomainEvent
        {
            _messages.Send(sender, typeof(TEvent).Name, @event);
        }

        public void Subscribe<TEvent>(object target, Action<object, TEvent> handler) where TEvent : DomainEvent
        {
            try
            {
                _messages.Subscribe<object, TEvent>(target, (s, _, e) => handler(s, e));
            }
            catch (ArgumentException)
            {
                // Aynı hedef + aynı olay ikinci kez abone olursa idempotent: ilk kayıt korunur.
            }
        }

        public void Unsubscribe(object target)
        {
            _messages.Unsubscribe(target);
        }
    }
}
