// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis.Extensions.Core.NET7.Abstractions;
using StackExchange.Redis.Extensions.Core.NET7.Configuration;

namespace StackExchange.Redis.Extensions.Core.NET7.Implementations;

/// <inheritdoc />
public class RedisClientFactory : IRedisClientFactory
{
    private readonly string? defaultConnectionName;
    private readonly Dictionary<string, IRedisClient> redisCacheClients;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedisClientFactory" /> class.
    /// </summary>
    /// <param name="configurations">The connection configurations.</param>
    /// <param name="loggerFactory">The logger factory</param>
    /// <param name="serializer">The cache serializer</param>
    public RedisClientFactory(IEnumerable<RedisConfiguration> configurations, ILoggerFactory? loggerFactory,
        ISerializer serializer)
    {
        // First of all, I need to validate the configurations
        var hasDefaultConfigured = false;
        var hashSet = new HashSet<string>();
        var redisClientFactoryLogger =
            loggerFactory?.CreateLogger<RedisClientFactory>() ?? NullLogger<RedisClientFactory>.Instance;

        var redisConfigurations = configurations.ToArray();

        if (redisConfigurations.Length == 1)
            redisConfigurations[0].IsDefault = true;

        for (var i = 0; i < redisConfigurations.Length; i++)
        {
            var configuration = redisConfigurations[i];

            if (configuration.IsDefault && hasDefaultConfigured)
                throw new ArgumentException(
                    "There is more than one default configuration. Only one default configuration is allowed.");

            if (string.IsNullOrEmpty(configuration.Name))
            {
                configuration.Name = Guid.NewGuid().ToString();
                redisClientFactoryLogger.LogWarning(
                    "There is no name configured for the Redis configuration. A new one will be created {Name}",
                    configuration.Name);
            }

            if (configuration.IsDefault)
            {
                hasDefaultConfigured = true;
                defaultConnectionName = configuration.Name;
            }

            if (hashSet.Contains(configuration.Name!))
                throw new ArgumentException($"{nameof(RedisConfiguration.Name)} must be unique");

            hashSet.Add(configuration.Name!);
        }

        if (!hasDefaultConfigured)
            throw new ArgumentException(
                "There is no default configuration. At least one default configuration is required.");

        redisCacheClients = new Dictionary<string, IRedisClient>(redisConfigurations.Length);

        var poolManagerLogger = loggerFactory?.CreateLogger<RedisConnectionPoolManager>() ??
                                NullLogger<RedisConnectionPoolManager>.Instance;

        for (var i = 0; i < redisConfigurations.Length; i++)
        {
            var configuration = redisConfigurations[i];

            var poolManager = new RedisConnectionPoolManager(configuration, poolManagerLogger);

            redisCacheClients.Add(configuration.Name!, new RedisClient(poolManager, serializer, configuration));
        }
    }

    /// <inheritdoc />
    public IEnumerable<IRedisClient> GetAllClients()
    {
        return redisCacheClients.Values;
    }

    /// <inheritdoc />
    public IRedisClient GetDefaultRedisClient()
    {
        return redisCacheClients[defaultConnectionName!];
    }

    /// <inheritdoc />
    public IRedisClient GetRedisClient(string? name = null)
    {
        name ??= defaultConnectionName!;

        return redisCacheClients[name];
    }

    /// <inheritdoc />
    public IRedisDatabase GetDefaultRedisDatabase()
    {
        return redisCacheClients[defaultConnectionName!].GetDefaultDatabase();
    }

    /// <inheritdoc />
    public IRedisDatabase GetRedisDatabase(string? name = null)
    {
        name ??= defaultConnectionName!;

        return redisCacheClients[name].GetDefaultDatabase();
    }
}