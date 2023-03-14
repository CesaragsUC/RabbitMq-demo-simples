using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq.AutoMock;
using Rabbit.Subscription.Domain;
using RabbitAppContext;
using System.Net.Http.Json;
using Xunit;


namespace Rabbit.Subscription.Demo.Tests
{
    [TestCaseOrderer("Rabbit.Subscription.Demo.Tests.PriorityOrderer", "Rabbit.Subscription.Demo.Tests")]
    [CollectionDefinition(nameof(IntegrationApiFixture))]
    public class ProdutoIntegrationTest : IClassFixture<IntegrationApiTestFixture<Program>>
    {
        private readonly IntegrationApiTestFixture<Program> _fixture;
        private readonly AutoMocker _mocker;
        private string _config;

        private void IniciarMocks()
        {
            _config = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                                .Build()
                                    .GetSection("RabbitMQ")["ConnectionUrl"];
        }


        public ProdutoIntegrationTest(IntegrationApiTestFixture<Program> fixture)
        {
            IniciarMocks();
            _mocker = new AutoMocker();
            _fixture = fixture;

            var _options = new DbContextOptionsBuilder<RabbitContext>()
                               .UseInMemoryDatabase("MyDemoRabbitMqSubscription").Options;

        }

        [Fact(DisplayName = "Cadastrar produto com sucesso"), TestPriority(1)]
        [Trait("Produto", "Integração - API Produto")]
        public async void Cadastrar_Produto_Com_Sucesso_Deve_Retornar_OK()
        {
            //Arrage
            //Act
            var response = await _fixture.Client.PostAsJsonAsync("api/Produto/Add", _fixture.Produto);

            var produtoCadastrado = await response.Content.ReadFromJsonAsync<Produto>();

            //Assert
            response.EnsureSuccessStatusCode();
            Xunit.Assert.True(response.IsSuccessStatusCode);

        }

        [Fact(DisplayName = "Cadastrar produto com erro")]
        [Trait("Produto", "Integração - API Produto"), TestPriority(5)]
        public async void Cadastrar_Produto_Invalido_Deve_Retornar_Error()
        {
            //Arrage
            //Act
            var response = await _fixture.Client.PostAsJsonAsync("api/Produto/Add", _fixture.ProdutoInvalido);

            //Assert
            Xunit.Assert.False(response.IsSuccessStatusCode);

        }

        [Fact(DisplayName = "Cadastrar produto em massa com sucesso"), TestPriority(6)]
        [Trait("Produto", "Integração - API Produto")]
        public async void Cadastrar_Produto_Em_Massa_Deve_Retornar_Ok()
        {
            //Arrage
            //Act
            var response = await _fixture.Client.PostAsync("api/Produto/AutoAdd", null);

            //Assert
            response.EnsureSuccessStatusCode();
            Xunit.Assert.True(response.IsSuccessStatusCode);
        }

        [Fact(DisplayName = "Atualizar produto com sucesso"), TestPriority(2)]
        [Trait("Produto", "Integração - API Produto")]
        public async void Atualizar_Produto_Deve_Retornar_Ok()
        {
            //Arrage
            var produtoResult = new Produto();  

            var requestResult = await _fixture.Client.GetAsync($"api/Produto/get/{_fixture.Produto.Id}");

            if(requestResult.IsSuccessStatusCode)
                 produtoResult = await requestResult.Content.ReadFromJsonAsync<Produto>();


            var produto = new Produto
            {
                Id = _fixture.Produto.Id,
                CreatAt = DateTime.Now,
                Nome = "Balança Digital",
                Preco = 180,
                Quantidade = 22
            };
            _fixture.Produto = produto;

            //Act
            var response = await _fixture.Client.PutAsJsonAsync("api/Produto/Update", produto);

            //Assert
            response.EnsureSuccessStatusCode();
            Xunit.Assert.True(response.IsSuccessStatusCode);
        }

        [Fact(DisplayName = "Atualizar produto com Invalido"), TestPriority(3)]
        [Trait("Produto", "Integração - API Produto")]
        public async void Atualizar_Produto_Invalido_Deve_Retornar_Erro()
        {
            //Arrage
            //Act
            var response = await _fixture.Client.PutAsJsonAsync("api/Produto/Update", _fixture.ProdutoInvalido);

            //Assert
            Xunit.Assert.False(response.IsSuccessStatusCode);

        }

        [Fact(DisplayName = "Deletar produto com sucesso"), TestPriority(4)]
        [Trait("Produto", "Integração - API Produto")]
        public async void Deletar_Produto_Com_Sucesso_Deve_Retornar_OK()
        {
            //Arrage
            var produtoResult = new Produto();

            var requestResult = await _fixture.Client.GetAsync($"api/Produto/get/{_fixture.Produto.Id}");

            if (requestResult.IsSuccessStatusCode)
                produtoResult = await requestResult.Content.ReadFromJsonAsync<Produto>();

            //Act
            var response = await _fixture.Client.DeleteAsync($"api/Produto/Delete/{produtoResult.Id}");

            //Assert
            response.EnsureSuccessStatusCode();
            Xunit.Assert.True(response.IsSuccessStatusCode);
        }

        [Fact(DisplayName = "Deletar produto inexistente"), TestPriority(7)]
        [Trait("Produto", "Integração - API Produto")]
        public async void Deletar_Produto_Invalido_Deve_Retornar_Erro()
        {
            //Act
            var response = await _fixture.Client.DeleteAsync($"api/Produto/Delete/{Guid.NewGuid()}");

            //Assert
            Xunit.Assert.False(response.IsSuccessStatusCode);
        }

    }
}