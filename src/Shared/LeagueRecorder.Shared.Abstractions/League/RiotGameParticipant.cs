﻿namespace LeagueRecorder.Shared.Abstractions.League
{
    public class RiotGameParticipant
    {
        public long SummonerId { get; set; }
        public long ChampionId { get; set; }
        public string SummonerName { get; set; }
        public Team Team { get; set; }
    }
}