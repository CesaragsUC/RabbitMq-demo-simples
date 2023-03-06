namespace Rabbit.Subscription.Domain
{
    public class Cliente
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public int Idade { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }

        public DateTime? UltimaAtualizacao { get; set; }
    }
}