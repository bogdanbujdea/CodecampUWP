using Codecamp.Common.Agenda;
using Codecamp.Common.Models;
using Codecamp.Common.Tools;

namespace Codecamp.UWP.ViewModels
{
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Windows.Media.SpeechRecognition;
    using Windows.UI.Popups;

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
    using Annotations;

    public class HomeViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Session> _codecampSessions;
        private AgendaService _agendaService;

        public async Task Activate()
        {
            _agendaService = new AgendaService();
            var sessions = await _agendaService.GetSessionsAsync();
            CodecampSessions = new ObservableCollection<Session>(sessions);
           /* Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinition commandSetEnUs;

            if (Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.
                  InstalledCommandDefinitions.TryGetValue(
                    "CodecampCommandSet", out commandSetEnUs))
            {
                var wordsFromJson = _agendaService.GetWordsFromJson();
                await commandSetEnUs.SetPhraseListAsync("keyword", wordsFromJson);
            }*/
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

        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                var results = _agendaService.FindSessionsByKeyword(args.Result.Text, CodecampSessions.ToList());
                var list = results.Where(r => r.Value > 0).OrderByDescending(r => r.Value).Take(10);
                foreach (var i in list)
                {
                    Debug.WriteLine(i.Key.Title + " " + i.Value);
                }
            }
            else
            {
                //await new MessageDialog("Discarded due to low/rejected Confidence").ShowAsync();
            }
        }
    }
}