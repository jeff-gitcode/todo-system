using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using TodoSystem.Application.Services;

namespace TodoSystem.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            if (_cache.TryGetValue(key, out var value) && value is T typedValue)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return Task.FromResult<T?>(typedValue);
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                Size = 1 // Assume each entry has size 1 for simplicity
            };

            _cache.Set(key, value, options);
            _keys.TryAdd(key, 0);

            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
            _logger.LogDebug("Removed cache entry for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            var keysToRemove = _keys.Keys.Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _keys.TryRemove(key, out _);
            }

            _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache values by pattern: {Pattern}", pattern);
        }

        return Task.CompletedTask;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var value = await getItem();
        if (value != null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }
}