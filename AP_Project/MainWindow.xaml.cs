using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ookii.Dialogs.Wpf;
using System.Collections.Concurrent;

namespace AP_Project
{
    public partial class MainWindow : Window
    {
        // threading-related fields
        private CancellationTokenSource _cts;
        private BlockingCollection<string> _fileQueue; // thread-safe queue for producer-consumer pattern
        private Thread _scanThread;
        private Thread _copyThread;
        private Thread _logThread;

        // File tracking fields
        private int _totalFiles = 0;
        private int _filesCopied = 0;
        private readonly object _syncLock = new(); // lock object for monitor synchronisation

        [ThreadStatic] // per-thread static field, creates a static field that is unique to each thread
        private static int _threadFileCount;

        public MainWindow()
        {
            InitializeComponent();
            LoadPreferences();  // load saved folder paths from isolated storage on startup
        }

        private string SelectFolder()
        {
            // sisplay folder picker dialog and return selected path
            var dialog = new VistaFolderBrowserDialog();
            return dialog.ShowDialog() == true ? dialog.SelectedPath : string.Empty;
        }

        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            // set source folder path
            var path = SelectFolder();
            if (!string.IsNullOrEmpty(path))
                SourceTextBox.Text = path;
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            // set destination folder path
            var path = SelectFolder();
            if (!string.IsNullOrEmpty(path))
                DestinationTextBox.Text = path;
        }

        private void StartInfoThread(string message)
        {
            // shows creating a thread that accepts a parameter
            Thread infoThread = new Thread((obj) =>
            {
                string msg = (string)obj;
                Log($"[InfoThread] Started with message: {msg}");
            });

            // set thread properties to demonstrate course concepts
            infoThread.Name = "InfoLogger";
            infoThread.IsBackground = true; 
            infoThread.Priority = ThreadPriority.Lowest; 
            infoThread.Start(message); // pass a message as parameter
        }

        private void StartBackup_Click(object sender, RoutedEventArgs e)
        {
            StartInfoThread("Backup process initiated.");  // log initial message using separate thread

            string source = SourceTextBox.Text;
            string destination = DestinationTextBox.Text;

            if (!Directory.Exists(source) || string.IsNullOrWhiteSpace(destination))
            {
                MessageBox.Show("Please select valid source and destination folders.");
                return;
            }

            SavePreferences(source, destination); // save paths to isolated storage
            _fileQueue = new BlockingCollection<string>(); // initialise teh producer consumer queue
            _cts = new CancellationTokenSource();  // create cancellation token
            _filesCopied = 0;

            _totalFiles = Directory.GetFiles(source).Length;  // count files in source
            ProgressBar.Value = 0;
            ProgressBar.Maximum = _totalFiles;

            Log("Starting backup...");

            // create scanning thread to scan and queue files
            _scanThread = new Thread(() => ScanFiles(source, _cts.Token))
            {
                Name = "Scanner",
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true
            };

            // create copying thread to consume and copy files
            _copyThread = new Thread(() => CopyFiles(destination, _cts.Token))
            {
                Name = "Copier",
                Priority = ThreadPriority.Normal,
                IsBackground = true
            };

            // create logger thread to update progress bar
            _logThread = new Thread(() => MonitorProgress(_cts.Token))
            {
                Name = "Logger",
                Priority = ThreadPriority.Lowest,
                IsBackground = true
            };

            // start all threads
            _scanThread.Start();
            _copyThread.Start(); 
            _logThread.Start();  
        }

        private void CancelBackup_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel(); // request cancellation
            Log("Cancellation requested.");

            // wait for all threads to finish
            _scanThread?.Join();
            _copyThread?.Join(); 
            _logThread?.Join();

