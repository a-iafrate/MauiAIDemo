#if WINDOWS
using Microsoft.Graphics.Imaging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.ContentSafety;
using Microsoft.Windows.AI.Imaging;
using Microsoft.Windows.Management.Deployment;

using System.Text;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

namespace MauiAIDemo
{
    public partial class ImagingPage : ContentPage
    {
        private string? selectedImagePath;

        public ImagingPage()
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
            if (ImageDescriptionGenerator.GetReadyState() != AIFeatureReadyState.Ready)
            {
                var result = await ImageDescriptionGenerator.EnsureReadyAsync();
                if (result.Status != AIFeatureReadyResultState.Success)
                {
                    throw result.ExtendedError;
                }
            }

            ImageDescriptionGenerator imageDescriptionGenerator = await ImageDescriptionGenerator.CreateAsync();

            // Convert already available softwareBitmap to ImageBuffer.
            ImageBuffer inputImage = await LoadImageBufferFromFileAsync(selectedImagePath);

            // Create content moderation thresholds object.
            ContentFilterOptions filterOptions = new ContentFilterOptions();
            //filterOptions.PromptMaxAllowedSeverityLevel. = SeverityLevel.Medium;
            //filterOptions.ResponseMinSeverityLevelToBlock.ViolentContentSeverity = SeverityLevel.Medium;

            // Get text description.
            var languageModelResponse = await imageDescriptionGenerator.DescribeAsync(inputImage, ImageDescriptionKind.BriefDescription, filterOptions);
            string response = languageModelResponse.Description;
            ResponseEditor.Text = response;


#else
            ResponseEditor.Text = "OCR features are only available on Windows platform.";
#endif

            CounterBtn.IsEnabled = true;
            CounterBtn.Text = "Extract Text from Image";
        }
    }
}
