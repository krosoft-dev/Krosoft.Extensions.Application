using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Commands;

public class PayloadCacheRefreshCommandHandler : IRequestHandler<PayloadCacheRefreshCommand>
{
    private readonly ILogger<PayloadCacheRefreshCommandHandler> _logger;
    private readonly IMediator _mediator;

    public PayloadCacheRefreshCommandHandler(ILogger<PayloadCacheRefreshCommandHandler> logger,
                                             IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Handle(PayloadCacheRefreshCommand request,
                             CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refresh du cache...");

        var message = JsonConvert.DeserializeObject<CacheRefreshMessage>(request.Payload);
        if (message != null)
        {
            _logger.LogInformation("Refresh du cache pour le tenant {TenantId}", message.TenantId);
            var command = new AuthCacheRefreshCommand(false, false)
            {
                CurrentTenantId = message.TenantId
            };

            await _mediator.Send(command, cancellationToken);
        }
        else
        {
            _logger.LogError("Impossible de refresh le cache à partir du payload : {Payload}", request.Payload);
        }
    }
}