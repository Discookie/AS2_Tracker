using System.Collections.Generic;

namespace ASh_Tracker
{
    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
        //public string SoundcloudLink { get; set; }

        public List<Score> Scores = new List<Score>();

        public Scoreboard Scoreboard;

        public static int Count = 0; // Total number of songs loaded

        public string SongID { get; set; }
        public string UserID { get; set; }
        public string UserRegion { get; set; }
        public string UserEmail { get; set; }
        public bool CanPostScore { get; set; }

        // Construct the song
        public Song()
        {
            //this needs to have no parameters so we can serialize it
            Count++; // Increment song count
        }

        // Score class
        public class Score
        {
            public string Mode { get; set; }
            public string Value { get; set; }
        }

        // Song methods
        public void AddScore(Score _score)
        {
            this.Scores.Add(_score);
        }

        public void AddScoreboard(Scoreboard _scoreboard)
        {
            this.Scoreboard = _scoreboard;
        }

        public void SetCanPost(string _canpost)
        {
            CanPostScore = _canpost == "true";
        }
    }
}
