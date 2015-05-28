namespace LeagueRecorder.Shared.Abstractions.Recordings
{
    public class Recording
    {
        public long GameId { get; set; }
        public Region Region { get; set; }
        public int ErrorCount { get; set; }
    }
}