using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Core;

/// <summary>
/// Expose le traitement planifié de <see cref="CacheRefreshHostedService{TCommand}" /> sans passer par le timer.
/// </summary>
internal sealed class TestableCacheRefreshHostedService : CacheRefreshHostedService<AuthCacheRefreshCommand>
{
    public TestableCacheRefreshHostedService(ILogger<CacheRefreshHostedService<AuthCacheRefreshCommand>> logger,
                                             CacheScheduleConfig<AuthCacheRefreshCommand> config,
                                             IMediator mediator)
        : base(logger, config, mediator)
    {
    }

    public Task RunAsync(CancellationToken cancellationToken) => DoWork(cancellationToken);
}
