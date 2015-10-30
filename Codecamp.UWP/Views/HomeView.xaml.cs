using System;
using System.Linq;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Codecamp.UWP.Views
{
    using ViewModels;

    public sealed partial class HomeView
    {
        public HomeView()
        {
            InitializeComponent();
        }

        public HomeViewModel ViewModel => DataContext as HomeViewModel;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.Activate();
            SpeechRecognitionResult result = e.Parameter as SpeechRecognitionResult;
            if (result != null)
            {
                TextSpoken.Text = result.Text;
                ProcessCommands(result);
            }
        }

        public async void ProcessCommands(SpeechRecognitionResult result)
        {
            string voiceCommandName = result.RulePath.First();
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            switch (voiceCommandName)
            {
                case "showSessions":
                    var stream = await synthesizer.SynthesizeTextToStreamAsync("There are " + ViewModel.CodecampSessions.Count + " sessions!");
                    AudioPlayer.SetSource(stream, string.Empty);
                    break;
                case "findSessions":
                    string tag = result.SemanticInterpretation.Properties["tag"][0];
                    var sessionCount = ViewModel.CodecampSessions.Count(s => s.Tags.Contains(tag));
                    var findStream = await synthesizer.SynthesizeTextToStreamAsync("There are " + sessionCount + " sessions related to " + tag + "!");
                    AudioPlayer.SetSource(findStream, string.Empty);
                    break;
            }
        }

        private async void FindByVoice(object sender, RoutedEventArgs e)
        {
            await ViewModel.FindByVoiceAsync();
        }
    }
}