using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using LicenseManagerMVVM.Models;
using LicenseManagerMVVM.ViewModels;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

public class MainWindowViewModel : ViewModelBase
{
    private ObservableCollection<Client> _clients;
    private ObservableCollection<License> _clientLicenses;
    private ObservableCollection<Payment> _licensePayments;
    
    private License _selectedLicense;
    public License SelectedLicense
    {
        get => _selectedLicense; 
        set => this.RaiseAndSetIfChanged(ref _selectedLicense, value);
    }

    
    public ObservableCollection<Payment> Payments
    {
        get => _licensePayments;
        set => this.RaiseAndSetIfChanged(ref _licensePayments, value);
    }
    
    public ObservableCollection<Client> Clients
    {
        get => _clients;
        private set => this.RaiseAndSetIfChanged(ref _clients, value);
    }
    public ObservableCollection<License> Licenses
    {
        get => _clientLicenses;
        private set => this.RaiseAndSetIfChanged(ref _clientLicenses, value);
    }

    private readonly AppDbContext _appDbContext;


    public MainWindowViewModel(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;

        InitializeAsync();
    }

    
    
    public void Activate()
    {
        // Update the value in the database
        // For example, if you have a License object:
        var licenseToUpdate = _appDbContext.License.Find(1);
        licenseToUpdate.active = true; // Update the IsActive property to true
        _appDbContext.SaveChanges();

        var clients = _appDbContext.Client.Include(x=>x.licenses).ThenInclude(x=>x.product).Include(x=>x.licenses).ThenInclude(x=>x.payments).ToList();
        var licenses = clients.First().licenses;
        Dispatcher.UIThread.Post(() =>
        {
            Licenses = new ObservableCollection<License>(licenses);
        });
    }
    
    
    public void Deactivate()
    {
        var licenseToUpdate = _appDbContext.License.Find(1);
        licenseToUpdate.active = false; // Update the IsActive property to true
        _appDbContext.Update(licenseToUpdate);
        _appDbContext.SaveChanges();

        var clients = _appDbContext.Client.Include(x=>x.licenses).ThenInclude(x=>x.product).Include(x=>x.licenses).ThenInclude(x=>x.payments).ToList();
        var licenses = clients.First().licenses;
        Dispatcher.UIThread.Post(() =>
        {
            Licenses = new ObservableCollection<License>(licenses);
        });
    }
    
    
    private async void InitializeAsync()
    {
        var clients = await _appDbContext.Client.Include(x=>x.licenses).ThenInclude(x=>x.product).Include(x=>x.licenses).ThenInclude(x=>x.payments).ToListAsync();
        var licenses = clients.First().licenses;
        SelectedLicense = licenses.First();

        var payments = SelectedLicense.payments.ToList();

        // Use Dispatcher to update UI on the UI thread
        Dispatcher.UIThread.Post(() =>
        {

            Payments = new ObservableCollection<Payment>(payments);
            Clients = new ObservableCollection<Client>(clients);
            Licenses = new ObservableCollection<License>(licenses);
        });
    }
}