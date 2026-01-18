using System.Collections.Concurrent;

namespace TechWrite.Web.Services;

public class RateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _submissions = new();
    private readonly ILogger<RateLimitService> _logger;
    private readonly int _maxSubmissionsPerHour;
    private readonly TimeSpan _windowDuration;

    public RateLimitService(ILogger<RateLimitService> logger)
    {
        _logger = logger;
        _maxSubmissionsPerHour = 3;
        _windowDuration = TimeSpan.FromHours(1);
    }

    public bool IsRateLimited(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return false;

        CleanupOldEntries();

        if (_submissions.TryGetValue(ipAddress, out var timestamps))
        {
            var recentCount = timestamps.Count(t => t > DateTime.UtcNow - _windowDuration);
            if (recentCount >= _maxSubmissionsPerHour)
            {
                _logger.LogWarning("Rate limit exceeded for IP {IpAddress}. Submissions in last hour: {Count}",
                    ipAddress, recentCount);
                return true;
            }
        }

        return false;
    }

    public void RecordSubmission(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return;

        _submissions.AddOrUpdate(
            ipAddress,
            _ => new List<DateTime> { DateTime.UtcNow },
            (_, existing) =>
            {
                existing.Add(DateTime.UtcNow);
                return existing;
            });

        _logger.LogInformation("Recorded contact form submission from IP {IpAddress}", ipAddress);
    }

    private void CleanupOldEntries()
    {
        var cutoff = DateTime.UtcNow - _windowDuration;

        foreach (var kvp in _submissions)
        {
            kvp.Value.RemoveAll(t => t < cutoff);
            if (kvp.Value.Count == 0)
            {
                _submissions.TryRemove(kvp.Key, out _);
            }
        }
    }
}
