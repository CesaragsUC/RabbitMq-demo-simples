using Microsoft.EntityFrameworkCore;
using Rabbit.Subscription.Domain;
using Rabbit.Subscription.Interfaces;

namespace RabbitAppContext.Repository
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly RabbitContext _ctx;

        public ProdutoRepository(RabbitContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<Produto>> All()
        {
           return  await _ctx.Produtos.AsNoTracking().ToListAsync();
        }

        public async Task<Produto> Get(Guid id)
        {
            return _ctx.Produtos.FirstOrDefault(x => x.Id == id);
        }

        public async Task Add(Produto produto)
        {
            _ctx.Add(produto);
            _ctx.SaveChanges();
        }
        public async Task Update(Produto produto)
        {
            _ctx.Update(produto);
            _ctx.SaveChanges();
        }

        public async Task Delete(Guid id)
        {
            var produto =  _ctx.Produtos.FirstOrDefault(x => x.Id == id);
            if (produto != null)
            {
                _ctx.Produtos.Remove(produto);
                _ctx.SaveChanges();
            }
                
        }

    }
}
