using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Queries;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Queries;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Testing;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Handlers.Queries;

[TestClass]
public class TenantCacheQueryHandlerTests : BaseTest
{
    private const string TenantId = "tenant-1";

    private TenantCacheQueryHandler _handler = null!;
    private Mock<ILogger<TenantCacheQueryHandler>> _loggerMock = null!;
    private Mock<ITenantDistributedCacheProvider> _tenantDistributedCacheProviderMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<TenantCacheQueryHandler>>();
        _tenantDistributedCacheProviderMock = new Mock<ITenantDistributedCacheProvider>();

        _handler = new TenantCacheQueryHandler(_loggerMock.Object, _tenantDistributedCacheProviderMock.Object);
    }

    [TestMethod]
    public async Task Handle_CacheAlimente_RetourneLaTailleDeChaqueCleDuTenant()
    {
        _tenantDistributedCacheProviderMock.Setup(p => p.GetKeys(TenantId, string.Empty))
                                           .Returns(new[] { "adresses", "clients" });
        _tenantDistributedCacheProviderMock.Setup(p => p.GetLengthAsync(TenantId, "adresses", It.IsAny<CancellationToken>()))
                                           .ReturnsAsync(3);
        _tenantDistributedCacheProviderMock.Setup(p => p.GetLengthAsync(TenantId, "clients", It.IsAny<CancellationToken>()))
                                           .ReturnsAsync(12);

        var query = new TenantCacheQuery { CurrentTenantId = TenantId };

        var result = await _handler.Handle(query, CancellationToken.None);

        Check.That(result).HasSize(2);
        Check.That(result["adresses"]).IsEqualTo(3);
        Check.That(result["clients"]).IsEqualTo(12);
    }

    [TestMethod]
    public async Task Handle_CacheVide_RetourneUnDictionnaireVide()
    {
        _tenantDistributedCacheProviderMock.Setup(p => p.GetKeys(TenantId, string.Empty))
                                           .Returns(Array.Empty<string>());

        var query = new TenantCacheQuery { CurrentTenantId = TenantId };

        var result = await _handler.Handle(query, CancellationToken.None);

        Check.That(result).IsEmpty();
        _tenantDistributedCacheProviderMock.Verify(p => p.GetLengthAsync(It.IsAny<string>(),
                                                                         It.IsAny<string>(),
                                                                         It.IsAny<CancellationToken>()),
                                                   Times.Never);
    }
}
