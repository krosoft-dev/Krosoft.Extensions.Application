using Krosoft.Extensions.Application.Cache.Distributed.Redis.Extensions;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Events;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Handlers.Queries;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Commands;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Events;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Models.Queries;
using Krosoft.Extensions.Application.Cache.Distributed.Redis.Services;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Testing;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Krosoft.Extensions.Application.Cache.Distributed.Redis.Tests.Extensions;

[TestClass]
public class ServiceCollectionExtensionTests : BaseTest
{
    private static void AddLoggers(IServiceCollection services)
    {
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
    }

    private static IConfiguration CreateConfiguration(string cacheRefreshTimeSpan) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:CacheRefreshTimeSpan"] = cacheRefreshTimeSpan
            })
            .Build();

    [TestMethod]
    public void AddCacheHandlers_EnregistreLesHandlersDEvenements()
    {
        using var serviceProvider = CreateServiceCollection(services =>
        {
            AddLoggers(services);
            services.AddCacheHandlers();
        });

        Check.That(serviceProvider.GetService<INotificationHandler<GlobalRefreshCacheEvent>>()).IsInstanceOf<GlobalRefreshCacheEventHandler>();
        Check.That(serviceProvider.GetService<INotificationHandler<TenantRefreshCacheEvent>>()).IsInstanceOf<TenantRefreshCacheEventHandler>();
        Check.That(serviceProvider.GetService<INotificationHandler<TenantsRefreshCacheEvent>>()).IsInstanceOf<TenantsRefreshCacheEventHandler>();
        Check.That(serviceProvider.GetService<INotificationHandler<KrosoftTokenRefreshCacheEvent>>()).IsInstanceOf<KrosoftTokenRefreshCacheEventHandler>();
    }

    [TestMethod]
    public void AddCacheHandlers_EnregistreLesHandlersDeRequetes()
    {
        using var serviceProvider = CreateServiceCollection(services =>
        {
            AddLoggers(services);
            services.AddSingleton(new Mock<IDistributedCacheProvider>().Object);
            services.AddSingleton(new Mock<ITenantDistributedCacheProvider>().Object);
            services.AddCacheHandlers();
        });

        Check.That(serviceProvider.GetService<IRequestHandler<CacheQuery, IDictionary<string, long>>>()).IsInstanceOf<CacheQueryHandler>();
        Check.That(serviceProvider.GetService<IRequestHandler<TenantCacheQuery, IDictionary<string, long>>>()).IsInstanceOf<TenantCacheQueryHandler>();
    }

    [TestMethod]
    public void AddCacheHandlers_EnregistreLeMediateur()
    {
        using var serviceProvider = CreateServiceCollection(services =>
        {
            AddLoggers(services);
            services.AddCacheHandlers();
        });

        Check.That(serviceProvider.GetService<IMediator>()).IsNotNull();
    }

    [TestMethod]
    public void AddCacheRefreshHostedService_SansCommande_EnregistreLaCommandeAuthParDefaut()
    {
        using var serviceProvider = CreateServiceCollection(services =>
        {
            AddLoggers(services);
            services.AddCacheHandlers();
            services.AddCacheRefreshHostedService(CreateConfiguration("00:15:00"));
        });

        var config = serviceProvider.GetRequiredService<CacheScheduleConfig<AuthCacheRefreshCommand>>();

        Check.That(config.Interval).IsEqualTo(TimeSpan.FromMinutes(15));
        Check.That(config.Command).IsNotNull();
        Check.That(config.Command!.IsUserIdRequired).IsFalse();
        Check.That(config.Command.IsTenantIdRequired).IsFalse();
    }

    [TestMethod]
    public void AddCacheRefreshHostedService_SansCommande_EnregistreLeServiceHeberge()
    {
        using var serviceProvider = CreateServiceCollection(services =>
        {
            AddLoggers(services);
            services.AddCacheHandlers();
            services.AddCacheRefreshHostedService(CreateConfiguration("00:15:00"));
        });

        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();

        Check.That(hostedServices).HasSize(1);
        Check.That(hostedServices[0]).IsInstanceOf<CacheRefreshHostedService<AuthCacheRefreshCommand>>();
    }

    [TestMethod]
    public void AddCacheRefreshHostedService_AvecCommandeDediee_EnregistreLaCommandeFournie()
    {
        var command = new GlobalCacheRefreshCommand();

        using var serviceProvider = CreateServiceCollection(services =>
        {
            AddLoggers(services);
            services.AddCacheHandlers();
            services.AddCacheRefreshHostedService(command, CreateConfiguration("01:00:00"));
        });

        var config = serviceProvider.GetRequiredService<CacheScheduleConfig<GlobalCacheRefreshCommand>>();

        Check.That(config.Interval).IsEqualTo(TimeSpan.FromHours(1));
        Check.That(config.Command).IsEqualTo(command);
    }
}
