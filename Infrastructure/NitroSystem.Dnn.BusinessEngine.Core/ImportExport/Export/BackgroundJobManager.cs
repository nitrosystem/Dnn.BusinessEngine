using System;
using System.ComponentModel;
using System.IO;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export
{
    /// <summary>
    /// یک کنترل‌کننده‌ی عمومی برای اجرای jobها در پس‌زمینه
    /// قابل استفاده برای ExportFramework، ImportFramework و سایر سرویس‌ها.
    /// </summary>
    public class BackgroundJobManager<TContext> where TContext : class
    {
        private readonly BackgroundWorker _worker;
        private readonly string _workingPath;
        private readonly Action<TContext, Action<int>, BackgroundWorker> _job;
        private readonly Action<int> _reportProgress;
        private readonly TContext _context;

        public event Action<string> OnError;
        public event Action OnCompleted;

        public BackgroundJobManager(
            string workingPath,
            TContext context,
            Action<TContext, Action<int>, BackgroundWorker> job,
            Action<int> reportProgress)
        {
            _workingPath = workingPath;
            _context = context;
            _job = job;
            _reportProgress = reportProgress;

            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _worker.DoWork += DoWorkHandler;
            _worker.ProgressChanged += ProgressChangedHandler;
            _worker.RunWorkerCompleted += WorkerCompletedHandler;
        }

        public void Start()
        {
            try
            {
                if (Directory.Exists(_workingPath))
                    Directory.Delete(_workingPath, true);
                Directory.CreateDirectory(_workingPath);

                _worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Failed to start job: {ex.Message}");
            }
        }

        public void Cancel()
        {
            if (_worker.IsBusy)
                _worker.CancelAsync();
        }

        private void DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            try
            {
                _job?.Invoke(_context, progress =>
                {
                    if (_worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    _worker.ReportProgress(progress);
                }, _worker);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Job execution failed: {ex.Message}");
            }
        }

        private void ProgressChangedHandler(object sender, ProgressChangedEventArgs e)
        {
            _reportProgress?.Invoke(e.ProgressPercentage);
        }

        private void WorkerCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                OnError?.Invoke("Job was canceled.");
            else if (e.Error != null)
                OnError?.Invoke($"Unhandled error: {e.Error.Message}");
            else
                OnCompleted?.Invoke();
        }
    }
}
