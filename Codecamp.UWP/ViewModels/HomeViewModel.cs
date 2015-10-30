using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Media.SpeechRecognition;
using Windows.UI.Popups;

namespace Codecamp.UWP.ViewModels
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Newtonsoft.Json;

    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    using Models;
    using Annotations;

    public class HomeViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Session> _codecampSessions;
        private string json;

        public async Task Activate()
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Resources/codecamp.json"));
            var stream = await storageFile.OpenStreamForReadAsync();
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, (int)stream.Length);
            json = Encoding.UTF8.GetString(buffer);
            var list = JsonConvert.DeserializeObject<List<Session>>(json);
            CodecampSessions = new ObservableCollection<Session>(list);
        }

        public ObservableCollection<Session> CodecampSessions
        {
            get { return _codecampSessions; }
            set
            {
                _codecampSessions = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public async Task FindByVoiceAsync()
        {
            SpeechRecognizer speechRecognizer = new SpeechRecognizer();
            await speechRecognizer.CompileConstraintsAsync();
            speechRecognizer.ContinuousRecognitionSession.ResultGenerated +=
                ContinuousRecognitionSession_ResultGenerated;
            await speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private int GetScoreFrom(string[] text, List<string> keywords)
        {
            var score = 0;

            foreach (var keyword in keywords)
            {
                var occurrences = text.Count(t => t == keyword);
                score += occurrences*keyword.Length;
            }
            return score;
        }

        static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select TrimSuffix(m.Value);

            return words.ToArray();
        }

        static string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }
        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                var words = GetWords(json.ToLower());
                var text = args.Result.Text.ToLower().Split(' ');
                var keywords = text.Where(w => words.Contains(w.ToLower())).ToList();
                var foundList = new Dictionary<Session, int>();
                foreach (var codecampSession in CodecampSessions)
                {
                    foundList[codecampSession] = 0;
                    foundList[codecampSession] += GetScoreFrom(GetWords(codecampSession.Title.ToLower()), keywords);
                    foundList[codecampSession] += GetScoreFrom(GetWords(codecampSession.Description.ToLower()), keywords);
                }
                var results = foundList.OrderByDescending(f => f.Value).Take(10);
                foreach (var i in results)
                {
                    Debug.WriteLine(i.Key.Title + " " + i.Value);
                }
            }
            else
            {
                await new MessageDialog("Discarded due to low/rejected Confidence").ShowAsync();
            }
        }
    }
}
