// Required namespaces
using System.ComponentModel;                  // For BackgroundWorker
using System.IO.IsolatedStorage;              // For isolated storage functionality
using System.IO;                              // For file and stream handling
using System.Text;                            // For encoding (not used here, but common with IO)
using System.Windows;                         // Core WPF window controls
using System.Windows.Controls;                // For controls like TextBox, ComboBox, etc.
using System.Windows.Media;                   // For handling colours and brushes

namespace _2022ExamQ2
{
    public partial class MainWindow : Window
    {
        private BackgroundWorker backgroundWorker; // Worker that runs tasks in the background

        public MainWindow()
        {
            InitializeComponent(); // Load the XAML UI

            // Create and configure the background worker
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork; // Task to do in the background
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted; // What to do when the background task finishes
        }

        // Triggered when the "Save colour to iso storage" button is clicked
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item from the dropdown
            if (StorageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedStorageType = selectedItem.Content.ToString();

                // Only run if the background worker is not already running
                if (!backgroundWorker.IsBusy)
                {
                    ErrorMessage.Text = string.Empty; // Clear old errors
                    backgroundWorker.RunWorkerAsync(selectedStorageType); // Start background task, passing the selected storage type
                }
            }
        }

        // Background worker logic - this runs on a separate thread
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string storageType = e.Argument.ToString(); // Retrieve the selected storage type
            string colourName = string.Empty;

            // Use Dispatcher to access the UI thread and get the text from the colour input box
            Dispatcher.Invoke(() => colourName = ColorTextBox.Text.Trim());

            try
            {
                // Try converting the input to a valid colour
                Color color = (Color)ColorConverter.ConvertFromString(colourName);
            }
            catch
            {
                // If invalid, show an error message on the UI thread
                Dispatcher.Invoke(() => ErrorMessage.Text = "Invalid colour name.");
                return; // Exit the background task
            }

            IsolatedStorageFile store = null;

            // Select isolated storage type based on dropdown selection
            if (storageType == "GetUserStoreForDomain")
                store = IsolatedStorageFile.GetUserStoreForDomain();
            else if (storageType == "GetUserStoreForAssembly")
                store = IsolatedStorageFile.GetUserStoreForAssembly();

            if (store != null)
            {
                // Define folder and file name within isolated storage
                string folder = "MyColourFolder";
                string fileName = "ColourData.txt";

                // Create folder if it doesn't exist
                if (!store.DirectoryExists(folder))
                    store.CreateDirectory(folder);

                // Open a stream to the file for writing
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream($"{folder}/{fileName}", FileMode.Create, store))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(colourName); // Write the colour name to the file
                }
            }
        }

        // Runs after the background task is complete
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Only show success message if no error occurred
            if (string.IsNullOrEmpty(ErrorMessage.Text))
                ErrorMessage.Text = "Colour saved successfully.";
        }

        // Triggered when "Apply colour from iso storage" button is clicked
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected storage type from the dropdown
            if (StorageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string storageType = selectedItem.Content.ToString();
                IsolatedStorageFile store = null;

                // Select correct storage scope
                if (storageType == "GetUserStoreForDomain")
                    store = IsolatedStorageFile.GetUserStoreForDomain();
                else if (storageType == "GetUserStoreForAssembly")
                    store = IsolatedStorageFile.GetUserStoreForAssembly();

                string folder = "MyColourFolder";
                string fileName = "ColourData.txt";

                try
                {
                    // Open file and read the stored colour name
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream($"{folder}/{fileName}", FileMode.Open, store))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string colourName = reader.ReadLine();
                        Color color = (Color)ColorConverter.ConvertFromString(colourName); // Convert text to colour
                        this.Background = new SolidColorBrush(color); // Apply colour to window background
                        ErrorMessage.Text = $"Applied: {colourName}";
                    }
                }
                catch (Exception ex)
                {
                    // If anything goes wrong (e.g. file not found), show error message
                    ErrorMessage.Text = "Failed to apply colour. Was it saved?";
                }
            }
        }
    }
}
