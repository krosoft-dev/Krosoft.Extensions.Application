using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Events;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Events;
using Krosoft.Extensions.Testing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Handlers.Events;

[TestClass]
public class TenantsRefreshCacheEventHandlerTests : BaseTest
{
    private TenantsRefreshCacheEventHandler _handler = null!;
    private Mock<ILogger<TenantsRefreshCacheEventHandler>> _loggerMock = null!;
    private Mock<IMediator> _mediatorMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<TenantsRefreshCacheEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Unit.Value);

        _handler = new TenantsRefreshCacheEventHandler(_loggerMock.Object, _mediatorMock.Object);
    }

    [TestMethod]
    public async Task Handle_PlusieursTenants_EnvoieUneCommandeParTenant()
    {
        var tenantsId = new HashSet<string> { "tenant-1", "tenant-2", "tenant-3" };

        await _handler.Handle(new TenantsRefreshCacheEvent(tenantsId), CancellationToken.None);

        foreach (var tenantId in tenantsId)
        {
            _mediatorMock.Verify(m => m.Send(It.Is<TenantCacheRefreshCommand>(c => c.CurrentTenantId == tenantId),
                                             It.IsAny<CancellationToken>()),
                                 Times.Once);
        }

        _mediatorMock.Verify(m => m.Send(It.IsAny<TenantCacheRefreshCommand>(), It.IsAny<CancellationToken>()),
                             Times.Exactly(tenantsId.Count));
    }

    [TestMethod]
    public async Task Handle_AucunTenant_NEnvoieAucuneCommande()
    {
        await _handler.Handle(new TenantsRefreshCacheEvent(new HashSet<string>()), CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()),
                             Times.Never);
    }
}
