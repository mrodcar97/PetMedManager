using System.Net.Http.Json;
using Domain;


namespace PetMedManager.Views
{
    public partial class DailyScheduleView : ContentView
    {
        private readonly HttpClient _httpClient;

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
            var response = await _httpClient.GetAsync($"api/Appointment/for{"2024-06-12"}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Appointment>>();
        }

        private void DisplayAppointments(List<Appointment> appointments)
        {
            // Clear existing children and row/column definitions
            ScheduleGrid.Children.Clear();
            ScheduleGrid.RowDefinitions.Clear();
            ScheduleGrid.ColumnDefinitions.Clear();

            // Calculate the width of each column based on the total width of the grid (1500) divided by the number of columns
            int numColumns = 30; // Example: from 8 AM to 6 PM
            double columnWidth = 1500 / numColumns;

            // Add row definition for time headers
            ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Add column definitions for time slots
            for (int i = 0; i < numColumns; i++)
            {
                ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(columnWidth, GridUnitType.Absolute) });
            }

            // Add time labels above each column
            for (int i = 0; i < numColumns; i++)
            {
                var timeLabel = new Label
                {
                    Text = $"{8 + (i / 2)}:{(i % 2 == 0 ? "00" : "30")}",
                    FontSize = 12,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                    Margin = new Thickness(0, 0, 0, 5),
                    WidthRequest = columnWidth
                };
                Grid.SetRow(timeLabel, 0);
                Grid.SetColumn(timeLabel, i + 1);
                ScheduleGrid.Children.Add(timeLabel);
            }

            // Group appointments by veterinarian
            var groupedAppointments = appointments.GroupBy(a => a);

            int rowIndex = 1;
            foreach (var group in groupedAppointments)
            {
                // Add row definition for each veterinarian's appointments
                ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Add veterinarian name to the first column
                var vetNameLabel = new Label
                {
                    Text = "Juan", // Change this to the veterinarian's name
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Start,
                    Margin = new Thickness(5, 0, 0, 0),
                };
                Grid.SetRow(vetNameLabel, rowIndex);
                Grid.SetColumn(vetNameLabel, 0);
                ScheduleGrid.Children.Add(vetNameLabel);

                foreach (var appointment in group)
                {
                    // Calculate column based on appointment time
                    int startTimeIndex = (appointment.StartTime.Hour - 8) * 2 + (appointment.StartTime.Minute / 30);
                    int endTimeIndex = (appointment.StartTime.Hour - 8) * 2 + ((appointment.StartTime.Minute + 30) / 30);

                    // Create label for appointment
                    var appointmentLabel = new Label
                    {
                        Text = appointment.Title,
                        BackgroundColor = Colors.Aquamarine,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Margin = 1 // Small margin for spacing
                    };

                    // Set Grid row and column
                    Grid.SetRow(appointmentLabel, rowIndex);
                    Grid.SetColumnSpan(appointmentLabel, endTimeIndex - startTimeIndex);
                    Grid.SetColumn(appointmentLabel, startTimeIndex + 1);

                    // Add label to Grid
                    ScheduleGrid.Children.Add(appointmentLabel);
                }

                rowIndex++;
            }

            addLineGrid(numColumns, rowIndex);
        }

        private void addLineGrid(int numColumns, int rowIndex)
        {
            // Add grid lines
            for (int col = 1; col < numColumns + 1; col++)
            {
                // Create a Frame to act as the border
                var frame = new Frame
                {
                    BorderColor = Colors.White,
                    CornerRadius = 0, // Optional: Set to 0 if you don't want rounded corners
                    HasShadow = false, // Optional: Set to false to remove the shadow
                    Padding = 10, // Optional: Set padding if needed
                    BackgroundColor = Colors.Transparent // Optional: Set background color if needed
                };

                // Set Grid row and column for the Frame
                Grid.SetRow(frame, 1);
                Grid.SetRowSpan(frame, rowIndex);
                Grid.SetColumn(frame, col);

                // Add the Frame to the Grid
                ScheduleGrid.Children.Add(frame);
            }
        }
    }
}

