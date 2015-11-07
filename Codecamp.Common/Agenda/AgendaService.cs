namespace Codecamp.Common.Agenda
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Storage;

    using Models;
    using Tools;

    using Newtonsoft.Json;

    public class AgendaService
    {
        private string _json;
        public List<Session> AllSessions { get; set; }

        public async Task<List<Session>> GetSessionsAsync()
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Resources/codecamp.json"));
            var stream = await storageFile.OpenStreamForReadAsync();
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, (int)stream.Length);
            _json = Encoding.UTF8.GetString(buffer);
            var list = JsonConvert.DeserializeObject<List<Session>>(_json);
            AllSessions = list;
            return new List<Session>(list);
        }

        public Dictionary<Session, int> FindSessionsByKeyword(string userText)
        {
            var words = GetWordsFromJson();
            var text = userText.ToLower().Split(' ');
            var keywords = text.Where(w => words.Contains(w.ToLower())).ToList();
            var foundList = new Dictionary<Session, int>();
            foreach (var codecampSession in AllSessions)
            {
                foundList[codecampSession] = 0;
                foundList[codecampSession] += GetScoreFrom(codecampSession.Title.ToLower().GetWords().ToArray(), keywords, 5);
                foundList[codecampSession] += GetScoreFrom(codecampSession.Description.ToLower().GetWords().ToArray(), keywords, 2);
            }
            return foundList;            
        }

        private int GetScoreFrom(string[] text, List<string> keywords, int importance)
        {
            var score = 0;

            foreach (var keyword in keywords)
            {
                var occurrences = text.Count(t => t == keyword);
                score += occurrences * keyword.Length * importance;
            }
            return score;
        }

        public List<string> GetWordsFromJson()
        {
            return _json.ToLower().GetWords();
        }

        public async Task<List<Session>> FindSessionsByRoom(string room)
        {
            var sessions = await GetSessionsAsync();
            return sessions.Where(s => s.Location.Room == room).OrderBy(s => s.StartTime).ToList();
        }
    }
}