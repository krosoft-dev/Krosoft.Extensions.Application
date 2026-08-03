using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Events;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Events;
using Krosoft.Extensions.Testing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Handlers.Events;

[TestClass]
public class GlobalRefreshCacheEventHandlerTests : BaseTest
{
    private GlobalRefreshCacheEventHandler _handler = null!;
    private Mock<ILogger<GlobalRefreshCacheEventHandler>> _loggerMock = null!;
    private Mock<IMediator> _mediatorMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<GlobalRefreshCacheEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Unit.Value);

        _handler = new GlobalRefreshCacheEventHandler(_loggerMock.Object, _mediatorMock.Object);
    }

    [TestMethod]
    public async Task Handle_EvenementGlobal_EnvoieLaCommandeDeRafraichissementGlobal()
    {
        await _handler.Handle(new GlobalRefreshCacheEvent(), CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.IsAny<GlobalCacheRefreshCommand>(), It.IsAny<CancellationToken>()),
                             Times.Once);
    }

    [TestMethod]
    public async Task Handle_EvenementGlobal_NEnvoieAucuneCommandeDeTenant()
    {
        await _handler.Handle(new GlobalRefreshCacheEvent(), CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.IsAny<TenantCacheRefreshCommand>(), It.IsAny<CancellationToken>()),
                             Times.Never);
    }
}
