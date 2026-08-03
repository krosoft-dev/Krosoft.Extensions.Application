using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models;
using Krosoft.Extensions.Hosting.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Services;

public class CacheRefreshHostedService<TCommand> : ScheduledHostedService
{
    private readonly CacheScheduleConfig<TCommand> _config;
    private readonly IMediator _mediator;

    public CacheRefreshHostedService(ILogger<CacheRefreshHostedService<TCommand>> logger,
                                     CacheScheduleConfig<TCommand> config,
                                     IMediator mediator) : base(logger, config)
    {
        _config = config;
        _mediator = mediator;
    }

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        Logger.LogInformation("{ServiceName} > {Timestamp:hh:mm:ss} > Run", nameof(CacheRefreshHostedService<TCommand>), DateTime.Now);

        try
        {
            await _mediator.Send(_config.Command!, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }
}