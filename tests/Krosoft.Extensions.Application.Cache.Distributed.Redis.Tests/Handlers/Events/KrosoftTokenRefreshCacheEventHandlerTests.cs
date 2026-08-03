using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Events;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Events;
using Krosoft.Extensions.Core.Models;
using Krosoft.Extensions.Testing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Handlers.Events;

[TestClass]
public class KrosoftTokenRefreshCacheEventHandlerTests : BaseTest
{
    private KrosoftTokenRefreshCacheEventHandler _handler = null!;
    private Mock<ILogger<KrosoftTokenRefreshCacheEventHandler>> _loggerMock = null!;
    private Mock<IMediator> _mediatorMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<KrosoftTokenRefreshCacheEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Unit.Value);

        _handler = new KrosoftTokenRefreshCacheEventHandler(_loggerMock.Object, _mediatorMock.Object);
    }

    private static KrosoftToken CreateToken(params string[] tenantsId)
    {
        var krosoftToken = new KrosoftToken { Id = "utilisateur-1" };
        foreach (var tenantId in tenantsId)
        {
            krosoftToken.TenantsId.Add(tenantId);
        }

        return krosoftToken;
    }

    [TestMethod]
    public async Task Handle_TokenAvecPlusieursTenants_EnvoieUneCommandeParTenant()
    {
        var krosoftToken = CreateToken("tenant-1", "tenant-2");

        await _handler.Handle(new KrosoftTokenRefreshCacheEvent(krosoftToken), CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.Is<TenantCacheRefreshCommand>(c => c.CurrentTenantId == "tenant-1"),
                                         It.IsAny<CancellationToken>()),
                             Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<TenantCacheRefreshCommand>(c => c.CurrentTenantId == "tenant-2"),
                                         It.IsAny<CancellationToken>()),
                             Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<TenantCacheRefreshCommand>(), It.IsAny<CancellationToken>()),
                             Times.Exactly(2));
    }

    [TestMethod]
    public async Task Handle_TokenSansTenant_NEnvoieAucuneCommande()
    {
        var krosoftToken = CreateToken();

        await _handler.Handle(new KrosoftTokenRefreshCacheEvent(krosoftToken), CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()),
                             Times.Never);
    }
}
