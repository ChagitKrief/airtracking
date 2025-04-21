using System.Collections.Concurrent;
namespace kriefTrackAiApi.Infrastructure.Services;

public class PrerenderCache
{
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public void Save(string containerId, string html) => _cache[containerId] = html;
    public string? Get(string containerId) => _cache.TryGetValue(containerId, out var html) ? html : null;
    public bool TryGet(string containerId, out string html) => _cache.TryGetValue(containerId, out html!);

}