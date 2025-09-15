#if WINDOWS
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.ContentSafety;
using Microsoft.Windows.AI.Text;
using Windows.ApplicationModel;
#endif

namespace MauiAIDemo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object? sender, EventArgs e)
        {
            // Clear previous response and disable button
            ResponseEditor.Text = "Processing...";
            CounterBtn.IsEnabled = false;
            CounterBtn.Text = "Processing...";

#if WINDOWS
            try
            {
            var access = Windows.ApplicationModel.LimitedAccessFeatures.TryUnlockFeature(
    "com.microsoft.windows.ai.languagemodel",
    "rJsxYZYAw4FkM43LmMIr5A==",
    "9f7cbwqwexwb0 has registered their use of com.microsoft.windows.ai.languagemodel with Microsoft and agrees to the terms of use.");
if ((access.Status == LimitedAccessFeatureStatus.Available) ||
    (access.Status == LimitedAccessFeatureStatus.AvailableWithoutToken))
{
    /* consume the feature */
}

                // Check the ready state
                var readyState = LanguageModel.GetReadyState();
                if (readyState == AIFeatureReadyState.Ready)
                {
                    var op = await LanguageModel.EnsureReadyAsync();
                }
                else if (readyState == AIFeatureReadyState.NotReady)
                {
                    ResponseEditor.Text = "Language Model is not ready. Please wait and try again.";
                    return;
                }
                /*else if (readyState == AIFeatureReadyState.Unavailable)
                {
                    ResponseEditor.Text = "Language Model is unavailable on this system.";
                    return;
                }*/

                using LanguageModel languageModel = await LanguageModel.CreateAsync();

                // Get prompt from Entry field
                string prompt = string.IsNullOrWhiteSpace(PromptEntry.Text) 
                    ? "Provide the molecular formula for glucose." 
                    : PromptEntry.Text;

                LanguageModelOptions options = new LanguageModelOptions();
                ContentFilterOptions filterOptions = new ContentFilterOptions();
                filterOptions.PromptMaxAllowedSeverityLevel.Violent = SeverityLevel.Minimum;
                options.ContentFilterOptions = filterOptions;

                var result = await languageModel.GenerateResponseAsync(prompt, options);

                // Display response in Editor
                ResponseEditor.Text = result.Text;
            }
            catch (Exception ex)
            {
                ResponseEditor.Text = $"Error: {ex.Message}";
            }
            finally
            {
                // Re-enable button
                CounterBtn.IsEnabled = true;
                CounterBtn.Text = "Ask AI";
            }
#else
            ResponseEditor.Text = "AI features are only available on Windows platform.";
            CounterBtn.IsEnabled = true;
            CounterBtn.Text = "Ask AI";
#endif
        }
    }
}
