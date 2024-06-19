using Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PetMedManager.Views;

public partial class HomeVetView : ContentView
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;
    private List<Shift> monthShifts;
    private DateTime currentDate;

    public HomeVetView(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
    {
        InitializeComponent();
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
        _serviceProvider = serviceProvider;
        currentDate = DateTime.Now;
        LoadVetScheduleAsync(currentDate);
    }

    private async Task LoadVetScheduleAsync(DateTime date)
    {
        var authService = new AuthService();
        var vetId = await authService.GetDNIClaimAsync();

        var response = await _httpClient.GetAsync($"api/Shift/forMonth/{date.Year}/{date.Month}");
        var content = await response.Content.ReadAsStringAsync();
        monthShifts = JsonConvert.DeserializeObject<List<Shift>>(content);

        string formatDate = date.ToString("yyyy-MM-dd");
        DateOnly onlyDate = DateOnly.Parse(formatDate);

        var todayShift = monthShifts?.FirstOrDefault(shift => shift.Date == onlyDate && shift.VeterinarianId == vetId);

        DisplayDateNavigation(date);

        if (todayShift != null)
        {
            var appointments = await FetchAppointmentsForToday(vetId, date);
            DisplaySchedule(todayShift, appointments);
        }
        else
        {
            DisplayDayOff();
        }
    }

    private async Task<List<Appointment>> FetchAppointmentsForToday(string vetId, DateTime date)
    {
        var response = await _httpClient.GetAsync($"api/Appointment/for{date:yyyy-MM-dd}");
        var content = await response.Content.ReadAsStringAsync();

        JToken token = JToken.Parse(content);
        if (token is JArray)
        {
            var todayAppointments = JsonConvert.DeserializeObject<List<Appointment>>(content);
            var appointmentsToVet = todayAppointments?.FindAll(a => a.VetId == vetId);
            return appointmentsToVet;
        }
        else
        {
            Console.WriteLine("Error: Expected a JSON array");
            return new List<Appointment>();
        }
    }

    private void DisplayDateNavigation(DateTime date)
    {
        MainStackLayout.Children.Clear();

        // Agregamos la etiqueta de la fecha
        MainStackLayout.Children.Add(new Label
        {
            Text = "Tus citas para " + date.ToString("dd-MM-yyyy"),
            FontSize = 24,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.Black,
            Margin = new(0, 20, 0, 10)
        });

        // Agregamos botones de navegación en un Grid
        var navigationLayout = new Grid
        {
            Margin = new(0, 0, 0, 20),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            }
        };

        var previousDayButton = new Button {
            Text = "<",
            TextColor = Color.FromHex("#4682B4"),
            BorderColor = Color.FromHex("#4682B4"),
            BorderWidth = 2,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Start
        };

        previousDayButton.Clicked += (s, e) => ChangeDay(-1);

        var nextDayButton = new Button
        {   Text = ">",
            TextColor = Color.FromHex("#4682B4"),
            BorderColor = Color.FromHex("#4682B4"),
            BorderWidth = 2,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.End
        };
        nextDayButton.Clicked += (s, e) => ChangeDay(1);

        // Agregar botones al Grid usando Children.Add y luego SetColumn
        navigationLayout.Children.Add(previousDayButton);
        Grid.SetColumn(previousDayButton, 0);

        navigationLayout.Children.Add(nextDayButton);
        Grid.SetColumn(nextDayButton, 2);

        MainStackLayout.Children.Add(navigationLayout);
    }

    private void DisplayDayOff()
    {
        MainStackLayout.Children.Add(new Label
        {
            Text = "Día libre",
            FontSize = 24,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.Black
        });
    }

    private void DisplaySchedule(Shift shift, List<Appointment> appointments)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 20,  // Reduce row spacing
            ColumnSpacing = 5 // Reduce column spacing
        };

        var appointmentDict = appointments.ToDictionary(a => a.StartTime.Hour);

        int column = 0;
        int row = 0;

        for (int hour = shift.StartTime.Hour; hour < shift.EndTime.Hour; hour++)
        {
            Appointment appointment;
            bool hasAppointment = appointmentDict.TryGetValue(hour, out appointment);

            var frame = new Frame
            {
                BorderColor = Color.FromHex("#4682B4"),
                BackgroundColor = Colors.Transparent,
                Padding = 10,
                Content = new StackLayout
                {
                    Children =
                    {
                        new Label { HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromHex("#4682B4"), Text = $"Hora:\n {hour:00}:00", FontSize = 16 },
                        new Label { HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromHex("#4682B4"), Text = $"Título:\n {(hasAppointment ? appointment.Title : "Cita no asignada")}", FontSize = 16 },
                        new Label { HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromHex("#4682B4"), Text = $"Descripción:\n {(hasAppointment ? appointment.Description : "Cita no asignada")}", FontSize = 16 }
                    }
                },
                WidthRequest = 300
            };

            grid.Children.Add(frame);
            Grid.SetColumn(frame, column);
            Grid.SetRow(frame, row);

            column++;
            if (column == 4)
            {
                column = 0;
                row++;
            }
        }

        MainStackLayout.Children.Add(grid);
    }

    private async void ChangeDay(int days)
    {
        DateTime previousDate = currentDate;
        currentDate = currentDate.AddDays(days);

        if (currentDate.Month != previousDate.Month)
        {
            await LoadVetScheduleAsync(currentDate);
        }
        else
        {
            DisplayDateNavigation(currentDate);
            var authService = new AuthService();
            var vetId = await authService.GetDNIClaimAsync();
            string formatDate = currentDate.ToString("yyyy-MM-dd");
            DateOnly onlyDate = DateOnly.Parse(formatDate);
            var todayShift = monthShifts?.FirstOrDefault(shift => shift.Date == onlyDate && shift.VeterinarianId == vetId);

            if (todayShift != null)
            {
                var appointments = await FetchAppointmentsForToday(vetId, currentDate);
                DisplaySchedule(todayShift, appointments);
            }
            else
            {
                DisplayDayOff();
            }
        }
    }

    private async void OpenAppointmentDetails(Appointment appointment)
    {
        //await Navigation.PushAsync(new PetHistoryPage(appointment.PetId));
    }
}
