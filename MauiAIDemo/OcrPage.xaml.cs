#if WINDOWS
using Microsoft.Graphics.Imaging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Imaging;
using Microsoft.Windows.Management.Deployment;

using System.Text;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

namespace MauiAIDemo
{
    public partial class OcrPage : ContentPage
    {
        private string? selectedImagePath;

        public OcrPage()
        {
            InitializeComponent();
        }

        private async void OnImageAreaTapped(object sender, EventArgs e)
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select an image for OCR"
                });

                if (result != null)
                {
                    selectedImagePath = result.FullPath;
                    
                    // Show the selected image
                    SelectedImage.Source = ImageSource.FromFile(result.FullPath);
                    SelectedImage.IsVisible = true;
                    PlaceholderLabel.IsVisible = false;
                    
                    // Enable the extract button
                    CounterBtn.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to select image: {ex.Message}", "OK");
            }
        }

#if WINDOWS

    public async Task<ImageBuffer?> LoadImageBufferFromFileAsync(string filePath)
{
    StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
    SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync();

    if (bitmap == null)
    {
        return null;
    }

    // Create ImageBuffer from SoftwareBitmap
    ImageBuffer imageBuffer = ImageBuffer.CreateForSoftwareBitmap(bitmap);
    
    return imageBuffer;
}
#endif

        private async void OnCounterClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                await DisplayAlert("No Image", "Please select an image first.", "OK");
                return;
            }

            // Clear previous response and disable button
            ResponseEditor.Text = "Processing image...";
            CounterBtn.IsEnabled = false;
            CounterBtn.Text = "Processing...";

#if WINDOWS
            // OCR implementation will be added here
            if (TextRecognizer.GetReadyState() == AIFeatureReadyState.NotReady)
            {
                var loadResult = await TextRecognizer.EnsureReadyAsync();
                if (loadResult.Status != AIFeatureReadyResultState.Success)
                {
                    throw new Exception(loadResult.ExtendedError.Message);
                }
            }

            var textRecognition= await TextRecognizer.CreateAsync();

            ImageBuffer imageBuffer = await LoadImageBufferFromFileAsync(selectedImagePath);
            RecognizedText recognizedText = textRecognition.RecognizeTextFromImage(imageBuffer);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var line in recognizedText.Lines)
            {
                stringBuilder.AppendLine(line.Text);
            }

            ResponseEditor.Text = stringBuilder.ToString();


           
#else
            ResponseEditor.Text = "OCR features are only available on Windows platform.";
#endif

            CounterBtn.IsEnabled = true;
            CounterBtn.Text = "Extract Text from Image";
        }
    }
}
