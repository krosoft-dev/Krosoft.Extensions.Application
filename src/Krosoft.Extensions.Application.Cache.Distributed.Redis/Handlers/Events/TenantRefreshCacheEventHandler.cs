using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Events;

public class TenantRefreshCacheEventHandler : INotificationHandler<TenantRefreshCacheEvent>
{
    private readonly ILogger<TenantRefreshCacheEventHandler> _logger;
    private readonly IMediator _mediator;

    public TenantRefreshCacheEventHandler(ILogger<TenantRefreshCacheEventHandler> logger,
                                          IMediator mediator)
    {
        _logger = logger;

        _mediator = mediator;
    }

    public async Task Handle(TenantRefreshCacheEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mise à jour du cache pour le tenant {TenantId}...", notification.TenantId);

        var command = new TenantCacheRefreshCommand(false)
        {
            CurrentTenantId = notification.TenantId
        };

        await _mediator.Send(command, cancellationToken);
    }
}