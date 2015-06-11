namespace LeagueRecorder.Shared.Abstractions.Replays
{
    public class ReplayGameParticipant
    {
        public long SummonerId { get; set; }
        public long ChampionId { get; set; }
        public string SummonerName { get; set; }
        public Team Team { get; set; }
    }
}