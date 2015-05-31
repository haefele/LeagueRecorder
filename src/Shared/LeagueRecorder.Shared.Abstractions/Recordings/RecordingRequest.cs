namespace LeagueRecorder.Shared.Abstractions.Recordings
{
    public class RecordingRequest
    {
        public long GameId { get; set; }
        public Region Region { get; set; }
    }
}