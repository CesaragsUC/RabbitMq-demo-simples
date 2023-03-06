using Rabbit.Subscription.Domain;


namespace Rabbit.Subscription.Interfaces
{
    public interface IProdutoRepository
    {
        Task Add(Produto produto);
        Task Update(Produto produto);
        Task Delete(Guid id);
        Task<Produto> Get(Guid id);
        Task<IEnumerable<Produto>> All();
    }
}
