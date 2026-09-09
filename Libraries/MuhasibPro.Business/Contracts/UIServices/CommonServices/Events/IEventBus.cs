namespace MuhasibPro.Business.Contracts.UIServices.CommonServices.Events
{
    /// <summary>
    /// Tipli olay hattı — ham <c>IMessageService</c> transportunun üstünde idempotent abonelik sunar.
    /// Yeni kod burayı kullanır; ham string Send yeni kodda yasaktır.
    /// </summary>
    public interface IEventBus
    {
        void Publish<TEvent>(object sender, TEvent @event) where TEvent : DomainEvent;
        void Subscribe<TEvent>(object target, Action<object, TEvent> handler) where TEvent : DomainEvent;
        void Unsubscribe(object target);
    }
}
