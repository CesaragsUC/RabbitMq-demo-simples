using Rabbit.EventBus.Config.Eventos;

namespace Rabbit.Subscription.Demo.IntegrationEvents.Events
{
    public class ProdutoExcluidoEvent : Event
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public decimal Preco { get; private set; }
        public int Quantidade { get; private set; }
        public DateTime CreatAt { get; private set; }

        public ProdutoExcluidoEvent(Guid id,string nome, decimal preco, int quantidade, DateTime creatAt)
        {
            Nome = nome;
            Preco = preco;
            Quantidade = quantidade;
            CreatAt = creatAt;
            Id = id;
        }

    }
}
