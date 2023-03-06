﻿using Rabbit.EventBus.Config.Eventos;

namespace Rabbit.EventBus.Config.Subscription
{
    /// <summary>
    /// Contrato que define como os eventos são rastreados no aplicativo..
    /// A implementação dessa classe controla as assinaturas atuais, bem como resolve manipuladores de eventos para uso.
    /// </summary>
    public interface IEventBusSubscriptionManager
    {
        #region Event Handlers
        event EventHandler<string> OnEventRemoved;
        #endregion

        #region Status
        bool IsEmpty { get; }
        bool HasSubscriptionsForEvent(string eventName);
        #endregion

        #region Events info
        string GetEventIdentifier<TEvent>();
        Type GetEventTypeByName(string eventName);
        IEnumerable<Subscription> GetHandlersForEvent(string eventName);
        Dictionary<string, List<Subscription>> GetAllSubscriptions();
        #endregion

        #region Subscription management
        void AddSubscription<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;

        void RemoveSubscription<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;

        void Clear();
        #endregion
    }
}
