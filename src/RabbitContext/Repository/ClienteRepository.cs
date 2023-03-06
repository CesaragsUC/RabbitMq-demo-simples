using Microsoft.EntityFrameworkCore;
using Rabbit.Subscription.Domain;
using Rabbit.Subscription.Interfaces;

namespace RabbitAppContext.Repository
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly RabbitContext _ctx;

        public ClienteRepository(RabbitContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<Cliente>> All()
        {
           return  await _ctx.Cliente.AsNoTracking().ToListAsync();
        }

        public async Task<Cliente> Get(Guid id)
        {
            return _ctx.Cliente.FirstOrDefault(x => x.Id == id);
        }

        public async Task Add(Cliente cliente)
        {
            _ctx.Add(cliente);
            _ctx.SaveChanges();
        }
        public async Task Update(Cliente cliente)
        {
            _ctx.Update(cliente);
            _ctx.SaveChanges();
        }

        public async Task Delete(Guid id)
        {
            var cliente =  _ctx.Cliente.FirstOrDefault(x => x.Id == id);
            if (cliente != null)
            {
                _ctx.Cliente.Remove(cliente);
                _ctx.SaveChanges();
            }
                
        }

    }
}
