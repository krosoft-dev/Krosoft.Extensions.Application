using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Events;

public class TenantsRefreshCacheEventHandler : INotificationHandler<TenantsRefreshCacheEvent>
{
    private readonly ILogger<TenantsRefreshCacheEventHandler> _logger;
    private readonly IMediator _mediator;

    public TenantsRefreshCacheEventHandler(ILogger<TenantsRefreshCacheEventHandler> logger,
                                           IMediator mediator)
    {
        _logger = logger;

        _mediator = mediator;
    }

    public async Task Handle(TenantsRefreshCacheEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mise à jour du cache pour {TenantCount} tenants...", notification.TenantsId.Count);

        foreach (var tenantId in notification.TenantsId)
        {
            _logger.LogInformation("Mise à jour du cache pour le tenant {TenantId}...", tenantId);

            var command = new TenantCacheRefreshCommand(false)
            {
                CurrentTenantId = tenantId
            };

            await _mediator.Send(command, cancellationToken);
        }
    }
}