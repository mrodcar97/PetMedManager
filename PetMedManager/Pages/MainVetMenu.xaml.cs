using PetMedManager.Views;
using Domain;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Maui.Graphics.Text;
using Newtonsoft.Json.Linq;

namespace PetMedManager.Pages
{
    public partial class MainVetMenu : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public static MainVetMenu Instance { get; private set; }

        public MainVetMenu(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            Instance = this;
            ShowAppointments();

        }

        public void LoadContent(ContentView view)
        {
            MainContentArea.Content = view;
        }

        public void ShowAppointments()
        {
            var homeVetView = _serviceProvider.GetRequiredService<HomeVetView>();
            LoadContent(homeVetView);
        }

        public void ShowAppointmentsButton(object sender, EventArgs e)
        {
            var homeVetView = _serviceProvider.GetRequiredService<HomeVetView>();
            LoadContent(homeVetView);
        }

        public void ShowPetMainView(object sender, EventArgs e)
        {
            var PetMainView = _serviceProvider.GetRequiredService<MainPetView>();
            LoadContent(PetMainView);
        }

        public void ShowPetRegisterView()
        {
            var PetMainView = _serviceProvider.GetRequiredService<PetRegisterView>();
            LoadContent(PetMainView);
        }

        public void ShowPetModifyView(Pet p)
        {
            var registerClientView = new PetRegisterView(_serviceProvider.GetRequiredService<IHttpClientFactory>(), p);
            LoadContent(registerClientView);
        }

        public void ShowHistoryMainView(object sender, EventArgs e)
        {
            var PetMainView = _serviceProvider.GetRequiredService<HistoryPetView>();
            LoadContent(PetMainView);
        }
        public async void ShowLoginPage(object sender, EventArgs e)
        {
            var mainMenu = _serviceProvider.GetRequiredService<LoginPage>();
            await Navigation.PushAsync(mainMenu);
        }

    }
}
