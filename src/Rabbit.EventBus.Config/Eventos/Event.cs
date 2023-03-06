using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.EventBus.Config.Eventos
{
    /// <summary>
    /// Representa um evento de integração. 
    /// Um Evento é “algo que aconteceu no passado”. Um Evento de Integração é um evento que pode causar efeitos colaterais em outros microsserviços.
    /// </summary>
    public abstract class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
