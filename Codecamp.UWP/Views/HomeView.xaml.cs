using System;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
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
            var voiceCommandActivatedEventArgs = e.Parameter as VoiceCommandActivatedEventArgs;
            SpeechRecognitionResult result = voiceCommandActivatedEventArgs?.Result;
            if (result == null) return;
            TextSpoken.Text = result.Text;
            ProcessCommands(result);
        }

        public async void ProcessCommands(SpeechRecognitionResult result)
        {
            string voiceCommandName = result.RulePath.First();
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            switch (voiceCommandName)
            {
                case "showSessionsByRoom":
                    var roomNumber = result.SemanticInterpretation.Properties["room"][0];
                    await ViewModel.FindSessionsByRoom(roomNumber);
                    break;
                case "showSessions":
                    var stream = await synthesizer.SynthesizeTextToStreamAsync("There are " + ViewModel.CodecampSessions.Count + " sessions!");
                    AudioPlayer.SetSource(stream, string.Empty);
                    break;
                case "findSessionsByKeyword":
                    string tag = result.SemanticInterpretation.Properties["tag"][0];
                    var sessionCount = ViewModel.CodecampSessions.Count(s => s.Tags.Contains(tag));
                    var findStream = await synthesizer.SynthesizeTextToStreamAsync("There are " + sessionCount + " sessions related to " + tag + "!");
                    AudioPlayer.SetSource(findStream, string.Empty);
                    break;
            }
        }

        private async void FindByVoice(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await ViewModel.FindByVoiceAsync());
        }
    }
}