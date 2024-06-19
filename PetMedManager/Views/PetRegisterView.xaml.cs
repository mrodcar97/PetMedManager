using Domain;
using Newtonsoft.Json;
using PetMedManager.Pages;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace PetMedManager.Views;

public partial class PetRegisterView : ContentView
{
    private readonly HttpClient _httpClient;
    private Boolean _isModifying = false;
    private Pet modifyPet;
    private List<Person>? clientes;


    public PetRegisterView(IHttpClientFactory httpClientFactory)
	{
		InitializeComponent();
        _httpClient = httpClientFactory.CreateClient("DefaultClient");

        // Establecer la fecha máxima en el DatePicker
        DateTime today = DateTime.Today;
        FechaNacimientoEntry.MaximumDate = today;
        FechaNacimientoEntry.BackgroundColor = default;
        LoadClients();
    }
   

    public PetRegisterView(IHttpClientFactory httpClientFactory, Pet p)
    {
        InitializeComponent();
        _httpClient = httpClientFactory.CreateClient("DefaultClient");

        FechaNacimientoEntry.BackgroundColor = default;
        LoadClients();

        if (p != null)
        {
            LoadDataModify(p);
            modifyPet = p;
            _isModifying = true;
        }
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

    private void LoadDataModify(Pet p)
    {
        DateOnly date = p.DateOfBirth;

         NombreEntry.Text = p.Name;
         RazaEntry.Text = p.Breed;
         EspecieEntry.Text = p.Species;
         propietarioEntry.Text = p.OwnerId;
         SexoEntry.SelectedItem = p.Sex;
         FechaNacimientoEntry.Date = new DateTime(date.Year, date.Month, date.Day);
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        bool isValid = ValidateEntries();
        if (isValid)
        {
            DateTime fechaNacimientoDateTime = FechaNacimientoEntry.Date;
            string fechaFormateada = fechaNacimientoDateTime.ToString("yyyy-MM-dd");
            DateOnly fechaNacimientoSolo = DateOnly.Parse(fechaFormateada);

            // Crear el objeto Pet si todas las validaciones son correctas
            var pet = new Pet
            {
                Name = NombreEntry.Text,
                Breed = RazaEntry.Text,
                Sex = SexoEntry.SelectedItem.ToString(),
                Species = EspecieEntry.Text,
                DateOfBirth = fechaNacimientoSolo,
                OwnerId = propietarioEntry.Text
            };

            try
            {
                // Serializa el objeto Person a JSON
                string petJson;

                // Crea un contenido JSON a partir del objeto serializado
                HttpContent content;
                HttpResponseMessage response;

                // Realiza la solicitud POST a la URL especificada con el contenido JSON
                if (!_isModifying)
                {
                    petJson = JsonConvert.SerializeObject(pet);
                    content = new StringContent(petJson, Encoding.UTF8, "application/json");

                    response = await _httpClient.PostAsync("api/Pet", content);
                }
                else
                {
                    pet.Id = modifyPet.Id;
                    petJson = JsonConvert.SerializeObject(pet);
                    content = new StringContent(petJson, Encoding.UTF8, "application/json");
                    response = await _httpClient.PutAsync($"api/Pet/{modifyPet.Id}", content);
                }

                // Verifica si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    MainVetMenu.Instance.ShowPetMainView(sender, e);
                }
                else
                {
                    // La solicitud falló, muestra un mensaje de error
                    await Application.Current.MainPage.DisplayAlert("Error", response.Content + "Error al realizar la solicitud", "OK");
                }
            }
            catch (Exception ex)
            {
                // Captura cualquier excepción que pueda ocurrir durante la solicitud
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al realizar la solicitud: {ex.Message}", "OK");
            }
        }
    }
    private async void OnPetSelected(object sender, ItemTappedEventArgs e)
    {
        try
        {
            var selectedClient = e.Item as Person;
            if (selectedClient == null) return;
            propietarioEntry.Text = selectedClient.NationalId;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los clientes: {ex.Message}", "OK");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        MainVetMenu.Instance.ShowPetMainView(sender, e);
    }

    private bool ValidateEntries()
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(propietarioEntry.Text))
        {
            propietarioEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";
        }
        else if (!Regex.IsMatch(propietarioEntry.Text, @"^\d{8}[A-Z]$"))
        {
            propietarioEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "El DNI introducido no es valido";
        }
        else
        {
            propietarioEntry.BackgroundColor = default;
        }

        if (string.IsNullOrWhiteSpace(NombreEntry.Text))
        {
            NombreEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";

        }
        else
        {
            NombreEntry.BackgroundColor = default;
        }

        if (string.IsNullOrWhiteSpace(RazaEntry.Text))
        {
            RazaEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";
        }
        else
        {
            RazaEntry.BackgroundColor = default;
        }

        if (string.IsNullOrWhiteSpace(EspecieEntry.Text))
        {
            EspecieEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";

        }
        else
        {
            EspecieEntry.BackgroundColor = default;
        }

        return isValid;
    }
    private void OnFilterTextChanged(object sender, TextChangedEventArgs e)
    {
        var dniFilter = dniSearch.Text;
        var nameFilter = nameSearch.Text;

        var filteredPets = clientes.Where(p =>
            (string.IsNullOrEmpty(dniFilter) || p.NationalId.Contains(dniFilter, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(nameFilter) || p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        ClientsListView.ItemsSource = new ObservableCollection<Person>(filteredPets);
    }

}