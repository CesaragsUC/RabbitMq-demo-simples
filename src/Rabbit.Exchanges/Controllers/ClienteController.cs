using AutoMapper;
using Bogus;
using Bogus.DataSets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rabbit.Exchanges.Integration;
using Rabbit.Exchanges.Models;
using Rabbit.Exchanges.RabbitMQConfig.Basic;
using Rabbit.Exchanges.Utils;
using Rabbit.Subscription.Domain;
using Rabbit.Subscription.Interfaces;
using RabbitAppContext;
using RabbitMQ.Client;
using System;

namespace Rabbit.Exchanges.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;
        private readonly IRabbitManager _manager;

        public ClienteController(
            IMapper mapper,
            IRabbitManager manager,
            IClienteRepository clienteRepository)
        {
            _mapper = mapper;
            _manager = manager;
            _clienteRepository = clienteRepository;
        }

        [HttpGet]
        [Route("all")]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        public async Task<IActionResult> All()
        {
            var clientes = await _clienteRepository.All();
            return Ok(clientes);
        }


        [HttpGet]
        [Route("get/{id}")]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var clientes = await _clienteRepository.Get(id);
            return Ok(clientes);
        }


        [HttpPost]
        [Route("Add")]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Add(ClienteModel model)
        {

            
            try
            {
                var cliente = _mapper.Map<Cliente>(model);
                cliente.DataCadastro = DateTime.Now;
                await _clienteRepository.Add(cliente);

                _manager.SendMessage(cliente, "Cliente-Cadastrado", ExchangeType.Topic, "FilaCliente", "ClienteCadastrado");

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }


            return Ok(new ResponseMessage("Cliente cadastrado com sucesso."));
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
                var clientes = new List<Cliente>();
                var clienteFaker = new Faker<Cliente>("pt_BR")
                    .RuleFor(c => c.Id, f => f.Random.Guid())
                    .RuleFor(c => c.Nome, f => f.Name.FirstName(Name.Gender.Male))
                    .RuleFor(c => c.Idade, f => f.Random.Int(18, 100))
                    .RuleFor(c => c.Ativo, false)
                    .RuleFor(c => c.DataCadastro, DateTime.Now);

                var clienteList = clienteFaker.Generate(10);

                foreach (var cliente in clienteList)
                {
                    await _clienteRepository.Add(cliente);
                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }


            return Ok(new ResponseMessage("Clientes cadastrado com sucesso."));
        }


        [HttpPost]
        [Route("Update")]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(ClienteModel model)
        {

            try
            {
                var cliente = await _clienteRepository.Get(model.Id);

                if (cliente is null) return BadRequest("Cliente não encontrado");

                cliente.Nome = model.Nome;
                cliente.Idade = model.Idade;
                cliente.Ativo = model.Ativo;


                await _clienteRepository.Update(cliente);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }


            return Ok(new ResponseMessage("Clientes atualizado com sucesso."));
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

                await _clienteRepository.Delete(id);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

            return Ok(new ResponseMessage("Cliente excluido com sucesso"));
        }

        static void AddCustomerData(WebApplication app)
        {
            var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetService<RabbitContext>();
        }

    }
}
