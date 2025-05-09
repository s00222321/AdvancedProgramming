using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Cheatsheet
{
    public partial class MainWindow : Window
    {
        private Thread dataCollectionThread; // Thread 1
        private Thread inputCaptureThread;   // Thread 2
        private BackgroundWorker reportingWorker; // Thread 3
        private object lockObject = new object(); // For synchronization
        private List<string> sharedResource = new List<string>(); // Shared data structure
        private bool isRunning = true; // Control flag for threads

        public MainWindow()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
        }

        private void StartThreads_Click(object sender, RoutedEventArgs e)
        {
            // Start Data Collection Thread (Thread 1)
            dataCollectionThread = new Thread(DataCollection);
            dataCollectionThread.IsBackground = true;
            dataCollectionThread.Start();

            // Start Input Capture Thread (Thread 2)
            inputCaptureThread = new Thread(InputCapture);
            inputCaptureThread.IsBackground = true;
            inputCaptureThread.Start();

            // Start Reporting BackgroundWorker (Thread 3)
            if (!reportingWorker.IsBusy)
                reportingWorker.RunWorkerAsync();
        }

        private void StopThreads_Click(object sender, RoutedEventArgs e)
        {
            isRunning = false; // Stop flag
            dataCollectionThread?.Join(); // Ensure threads exit properly
            inputCaptureThread?.Join();
            if (reportingWorker.WorkerSupportsCancellation)
                reportingWorker.CancelAsync();
        }

        // ------------------------------ THREAD 1: DATA COLLECTION ------------------------------
        private void DataCollection()
        {
            while (isRunning)
            {
                Thread.Sleep(2000); // Simulate data entry delay
                string newData = $"Entry {DateTime.Now:HH:mm:ss}";

                lock (lockObject) // Synchronizing access to shared resource
                {
                    sharedResource.Add(newData);
                }

                Dispatcher.Invoke(() => ListBoxData.Items.Add(newData)); // UI update
            }
        }

        // ------------------------------ THREAD 2: INPUT CAPTURE ------------------------------
        private void InputCapture()
        {
            while (isRunning)
            {
                Thread.Sleep(3000); // Simulating periodic checks

                lock (lockObject)
                {
                    if (sharedResource.Count > 0)
                    {
                        string lastEntry = sharedResource[sharedResource.Count - 1];
                        Dispatcher.Invoke(() => ListBoxCriteria.Items.Add($"Checked: {lastEntry}"));
                    }
                }
            }
        }

        // ------------------------------ THREAD 3: REPORTING (BACKGROUNDWORKER) ------------------------------
        private void InitializeBackgroundWorker()
        {
            reportingWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            reportingWorker.DoWork += Reporting_DoWork;
            reportingWorker.ProgressChanged += Reporting_ProgressChanged;
            reportingWorker.RunWorkerCompleted += Reporting_Completed;
        }

        private void Reporting_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!reportingWorker.CancellationPending)
            {
                Thread.Sleep(1500); // Simulated processing delay

                string reportData = "";
                lock (lockObject)
                {
                    if (sharedResource.Count > 0)
                    {
                        reportData = $"Processed: {sharedResource[sharedResource.Count - 1]}";
                    }
                }

                reportingWorker.ReportProgress(0, reportData);
            }

            if (reportingWorker.CancellationPending)
                e.Cancel = true;
        }

        private void Reporting_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string reportData && !string.IsNullOrEmpty(reportData))
            {
                ListBoxReports.Items.Add(reportData);
                ProgressBarStatus.Value += 10; // Simulate progress updates
            }
        }

        private void Reporting_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Reporting stopped!");
            }
        }
    }
}
