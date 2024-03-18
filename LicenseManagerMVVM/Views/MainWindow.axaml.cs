using System;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LicenseManagerMVVM.Models;
using LicenseManagerMVVM.ViewModels;

namespace LicenseManagerMVVM.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            DataContext = mainWindowViewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Your logic to handle selection change
            if (sender is DataGrid dataGrid)
            {
                // Do something with the selected item
                if (dataGrid.SelectedItem is License selectedLicense)
                {
                    (DataContext as MainWindowViewModel)!.SelectedLicense =selectedLicense;
                    (DataContext as MainWindowViewModel)!.Payments = new ObservableCollection<Payment>(selectedLicense.payments);
                }
                else
                {
                    Console.WriteLine("no");
                }
                
            }
        }


        private void Activate_Button_OnClick(object? sender, RoutedEventArgs e)
        {
            
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Activate();
            }
            
        }

        private void  Deactivate_Button_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Deactivate();
            }        
        }
    }
}