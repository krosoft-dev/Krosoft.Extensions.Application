using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Events;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Events;
using Krosoft.Extensions.Testing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Handlers.Events;

[TestClass]
public class TenantRefreshCacheEventHandlerTests : BaseTest
{
    private TenantRefreshCacheEventHandler _handler = null!;
    private Mock<ILogger<TenantRefreshCacheEventHandler>> _loggerMock = null!;
    private Mock<IMediator> _mediatorMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<TenantRefreshCacheEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Unit.Value);

        _handler = new TenantRefreshCacheEventHandler(_loggerMock.Object, _mediatorMock.Object);
    }

    [TestMethod]
    public async Task Handle_EvenementTenant_EnvoieLaCommandeDeRafraichissementDuTenant()
    {
        await _handler.Handle(new TenantRefreshCacheEvent("tenant-1"), CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.Is<TenantCacheRefreshCommand>(c => c.CurrentTenantId == "tenant-1"),
                                         It.IsAny<CancellationToken>()),
                             Times.Once);
    }

    [TestMethod]
    public async Task Handle_EvenementTenant_NExigeNiUtilisateurNiTenant()
    {
        await _handler.Handle(new TenantRefreshCacheEvent("tenant-1"), CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.Is<TenantCacheRefreshCommand>(c => !c.IsUserIdRequired && !c.IsTenantIdRequired),
                                         It.IsAny<CancellationToken>()),
                             Times.Once);
    }
}
