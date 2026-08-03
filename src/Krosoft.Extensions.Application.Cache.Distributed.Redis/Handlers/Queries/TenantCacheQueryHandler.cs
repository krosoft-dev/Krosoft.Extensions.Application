using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Queries;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Queries;

public class TenantCacheQueryHandler : IRequestHandler<TenantCacheQuery, IDictionary<string, long>>
{
    private readonly ILogger<TenantCacheQueryHandler> _logger;
    private readonly ITenantDistributedCacheProvider _tenantDistributedCacheProvider;

    public TenantCacheQueryHandler(ILogger<TenantCacheQueryHandler> logger, ITenantDistributedCacheProvider tenantDistributedCacheProvider)
    {
        _tenantDistributedCacheProvider = tenantDistributedCacheProvider;
        _logger = logger;
    }

    public async Task<IDictionary<string, long>> Handle(TenantCacheQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Récupération du contenu du cache du tenant {TenantId}...", request.CurrentTenantId);

        var lengthByKey = new Dictionary<string, long>();
        var keys = _tenantDistributedCacheProvider.GetKeys(request.CurrentTenantId!, string.Empty);
        foreach (var key in keys)
        {
            var length = await _tenantDistributedCacheProvider.GetLengthAsync(request.CurrentTenantId!, key, cancellationToken);
            lengthByKey.Add(key, length);
        }

        return lengthByKey;
    }
}