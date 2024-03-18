using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LicenseManager
{
    public partial class NotificationWindow : Window
    {
        public NotificationWindow(string title, string message)
        {
            InitializeComponent();
            Title = title;
            DataContext =  "test22";

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}