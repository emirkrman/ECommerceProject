using ECommerceProject.Business.Options;
using ECommerceProject.Business.Services.Abstract;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ECommerceProject.Business.Services.Concrete;

public class RedisStockReservationService : IStockReservationService
{
    private const string ReserveScript = """
        local quantity = tonumber(ARGV[1])
        local actualStock = tonumber(ARGV[2])
        local now = tonumber(ARGV[3])
        local expireAt = tonumber(ARGV[4])
        local userField = ARGV[5]

        local expired = redis.call('ZRANGEBYSCORE', KEYS[2], '-inf', now)
        if #expired > 0 then
            redis.call('HDEL', KEYS[1], unpack(expired))
            redis.call('ZREM', KEYS[2], unpack(expired))
        end

        if quantity <= 0 then
            redis.call('HDEL', KEYS[1], userField)
            redis.call('ZREM', KEYS[2], userField)
            return 1
        end

        local reservations = redis.call('HGETALL', KEYS[1])
        local reserved = 0
        for i = 2, #reservations, 2 do
            reserved = reserved + tonumber(reservations[i])
        end

        local current = tonumber(redis.call('HGET', KEYS[1], userField) or '0')
        local nextReserved = reserved - current + quantity

        if nextReserved > actualStock then
            return 0
        end

        redis.call('HSET', KEYS[1], userField, quantity)
        redis.call('ZADD', KEYS[2], expireAt, userField)

        local latest = redis.call('ZRANGE', KEYS[2], -1, -1, 'WITHSCORES')
        if #latest > 0 then
            redis.call('PEXPIREAT', KEYS[1], latest[2])
            redis.call('PEXPIREAT', KEYS[2], latest[2])
        end

        return 1
        """;

    private const string CleanupAndSumScript = """
        local now = tonumber(ARGV[1])

        local expired = redis.call('ZRANGEBYSCORE', KEYS[2], '-inf', now)
        if #expired > 0 then
            redis.call('HDEL', KEYS[1], unpack(expired))
            redis.call('ZREM', KEYS[2], unpack(expired))
        end

        local quantities = redis.call('HVALS', KEYS[1])
        local reserved = 0
        for i = 1, #quantities do
            reserved = reserved + tonumber(quantities[i])
        end

        return reserved
        """;

    private readonly IDatabase _database;
    private readonly StockReservationOptions _options;

    public RedisStockReservationService(
        IConnectionMultiplexer redis,
        IOptions<StockReservationOptions> options)
    {
        _database = redis.GetDatabase();
        _options = options.Value;
    }

    public async Task<bool> TryReserveAsync(int userId, int productId, int quantity, int actualStock)
    {
        var now = DateTimeOffset.UtcNow;
        var expireAt = now.Add(GetReservationDuration());

        var result = await _database.ScriptEvaluateAsync(
            ReserveScript,
            [QuantitiesKey(productId), ExpirationsKey(productId)],
            [
                quantity,
                actualStock,
                now.ToUnixTimeMilliseconds(),
                expireAt.ToUnixTimeMilliseconds(),
                UserField(userId)
            ]);

        return (long)result == 1;
    }

    public async Task ReleaseAsync(int userId, int productId)
    {
        var userField = UserField(userId);

        await _database.HashDeleteAsync(QuantitiesKey(productId), userField);
        await _database.SortedSetRemoveAsync(ExpirationsKey(productId), userField);
    }

    public async Task ReleaseManyAsync(int userId, IEnumerable<int> productIds)
    {
        foreach (var productId in productIds.Distinct())
        {
            await ReleaseAsync(userId, productId);
        }
    }

    private async Task<int> GetReservedQuantityAsync(int productId)
    {
        var result = await _database.ScriptEvaluateAsync(
            CleanupAndSumScript,
            [QuantitiesKey(productId), ExpirationsKey(productId)],
            [DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()]);

        return (int)(long)result;
    }

    public async Task<int> GetAvailableStockAsync(int productId, int actualStock)
    {
        var reserved = await GetReservedQuantityAsync(productId);
        return Math.Max(0, actualStock - reserved);
    }

    private static RedisKey QuantitiesKey(int productId)
    {
        return $"stock:reservation:{productId}:quantities";
    }

    private static RedisKey ExpirationsKey(int productId)
    {
        return $"stock:reservation:{productId}:expirations";
    }

    private static RedisValue UserField(int userId)
    {
        return userId.ToString();
    }

    private TimeSpan GetReservationDuration()
    {
        return TimeSpan.FromMinutes(Math.Max(1, _options.DurationMinutes));
    }
}
