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

                // Depending on the operation (defined in AdventureWorks:AdventureWorksCommands.xml)
                // perform the appropriate command.
                switch (voiceCommand.CommandName)
                {
                    case "findSessionsWithCortana":
                        //var tag = voiceCommand.Properties["Search"][0];

                        var tags = voiceCommand.SpeechRecognitionResult.SemanticInterpretation.Properties["search"][0];
                        var list = voiceCommand.SpeechRecognitionResult.SemanticInterpretation.Properties["search"];
                        foreach (var keyword in list)
                        {
                            Debug.WriteLine(keyword);
                        }
                        await FindSessionsByTag(tags);
                        break;
                    case "findSessionsByRoom":
                        var roomNumber = voiceCommand.SpeechRecognitionResult.SemanticInterpretation.Properties["room"][0];
                        await FindSessionsByRoom(roomNumber);
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

        private async Task FindSessionsByRoom(string roomNumber)
        {
            var userMessage = new VoiceCommandUserMessage();
            var results = await _agendaService.FindSessionsByRoom("Room " + roomNumber);
            userMessage.DisplayMessage = "These are the sessions from room " + roomNumber;
            userMessage.SpokenMessage = "These are the sessions from room " + roomNumber;
            await ShowResults(results, userMessage);
        }

        private async Task FindSessionsByTag(string tags)
        {
            try
            {
                var list = _agendaService.FindSessionsByKeyword(tags);
                var results = list.Where(f => f.Value > 0).OrderByDescending(f => f.Value).Select(l => l.Key).Take(10).ToList();

            var userMessage = new VoiceCommandUserMessage();
                userMessage.DisplayMessage = "Showing top " + results.Count() + " sessions related to " + tags;
                userMessage.SpokenMessage = "Showing top " + results.Count() + " sessions related to " + tags;
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
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Task cancelled, clean up");
            if (this.serviceDeferral != null)
            {
                //Complete the service deferral
                this.serviceDeferral.Complete();
            }
        }
    }
}