            // log final states
            Log($"{_scanThread?.Name} state: {_scanThread?.ThreadState}");
            Log($"{_copyThread?.Name} state: {_copyThread?.ThreadState}");
            Log($"{_logThread?.Name} state: {_logThread?.ThreadState}");
        }

        private void ScanFiles(string source, CancellationToken token)
        {
            try
            {
                foreach (var file in Directory.GetFiles(source))
                {
                    if (token.IsCancellationRequested) break;

                    bool lockTaken = false;
                    try
                    {
                        // use Monitor to ensure thread-safe access to shared resource
                        Monitor.Enter(_syncLock, ref lockTaken);
                        _fileQueue.Add(file);  // add file to queue - producer
                        Log($"Queued: {Path.GetFileName(file)}");
                        _threadFileCount++;  // count files per thread

                        // notify waiting threads that a new item is available
                        Monitor.Pulse(_syncLock);
                    }
                    finally
                    {
                        if (lockTaken) Monitor.Exit(_syncLock);
                    }

                    Thread.Sleep(100); // simulate a delay
                }
            }
            catch (Exception ex)
            {
                Log($"Error during file scan: {ex.Message}");
            }
            finally
            {
                _fileQueue.CompleteAdding();  // mark queue as complete
            }
        }

        private void CopyFiles(string destination, CancellationToken token)
        {
            try
            {
                foreach (var file in _fileQueue.GetConsumingEnumerable())
                {
                    if (token.IsCancellationRequested) break;

                    string destPath = Path.Combine(destination, Path.GetFileName(file));
                    try
                    {
                        File.Copy(file, destPath, overwrite: true);  // copy file to destination
                        Interlocked.Increment(ref _filesCopied);  // safely increment shared counter
                        Log($"Copied: {Path.GetFileName(file)}");
                        _threadFileCount++;  // track copies per thread

                        // notify logger thread to update progress
                        bool lockTaken = false;
                        try
                        {
                            Monitor.Enter(_syncLock, ref lockTaken);
                            Monitor.Pulse(_syncLock);
                        }
                        finally
                        {
                            if (lockTaken) Monitor.Exit(_syncLock);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Error copying file {file}: {ex.Message}");
                    }

                    Thread.Sleep(200); // simulate delay
                }
            }
            catch (Exception ex)
            {
                Log($"Error during file copy: {ex.Message}");
            }
        }

        private void MonitorProgress(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    bool lockTaken = false;
                    try
                    {
                        Monitor.Enter(_syncLock, ref lockTaken);

                        // wait for pulse or timeout every 500ms
                        Monitor.Wait(_syncLock, 500);

                        if (token.IsCancellationRequested) break;

                        // update progress bar on UI thread
                        Dispatcher.Invoke(() =>
                        {
                            StatusTextBlock.Text = $"Files Copied: {_filesCopied}/{_totalFiles}";
                            ProgressBar.Value = _filesCopied;
                        });
                    }
                    finally
                    {
                        if (lockTaken) Monitor.Exit(_syncLock);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                Log("Monitor thread interrupted.");
            }
        }

        private void Log(string message)
        {
            // append log message to textbox on UI thread
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                LogTextBox.ScrollToEnd();
            });
        }

        private void SavePreferences(string source, string destination)
        {
            try
            {
                // isolated storage: Write
                // save paths to isolated storage using assembly scope
                using var store = IsolatedStorageFile.GetUserStoreForAssembly();
                using var writer = new StreamWriter(store.CreateFile("prefs.txt"));
                writer.WriteLine(source);
                writer.WriteLine(destination);
            }
            catch (Exception ex)
            {
                Log($"Error saving preferences: {ex.Message}");
            }
        }

        private void LoadPreferences()
        {
            try
            {
                // isolated storage: Read
                // load saved folder paths if file exists in isolated storage
                using var store = IsolatedStorageFile.GetUserStoreForAssembly();
                if (!store.FileExists("prefs.txt")) return;

                using var reader = new StreamReader(store.OpenFile("prefs.txt", FileMode.Open));
                SourceTextBox.Text = reader.ReadLine() ?? "";
                DestinationTextBox.Text = reader.ReadLine() ?? "";
            }
            catch (Exception ex)
            {
                Log($"Failed to load previous preferences: {ex.Message}");
            }
        }
    }
}
