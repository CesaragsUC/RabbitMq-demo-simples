using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.AutoMock;
using Polly;
using Rabbit.Subscription.Demo.Tests;
using Rabbit.Subscription.Interfaces;
using RabbitAppContext;
using RabbitAppContext.Repository;
using Xunit;

namespace Rabbit.Subscription.Domain.Tests
{
    /// <summary>
    /// Esse teste em especifico esta sendo usado SQLite
    /// </summary>

    [TestCaseOrderer("Rabbit.Subscription.Domain.Tests.PriorityOrderer", "Rabbit.Subscription.Domain.Tests")]
    public class ProdutoTest : IDisposable
    {
        private readonly ShareDatabaseFixture _databaseFixture;
        private readonly RabbitContext _context;
        private ProdutoFactory _produtoFactory;

        public ProdutoTest()
        {
            _databaseFixture = new ShareDatabaseFixture();
            _context = _databaseFixture.CreateContext();
            _produtoFactory = new ProdutoFactory();
        }


        [Fact(DisplayName = "Cadastrar Produto com sucesso"), TestPriority(1)]
        [Trait("Produto", "Repositorio")]
        public async void Cadastrar_ProdutoValido_Deve_Retornar_OK()
        {
            // Arrange
            var produto = _produtoFactory.Produto;
            // Act

            _context.Produtos.Add(produto);
            var result = _context.SaveChanges();

            var produtoSalvo = _context.Produtos.FirstOrDefault(x => x.Id == produto.Id);

            // Assert
            Xunit.Assert.Equal(1, result);
            Xunit.Assert.NotNull(produtoSalvo);
        }


        [Fact(DisplayName = "Atualizar produto com sucesso"), TestPriority(3)]
        [Trait("Produto", "Repositorio")]
        public async void Atualizar_ProdutoValido_Deve_Retornar_Ok()
        {
            // Arrange
            var novoproduto = _produtoFactory.Produto;

            _context.Produtos.Add(novoproduto);
            _context.SaveChanges();

            // Act

            var produtoAtualizado = _context.Produtos.FirstOrDefault(x => x.Id == novoproduto.Id);

            produtoAtualizado.CreatAt = DateTime.Now;
            produtoAtualizado.Nome = "Balança Digital";
            produtoAtualizado.Preco = 180;
            produtoAtualizado.Quantidade = 22;

            var produto = _context.Entry(produtoAtualizado);
            produto.State = EntityState.Modified;

            _context.Produtos.Update(produtoAtualizado);
            var result = _context.SaveChanges();

            // Assert
            Xunit.Assert.Equal(1, result);

        }

        [Fact(DisplayName = "Atualizar produto invalido"), TestPriority(4)]
        [Trait("Produto", "Repositorio")]
        public void Atualizar_ProdutoInValido_Deve_Retornar_Erro()
        {
            // Arrange
            var novoproduto = _produtoFactory.Produto;

            _context.Produtos.Add(novoproduto);
            _context.SaveChanges();

            // Act
            Guid IdInvalido = Guid.NewGuid();

            var produtoCadastrado = _context.Produtos.FirstOrDefault(x => x.Id == IdInvalido);

            //Assert
            Xunit.Assert.Null(produtoCadastrado);

        }
        [Fact(DisplayName = "Deletar produto com sucesso"), TestPriority(5)]
        [Trait("Produto", "Repositorio")]
        public void Deletar_ProdutoValido_Deve_Retornar_Ok()
        {
            // Arrange
            var novoproduto = _produtoFactory.Produto;

            _context.Produtos.Add(novoproduto);
            _context.SaveChanges();

            _context.Produtos.Remove(novoproduto);
            var result = _context.SaveChanges();
            //Assert
            Xunit.Assert.Equal(1, result);

        }


        [Fact(DisplayName = "Obeter todos produtos cadastrados")]
        [Trait("Produto", "Repositorio"), TestPriority(7)]
        public async void Obter_Todos_Produtos()
        {
            var produtos = new List<Produto>(); 

            // Arrange
            var produtoLista = _produtoFactory.Produtos;

            foreach (var item in produtoLista)
            {
                _context.Produtos.Add(item);
                _context.SaveChanges();
            }


            // Act

            produtos = await _context.Produtos.AsNoTracking().ToListAsync();

            // Assert
            Xunit.Assert.NotNull(produtos);
            Xunit.Assert.True(produtos.Any());
            Xunit.Assert.True(produtos.Count() > 0);
        }

        public void Dispose()
        {

        }
    }
}
