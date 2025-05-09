namespace _2023ExamQ2
{
    internal class Program
    {
        private static double bankAccount = 1000.00;
        private static readonly object accountLock = new object();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Thread moneyIn = new Thread(MoneyIn) { Name = "Lodge" };
            Thread moneyOut = new Thread(MoneyOut) { Name = "Withdraw" };

            moneyIn.Start();
            moneyOut.Start();

            moneyIn.Join();
            moneyOut.Join();

            Console.WriteLine("Final balance: €" + bankAccount.ToString("F2"));
        }

        static void MoneyIn()
        {
            Random random = new Random();
            for (int i = 0; i < 3; i++)
            {
                Monitor.Enter(accountLock);
                try
                {
                    double amount = random.Next(100, 1001);
                    bankAccount += amount;
                    Console.WriteLine($"{Thread.CurrentThread.Name} added €{amount:F2}, balance is now €{bankAccount:F2}");

                    Monitor.Pulse(accountLock); // Notify waiting threads

                    if (i < 2)
                    {
                        Monitor.Wait(accountLock); // Wait for the other thread to finish
                    }
                }
                finally
                {
                    Monitor.Exit(accountLock);
                }
                Thread.Sleep(1000); // Simulate work
            }
        }

        static void MoneyOut()
        {
            Random random = new Random();
            for (int i = 0; i < 3; i++)
            {
                Monitor.Enter(accountLock);
                try
                {
                    double amount = random.Next(50, 501);
                    if (bankAccount >= amount)
                    {
                        bankAccount -= amount;
                        Console.WriteLine($"{Thread.CurrentThread.Name} withdrew €{amount:F2}, balance is now €{bankAccount:F2}");
                    }
                    else
                    {
                        Console.WriteLine($"{Thread.CurrentThread.Name} tried to withdraw €{amount:F2}, but insufficient funds. Balance remains €{bankAccount:F2}");
                    }
                    Monitor.Pulse(accountLock); // Notify waiting threads
                    if (i < 2)
                    {
                        Monitor.Wait(accountLock); // Wait for the other thread to finish
                    }
                }
                finally
                {
                    Monitor.Exit(accountLock);
                }
                Thread.Sleep(1000); // Simulate work
            }
        }
    }
}
