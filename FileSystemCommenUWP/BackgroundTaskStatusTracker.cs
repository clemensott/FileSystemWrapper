using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace FileSystemCommonUWP
{
    public class BackgroundTaskStatusTracker : IDisposable
    {
        private readonly TimeSpan statusUpdateTimeout = TimeSpan.FromSeconds(10);

        private BackgroundTaskStatus status;
        private readonly IBackgroundTaskRegistration taskRegistration;

        public BackgroundTaskStatus Status
        {
            get => status;
            private set
            {
                status = value;
                LastStatusUpdate = DateTime.Now;
            }
        }

        public DateTime LastStatusUpdate { get; private set; }

        private BackgroundTaskStatusTracker(IBackgroundTaskRegistration taskRegistration)
        {
            Status = BackgroundTaskStatus.Unkown;
            this.taskRegistration = taskRegistration;
        }

        private void StartTracking()
        {
            taskRegistration.Progress += TaskRegistration_Progress;
            taskRegistration.Completed += TaskRegistration_Completed;
        }

        public static BackgroundTaskStatusTracker Start(IBackgroundTaskRegistration taskRegistration)
        {
            BackgroundTaskStatusTracker tracker = new BackgroundTaskStatusTracker(taskRegistration);
            tracker.StartTracking();

            return tracker;
        }

        private void TaskRegistration_Progress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            Status = (BackgroundTaskStatus)args.Progress;
        }

        private void TaskRegistration_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Status = BackgroundTaskStatus.Stopped;
        }

        private bool HitStatusTimeout()
        {
            return DateTime.Now - LastStatusUpdate > statusUpdateTimeout;
        }

        public async Task<bool> IsStopped()
        {
            while (true)
            {
                if (HitStatusTimeout() || Status == BackgroundTaskStatus.Unkown || Status == BackgroundTaskStatus.Stopped) return true;
                if (Status == BackgroundTaskStatus.RunningA || Status == BackgroundTaskStatus.RunningB) return false;

                await Task.Delay(100);
            }
        }

        public void SetTriggered()
        {
            Status = BackgroundTaskStatus.Triggered;
        }

        public void Dispose()
        {
            taskRegistration.Progress -= TaskRegistration_Progress;
            taskRegistration.Completed -= TaskRegistration_Completed;
        }
    }
}
