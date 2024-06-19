using Domain;
using Newtonsoft.Json;
using PetMedManager.Pages;
using System.Collections.ObjectModel;

namespace PetMedManager.Views;

public partial class MainClientView : ContentView
{
    private readonly HttpClient _httpClient;
    private List<Person>? clientes;

    public MainClientView(IHttpClientFactory httpClientFactory)
    {
        InitializeComponent();
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
        LoadClients();
    }

    private async void LoadClients()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Person");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            clientes = JsonConvert.DeserializeObject<List<Person>>(content);
            ClientsListView.ItemsSource = clientes;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los clientes: {ex.Message}", "OK");
        }
    }

    private void OnRegisterClicked(object sender, EventArgs e)
    {
        MainAdminMenu.Instance.ShowClientRegisterView();
    }
    private void OnModifyClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null)
            return;

        var client = button.BindingContext as Person;
        MainAdminMenu.Instance.ShowClientModifyView(client); // Usar la referencia estática
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null)
            return;

        var client = button.BindingContext as Person; 
        var response = await _httpClient.DeleteAsync($"api/Person/{client.NationalId}");
        LoadClients();
    }
    private void OnFilterTextChanged(object sender, TextChangedEventArgs e)
    {
        var dniFilter = dniSearch.Text;
        var nameFilter = nameSearch.Text;
        var emailFilter = emailSearch.Text;
        var phoneFilter = phoneSearch.Text;

        var filteredPets = clientes.Where(p =>
            (string.IsNullOrEmpty(dniFilter) || p.NationalId.Contains(dniFilter, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(nameFilter) || p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(emailFilter) || p.Email.Contains(emailFilter, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(phoneFilter) || p.Phone.ToString().Contains(phoneFilter, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        ClientsListView.ItemsSource = new ObservableCollection<Person>(filteredPets);
    }
}
