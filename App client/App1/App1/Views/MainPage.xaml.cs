using Xamarin.Forms;

namespace App1.Views
{
    public partial class MainPage : ContentPage
    {
        public string str = "";
        public MainPage()
        {
            InitializeComponent();
            zxing.OnScanResult += (result) => Device.BeginInvokeOnMainThread(() =>
            {
                str = result.Text;
                Navigation.PopModalAsync();
            });
            
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            zxing.IsScanning = true;
        }
        protected override void OnDisappearing()
        {
            zxing.IsScanning = false;

            base.OnDisappearing();
        }
    }
}
