using Domain;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PetMedManager.Pages;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace PetMedManager.Views;

public partial class AppointmentCreateView : ContentView
{
    private readonly HttpClient _httpClient;
    private List<Person>? clientes;
    private List<Pet>? pets;
    private Boolean _isModifying = false;
    private string vetId;


    public AppointmentCreateView(IHttpClientFactory httpClientFactory)
    {
        InitializeComponent();
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
        LoadClients();

    }

    public AppointmentCreateView(IHttpClientFactory httpClientFactory, DateTime selectedDate,string Vetid,string time)
    {
        InitializeComponent();
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
        LoadClients();
        DayPicker.Date = selectedDate.Date;
        DayPicker.IsEnabled = false;

        if (TimeSpan.TryParse(time, out TimeSpan timeSpan))
        {
           TimePicker.Time = timeSpan;
            TimePicker.IsEnabled = false; 
        }
        vetId = Vetid;
        
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

    private async void OnClientSelected(object sender, ItemTappedEventArgs e)
    {
        try
        {
            var selectedClient = e.Item as Person;
            if (selectedClient == null) return;

            var response = await _httpClient.GetAsync($"api/Pet/ByOwner{selectedClient.NationalId}");
            var content = await response.Content.ReadAsStringAsync();
            pets = JsonConvert.DeserializeObject<List<Pet>>(content);
            var petNames = pets.Select(pet => pet.Name).ToList();
            PetPicker.ItemsSource = petNames;
            PetPicker.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los clientes: {ex.Message}", "OK");
        }
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        bool isValid = ValidateEntries();

        if (isValid)
        {
            DateTime fechaNacimientoDateTime = DayPicker.Date;
            string fechaFormateada = fechaNacimientoDateTime.ToString("yyyy-MM-dd");
            DateOnly fechaCita = DateOnly.Parse(fechaFormateada);

            var selectedTime = TimePicker.Time.ToString();
            Guardar.Text = selectedTime;
            TimeOnly appointmentTime = TimeOnly.Parse(selectedTime);

            var pet = pets.FirstOrDefault(pet => pet.Name == PetPicker.SelectedItem);

            // Crear el objeto Pet si todas las validaciones son correctas
            var Appointment = new Appointment
            {
                Title = AppointmentTitle.Text,
                Description = AppointmentDescription.Text,
                Date = fechaCita,
                StartTime = appointmentTime,
                PetId = pet.Id
            };

            if (!vetId.Equals("General"))
            {
                Appointment.VetId = vetId;
            }

            try
            {
                // Serializa el objeto Person a JSON
                string AppointmentJson = JsonConvert.SerializeObject(Appointment);

                // Crea un contenido JSON a partir del objeto serializado
                HttpContent content = new StringContent(AppointmentJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response;
                // Realiza la solicitud POST a la URL especificada con el contenido JSON
                if (!_isModifying)
                {
                    response = await _httpClient.PostAsync("api/Appointment", content);
                }
                else
                {
                    response = await _httpClient.PutAsync("api/Person/{}", content);
                }

                // Verifica si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    MainAdminMenu.Instance.ShowMainClientView(sender, e);
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

    private void OnCancelClicked(object sender, EventArgs e)
    {
        MainAdminMenu.Instance.ShowMainClientView(sender, e);
    }

    private bool ValidateEntries()
    {
        bool isValid = true;

        if (PetPicker.SelectedIndex == -1)
        {
            PetPicker.BackgroundColor = Colors.LightCoral;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";
        }
        else
        {
            PetPicker.BackgroundColor = default;
        }

        if (string.IsNullOrWhiteSpace(DayPicker.ToString()))
        {
            DayPicker.BackgroundColor = Colors.LightCoral;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";
        }
        else
        {
            DayPicker.BackgroundColor = default;
        }

        if (string.IsNullOrWhiteSpace(TimePicker.ToString()))
        {
            TimePicker.BackgroundColor = Colors.LightCoral;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";
        }
        else
        {
            TimePicker.BackgroundColor = default;
        }

        if (string.IsNullOrWhiteSpace(AppointmentTitle.Text))
        {
            AppointmentTitle.BackgroundColor = Colors.LightCoral;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";

        }
        else
        {
            AppointmentTitle.BackgroundColor = default;
        }

        if (string.IsNullOrWhiteSpace(AppointmentDescription.Text))
        {
            AppointmentDescription.BackgroundColor = Colors.LightCoral;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";
        }
        else
        {
            AppointmentDescription.BackgroundColor = default;
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