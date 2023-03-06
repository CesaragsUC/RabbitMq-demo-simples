using Rabbit.EventBus.Config.Eventos;

namespace Rabbit.EventBus.Config.Bus
{
    /// <summary>
    /// Contrato para o ônibus do evento. O barramento de eventos usa um intermediário de mensagem para enviar e assinar eventos.
    /// </summary>
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent @event)
            where TEvent : Event;

        void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;

        void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;
    }
}
