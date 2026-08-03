using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Core;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Core.Interfaces;
using Krosoft.Extensions.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Handlers.Commands;

[TestClass]
public class TenantCacheRefreshCommandBaseHandlerTests : BaseTest
{
    private const string CacheKeyLastRefresh = "cache-last-refresh";
    private static readonly DateTimeOffset Now = new(2026, 8, 3, 14, 30, 0, TimeSpan.Zero);

    private Mock<IDateTimeService> _dateTimeServiceMock = null!;
    private Mock<ILogger<TenantCacheRefreshCommandBaseHandler>> _loggerMock = null!;
    private ServiceProvider _serviceProvider = null!;
    private Mock<ITenantDistributedCacheProvider> _tenantDistributedCacheProviderMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<TenantCacheRefreshCommandBaseHandler>>();
        _tenantDistributedCacheProviderMock = new Mock<ITenantDistributedCacheProvider>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _dateTimeServiceMock.Setup(x => x.Now).Returns(Now);
        _serviceProvider = CreateServiceCollection();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }

    private TestTenantCacheRefreshCommandHandler CreateHandler(params string[] tenantsId) =>
        new(CacheKeyLastRefresh,
            _loggerMock.Object,
            _serviceProvider,
            _tenantDistributedCacheProviderMock.Object,
            _dateTimeServiceMock.Object,
            tenantsId);

    [TestMethod]
    public async Task Handle_TenantCourantRenseigne_RafraichitUniquementCeTenant()
    {
        var handler = CreateHandler("tenant-1", "tenant-2", "tenant-3");
        var command = new TenantCacheRefreshCommand(false) { CurrentTenantId = "tenant-2" };

        var result = await handler.Handle(command, CancellationToken.None);

        Check.That(result).IsEqualTo(Unit.Value);
        Check.That(handler.TenantsRafraichis).ContainsExactly("tenant-2");
        Check.That(handler.GetTenantsIdCount).IsEqualTo(0);
    }

    [TestMethod]
    public async Task Handle_TenantCourantRenseigne_EcritLaDateDeDernierRafraichissementDuTenant()
    {
        var handler = CreateHandler();
        var command = new TenantCacheRefreshCommand(false) { CurrentTenantId = "tenant-2" };

        await handler.Handle(command, CancellationToken.None);

        _tenantDistributedCacheProviderMock.Verify(p => p.SetAsync("tenant-2",
                                                                   CacheKeyLastRefresh,
                                                                   Now,
                                                                   It.IsAny<CancellationToken>()),
                                                   Times.Once);
    }

    [TestMethod]
    public async Task Handle_TenantCourantVide_RafraichitTousLesTenants()
    {
        var handler = CreateHandler("tenant-1", "tenant-2", "tenant-3");
        var command = new TenantCacheRefreshCommand(false);

        await handler.Handle(command, CancellationToken.None);

        Check.That(handler.GetTenantsIdCount).IsEqualTo(1);
        Check.That(handler.TenantsRafraichis).ContainsExactly("tenant-1", "tenant-2", "tenant-3");
    }

    [TestMethod]
    public async Task Handle_TenantCourantVide_EcritLaDateDeDernierRafraichissementDeChaqueTenant()
    {
        var handler = CreateHandler("tenant-1", "tenant-2");
        var command = new TenantCacheRefreshCommand(false) { CurrentTenantId = string.Empty };

        await handler.Handle(command, CancellationToken.None);

        _tenantDistributedCacheProviderMock.Verify(p => p.SetAsync("tenant-1", CacheKeyLastRefresh, Now, It.IsAny<CancellationToken>()), Times.Once);
        _tenantDistributedCacheProviderMock.Verify(p => p.SetAsync("tenant-2", CacheKeyLastRefresh, Now, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_AucunTenant_NEcritRien()
    {
        var handler = CreateHandler();
        var command = new TenantCacheRefreshCommand(false);

        await handler.Handle(command, CancellationToken.None);

        Check.That(handler.TenantsRafraichis).IsEmpty();
        _tenantDistributedCacheProviderMock.Verify(p => p.SetAsync(It.IsAny<string>(),
                                                                   It.IsAny<string>(),
                                                                   It.IsAny<DateTimeOffset>(),
                                                                   It.IsAny<CancellationToken>()),
                                                   Times.Never);
    }

    [TestMethod]
    public async Task Handle_QuelQueSoitLeTenant_AppelleBeforeEtAfter()
    {
        var handler = CreateHandler("tenant-1");
        var command = new TenantCacheRefreshCommand(false);

        await handler.Handle(command, CancellationToken.None);

        Check.That(handler.BeforeCount).IsEqualTo(1);
        Check.That(handler.AfterCount).IsEqualTo(1);
    }
}
