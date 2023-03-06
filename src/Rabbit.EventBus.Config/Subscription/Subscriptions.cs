namespace Rabbit.EventBus.Config.Subscription
{
    /// <summary>
    /// Representa uma assinatura de evento. As assinaturas controlam quando ouvimos eventos.
    /// </summary>
    public class Subscription
    {
        public Type EventType { get; private set; }
        public Type HandlerType { get; private set; }

        public Subscription(Type eventType, Type handlerType)
        {
            EventType = eventType;
            HandlerType = handlerType;
        }
    }
}
