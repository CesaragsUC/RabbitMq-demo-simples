using Bogus;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Rabbit.EventBus.Config.Bus;
using Rabbit.Subscription.Demo.DTO;
using Rabbit.Subscription.Demo.IntegrationEvents.Events;
using Rabbit.Subscription.Domain;
using Rabbit.Subscription.Interfaces;
using RabbitAppContext;

namespace Rabbit.Subscription.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutoController : ControllerBase
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IEventBus _eventBus;
        public ProdutoController(IProdutoRepository produtoRepository, IEventBus eventBus)
        {
            _produtoRepository = produtoRepository;
            _eventBus = eventBus;
        }


        [HttpGet]
        [Route("all")]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        public async Task<IActionResult> All()
        {
            var Produtos = await _produtoRepository.All();
            return Ok(Produtos);
        }


        [HttpGet]
        [Route("get/{id}")]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var Produtos = await _produtoRepository.Get(id);
            return Ok(Produtos);
        }


        [HttpPost]
        [Route("Add")]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Add(Produto model)
        {

            try
            {
                model.Id = Guid.NewGuid();
                model.CreatAt = DateTime.Now;
                await _produtoRepository.Add(model);

                var evento = new ProdutoCadastradoEvent(model.Id,model.Nome, model.Preco, model.Quantidade, model.CreatAt);
                _eventBus.Publish(evento);

                return Ok("Produto Cadastrado e Mensagem enviada.");
            }
            catch (Exception ex)
            {
               return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        [Route("AutoAdd")]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AutoAdd()
        {


            try
            {
                //cria produtos com Bogus
                var produtoFaker = new Faker<Produto>("pt_BR")
                    .RuleFor(c => c.Id, f => f.Random.Guid())
                    .RuleFor(c => c.Nome, f => f.Name.FirstName(Name.Gender.Male))
                    .RuleFor(c => c.Preco, f => f.Random.Int(18, 1000))
                    .RuleFor(c => c.Quantidade, f=> f.Random.Int(1,10))
                    .RuleFor(c => c.CreatAt, DateTime.Now);

                var produtoListList = produtoFaker.Generate(10);

                foreach (var produto in produtoListList)
                {
                    await _produtoRepository.Add(produto);

                    var evento = new ProdutoCadastradoEvent(produto.Id, produto.Nome, produto.Preco, produto.Quantidade, produto.CreatAt);
                    _eventBus.Publish(evento);
                }

                return Ok("Produtos Cadastrados e Mensagens enviadas.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("Update")]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Produto model)
        {

            try
            {
                var produto = await _produtoRepository.Get(model.Id);

                if (produto is null) return BadRequest("Produto não encontrado");

                produto.Nome = model.Nome;
                produto.Quantidade = model.Quantidade;
                produto.Preco = model.Preco;


                await _produtoRepository.Update(produto);

                var evento = new ProdutoAtualizadoEvent(produto.Id, produto.Nome, produto.Preco, produto.Quantidade, produto.CreatAt);
                _eventBus.Publish(evento);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }


            return Ok(new ResponseMessage("Produtos atualizado com sucesso."));
        }

        [HttpPost]
        [Route("Delete/{id}")]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {

            try
            {
                if (id == Guid.Empty) return BadRequest("ID inválido");

                var produto = await _produtoRepository.Get(id);

                if (produto is null) return BadRequest("Produto não encontrado");

                await _produtoRepository.Delete(id);

                var evento = new ProdutoExcluidoEvent(produto.Id, produto.Nome, produto.Preco, produto.Quantidade, produto.CreatAt);
                _eventBus.Publish(evento);

                return Ok(new ResponseMessage("Produto excluido com sucesso"));
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        static void AddCustomerData(WebApplication app)
        {
            var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetService<RabbitContext>();
        }

    }
}
