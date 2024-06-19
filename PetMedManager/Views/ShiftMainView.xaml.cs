using Domain;
using PetMedManager.Pages;
using Newtonsoft.Json;
using XCalendar.Core.Models;
using Microsoft.Maui.Controls;
using System.Text;

namespace PetMedManager.Views;

public partial class ShiftMainView : ContentView
{
    private DateTime _currentDate;
    private readonly HttpClient _httpClient;
    private Label MonthYearLabel;
    private AuthService Auth = new AuthService();

    public ShiftMainView(IHttpClientFactory httpClientFactory)
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

        var nextMonthButton = new Button { Text = ">", FontSize = 24, HeightRequest = 20, Margin = 10, BorderWidth = 1, BackgroundColor = Colors.Transparent, TextColor = Color.FromHex("#4682B4"), BorderColor = Color.FromHex("#4682B4") };
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

    private async void BuildCalendar()
    {
        CalendarGrid.Children.Clear();

        MonthYearLabel.Text = _currentDate.ToString("MMMM yyyy").ToUpper();

        var dayNames = new[] { "Lun", "Mar", "Mie", "Jue", "Vie", "Sab", "Dom" };
        for (int i = 0; i < 7; i++)
        {
            var label = new Label
            {
                TextColor = Colors.Black,
                Text = dayNames[i],
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

        // Obtener turnos para todo el mes
        var shiftsResponse = await _httpClient.GetAsync($"api/Shift/forMonth/{_currentDate:yyyy/MM}");
        List<Shift> shiftsForMonth = new List<Shift>();
        if (shiftsResponse.IsSuccessStatusCode)
        {
            var shiftsContent = await shiftsResponse.Content.ReadAsStringAsync();
            shiftsForMonth = JsonConvert.DeserializeObject<List<Shift>>(shiftsContent);
        }

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_currentDate.Year, _currentDate.Month, day);
            var button = new Button
            {
                Text = day.ToString(),
                IsEnabled = date >= DateTime.Today,
                BackgroundColor = date < DateTime.Today ? Colors.LightGray : GetButtonColorForDate(date, shiftsForMonth)
            };

            Grid.SetColumn(button, col);
            Grid.SetRow(button, row);
            CalendarGrid.Children.Add(button);

            button.Clicked += async (sender, args) => await OnDateClicked(date);

            col++;
            if (col == 7)
            {
                col = 0;
                row++;
            }
        }
    }

    private Color GetButtonColorForDate(DateTime date, List<Shift> shiftsForMonth)
    {
        string formatedDate = date.ToString("yyyy-MM-dd");
        DateOnly shiftDate = DateOnly.Parse(formatedDate);

        var shiftsForTheDay = shiftsForMonth.Where(s => s.Date == shiftDate).ToList();

        var morningShift = shiftsForTheDay.FirstOrDefault(s => s.StartTime == new TimeOnly(08, 00, 00));
        var afternoonShift = shiftsForTheDay.FirstOrDefault(s => s.StartTime == new TimeOnly(15, 00, 00));

        if (morningShift != null && afternoonShift != null)
        {
            // Turno de mañana y tarde asignados
            return Colors.LightGreen;
        }
        else if (morningShift != null || afternoonShift != null)
        {
            // Solo uno de los turnos asignado
            return Color.FromHex("#eb9763");
        }
        else
        {
            
            return Colors.LightCoral;
        }
    }

