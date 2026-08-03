using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Testing;
using Krosoft.Extensions.Testing.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Handlers.Commands;

[TestClass]
public class PayloadCacheRefreshCommandHandlerTests : BaseTest
{
    private PayloadCacheRefreshCommandHandler _handler = null!;
    private Mock<ILogger<PayloadCacheRefreshCommandHandler>> _loggerMock = null!;
    private Mock<IMediator> _mediatorMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<PayloadCacheRefreshCommandHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Unit.Value);

        _handler = new PayloadCacheRefreshCommandHandler(_loggerMock.Object, _mediatorMock.Object);
    }

    [TestMethod]
    public async Task Handle_PayloadValide_EnvoieLaCommandeDeRafraichissementAvecLeTenant()
    {
        var command = new PayloadCacheRefreshCommand("{\"TenantId\":\"tenant-1\",\"UtilisateurId\":\"utilisateur-1\"}");

        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.Is<AuthCacheRefreshCommand>(c => c.CurrentTenantId == "tenant-1"),
                                         It.IsAny<CancellationToken>()),
                             Times.Once);
    }

    [TestMethod]
    public async Task Handle_PayloadValide_NExigeNiUtilisateurNiTenant()
    {
        var command = new PayloadCacheRefreshCommand("{\"TenantId\":\"tenant-1\"}");

        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.Is<AuthCacheRefreshCommand>(c => !c.IsUserIdRequired && !c.IsTenantIdRequired),
                                         It.IsAny<CancellationToken>()),
                             Times.Once);
    }

    [TestMethod]
    public async Task Handle_PayloadSansTenant_EnvoieLaCommandeSansTenant()
    {
        var command = new PayloadCacheRefreshCommand("{\"UtilisateurId\":\"utilisateur-1\"}");

        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.Is<AuthCacheRefreshCommand>(c => c.CurrentTenantId == null),
                                         It.IsAny<CancellationToken>()),
                             Times.Once);
    }

    [TestMethod]
    public async Task Handle_PayloadIllisible_NEnvoieAucuneCommande()
    {
        var command = new PayloadCacheRefreshCommand("null");

        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()),
                             Times.Never);
    }

    [TestMethod]
    public async Task Handle_PayloadIllisible_LogUneErreur()
    {
        var command = new PayloadCacheRefreshCommand("null");

        await _handler.Handle(command, CancellationToken.None);

        _loggerMock.Verify(LogLevel.Error, "Impossible de refresh le cache à partir du payload", Times.Once());
    }
}
