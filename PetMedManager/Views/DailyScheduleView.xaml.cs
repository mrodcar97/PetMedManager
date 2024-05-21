using System.Net;
using System.Net.Http.Json;
using Domain;

namespace PetMedManager.Views
{
    public partial class DailyScheduleView : ContentView
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public DailyScheduleView(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClient = httpClientFactory.CreateClient("DefaultClient");
            LoadAppointments();
        }

        private async void LoadAppointments()
        {
            try
            {
                var appointments = await GetDailyAppointments();
                DisplayAppointments(appointments);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudieron cargar las citas: " + ex.Message, "OK");
            }
        }

        private async Task<List<Appointment>> GetDailyAppointments()
        {
            var response = await _httpClient.GetAsync(""); 
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Appointment>>();
        }

        private void DisplayAppointments(List<Appointment> appointments)
        {
            foreach (var appointment in appointments)
            {
                // Convertir startTime a DateTime
                DateOnly date = DateOnly.FromDateTime(DateTime.Today); // Usa la fecha de hoy, o la fecha que necesites
                DateTime startTime = date.ToDateTime(appointment.StartTime);
                DateTime endTime = date.ToDateTime(appointment.EndTime);


                // Encontrar la fila correcta en el Grid basado en startTime
                int row = (startTime.Hour - 9) * 2 + (startTime.Minute / 30);

                // Crear Label para la cita
                Label appointmentLabel = new Label
                {
                    Text = appointment.Title,
                    BackgroundColor = Colors.Aquamarine,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    HeightRequest = (endTime - startTime).TotalMinutes
                };

                // Añadir la cita al Grid
                Grid.SetRow(appointmentLabel, row);
                Grid.SetColumn(appointmentLabel, 1);
                ScheduleGrid.Children.Add(appointmentLabel);
            }
        }
    }
}
