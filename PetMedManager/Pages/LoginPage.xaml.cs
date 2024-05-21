using System.Net;
using System.Net.Http.Json;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using PetMedManager.Pages;

namespace PetMedManager
{
    public partial class LoginPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;

        public LoginPage(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _httpClient = httpClientFactory.CreateClient("DefaultClient");
            _serviceProvider = serviceProvider;
        }

        private async void OnShowUsersClicked(object sender, EventArgs e)
        {
            var email = userEntry.Text;
            var password = passEntry.Text;

            errorLabel.IsVisible = false;

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                // Objeto con los datos a enviar al servidor
                var user = new User
                {
                    Email = email,
                    Password = password,
                    Rango = "A"
                };

                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", user);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (loginResponse != null)
                    {
                        // Usar el token de acceso obtenido, por ejemplo, almacenarlo en la aplicación para futuras solicitudes
                        await SecureStorage.SetAsync("AuthToken", loginResponse.token);
                        var mainMenu = _serviceProvider.GetRequiredService<MainMenu>();
                        await Navigation.PushAsync(mainMenu);

                    }
                    else
                    {
                        errorLabel.Text = "Error al procesar la solicitud de login";
                        errorLabel.IsVisible = true;
                    }
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    // Leer el contenido de la respuesta
                    var content = await response.Content.ReadAsStringAsync();

                    errorLabel.Text = content;
                    errorLabel.IsVisible = true;
                }
                else
                {
                    errorLabel.Text = "Error en la solicitud de login";
                    errorLabel.IsVisible = true;
                }
            }
            else
            {
                errorLabel.Text = "Por favor, complete todos los campos";
                errorLabel.IsVisible = true;
            }
        }
    }
}
