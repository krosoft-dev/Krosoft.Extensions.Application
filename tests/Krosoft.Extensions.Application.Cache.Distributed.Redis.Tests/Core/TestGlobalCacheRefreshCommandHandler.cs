using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Commands;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Core;

/// <summary>
/// Implémentation concrète de <see cref="GlobalCacheRefreshCommandBaseHandler" /> permettant de tester la classe de base.
/// </summary>
internal sealed class TestGlobalCacheRefreshCommandHandler : GlobalCacheRefreshCommandBaseHandler
{
    public TestGlobalCacheRefreshCommandHandler(string cacheKeyLastRefresh,
                                                ILogger<GlobalCacheRefreshCommandBaseHandler> logger,
                                                IServiceProvider serviceProvider,
                                                IDistributedCacheProvider distributedCacheProvider,
                                                IDateTimeService dateTimeService)
        : base(cacheKeyLastRefresh, logger, serviceProvider, distributedCacheProvider, dateTimeService)
    {
    }

    public int AfterCount { get; private set; }
    public int BeforeCount { get; private set; }
    public int RefreshCount { get; private set; }
    public IServiceScope? ScopeUtilise { get; private set; }

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

    protected override Task RefreshAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        RefreshCount++;
        ScopeUtilise = scope;
        return Task.CompletedTask;
    }
}
