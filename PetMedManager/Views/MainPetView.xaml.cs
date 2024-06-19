using Domain;
using Microsoft.Maui;
using Newtonsoft.Json;
using PetMedManager.Pages;
using System.Collections.ObjectModel;

namespace PetMedManager.Views;

public partial class MainPetView : ContentView
{
    private readonly HttpClient _httpClient;
    private List<Pet>? clientes;

    public MainPetView(IHttpClientFactory httpClientFactory)
    {
        InitializeComponent();
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
        LoadClients();
    }

    private async void LoadClients()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Pet");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            clientes = JsonConvert.DeserializeObject<List<Pet>>(content);
            ClientsListView.ItemsSource = clientes;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar las mascotas: {ex.Message}", "OK");
        }
    }

    private void OnRegisterClicked(object sender, EventArgs e)
    {
        MainVetMenu.Instance.ShowPetRegisterView();
    }

    private void OnModifyClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null)
            return;

        var pet = button.BindingContext as Pet;
        MainVetMenu.Instance.ShowPetModifyView(pet);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null)
            return;
        
        var pet = button.BindingContext as Pet;
        var response = await _httpClient.DeleteAsync($"api/Pet/{pet.Id}");
        LoadClients();
    }
    private void OnFilterTextChanged(object sender, TextChangedEventArgs e)
    {
        var dniFilter = dniSearch.Text;
        var nameFilter = nameSearch.Text;

        var filteredPets = clientes.Where(pet =>
            (string.IsNullOrEmpty(dniFilter) || pet.OwnerId.Contains(dniFilter, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(nameFilter) || pet.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        ClientsListView.ItemsSource = new ObservableCollection<Pet>(filteredPets);
    }
}