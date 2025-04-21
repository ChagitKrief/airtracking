namespace kriefTrackAiApi.Core.Interfaces;

public interface IContainerNotifier
{
  Task NotifyRenderedMapWithContainerAsync(string userId, string containerId);
}

