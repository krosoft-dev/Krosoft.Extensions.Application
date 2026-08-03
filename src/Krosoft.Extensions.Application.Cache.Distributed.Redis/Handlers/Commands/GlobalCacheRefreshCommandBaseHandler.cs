using System.Diagnostics;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Core.Extensions;
using Krosoft.Extensions.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Commands;

public abstract class GlobalCacheRefreshCommandBaseHandler : IRequestHandler<GlobalCacheRefreshCommand, Unit>
{
    private readonly string _cacheKeyLastRefresh;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<GlobalCacheRefreshCommandBaseHandler> _logger;
    private readonly IServiceProvider _serviceProvider;
    protected readonly IDistributedCacheProvider DistributedCacheProvider;

    protected GlobalCacheRefreshCommandBaseHandler(string cacheKeyLastRefresh,
                                                   ILogger<GlobalCacheRefreshCommandBaseHandler> logger,
                                                   IServiceProvider serviceProvider,
                                                   IDistributedCacheProvider distributedCacheProvider,
                                                   IDateTimeService dateTimeService)
    {
        _cacheKeyLastRefresh = cacheKeyLastRefresh;
        _logger = logger;
        _serviceProvider = serviceProvider;
        DistributedCacheProvider = distributedCacheProvider;
        _dateTimeService = dateTimeService;
    }

    public async Task<Unit> Handle(GlobalCacheRefreshCommand request,
                                   CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refresh du cache...");

        var sw = Stopwatch.StartNew();

        await BeforeAsync(cancellationToken);

        using (var scope = _serviceProvider.CreateScope())
        {
            await RefreshAsync(scope, cancellationToken);
        }

        await DistributedCacheProvider.SetAsync(_cacheKeyLastRefresh, _dateTimeService.Now, cancellationToken);

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

    protected abstract Task RefreshAsync(IServiceScope scope, CancellationToken cancellationToken);
}