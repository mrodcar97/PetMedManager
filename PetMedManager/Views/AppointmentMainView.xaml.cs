using Domain;
using Newtonsoft.Json;
using PetMedManager.Pages;

namespace PetMedManager.Views
{
    public partial class AppointmentMainView : ContentView
    {
        private DateTime _currentDate;
        private readonly HttpClient _httpClient;
        private Label MonthYearLabel;


        public AppointmentMainView(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClient = httpClientFactory.CreateClient("DefaultClient");
            _currentDate = DateTime.Now;
            BuildCalendarUI();
            BuildCalendar();
            
        }

        private void BuildCalendarUI()
        {
            var previousMonthButton = new Button { Text = "<", HeightRequest = 20, FontSize = 24, Margin = 10, BorderWidth = 1, BackgroundColor = Colors.Transparent, TextColor = Color.FromHex("#4682B4"), BorderColor = Color.FromHex("#4682B4") };
            previousMonthButton.Clicked += PreviousMonthButton_Clicked;

            var nextMonthButton = new Button { Text = ">", FontSize = 24, HeightRequest = 20, Margin = 10, BorderWidth = 1, BackgroundColor = Colors.Transparent,TextColor = Color.FromHex("#4682B4"), BorderColor = Color.FromHex("#4682B4") };
            nextMonthButton.Clicked += NextMonthButton_Clicked;

            MonthYearLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black
            };

            var headerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
            };

            headerGrid.Children.Add(previousMonthButton);
            Grid.SetColumn(previousMonthButton, 0);

            headerGrid.Children.Add(MonthYearLabel);
            Grid.SetColumn(MonthYearLabel, 1);

            headerGrid.Children.Add(nextMonthButton);
            Grid.SetColumn(nextMonthButton, 2);

            var mainStackLayout = new StackLayout();

            mainStackLayout.Children.Add(HeaderLabel);
            mainStackLayout.Children.Add(headerGrid);
            mainStackLayout.Children.Add(CalendarGrid);
            mainStackLayout.Children.Add(VeterinarianStack);

