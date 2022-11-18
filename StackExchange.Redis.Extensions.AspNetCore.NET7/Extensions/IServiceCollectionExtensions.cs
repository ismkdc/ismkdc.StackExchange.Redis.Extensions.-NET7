// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using StackExchange.Redis.Extensions.Core.NET7;
using StackExchange.Redis.Extensions.Core.NET7.Abstractions;
using StackExchange.Redis.Extensions.Core.NET7.Configuration;
using StackExchange.Redis.Extensions.Core.NET7.Implementations;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     A set of extension methods that help you to confire StackExchangeRedisExtensions into your dependency injection
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    ///     Add StackExchange.Redis with its serialization provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConfiguration">The redis configration.</param>
    /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
    public static IServiceCollection AddStackExchangeRedisExtensions<T>(
        this IServiceCollection services,
        RedisConfiguration redisConfiguration)
        where T : class, ISerializer
    {
        return services.AddStackExchangeRedisExtensions<T>(sp => new[] { redisConfiguration });
    }

    /// <summary>
    ///     Add StackExchange.Redis with its serialization provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConfiguration">The redis configration.</param>
    /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
    public static IServiceCollection AddStackExchangeRedisExtensions<T>(
        this IServiceCollection services,
        IEnumerable<RedisConfiguration> redisConfiguration)
        where T : class, ISerializer
    {
        return services.AddStackExchangeRedisExtensions<T>(sp => redisConfiguration);
    }

    /// <summary>
    ///     Add StackExchange.Redis with its serialization provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConfigurationFactory">The redis configration factory.</param>
    /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
    public static IServiceCollection AddStackExchangeRedisExtensions<T>(
        this IServiceCollection services,
        Func<IServiceProvider, IEnumerable<RedisConfiguration>> redisConfigurationFactory)
        where T : class, ISerializer
    {
        services.AddSingleton<IRedisClientFactory, RedisClientFactory>();
        services.AddSingleton<ISerializer, T>();

        services.AddSingleton(provider => provider
            .GetRequiredService<IRedisClientFactory>()
            .GetDefaultRedisClient());

        services.AddSingleton(provider => provider
            .GetRequiredService<IRedisClientFactory>()
            .GetDefaultRedisClient()
            .GetDefaultDatabase());

        services.AddSingleton(redisConfigurationFactory);

        return services;
    }
}