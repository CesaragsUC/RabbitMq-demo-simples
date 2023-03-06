using Rabbit.EventBus.Config.Eventos;
using Rabbit.Subscription.Demo.IntegrationEvents.Events;

namespace Rabbit.Subscription.Demo.IntegrationEvents.EventHandlers
{
    public class ProdutoventHandler : 
        IEventHandler<ProdutoCadastradoEvent>,
        IEventHandler<ProdutoAtualizadoEvent>,
        IEventHandler<ProdutoExcluidoEvent>

    {
        private readonly ILogger<ProdutoventHandler> _logger;

        public ProdutoventHandler(ILogger<ProdutoventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(ProdutoCadastradoEvent message)
        {
            string result = $"O produto {message.Nome} com preço: {message.Preco} que possui: {message.Quantidade} quantidades, foi cadastrado na data: {message.CreatAt}.";

            // Aqui você lida com o que acontece quando recebe um evento desse tipo do barramento de eventos.
            _logger.LogWarning(result);
            return Task.CompletedTask;
        }

        public Task HandleAsync(ProdutoAtualizadoEvent message)
        {
            _logger.LogWarning("-- Produto atualizado --");

            string result = $"Produto {message.Nome} preço: {message.Preco} Quantidade: {message.Quantidade} foi atualizado: {DateTime.Now}.";
            _logger.LogWarning(result);
            return Task.CompletedTask;
        }

        public Task HandleAsync(ProdutoExcluidoEvent message)
        {
            string result = $"O produto {message.Nome} com ID: {message.Id} foi excluido {DateTime.Now}.";
            _logger.LogWarning(result);
            return Task.CompletedTask;
        }
    }
}
