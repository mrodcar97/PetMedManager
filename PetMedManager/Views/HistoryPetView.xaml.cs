using Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace PetMedManager.Views
{
    public partial class HistoryPetView : ContentView
    {
        private readonly HttpClient _httpClient;
        private List<Pet>? clientes;


        public HistoryPetView(IHttpClientFactory httpClientFactory)
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

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            var selectedPet = e.Item as Pet;
            LoadHistories(selectedPet);

            propietarioEntry.Text = selectedPet.OwnerId;
            NombreEntry.Text = selectedPet.Name;
            RazaEntry.Text = selectedPet.Breed;
            EspecieEntry.Text = selectedPet.Species;

        }
        private async void LoadHistories(Pet p)
        {
            history.Children.Clear();

            var response = await _httpClient.GetAsync($"api/VisitHistory/ByPet{p.Id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var histories = JsonConvert.DeserializeObject<List<VisitHistory>>(content);

            if (histories.Count > 0)
            {
                foreach (VisitHistory v in histories)
                {
                    var date = v.Date.Value.ToString("dd-MM-yyyy");
                    var frame = new Frame
                    {
                        BorderColor = Color.FromHex("#4682B4"),
                        BackgroundColor = Colors.Transparent,
                        Padding = 10,
                        Content = new StackLayout
                        {
                            Children =
                    {
                        new Label { HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromHex("#4682B4"), Text = $"Fecha:\n {date}", FontSize = 16 },
                        new Label { HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromHex("#4682B4"), Text = $"Descripción:\n {v.Description}", FontSize = 16 },
                    }
                        },
                        WidthRequest = 300
                    };
                    history.Children.Add(frame);
                }
            }
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
}
