namespace Rabbit.EventBus
{
    public abstract class Event
    {
        public Guid Id { get; set; }
        public DateTime CreatAt { get; set; }

        protected Event()
        {

        }
    }


    public class FalhaPagamento: Event
    {
        public string TransacaoId { get; set; }
        public string Descricao { get; set; }

        public FalhaPagamento(string descricao, string transacaoId)
        {
            Descricao = descricao;
            TransacaoId = transacaoId;
        }
    }

}
