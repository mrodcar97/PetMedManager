using CommunityToolkit.Maui.Views;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using PetMedManager.Views;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PetMedManager.Pages;

public partial class MainAdminMenu : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    public static MainAdminMenu Instance { get; private set; }

    public MainAdminMenu(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        Instance = this; // Asignar la instancia actual a la propiedad estática
        ShowCalendarMainViewNoButton();
    }

    public void LoadContent(ContentView view) // Cambiado a public
    {
        MainContentArea.Content = view;
    }

    public void ShowMenu(object sender, EventArgs e)
    {
        var dailyScheduleView = _serviceProvider.GetRequiredService<DailyScheduleView>();
        LoadContent(dailyScheduleView);
    }

    public void ShowMainClientView(object sender, EventArgs e)
    {
        var mainClientView = _serviceProvider.GetRequiredService<MainClientView>();
        LoadContent(mainClientView);
    }

    public void ShowClientRegisterView()
    {
        var RegisterClientView = _serviceProvider.GetRequiredService<ClientRegisterView>();
        LoadContent(RegisterClientView);
    }
    
    public void ShowClientModifyView(Person p)
    {
        var registerClientView = new ClientRegisterView(_serviceProvider.GetRequiredService<IHttpClientFactory>(), p);
        LoadContent(registerClientView);
    }

    public void ShowCalendarMainView(object sender, EventArgs e)
    {
        var CalendarMainView = _serviceProvider.GetRequiredService<Views.AppointmentMainView>();
        LoadContent(CalendarMainView);
    }

    public void ShowCalendarMainViewNoButton()
    {
        var CalendarMainView = _serviceProvider.GetRequiredService<AppointmentMainView>();
        LoadContent(CalendarMainView);
    }

    public void ShowCreateAppointmentWithVetID(string vetid, DateTime date,string time)
    {
        var appointmentCreateView = new AppointmentCreateView(_serviceProvider.GetRequiredService<IHttpClientFactory>(),date,vetid,time);
        LoadContent(appointmentCreateView);
    }

    public void ShowShiftMainView(object sender, EventArgs e)
    {
        var ShiftMainView = _serviceProvider.GetRequiredService<ShiftMainView>();
        LoadContent(ShiftMainView);
    }
    public async void ShowLoginPage(object sender, EventArgs e)
    {
        var mainMenu = _serviceProvider.GetRequiredService<LoginPage>();
        await Navigation.PushAsync(mainMenu);
    }

}
