using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Queries;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Queries;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Testing;
using Microsoft.Extensions.Logging;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Handlers.Queries;

[TestClass]
public class CacheQueryHandlerTests : BaseTest
{
    private Mock<IDistributedCacheProvider> _distributedCacheProviderMock = null!;
    private CacheQueryHandler _handler = null!;
    private Mock<ILogger<CacheQueryHandler>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CacheQueryHandler>>();
        _distributedCacheProviderMock = new Mock<IDistributedCacheProvider>();

        _handler = new CacheQueryHandler(_loggerMock.Object, _distributedCacheProviderMock.Object);
    }

    [TestMethod]
    public async Task Handle_CacheAlimente_RetourneLaTailleDeChaqueCle()
    {
        _distributedCacheProviderMock.Setup(p => p.GetKeys(string.Empty))
                                     .Returns(new[] { "pays", "langues" });
        _distributedCacheProviderMock.Setup(p => p.GetLengthAsync("pays", It.IsAny<CancellationToken>()))
                                     .ReturnsAsync(42);
        _distributedCacheProviderMock.Setup(p => p.GetLengthAsync("langues", It.IsAny<CancellationToken>()))
                                     .ReturnsAsync(7);

        var result = await _handler.Handle(new CacheQuery(), CancellationToken.None);

        Check.That(result).HasSize(2);
        Check.That(result["pays"]).IsEqualTo(42);
        Check.That(result["langues"]).IsEqualTo(7);
    }

    [TestMethod]
    public async Task Handle_CacheVide_RetourneUnDictionnaireVide()
    {
        _distributedCacheProviderMock.Setup(p => p.GetKeys(string.Empty))
                                     .Returns(Array.Empty<string>());

        var result = await _handler.Handle(new CacheQuery(), CancellationToken.None);

        Check.That(result).IsEmpty();
        _distributedCacheProviderMock.Verify(p => p.GetLengthAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                                             Times.Never);
    }
}
