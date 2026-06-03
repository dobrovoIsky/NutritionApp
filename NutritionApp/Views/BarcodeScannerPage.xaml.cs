using ZXing.Net.Maui;
using System.Diagnostics;

namespace NutritionApp.Views
{
    public partial class BarcodeScannerPage : ContentPage
    {
        public delegate void BarcodeScannedEventHandler(string barcode);
        public event BarcodeScannedEventHandler OnBarcodeScanned;

        private bool _isScanned = false;

        public BarcodeScannerPage()
        {
            InitializeComponent();

            barcodeReader.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.OneDimensional,
                AutoRotate = true,
                Multiple = false
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _isScanned = false;
            barcodeReader.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            barcodeReader.IsDetecting = false;
        }

        private void CameraBarcodeReaderView_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            if (_isScanned) return;

            var first = e.Results?.FirstOrDefault();
            if (first != null)
            {
                _isScanned = true;
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    barcodeReader.IsDetecting = false;
                    Debug.WriteLine($"Barcode scanned: {first.Value}");
                    OnBarcodeScanned?.Invoke(first.Value);
                    await Navigation.PopModalAsync();
                });
            }
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            barcodeReader.IsDetecting = false;
            await Navigation.PopModalAsync();
        }
    }
}
