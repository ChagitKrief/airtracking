using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace kriefTrackAiApi.Infrastructure.Services;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<string, string> _connections = new(); // userId -> connectionId
    private readonly ConcurrentDictionary<string, HashSet<string>> _containerToUsers = new(); // containerId -> userIds
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(ILogger<ConnectionManager> logger)
    {
        _logger = logger;
    }

    public void AddConnection(string userId, string connectionId)
    {
        _connections[userId] = connectionId;
        _logger.LogInformation("Added connection: UserId={UserId}, ConnectionId={ConnectionId}", userId, connectionId);
    }

    public void RemoveConnection(string connectionId)
    {
        var userId = _connections.FirstOrDefault(x => x.Value == connectionId).Key;
        if (!string.IsNullOrEmpty(userId))
        {
            _connections.TryRemove(userId, out _);
            _logger.LogInformation("Removed connection: ConnectionId={ConnectionId}, UserId={UserId}", connectionId, userId);

            foreach (var container in _containerToUsers.Keys)
            {
                _containerToUsers[container].Remove(userId);
                if (_containerToUsers[container].Count == 0)
                {
                    _containerToUsers.TryRemove(container, out _);
                    _logger.LogInformation("Container {ContainerId} is no longer tracked by any user and has been removed.", container);
                }
            }
        }
        else
        {
            _logger.LogWarning("Attempted to remove connectionId={ConnectionId}, but no matching user was found.", connectionId);
        }
    }

    public void SetUserContainer(string userId, string newContainerId)
    {
        string? previousContainer = _containerToUsers.Keys.FirstOrDefault(c => _containerToUsers[c].Contains(userId));

        if (previousContainer != null)
        {
            _containerToUsers[previousContainer].Remove(userId);
            if (_containerToUsers[previousContainer].Count == 0)
            {
                _containerToUsers.TryRemove(previousContainer, out _);
                _logger.LogInformation("Previous container {PreviousContainer} removed as it is no longer tracked.", previousContainer);
            }
        }

        if (!_containerToUsers.ContainsKey(newContainerId))
        {
            _containerToUsers[newContainerId] = new HashSet<string>();
        }
        _containerToUsers[newContainerId].Add(userId);

        _logger.LogInformation("User {UserId} is now tracking container {NewContainerId}", userId, newContainerId);
    }

    public void RemoveContainerFromUser(string userId)
    {
        foreach (var container in _containerToUsers.Keys)
        {
            if (_containerToUsers[container].Remove(userId))
            {
                _logger.LogInformation("User {UserId} has stopped tracking container {ContainerId}", userId, container);
                if (_containerToUsers[container].Count == 0)
                {
                    _containerToUsers.TryRemove(container, out _);
                    _logger.LogInformation("Container {ContainerId} removed from tracking as no users are following it.", container);
                }
            }
        }
    }

    public Dictionary<string, HashSet<string>> GetTrackedContainers()
    {
        _logger.LogInformation("Retrieved tracked containers data.");
        return new Dictionary<string, HashSet<string>>(_containerToUsers);
    }

    public string? GetConnectionId(string userId)
    {
        if (_connections.TryGetValue(userId, out var connectionId))
        {
            _logger.LogInformation("Retrieved connectionId for UserId={UserId}: {ConnectionId}", userId, connectionId);
            return connectionId;
        }
        else
        {
            _logger.LogWarning("No connection found for UserId={UserId}", userId);
            return null;
        }
    }
}
