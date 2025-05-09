Advanced Programming Project — File Backup Program
Submitted by: Ellen Woodward S00222321
Date: 21st April 2025

---------------------
APPLICATION OVERVIEW
---------------------

This application is a multithreaded file backup program developed in WPF
The user selects a source folder and a destination folder
The app scans all files in the source folder and copies them to the destination using multiple background threads
The app displays real-time progress, logs every file copied, and saves user preferences (folder paths) in isolated storage
The code is commented to highlight where each threading concept is used

---------------------------------------
REQUIREMENT CHECKLIST & IMPLEMENTATION
---------------------------------------

1. INTERFACE
- WPF interface with TextBoxes, Buttons, a ListBox (log) and a ProgressBar

2. 4+ THREADS USED
- UI Thread (Main thread)
- Scan thread (finds and queues files)
- Copy thread (copies files to destination)
- Logger thread (updates progress display)
- Additional: One parameterised InfoLogger thread

3. THREAD INTERACTION USING SHARED RESOURCES + MONITOR CLASS 
- Shared resource: BlockingCollection<string> for file queue
- All file-copy and queue interactions are synchronised using Monitor.Enter and Monitor.Exit
- Monitor.Wait() is used in the Logger thread to pause until notified
- Monitor.Pulse() is used in Scan/Copy threads to wake the Logger

4. 6+ THREADING TECHNIQUES USED 
- Thread.Name - Set for each thread for clarity
- Thread.Priority - Assigned different priorities
- IsBackground - All threads run in the background
- Thread.Join() - Used on Cancel to wait for thread completion
- Thread.Sleep() - Used to simulate work in Scan and Copy threads
- ThreadState - Logged on cancellation
- ThreadStatic - threadFileCount used to track per-thread work
- Thread with parameters - InfoLogger thread uses a lambda to receive a message string

5. ISOLATED STORAGE 
- Uses IsolatedStorageFile.GetUserStoreForAssembly()
- Uses StreamWriter and StreamReader to save and load preferences
- Saves and loads last-used source and destination folders on app open/close

6. EXTRA THREADING/ADVANCED TECHNIQUES 
- Uses BlockingCollection<T> to implement a producer-consumer pattern
- Demonstrates cooperative cancellation using CancellationToken
- Implements parameterised threads using lambda functions
- Provides rich and real-time UI updates from background threads using Dispatcher
- Clean and structured thread lifecycle management with proper shutdown

7. WORKING, COMPLEX, FUNCTIONAL APP
- Realistic and usable application
- Threads do real wrok (file I/O, logging, queuing, UI updates)
- Gracefully handles cancellation, file errors and logs all activity