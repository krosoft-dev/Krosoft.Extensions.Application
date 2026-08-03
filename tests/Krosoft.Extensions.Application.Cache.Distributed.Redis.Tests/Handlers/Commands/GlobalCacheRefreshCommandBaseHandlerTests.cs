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
public class GlobalCacheRefreshCommandBaseHandlerTests : BaseTest
{
    private const string CacheKeyLastRefresh = "cache-global-last-refresh";
    private static readonly DateTimeOffset Now = new(2026, 8, 3, 14, 30, 0, TimeSpan.Zero);

    private Mock<IDateTimeService> _dateTimeServiceMock = null!;
    private Mock<IDistributedCacheProvider> _distributedCacheProviderMock = null!;
    private TestGlobalCacheRefreshCommandHandler _handler = null!;
    private Mock<ILogger<GlobalCacheRefreshCommandBaseHandler>> _loggerMock = null!;
    private ServiceProvider _serviceProvider = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<GlobalCacheRefreshCommandBaseHandler>>();
        _distributedCacheProviderMock = new Mock<IDistributedCacheProvider>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _dateTimeServiceMock.Setup(x => x.Now).Returns(Now);
        _serviceProvider = CreateServiceCollection();

        _handler = new TestGlobalCacheRefreshCommandHandler(CacheKeyLastRefresh,
                                                            _loggerMock.Object,
                                                            _serviceProvider,
                                                            _distributedCacheProviderMock.Object,
                                                            _dateTimeServiceMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }

    [TestMethod]
    public async Task Handle_CommandeGlobale_RafraichitLeCacheDansUnScopeDedie()
    {
        var result = await _handler.Handle(new GlobalCacheRefreshCommand(), CancellationToken.None);

        Check.That(result).IsEqualTo(Unit.Value);
        Check.That(_handler.RefreshCount).IsEqualTo(1);
        Check.That(_handler.ScopeUtilise).IsNotNull();
    }

    [TestMethod]
    public async Task Handle_CommandeGlobale_EcritLaDateDeDernierRafraichissement()
    {
        await _handler.Handle(new GlobalCacheRefreshCommand(), CancellationToken.None);

        _distributedCacheProviderMock.Verify(p => p.SetAsync(CacheKeyLastRefresh, Now, It.IsAny<CancellationToken>()),
                                             Times.Once);
    }

    [TestMethod]
    public async Task Handle_CommandeGlobale_AppelleBeforeEtAfter()
    {
        await _handler.Handle(new GlobalCacheRefreshCommand(), CancellationToken.None);

        Check.That(_handler.BeforeCount).IsEqualTo(1);
        Check.That(_handler.AfterCount).IsEqualTo(1);
    }
}
