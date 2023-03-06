namespace Rabbit.Subscription.Domain
{
    public class Produto 
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
        public DateTime CreatAt { get; set; }

    }
}