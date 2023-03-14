using Bogus;
using Bogus.DataSets;

namespace Rabbit.Subscription.Domain.Tests
{
    internal class ProdutoFactory
    {
        public Guid _produtoId = Guid.NewGuid();

        public Produto Produto;
        public Produto ProdutoInvalido;
        public List<Produto> Produtos;

        public ProdutoFactory()
        {
            Produto = NovoProduto();
            ProdutoInvalido = NovoProdutoInvalido();
            Produtos = CriarProdutoEmMassa();
        }


        public Produto NovoProduto()
        {
            var produto = new Produto
            {
                Id = _produtoId,
                CreatAt = DateTime.Now,
                Nome = "Produto Test",
                Preco = 160,
                Quantidade = 36
            };
            return produto;
        }

        public Produto NovoProdutoInvalido()
        {
            var produto = new Produto
            {
                CreatAt = DateTime.Now,
                Id = Guid.Empty,
                Nome = "",
                Preco = 0,
                Quantidade = 0
            };
            return produto;
        }

        public List<Produto> CriarProdutoEmMassa()
        {
            //cria produtos com Bogus
            var produtoFaker = new Faker<Produto>("pt_BR")
                .RuleFor(c => c.Id, f => f.Random.Guid())
                .RuleFor(c => c.Nome, f => f.Name.FirstName(Name.Gender.Male))
                .RuleFor(c => c.Preco, f => f.Random.Int(18, 1000))
                .RuleFor(c => c.Quantidade, f => f.Random.Int(1, 10))
                .RuleFor(c => c.CreatAt, DateTime.Now);

            var produtoListList = produtoFaker.Generate(10);
            return produtoListList;
        }
    }
}
