using Syncfusion.Maui.Core.Hosting;
using Microsoft.Extensions.Logging;
using PetMedManager.Views;
using PetMedManager.Pages;
using CommunityToolkit.Maui;

namespace PetMedManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            // Initialize the .NET MAUI Community Toolkit by adding the below line of code
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddHttpClient("DefaultClient", client =>
            {
                client.BaseAddress = new Uri("https://petmedmanagerapi.azurewebsites.net/");
            });

            builder.Services.AddTransient<HistoryPetView>();
            builder.Services.AddTransient<PetRegisterView>();
            builder.Services.AddTransient<MainPetView>();
            builder.Services.AddTransient<HomeVetView>();
            builder.Services.AddTransient<ShiftMainView>();
            builder.Services.AddTransient<AppointmentCreateView>();
            builder.Services.AddTransient<AppointmentMainView>();
            builder.Services.AddTransient<ClientRegisterView>();
            builder.Services.AddTransient<DailyScheduleView>();
            builder.Services.AddTransient<MainClientView>();
            builder.Services.AddTransient<MainVetMenu>();
            builder.Services.AddTransient<MainAdminMenu>();
            builder.Services.AddTransient<LoginPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
