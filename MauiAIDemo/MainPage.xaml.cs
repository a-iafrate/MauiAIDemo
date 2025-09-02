#if WINDOWS
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.ContentSafety;
using Microsoft.Windows.AI.Text;
#endif

namespace MauiAIDemo
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);

#if WINDOWS
            try
            {


                // Then check the ready state
                var readyState = LanguageModel.GetReadyState();
                if (readyState == AIFeatureReadyState.Ready)
                {
                    var op = await LanguageModel.EnsureReadyAsync();
                }
                else if (readyState == AIFeatureReadyState.NotReady)
                {
                    Console.WriteLine("Language Model is not ready. Please wait and try again.");
                    return;
                }
                else if (readyState == AIFeatureReadyState.NotReady)
                {
                    Console.WriteLine("Language Model is unavailable on this system.");
                    return;
                }

                using LanguageModel languageModel = await LanguageModel.CreateAsync();

                string prompt = "Provide the molecular formula for glucose.";

                LanguageModelOptions options = new LanguageModelOptions();
                ContentFilterOptions filterOptions = new ContentFilterOptions();
                filterOptions.PromptMaxAllowedSeverityLevel.Violent = SeverityLevel.Minimum;
                options.ContentFilterOptions = filterOptions;

                var result = await languageModel.GenerateResponseAsync(prompt, options);

                Console.WriteLine(result.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
#endif
        }
    }
}
