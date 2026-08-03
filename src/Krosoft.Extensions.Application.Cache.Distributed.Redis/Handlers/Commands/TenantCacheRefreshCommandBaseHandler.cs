using System.Diagnostics;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Core.Extensions;
using Krosoft.Extensions.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Commands;

public abstract class TenantCacheRefreshCommandBaseHandler : IRequestHandler<TenantCacheRefreshCommand, Unit>
{
    private readonly string _cacheKeyLastRefresh;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<TenantCacheRefreshCommandBaseHandler> _logger;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ITenantDistributedCacheProvider TenantDistributedCacheProvider;

    protected TenantCacheRefreshCommandBaseHandler(string cacheKeyLastRefresh,
                                                   ILogger<TenantCacheRefreshCommandBaseHandler> logger,
                                                   IServiceProvider serviceProvider,
                                                   ITenantDistributedCacheProvider tenantDistributedCacheProvider,
                                                   IDateTimeService dateTimeService)
    {
        _cacheKeyLastRefresh = cacheKeyLastRefresh;
        _logger = logger;
        ServiceProvider = serviceProvider;
        TenantDistributedCacheProvider = tenantDistributedCacheProvider;
        _dateTimeService = dateTimeService;
    }

    public async Task<Unit> Handle(TenantCacheRefreshCommand request,
                                   CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refresh du cache...");

        var sw = Stopwatch.StartNew();

        await BeforeAsync(cancellationToken);

        if (string.IsNullOrEmpty(request.CurrentTenantId))
        {
            //On récupére tous les tenants.
            var tenantsId = (await GetTenantsIdAsync(cancellationToken)).ToList();

            _logger.LogInformation("Récupération de {TenantCount} tenants.", tenantsId.Count);
            foreach (var tenantId in tenantsId)
            {
                await HandleRefreshAsync(tenantId, cancellationToken);
            }
        }
        else
        {
            //Uniquement pour le tenant donné.
            await HandleRefreshAsync(request.CurrentTenantId, cancellationToken);
        }

        _logger.LogInformation("Refresh du cache en {Elapsed}", sw.Elapsed.ToShortString());

        await AfterAsync(cancellationToken);

        return Unit.Value;
    }

    protected virtual async Task AfterAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    protected virtual async Task BeforeAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    protected abstract Task<IEnumerable<string>> GetTenantsIdAsync(CancellationToken cancellationToken);

    private async Task HandleRefreshAsync(string tenantId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refresh du cache du {TenantId}...", tenantId);
        await RefreshAsync(tenantId, cancellationToken);

        await TenantDistributedCacheProvider.SetAsync(tenantId, _cacheKeyLastRefresh, _dateTimeService.Now, cancellationToken);
    }

    protected abstract Task RefreshAsync(string tenantId, CancellationToken cancellationToken);
}