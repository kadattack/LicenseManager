using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace LicenseManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Handle button click event
            ShowNotification("title test", "test");
        }

        private void ShowNotification(string title, string message)
        {
            var notificationWindow = new NotificationWindow(title, message)
            {
                Width = 300,
                Height = 100,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            notificationWindow.ShowDialog(this);
        }
    }
}