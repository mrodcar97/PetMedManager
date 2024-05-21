using PetMedManager.Pages;

namespace PetMedManager
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            MainPage = new NavigationPage(_serviceProvider.GetRequiredService<LoginPage>());
        }
    }
}
