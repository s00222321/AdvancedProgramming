using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace SampleAssessment
{
    public partial class MainWindow : Window
    {
        private static string[] words = new string[] { "TEST", "LIST" }; // Predefined words
        private string targetWord;
        private char[] guessedLetters;
        private int wordIndex = 0;
        private int totalWords;
        private BackgroundWorker bgWorker;
        private int errors = 0;

        public MainWindow()
        {
            InitializeComponent();
            totalWords = words.Length;
            progressBar.Maximum = totalWords;
            btnCancel.IsEnabled = false;
            InitBackgroundWorker();
        }

        private void InitBackgroundWorker()
        {
            bgWorker = new BackgroundWorker();
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!bgWorker.IsBusy)
            {
                wordIndex = 0;
                progressBar.Value = 0;
                btnStart.IsEnabled = false;
                btnCancel.IsEnabled = true;
                lblErrors.Content = "Errors: ";
                bgWorker.RunWorkerAsync();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (; wordIndex < words.Length; wordIndex++)
            {
                if (bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    targetWord = words[wordIndex];
                    guessedLetters = new string('_', targetWord.Length).ToCharArray();
                    UpdateWordDisplay();
                    lblErrors.Content = "Errors: ";
                    MessageBox.Show("Guess this new word by typing letters in the text box and waiting.");
                });

                while (new string(guessedLetters) != targetWord)
                {
                    if (bgWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    Thread.Sleep(2000); // Wait for user input

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProcessUserGuess();
                        UpdateWordDisplay();
                    });
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    progressBar.Value += 1;
                    MessageBox.Show($"Well done! You guessed the word: {targetWord}");
                });
            }
        }

        private void ProcessUserGuess()
        {
            if (string.IsNullOrEmpty(txtGuess.Text)) return;

            char guessedLetter = char.ToUpper(txtGuess.Text[0]);
            txtGuess.Text = ""; // Clear input box

            if (targetWord.Contains(guessedLetter))
            {
                for (int i = 0; i < targetWord.Length; i++)
                {
                    if (targetWord[i] == guessedLetter)
                    {
                        guessedLetters[i] = guessedLetter;
                    }
                }
            }
            else
            {
                lblErrors.Content += $" {guessedLetter}";
                errors++;
            }
        }

        private void UpdateWordDisplay()
        {
            lbl1.Content = guessedLetters.Length > 0 ? guessedLetters[0].ToString() : "";
            lbl2.Content = guessedLetters.Length > 1 ? guessedLetters[1].ToString() : "";
            lbl3.Content = guessedLetters.Length > 2 ? guessedLetters[2].ToString() : "";
            lbl4.Content = guessedLetters.Length > 3 ? guessedLetters[3].ToString() : "";
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStart.IsEnabled = true;
            btnCancel.IsEnabled = false;

            if (e.Cancelled)
            {
                MessageBox.Show("Game was cancelled.");
            }
            else
            {
                MessageBox.Show("All words guessed! Game Over.");
                progressBar.Value = totalWords;
            }
        }
    }
}
