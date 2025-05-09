using System.Threading;

namespace ThreadingChallenge
{
    internal class Program
    {
        private static readonly int[] numbers = new int[20];
        private static int currentIndex = 0; // Tracks where reading stopped
        private static readonly object lockObject = new object(); // Used for synchronization

        [ThreadStatic] private static int countAccess = 0; // Tracks accesses per thread

        static Thread populateThread = new Thread(new ThreadStart(PopulateArray));
        static Thread readThread1 = new Thread(ReadArray) { Name = "ReaderThread-1" };
        static Thread readThread2 = new Thread(ReadArray) { Name = "ReaderThread-2" };

        static void Main(string[] args)
        {
            // configure population thread
            populateThread.Name = "Populate Thread";
            populateThread.Priority = ThreadPriority.Lowest;
            populateThread.IsBackground = true;

            // start population thread and wait to finish
            populateThread.Start();
            populateThread.Join();

            // start reader threads
            readThread1.Start();
            readThread2.Start();

            // Ensure main does not exit before threads finish
            readThread1.Join();
            readThread2.Join();

            Console.WriteLine("All threads have completed execution.");
        }

        static void PopulateArray()
        {
            // get thread name
            string threadName = Thread.CurrentThread.Name;

            Random random = new Random();
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = random.Next(1,10);
            }

            Console.WriteLine($"\n{threadName} Final Array: [{string.Join(", ", numbers)}]\n");
            Console.WriteLine($"{threadName} EXIT");
        }

        static void ReadArray()
        {
            while (true)
            {
                lock (lockObject)
                {
                    if (currentIndex >= numbers.Length)
                        break; // Exit when all elements are read

                    int maxAvailable = numbers.Length - currentIndex;

                    Console.WriteLine($"{Thread.CurrentThread.Name}: {maxAvailable} elements remaining.");
                    Console.Write($"{Thread.CurrentThread.Name}: Enter the number of elements to read (max {maxAvailable}): ");

                    if (!int.TryParse(Console.ReadLine(), out int toRead) || toRead <= 0)
                    {
                        Console.WriteLine($"{Thread.CurrentThread.Name}: Invalid input. Try again.");
                        continue;
                    }

                    if (toRead > maxAvailable)
                    {
                        Console.WriteLine($"{Thread.CurrentThread.Name}: Error! Requested {toRead}, but only {maxAvailable} available.");
                        countAccess++;
                        Monitor.Pulse(lockObject); // Let other thread proceed
                        Monitor.Wait(lockObject); // Wait for turn
                        continue;
                    }

                    // Read elements
                    Console.Write($"{Thread.CurrentThread.Name} reads: ");
                    for (int i = 0; i < toRead; i++)
                    {
                        Console.Write(numbers[currentIndex] + " ");
                        currentIndex++;
                    }
                    Console.WriteLine();

                    countAccess++;

                    Monitor.Pulse(lockObject); // Notify other thread
                    if (currentIndex < numbers.Length)
                        Monitor.Wait(lockObject); // Wait if elements remain
                }
            }

            Console.WriteLine($"{Thread.CurrentThread.Name} accessed the array {countAccess} times. EXIT.");
        }
    }
}
