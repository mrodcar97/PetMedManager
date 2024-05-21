using Domain;
using Microsoft.Extensions.DependencyInjection;
using PetMedManager.Views;

namespace PetMedManager.Pages;

public partial class MainMenu : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public MainMenu(IServiceProvider serviceProvider)
    {
		InitializeComponent();
        _serviceProvider = serviceProvider;
        mostrarTokenAsync();
    }
	 public async Task mostrarTokenAsync()
	{
        string? token = await SecureStorage.GetAsync("accessToken");
    }

    private void LoadContent(ContentView view)
    {
        MainContentArea.Content = view;
    }

    public void ShowMenu(object sender, EventArgs e)
	{
        var dailyScheduleView = _serviceProvider.GetRequiredService<DailyScheduleView>();
        LoadContent(dailyScheduleView);
    }
}