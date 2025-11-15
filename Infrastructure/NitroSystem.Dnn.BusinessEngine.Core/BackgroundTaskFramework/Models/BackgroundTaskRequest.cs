using System;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Enums;


namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models
{
    public class BackgroundTaskRequest
    {
        public IBackgroundTask Task { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Normal;
        public bool ForceRun { get; set; } = false; // اگر true باید فوراً اجرا شود (اجازه عبور از صف)
        public int RetryCount { get; set; } = 0;
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
        public string NotificationChannel { get; set; }

        public BackgroundTaskRequest(IBackgroundTask task, TaskPriority priority = TaskPriority.Normal, string notificationChannel = "")
        {
            Task = task;
            Priority = priority;
            NotificationChannel = notificationChannel;
        }
    }
}
