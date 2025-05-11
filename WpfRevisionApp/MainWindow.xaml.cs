using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;

namespace WpfRevisionApp
{
    public partial class MainWindow : Window
    {
        private BackgroundWorker worker;

        private Thread thread1, thread2;

        // Per-Thread Static Field
        [ThreadStatic] static int perThreadData;

        private object syncObject = new object(); // Used for Monitor synchronization

        public MainWindow()
        {
            InitializeComponent();
            SetupBackgroundWorker();

            // RunWorkerAsync(argument: xxxxx)
            StartWorkerBtn.Click += (s, e) => worker.RunWorkerAsync("SampleArg");

            // Cancel a background worker
            CancelWorkerBtn.Click += (s, e) => worker.CancelAsync();

            ThreadTestBtn.Click += StartThreads;
            IsoStorageBtn.Click += TestIsolatedStorage;
        }

        private void SetupBackgroundWorker()
        {
            worker = new BackgroundWorker
            {
                // WorkerReportsProgress
                WorkerReportsProgress = true,

                // WorkerSupportsCancellation
                WorkerSupportsCancellation = true
            };

            // DoWork
            worker.DoWork += (s, e) =>
            {
                // e.Argument
                string arg = (string)e.Argument;

                for (int i = 1; i <= 100; i++)
                {
                    // CancellationPending
                    if (worker.CancellationPending)
                    {
                        // e.Cancel
                        e.Cancel = true;
                        return;
                    }

                    // Sleep()
                    Thread.Sleep(30); // simulate work

                    // ReportProgress()
                    worker.ReportProgress(i);
                }

                // e.Result
                e.Result = $"Completed with argument: {arg}";
            };

            // ProgressChanged
            worker.ProgressChanged += (s, e) =>
            {
                // e.ProgressPercentage
                ProgressBar.Value = e.ProgressPercentage;

                // Access controls from BackgroundWorker
            };

            // RunWorkerCompleted
            worker.RunWorkerCompleted += (s, e) =>
            {
                // e.Cancelled
                if (e.Cancelled)
                    OutputBox.Text = "Cancelled.";
                else if (e.Error != null)
                    OutputBox.Text = $"Error: {e.Error.Message}";
                else
                    OutputBox.Text = e.Result.ToString();
            };
        }

        private void StartThreads(object sender, RoutedEventArgs e)
        {
            // Create threads with parameters
            thread1 = new Thread(ThreadMethod)
            {
                // Name
                Name = "Thread1",

                // IsBackground
                IsBackground = true,

                // Priority
                Priority = ThreadPriority.Normal
            };

            thread2 = new Thread(ThreadMethod)
            {
                Name = "Thread2",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };

            // Start()
            thread1.Start(10);
            thread2.Start(20);

            // Join()
            thread1.Join();
            thread2.Join();

            OutputBox.Text = "Threads completed.";
        }

        private void ThreadMethod(object arg)
        {
            int val = (int)arg;

            // Per-Thread Static Field usage
            perThreadData = val;

            // TryEnter()
            if (Monitor.TryEnter(syncObject))
            {
                try
                {
                    // Sleep()
                    Thread.Sleep(500);

                    Console.WriteLine($"{Thread.CurrentThread.Name} with value {perThreadData}");

                    // Pulse()
                    Monitor.Pulse(syncObject);

                    // Wait()
                    Monitor.Wait(syncObject, 100);
                }
                catch (ThreadInterruptedException)
                {
                    // Interrupt() would be caught here
                }
                finally
                {
                    // Exit()
                    Monitor.Exit(syncObject);
                }
            }

            // ThreadState can be checked (not shown here in UI)
        }

        private void TestIsolatedStorage(object sender, RoutedEventArgs e)
        {
            // IsolatedStorageFile (User + Assembly level)
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly();

            string folder = "MyFolder";
            string file = "data.txt";

            // Check if directory exists
            if (!store.DirectoryExists(folder))
                store.CreateDirectory(folder); // Create folder

            string path = Path.Combine(folder, file);

            // IsolatedStorageFileStream (Create)
            using (var stream = new IsolatedStorageFileStream(path, FileMode.Create, store))
            using (var writer = new StreamWriter(stream))
                writer.WriteLine("WPF Isolated Storage Test");

            // IsolatedStorageFileStream (Read)
            string readData;
            using (var stream = new IsolatedStorageFileStream(path, FileMode.Open, store))
            using (var reader = new StreamReader(stream))
                readData = reader.ReadToEnd();

            // Access control
            OutputBox.Text = $"Read from Isolated Storage: {readData}";

            // Delete file or directory
            if (store.FileExists(path))
                store.DeleteFile(path);

            if (store.DirectoryExists(folder))
                store.DeleteDirectory(folder);
        }
    }
}
