using Rabbit.Subscription.Domain;


namespace Rabbit.Subscription.Interfaces
{
    public interface IClienteRepository
    {
        Task Add(Cliente cliente);
        Task Update(Cliente cliente);
        Task Delete(Guid id);
        Task<Cliente> Get(Guid id);
        Task<IEnumerable<Cliente>> All();
    }
}