            Content = mainStackLayout;
        }

        private void PreviousMonthButton_Clicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddMonths(-1);
            BuildCalendar();
        }

        private void NextMonthButton_Clicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddMonths(1);
            BuildCalendar();
        }

        private void BuildCalendar()
        {
            CalendarGrid.Children.Clear();

            MonthYearLabel.Text = _currentDate.ToString("MMMM yyyy").ToUpper();
            MonthYearLabel.TextColor = Colors.Black;

            var dayNames = new[] { "Lun", "Mar", "Mie", "Jue", "Vie", "Sab", "Dom" };
            for (int i = 0; i < 7; i++)
            {
                var label = new Label
                {
                    Text = dayNames[i],
                    TextColor = Colors.Black,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontAttributes = FontAttributes.Bold
                };
                Grid.SetColumn(label, i);
                Grid.SetRow(label, 0);
                CalendarGrid.Children.Add(label);
            }

            DateTime firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);
            int startDayOfWeek = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

            int row = 1;
            int col = startDayOfWeek;

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(_currentDate.Year, _currentDate.Month, day);
                var button = new Button
                {
                    Text = day.ToString(),
                    IsEnabled = date >= DateTime.Today,
                    BackgroundColor = date < DateTime.Today ? Colors.LightGray : Colors.LightGreen
                };

                button.Clicked += async (sender, args) => await OnDateClicked(date);

                Grid.SetColumn(button, col);
                Grid.SetRow(button, row);
                CalendarGrid.Children.Add(button);

                col++;
                if (col == 7)
                {
                    col = 0;
                    row++;
                }
            }
        }

        private async Task OnDateClicked(DateTime date)
        {
            List<Appointment> appointmentsForTheDay;
            List<Shift> shiftsForTheDay;

            SelectedDateLabel.Text = $"Huecos libres para {date:dd MMM yyyy}";

            var appointmentsResponse = await _httpClient.GetAsync($"api/Appointment/for{date:yyyy-MM-dd}");
            var shiftsResponse = await _httpClient.GetAsync($"api/Shift/for{date:yyyy-MM-dd}");
            var vetResponse = await _httpClient.GetAsync($"api/Person");

            if (appointmentsResponse.IsSuccessStatusCode)
            {
                var appointmentsContent = await appointmentsResponse.Content.ReadAsStringAsync();
                appointmentsForTheDay = JsonConvert.DeserializeObject<List<Appointment>>(appointmentsContent);
            }
            else
            {
                appointmentsForTheDay = new List<Appointment>();
            }

            if (shiftsResponse.IsSuccessStatusCode)
            {
                var shiftsContent = await shiftsResponse.Content.ReadAsStringAsync();
                shiftsForTheDay = JsonConvert.DeserializeObject<List<Shift>>(shiftsContent);
            }
            else
            {
                shiftsForTheDay = new List<Shift>();
            }

            var vetContent = await vetResponse.Content.ReadAsStringAsync();
            List<Person> vetsForName = JsonConvert.DeserializeObject<List<Person>>(vetContent);

            VeterinarianStack.Children.Clear();

            GetAvailableSlotsAndUpdateUI(shiftsForTheDay, appointmentsForTheDay, vetsForName, date);
        }

        private void GetAvailableSlotsAndUpdateUI(List<Shift> shifts, List<Appointment> appointments, List<Person> vetsForName, DateTime date)
        {
            if (shifts == null || shifts.Count == 0)
            {
                DisplayGeneralSlots(appointments, date);
                return;
            }

            foreach (var shift in shifts)
            {
                var id = shift.VeterinarianId.ToString();
                string veterinarianName = vetsForName.Find(p => p.NationalId == id).Name;
                var allSlots = new List<string>();

                for (int hour = shift.StartTime.Hour; hour < shift.EndTime.Hour; hour++)
                {
                    allSlots.Add($"{hour:00}:00");
                }

                foreach (var appointment in appointments)
                {
                    if (appointment.VetId == shift.VeterinarianId)
                    {
                        var appointmentHour = appointment.StartTime;
                        allSlots.Remove($"{appointmentHour:HH:mm}");
                    }
                }

                if (allSlots.Count == 0)
                {
                    for (int hour = shift.StartTime.Hour; hour < shift.EndTime.Hour; hour++)
                    {
                        allSlots.Add($"{hour:00}:00");
                    }
                }

                var stackLayout = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(10, 10),
                };

                var nameLabel = new Label
                {
                    Text = veterinarianName,
                    TextColor = Colors.Black,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                };
                stackLayout.Children.Add(nameLabel);

                foreach (var slot in allSlots)
                {
                    var slotButton = new Button
                    {
                        Text = slot,
                        BorderWidth = 1,
                        BorderColor = Color.FromHex("#4682B4"),
                        TextColor = Color.FromHex("#4682B4"),
                        BackgroundColor = Colors.Transparent,
                        Margin = new Thickness(5)
                    };

                    slotButton.Clicked += (sender, args) => OnSlotClicked(sender, slotButton.Text, shift.VeterinarianId, date);

                    stackLayout.Children.Add(slotButton);
                }

                VeterinarianStack.Children.Add(stackLayout);
            }
        }
        private void DisplayGeneralSlots(List<Appointment> appointments, DateTime date)
        {
            var generalSlots = new List<string>();

            for (int hour = 8; hour < 22; hour++)
            {
                generalSlots.Add($"{hour:00}:00");
            }

            foreach (var appointment in appointments)
            {
                var appointmentHour = appointment.StartTime;
                generalSlots.Remove($"{appointmentHour:HH:mm}");
            }

            var stackLayout = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(10, 0, 10, 40),
                Orientation = StackOrientation.Vertical // Cambiamos a orientación vertical
            };

            var nameLabel = new Label
            {
                Text = "General",
                FontSize = 16,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            };
            stackLayout.Children.Add(nameLabel);

            var slotsStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center
            };

            foreach (var slot in generalSlots)
            {
                var slotButton = new Button
                {
                    Text = slot,
                    BorderWidth = 1,
                    BorderColor = Color.FromHex("#4682B4"),
                    TextColor = Color.FromHex("#4682B4"),
                    BackgroundColor = Colors.Transparent,
                    Margin = new Thickness(5)
                };

                slotButton.Clicked += (sender, args) => OnSlotClicked(sender, slotButton.Text, "General", date);

                slotsStack.Children.Add(slotButton);
            }

            stackLayout.Children.Add(slotsStack);
            VeterinarianStack.Children.Add(stackLayout);
        }

        private void OnSlotClicked(object sender, string time, string veterinarianId, DateTime date)
        {
            MainAdminMenu.Instance.ShowCreateAppointmentWithVetID(veterinarianId, date, time);
        }
    }
}
