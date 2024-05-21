using Microsoft.Extensions.Logging;
using PetMedManager.Views;
using PetMedManager.Pages;

namespace PetMedManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddHttpClient("DefaultClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5184");
            });
            builder.Services.AddTransient<DailyScheduleView>();
            builder.Services.AddTransient<MainMenu>();
            builder.Services.AddTransient<LoginPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
