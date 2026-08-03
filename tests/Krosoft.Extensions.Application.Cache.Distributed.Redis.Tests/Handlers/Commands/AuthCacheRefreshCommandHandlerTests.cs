using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Testing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Handlers.Commands;

[TestClass]
public class AuthCacheRefreshCommandHandlerTests : BaseTest
{
    private AuthCacheRefreshCommandHandler _handler = null!;
    private Mock<ILogger<AuthCacheRefreshCommandHandler>> _loggerMock = null!;
    private Mock<IMediator> _mediatorMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<AuthCacheRefreshCommandHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Unit.Value);

        _handler = new AuthCacheRefreshCommandHandler(_loggerMock.Object, _mediatorMock.Object);
    }

    [TestMethod]
    public async Task Handle_TenantRenseigne_PropageLeTenantSurLaCommandeTenant()
    {
        var command = new AuthCacheRefreshCommand(false, false) { CurrentTenantId = "tenant-1" };

        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.Is<TenantCacheRefreshCommand>(c => c.CurrentTenantId == "tenant-1"),
                                         It.IsAny<CancellationToken>()),
                             Times.Once);
    }

    [TestMethod]
    public async Task Handle_QuelQueSoitLeTenant_RafraichitAussiLeCacheGlobal()
    {
        var command = new AuthCacheRefreshCommand(false, false) { CurrentTenantId = "tenant-1" };

        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.IsAny<GlobalCacheRefreshCommand>(), It.IsAny<CancellationToken>()),
                             Times.Once);
    }

    [TestMethod]
    public async Task Handle_SansTenant_EnvoieLaCommandeTenantSansTenant()
    {
        var command = new AuthCacheRefreshCommand(false, false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Check.That(result).IsEqualTo(Unit.Value);
        _mediatorMock.Verify(m => m.Send(It.Is<TenantCacheRefreshCommand>(c => c.CurrentTenantId == null),
                                         It.IsAny<CancellationToken>()),
                             Times.Once);
    }
}
