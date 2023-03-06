namespace Rabbit.EventBus.Config.Eventos
{
    /// <summary>
    /// Contrato para manipuladores de eventos. Os manipuladores de eventos são responsáveis por processar eventos quando eles acontecem.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    public interface IEventHandler<in TEvent>
        where TEvent : Event
    {
        Task HandleAsync(TEvent @event);
    }
}
