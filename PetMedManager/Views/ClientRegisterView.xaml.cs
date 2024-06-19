using Domain;
using Newtonsoft.Json;
using PetMedManager.Pages;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace PetMedManager.Views;

public partial class ClientRegisterView : ContentView
{
    private readonly HttpClient _httpClient;
    private Boolean _isModifying = false;

    public ClientRegisterView(IHttpClientFactory httpClientFactory)
    {
        InitializeComponent();
        _httpClient = httpClientFactory.CreateClient("DefaultClient");

        var today = DateTime.Today;
        var maxDate = today.AddYears(-16);

        // Establecer la fecha mínima y máxima en el DatePicker
        FechaNacimientoEntry.MaximumDate = maxDate;
        FechaNacimientoEntry.BackgroundColor = default;
    }

    public ClientRegisterView(IHttpClientFactory httpClientFactory, Person p)
    {
        InitializeComponent();
        _httpClient = httpClientFactory.CreateClient("DefaultClient");

        var today = DateTime.Today;
        var maxDate = today.AddYears(-16);

        // Establecer la fecha mínima y máxima en el DatePicker
        FechaNacimientoEntry.MaximumDate = maxDate;
        FechaNacimientoEntry.BackgroundColor = default;

        if (p != null)
        {
            LoadDataModify(p);
            _isModifying = true;
        }
    }

    private void LoadDataModify(Person p)
    {
        DateOnly date = p.DateOfBirth.Value;

        DNIEntry.Text = p.NationalId;
        DNIEntry.IsEnabled = false;
        NombreEntry.Text = p.Name;
        ApellidoEntry.Text = p.Surname;
        TelefonoEntry.Text = p.Phone.ToString();
        ProvinciaEntry.Text = p.Province;
        LocalidadEntry.Text = p.Locality;
        EmailEntry.Text = p.Email;
        DireccionEntry.Text = p.Address;
        FechaNacimientoEntry.Date =  new DateTime(date.Year, date.Month, date.Day);
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        bool isValid = ValidateEntries();
        if (isValid)
        {
            DateTime fechaNacimientoDateTime = FechaNacimientoEntry.Date;
            string fechaFormateada = fechaNacimientoDateTime.ToString("yyyy-MM-dd");
            DateOnly fechaNacimientoSolo = DateOnly.Parse(fechaFormateada);

            // Crear el objeto Person si todas las validaciones son correctas
            var person = new Person
            {
                NationalId = DNIEntry.Text,
                Name = NombreEntry.Text,
                Surname = ApellidoEntry.Text,
                Address = DireccionEntry.Text,
                Phone = Convert.ToInt32(TelefonoEntry.Text),
                Province = ProvinciaEntry.Text,
                Locality = LocalidadEntry.Text,
                Email = EmailEntry.Text,
                DateOfBirth = fechaNacimientoSolo,
                Rol = "Cliente",
                ClinicId = 1
            };

            try
            {
                // Serializa el objeto Person a JSON
                string personJson = JsonConvert.SerializeObject(person);

                // Crea un contenido JSON a partir del objeto serializado
                HttpContent content = new StringContent(personJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response;
                // Realiza la solicitud POST a la URL especificada con el contenido JSON
                if (!_isModifying)
                {
                    response = await _httpClient.PostAsync("api/Person", content);
                }
                else
                {
                    response = await _httpClient.PutAsync($"api/Person/{person.NationalId}", content);
                }

                // Verifica si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    MainAdminMenu.Instance.ShowMainClientView(sender,e);
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

        if (string.IsNullOrWhiteSpace(DNIEntry.Text))
        {
            DNIEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";
        }
        else if (!Regex.IsMatch(DNIEntry.Text, @"^\d{8}[A-Z]$"))
        {
            DNIEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "El DNI introducido no es valido";
        }
        else
        {
            DNIEntry.BackgroundColor = default;
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

        if (string.IsNullOrWhiteSpace(ApellidoEntry.Text))
        {
            ApellidoEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";

        }
        else
        {
            ApellidoEntry.BackgroundColor = default;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            EmailEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";

        }
        else
        {
            EmailEntry.BackgroundColor = default;
        }

        if (string.IsNullOrWhiteSpace(DireccionEntry.Text))
        {
            DireccionEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";
        }
        else
        {
            DireccionEntry.BackgroundColor = default;
        }

        if (string.IsNullOrWhiteSpace(TelefonoEntry.Text))
        {
            TelefonoEntry.BackgroundColor = Colors.Red;
            isValid = false;
            ErrorLabel.Text = "Los campos marcados no pueden estar vacios";
        }
        else
        {
            TelefonoEntry.BackgroundColor = default;
        }

        return isValid;
    }
}