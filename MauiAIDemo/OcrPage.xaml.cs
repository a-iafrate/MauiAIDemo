#if WINDOWS
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.ContentSafety;
using Microsoft.Windows.AI.Text;
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
            await Task.Delay(2000); // Simulate processing
            ResponseEditor.Text = "OCR functionality will be implemented here.";
#else
            ResponseEditor.Text = "OCR features are only available on Windows platform.";
#endif

            CounterBtn.IsEnabled = true;
            CounterBtn.Text = "Extract Text from Image";
        }
    }
}