    private async Task OnDateClicked(DateTime date)
    {
        List<Shift> shiftsForTheDay;
        List<Person> vetsFoClinic;

        SelectedDateLabel.Text = $"Turnos para {date:dd MMM yyyy}";

        var clinicID = await Auth.GetSerialNumberClaimAsync();
        var shiftsResponse = await _httpClient.GetAsync($"api/Shift/for{date:yyyy-MM-dd}");
        var vetsResponse = await _httpClient.GetAsync($"api/Person/ByClinic{clinicID}");

        if (shiftsResponse.IsSuccessStatusCode)
        {
            var shiftsContent = await shiftsResponse.Content.ReadAsStringAsync();
            shiftsForTheDay = JsonConvert.DeserializeObject<List<Shift>>(shiftsContent);
        }
        else
        {
            shiftsForTheDay = new List<Shift>();
        }

        if (vetsResponse.IsSuccessStatusCode)
        {
            var vetContent = await vetsResponse.Content.ReadAsStringAsync();
            vetsFoClinic = JsonConvert.DeserializeObject<List<Person>>(vetContent);
        }
        else
        {
            vetsFoClinic = new List<Person>();
        }

        VeterinarianStack.Children.Clear();

        var stackLayoutShiftAdd = new StackLayout { HorizontalOptions = LayoutOptions.Center, Margin = 10};
        var stackLayoutShiftInfo = new StackLayout { HorizontalOptions = LayoutOptions.Center, Margin = 10 };

        var cbVets = new Picker
        {
            BackgroundColor = Color.FromHex("#bcd0eb"),
            TextColor = Colors.Black,
            WidthRequest = 300
        };
        var shiftPicker = new Picker
        {
            BackgroundColor = Color.FromHex("#bcd0eb"),
            TextColor = Colors.Black,
            WidthRequest = 300,
            ItemsSource = new List<string> { "Mañana", "Tarde" },
            Margin = 30
        };

        foreach (var shift in shiftsForTheDay)
        {
            var vet = vetsFoClinic.FirstOrDefault(v => v.NationalId == shift.VeterinarianId);

            if (vet != null)
            {
                var timeOfDay = shift.StartTime == new TimeOnly(8, 0, 0) ? "Mañana" : "Tarde";
                var vetLabel = new Label { Text = $"{vet.Name} {vet.Surname} - {timeOfDay}", TextColor = Colors.Black, FontSize = 16 };
                stackLayoutShiftInfo.Children.Add(vetLabel);
            }
        }

        var vetsNames = new List<string>();

        foreach (Person p in vetsFoClinic)
        {
            vetsNames.Add(p.Name + " " + p.Surname);
        }
        cbVets.ItemsSource = vetsNames;
        stackLayoutShiftAdd.Children.Add(cbVets);
        stackLayoutShiftAdd.Children.Add(shiftPicker);

        cbVets.SelectedIndexChanged += (sender, args) =>
        {
            if (cbVets.SelectedIndex == -1) return;

            var selectedVetName = cbVets.SelectedItem.ToString();
            var selectedVet = vetsFoClinic.FirstOrDefault(v => $"{v.Name} {v.Surname}" == selectedVetName);
            if (selectedVet == null) return;

            var vetShifts = shiftsForTheDay.Where(s => s.VeterinarianId == selectedVet.NationalId).ToList();
            var availableShifts = new List<string> { "Mañana", "Tarde" };

            if (vetShifts.Any(s => s.StartTime == new TimeOnly(08, 00, 00)))
            {
                availableShifts.Remove("Mañana");
            }
            if (vetShifts.Any(s => s.StartTime == new TimeOnly(15, 00, 00)))
            {
                availableShifts.Remove("Tarde");
            }

            shiftPicker.ItemsSource = availableShifts;

            shiftPicker.IsEnabled = availableShifts.Count > 0;
        };

        var addShiftButton = new Button
        {
            Text = "Aceptar",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TextColor = Color.FromHex("#4682B4"),
            BorderColor = Color.FromHex("#4682B4"),
            BackgroundColor = Colors.Transparent,
            BorderWidth = 2
        };

        addShiftButton.Clicked += async (sender, args) =>
        {
            if (cbVets.SelectedIndex == -1 || shiftPicker.SelectedIndex == -1) return;

            var selectedVetName = cbVets.SelectedItem.ToString();
            var selectedVet = vetsFoClinic.FirstOrDefault(v => $"{v.Name} {v.Surname}" == selectedVetName);
            if (selectedVet == null) return;

            var selectedShift = shiftPicker.SelectedItem.ToString();
            var shiftStartTime = selectedShift == "Mañana" ? new TimeOnly(8, 0, 0) : new TimeOnly(15, 0, 0);
            var shiftEndTime = selectedShift == "Mañana" ? new TimeOnly(15, 0, 0) : new TimeOnly(22, 0, 0);

            string formatedDate = date.ToString("yyyy-MM-dd");
            DateOnly shiftDate = DateOnly.Parse(formatedDate);

            var newShift = new Shift
            {
                VeterinarianId = selectedVet.NationalId,
                StartTime = shiftStartTime,
                EndTime = shiftEndTime,
                Date = shiftDate
            };

            var content = new StringContent(JsonConvert.SerializeObject(newShift), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Shift", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Turno añadido correctamente", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo añadir el turno", "OK");
            }
        };

        stackLayoutShiftAdd.Children.Add(addShiftButton);

        var addShiftTitle = new Label { Text = "Añadir turno", FontSize = 20, HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black };
        var frameAddShift = new Frame { Content = stackLayoutShiftAdd, BorderColor = Colors.White, Padding = 10, BackgroundColor = Colors.Transparent };
        var assignedShiftTitle = new Label { Text = "Turnos asignados", FontSize = 20, HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black };
        var frameAssignedShift = new Frame { Content = stackLayoutShiftInfo, BorderColor = Colors.White, Padding = 10, BackgroundColor = Colors.Transparent };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 500 });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 500 });

        var addShiftLayout = new StackLayout { Children = { addShiftTitle, frameAddShift } };
        var assignedShiftLayout = new StackLayout { Children = { assignedShiftTitle, frameAssignedShift } };

        grid.Children.Add(addShiftLayout);
        Grid.SetColumn(addShiftLayout, 0);

        grid.Children.Add(assignedShiftLayout);
        Grid.SetColumn(assignedShiftLayout, 1);

        VeterinarianStack.Children.Add(grid);
    }




    private void ChangeButtonColor(DateTime date, Color color)
    {
        foreach (var child in CalendarGrid.Children)
        {
            if (child is Button button && DateTime.TryParse(button.Text, out var buttonDate))
            {
                if (buttonDate.Day == date.Day && buttonDate.Month == date.Month && buttonDate.Year == date.Year)
                {
                    button.BackgroundColor = color;
                    break;
                }
            }
        }
    }
}
