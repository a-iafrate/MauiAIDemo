#if WINDOWS
using Microsoft.Graphics.Imaging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Imaging;
using Microsoft.Windows.Management.Deployment;
using System.Text;
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Maui.Graphics;
using System.Linq;
#endif

namespace MauiAIDemo
{
    public partial class OcrPage : ContentPage
    {
        private string? selectedImagePath;
        private readonly List<RectF> ocrRectangles = new();
        private readonly OcrRectanglesDrawable rectanglesDrawable = new();

        public OcrPage()
        {
            InitializeComponent();
            // Use the GraphicsView declared in XAML and set the Drawable
            OcrGraphicsView.Drawable = rectanglesDrawable;
            OcrGraphicsView.IsVisible = false;
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

                    // Enable the button
                    CounterBtn.IsEnabled = true;

                    // Hide rectangles overlay
                    OcrGraphicsView.IsVisible = false;
                    OcrGraphicsView.Invalidate();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to select image: {ex.Message}", "OK");
            }
        }

#if WINDOWS
        // Load both ImageBuffer and SoftwareBitmap to preserve original dimensions
        public async Task<(ImageBuffer?, SoftwareBitmap?)> LoadImageBufferAndBitmapFromFileAsync(string filePath)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync();
            if (bitmap == null)
            {
                return (null, null);
            }
            ImageBuffer imageBuffer = ImageBuffer.CreateForSoftwareBitmap(bitmap);
            return (imageBuffer, bitmap);
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
            // Ensure the feature is ready
            if (TextRecognizer.GetReadyState() == AIFeatureReadyState.NotReady)
            {
                var loadResult = await TextRecognizer.EnsureReadyAsync();
                if (loadResult.Status != AIFeatureReadyResultState.Success)
                {
                    ResponseEditor.Text = loadResult.ExtendedError?.Message ?? "TextRecognizer not ready.";
                    CounterBtn.IsEnabled = true;
                    CounterBtn.Text = "Extract Text from Image";
                    return;
                }
            }


            var textRecognition = await TextRecognizer.CreateAsync();
            var (imageBuffer, bitmap) = await LoadImageBufferAndBitmapFromFileAsync(selectedImagePath);
            if (imageBuffer == null || bitmap == null)
            {
                ResponseEditor.Text = "Failed to load image.";
                CounterBtn.IsEnabled = true;
                CounterBtn.Text = "Extract Text from Image";
                return;
            }

            RecognizedText recognizedText = textRecognition.RecognizeTextFromImage(imageBuffer);
            StringBuilder stringBuilder = new StringBuilder();
            ocrRectangles.Clear();

            // Original image size (pixels)
            double imageWidth = bitmap.PixelWidth;
            double imageHeight = bitmap.PixelHeight;

            // Overlay area size (matches the drawing area)
            double viewWidth = OcrGraphicsView.Width > 0 ? OcrGraphicsView.Width : SelectedImage.Width;
            double viewHeight = OcrGraphicsView.Height > 0 ? OcrGraphicsView.Height : SelectedImage.Height;

            if (viewWidth <= 0 || viewHeight <= 0)
            {
                // Force layout before computing (rare cases)
                await Task.Delay(16);
                viewWidth = OcrGraphicsView.Width > 0 ? OcrGraphicsView.Width : SelectedImage.Width;
                viewHeight = OcrGraphicsView.Height > 0 ? OcrGraphicsView.Height : SelectedImage.Height;
            }

            // AspectFit calculation: scale and offsets for letterboxing
            double scale = Math.Min(viewWidth / imageWidth, viewHeight / imageHeight);
            double drawWidth = imageWidth * scale;
            double drawHeight = imageHeight * scale;
            double offsetX = (viewWidth - drawWidth) / 2.0;
            double offsetY = (viewHeight - drawHeight) / 2.0;

            if(recognizedText.Lines == null || recognizedText.Lines.Count() == 0)
            {
                ResponseEditor.Text = "No text recognized in the image.";
                CounterBtn.IsEnabled = true;
                return;
            }
            foreach (var line in recognizedText.Lines)
            {
                stringBuilder.AppendLine(line.Text);
                var words = line.Words;
                if (words != null && words.Count() > 0)
                {
                    double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
                    foreach (var word in words)
                    {
                        var box = word.BoundingBox;
                        // Use TopLeft, TopRight, BottomLeft, BottomRight properties
                        var points = new[] { box.TopLeft, box.TopRight, box.BottomLeft, box.BottomRight };
                        foreach (var p in points)
                        {
                            minX = Math.Min(minX, p.X);
                            minY = Math.Min(minY, p.Y);
                            maxX = Math.Max(maxX, p.X);
                            maxY = Math.Max(maxY, p.Y);
                        }
                    }
                    if (minX < maxX && minY < maxY)
                    {
                        // Map to overlay coordinates taking scale and offset into account
                        var scaledRect = new RectF(
                            (float)(offsetX + minX * scale),
                            (float)(offsetY + minY * scale),
                            (float)((maxX - minX) * scale),
                            (float)((maxY - minY) * scale)
                        );
                        ocrRectangles.Add(scaledRect);
                    }
                }
            }

            rectanglesDrawable.Rectangles = ocrRectangles;
            OcrGraphicsView.IsVisible = ocrRectangles.Count > 0;
            OcrGraphicsView.Invalidate();
            ResponseEditor.Text = stringBuilder.ToString();
#else
            ResponseEditor.Text = "OCR features are only available on Windows platform.";
#endif

            CounterBtn.IsEnabled = true;
            CounterBtn.Text = "Extract Text from Image";
        }
    }
}
