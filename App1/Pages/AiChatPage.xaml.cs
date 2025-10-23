using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.ContentSafety;
using Microsoft.Windows.AI.Text;
using Windows.ApplicationModel;

namespace App1
{
    public sealed partial class AiChatPage : Page
    {
        public AiChatPage()
        {
    InitializeComponent();
        }

        private async void OnCounterClicked(object sender, RoutedEventArgs e)
        {
// Clear previous response and disable button
            ResponseEditor.Text = "Processing...";
  CounterBtn.IsEnabled = false;
          CounterBtn.Content = "Processing...";

            try
  {
        var access = LimitedAccessFeatures.TryUnlockFeature(
        "com.microsoft.windows.ai.languagemodel",
    "xxxxxxxxxxx",
         "xxxxxxxxxx has registered their use of com.microsoft.windows.ai.languagemodel with Microsoft and agrees to the terms of use.");
  
       if ((access.Status == LimitedAccessFeatureStatus.Available) ||
   (access.Status == LimitedAccessFeatureStatus.AvailableWithoutToken))
          {
  /* consume the feature */
          }

     // Check the ready state
    var readyState = LanguageModel.GetReadyState();
      if (readyState == AIFeatureReadyState.NotReady)
   {
    var op = await LanguageModel.EnsureReadyAsync();
          }
   else if (readyState == AIFeatureReadyState.NotReady)
        {
         ResponseEditor.Text = "Language Model is not ready. Please wait and try again.";
   return;
      }
      else if (readyState == AIFeatureReadyState.NotSupportedOnCurrentSystem)
                {
    ResponseEditor.Text = "Language Model is unavailable on this system.";
            return;
                }

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
 CounterBtn.Content = "Ask AI";
   }
  }
    }
}
