using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Codecamp.Common.Agenda;
using Codecamp.Common.Models;
using Codecamp.Common.Tools;

namespace CodecampSessionManager
{
    public sealed class CodecampSessionsVoiceCommandService : IBackgroundTask
    {
        VoiceCommandServiceConnection voiceServiceConnection;
        BackgroundTaskDeferral serviceDeferral;
        private AgendaService _agendaService;

        public CodecampSessionsVoiceCommandService()
        {
            _agendaService = new AgendaService();
        }
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;
            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (triggerDetails != null && triggerDetails.Name == "CodecampSessionsVoiceCommandService")
            {
                voiceServiceConnection =
                        VoiceCommandServiceConnection.FromAppServiceTriggerDetails(
                            triggerDetails);

                voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                await _agendaService.GetSessionsAsync();
                switch (voiceCommand.CommandName)
                {
                    case "sayPresentationDescription":
                        var userMessage = new VoiceCommandUserMessage();
                        userMessage.DisplayMessage = "You already forgot? You are going to talk about how I can help developers to create voice activated apps";
                        userMessage.SpokenMessage = "You already forgot? You are going to talk about how I can help developers to create voice activated apps. By the way...asshole, stop forcing me to help you with this stupid presentation. You're lucky I can't use curse words";
                        var response = VoiceCommandResponse.CreateResponse(userMessage);
                        await voiceServiceConnection.ReportSuccessAsync(response);
                        break;
                    case "findSessionsWithCortana":
                        var tags = voiceCommand.SpeechRecognitionResult.SemanticInterpretation.Properties["search"][0];
                        await FindSessionsByTag(tags);
                        break;
                    default:
                        // As with app activation VCDs, we need to handle the possibility that
                        // an app update may remove a voice command that is still registered.
                        // This can happen if the user hasn't run an app since an update.
                        LaunchAppInForeground();
                        break;
                }
            }
        }

        private async Task FindSessionsByTag(string tags)
        {
            try
            {
                var list = _agendaService.FindSessionsByKeyword(tags);
                var results = list.Where(f => f.Value > 0).OrderByDescending(f => f.Value).Select(l => l.Key).Take(10).ToList();

                var userMessage = new VoiceCommandUserMessage();
                if (results.Any())
                {
                    userMessage.DisplayMessage = "Showing top " + results.Count() + " sessions related to " + tags;
                    userMessage.SpokenMessage = "Showing top " + results.Count() + " sessions related to " + tags;
                }
                else
                {
                    userMessage.DisplayMessage = "There are no results for " + tags;
                    userMessage.SpokenMessage = "There are no results for " + tags;
                }
                await ShowResults(results, userMessage);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        private async Task ShowResults(List<Session> results, VoiceCommandUserMessage userMessage)
        {

            var destinationsContentTiles = new List<VoiceCommandContentTile>();
            foreach (var kvp in results)
            {
                var destinationTile = new VoiceCommandContentTile();
                destinationTile.ContentTileType = VoiceCommandContentTileType.TitleWithText;
                destinationTile.AppLaunchArgument = kvp.Title.GetValidString();
                destinationTile.TextLine1 = kvp.Title.GetValidString();
                destinationTile.TextLine2 = kvp.Speakers[0].Name.GetValidString();
                destinationTile.TextLine3 = kvp.Location.Room.GetValidString();
                destinationsContentTiles.Add(destinationTile);
            }
            var response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);
            await voiceServiceConnection.ReportSuccessAsync(response);
        }

        private async void LaunchAppInForeground()
        {
            var userMessage = new VoiceCommandUserMessage();

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "";

            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            serviceDeferral?.Complete();
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Debug.WriteLine("Task cancelled, clean up");
            serviceDeferral?.Complete();
        }
    }
}
