using System.Threading.Tasks;

namespace kriefTrackAiApi.Infrastructure.Services;

  public class BackgroundTaskSync
  {
      public TaskCompletionSource<bool> MiddlewareCompleted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
  }
