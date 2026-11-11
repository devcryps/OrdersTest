using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrdersManagement.Application.Events;
using OrdersManagement.Application.Interfaces;
using OrdersManagement.Infrastructure.Caching;
using OrdersManagement.Infrastructure.Caching.InMemory;
using OrdersManagement.Infrastructure.Caching.Redis;
using OrdersManagement.Infrastructure.Messaging.InMemory;
using OrdersManagement.Infrastructure.Messaging.RabbitMq;
using OrdersManagement.Infrastructure.Persistence.InMemory;
using OrdersManagement.Infrastructure.Persistence.Oracle;
using StackExchange.Redis;

namespace OrdersManagement.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registra persistencia, mensajería y caché según los proveedores indicados en la configuración.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddMessaging(services, configuration);
        AddCaching(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var provider = GetProvider(configuration, "Persistence", "InMemory", "Oracle");

        if (provider == "Oracle")
        {
            services.Configure<OracleOptions>(configuration.GetSection(OracleOptions.SectionName));
            services.AddSingleton<IOrderRepository, OracleOrderRepository>();
        }
        else
        {
            services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        }
    }

    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        var provider = GetProvider(configuration, "Messaging", "InMemory", "RabbitMQ");

        if (provider == "RabbitMQ")
        {
            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
            services.AddSingleton<RabbitMqConnectionProvider>();
            services.AddSingleton<IOrderEventPublisher, RabbitMqOrderEventPublisher>();
            services.AddSingleton<IOrderEventConsumer, RabbitMqOrderEventConsumer>();
        }
        else
        {
            services.AddSingleton(Channel.CreateUnbounded<OrderCreated>());
            services.AddSingleton<IOrderEventPublisher, ChannelOrderEventPublisher>();
            services.AddSingleton<IOrderEventConsumer, ChannelOrderEventConsumer>();
        }
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var provider = GetProvider(configuration, "Cache", "Memory", "Redis");

        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        if (provider == "Redis")
        {
            services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
            services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
            {
                var options = new RedisOptions();
                configuration.GetSection(RedisOptions.SectionName).Bind(options);

                var redisConfiguration = ConfigurationOptions.Parse(options.ConnectionString);
                redisConfiguration.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(redisConfiguration);
            });
            services.AddSingleton<IOrderCache, RedisOrderCache>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<IOrderCache, MemoryOrderCache>();
        }
    }

    private static string GetProvider(IConfiguration configuration, string section, string localValue, string realValue)
    {
        var value = configuration[$"{section}:Provider"] ?? localValue;

        if (value.Equals(localValue, StringComparison.OrdinalIgnoreCase))
        {
            return localValue;
        }

        if (value.Equals(realValue, StringComparison.OrdinalIgnoreCase))
        {
            return realValue;
        }

        throw new InvalidOperationException(
            $"El valor '{value}' de {section}:Provider no es válido. Valores permitidos: {localValue}, {realValue}.");
    }
}
