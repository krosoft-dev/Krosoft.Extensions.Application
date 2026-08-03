using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Commands;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Core;

/// <summary>
/// Implémentation concrète de <see cref="TenantCacheRefreshCommandBaseHandler" /> permettant de tester la classe de base.
/// </summary>
internal sealed class TestTenantCacheRefreshCommandHandler : TenantCacheRefreshCommandBaseHandler
{
    private readonly IEnumerable<string> _tenantsId;

    public TestTenantCacheRefreshCommandHandler(string cacheKeyLastRefresh,
                                                ILogger<TenantCacheRefreshCommandBaseHandler> logger,
                                                IServiceProvider serviceProvider,
                                                ITenantDistributedCacheProvider tenantDistributedCacheProvider,
                                                IDateTimeService dateTimeService,
                                                IEnumerable<string> tenantsId)
        : base(cacheKeyLastRefresh, logger, serviceProvider, tenantDistributedCacheProvider, dateTimeService)
    {
        _tenantsId = tenantsId;
    }

    public int AfterCount { get; private set; }
    public int BeforeCount { get; private set; }
    public int GetTenantsIdCount { get; private set; }
    public List<string> TenantsRafraichis { get; } = new();

    protected override Task AfterAsync(CancellationToken cancellationToken)
    {
        AfterCount++;
        return Task.CompletedTask;
    }

    protected override Task BeforeAsync(CancellationToken cancellationToken)
    {
        BeforeCount++;
        return Task.CompletedTask;
    }

    protected override Task<IEnumerable<string>> GetTenantsIdAsync(CancellationToken cancellationToken)
    {
        GetTenantsIdCount++;
        return Task.FromResult(_tenantsId);
    }

    protected override Task RefreshAsync(string tenantId, CancellationToken cancellationToken)
    {
        TenantsRafraichis.Add(tenantId);
        return Task.CompletedTask;
    }
}
