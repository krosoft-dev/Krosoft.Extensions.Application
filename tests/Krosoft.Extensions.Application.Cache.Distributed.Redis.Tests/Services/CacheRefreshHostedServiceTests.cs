using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Services;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Core;
using Krosoft.Extensions.Testing;
using Krosoft.Extensions.Testing.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Services;

[TestClass]
public class CacheRefreshHostedServiceTests : BaseTest
{
    private CacheScheduleConfig<AuthCacheRefreshCommand> _config = null!;
    private Mock<ILogger<CacheRefreshHostedService<AuthCacheRefreshCommand>>> _loggerMock = null!;
    private Mock<IMediator> _mediatorMock = null!;
    private TestableCacheRefreshHostedService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CacheRefreshHostedService<AuthCacheRefreshCommand>>>();
        _mediatorMock = new Mock<IMediator>();
        _config = new CacheScheduleConfig<AuthCacheRefreshCommand>
        {
            Interval = TimeSpan.FromMinutes(5),
            Command = new AuthCacheRefreshCommand(false, false)
        };

        _service = new TestableCacheRefreshHostedService(_loggerMock.Object, _config, _mediatorMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _service.Dispose();
    }

    [TestMethod]
    public async Task DoWork_CommandeConfiguree_EnvoieLaCommandeAuMediateur()
    {
        await _service.RunAsync(CancellationToken.None);

        _mediatorMock.Verify(m => m.Send((object)_config.Command!, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DoWork_MediateurEnErreur_LogUneErreurSansPropager()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new InvalidOperationException("Redis injoignable."));

        await _service.RunAsync(CancellationToken.None);

        _loggerMock.Verify(LogLevel.Error, "Redis injoignable.", Times.Once());
    }
}
