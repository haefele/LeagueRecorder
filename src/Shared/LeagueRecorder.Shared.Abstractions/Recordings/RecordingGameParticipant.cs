namespace LeagueRecorder.Shared.Abstractions.Recordings
{
    public class RecordingGameParticipant
    {
        public long SummonerId { get; set; }
        public long ChampionId { get; set; }
        public string SummonerName { get; set; }
        public Team Team { get; set; }
    }
}