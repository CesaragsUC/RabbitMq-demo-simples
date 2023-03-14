using Microsoft.AspNetCore.Mvc.Testing;
using Rabbit.Subscription.Domain;
using Xunit;

namespace Rabbit.Subscription.Demo.Tests
{

    [CollectionDefinition(nameof(IntegrationApiFixture))]
    public class IntegrationApiFixture : IClassFixture<IntegrationApiTestFixture<Program>>
    { }

    public class IntegrationApiTestFixture<TProgram> : IDisposable where TProgram: class
    {
        public readonly ApplicationFactory<TProgram> Factory;
        public HttpClient Client;
        public Produto Produto;
        public Produto ProdutoInvalido;
        public List<Produto> Produtos;
        public Guid _produtoId = Guid.NewGuid();

        public IntegrationApiTestFixture()
        {
            var clientOptions = new WebApplicationFactoryClientOptions
            { 
                AllowAutoRedirect= true,
                BaseAddress = new Uri("https://localhost"),
                HandleCookies=true,
                MaxAutomaticRedirections=7,
            };
            Factory = new ApplicationFactory<TProgram>();
            Client = Factory.CreateClient(clientOptions);
            Produto = NovoProduto();
            ProdutoInvalido = NovoProdutoInvalido();
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
                Id = _produtoId,
                Nome = "",
                Preco = 0,
                Quantidade = 0
            };
            return produto;
        }
        public Produto NovoProdutoInvalidoSemId()
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

        public void Dispose()
        {
            
        }
    }
}
