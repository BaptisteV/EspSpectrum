using EspSpectrum.MAUI.Models;
using EspSpectrum.MAUI.PageModels;

namespace EspSpectrum.MAUI.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}