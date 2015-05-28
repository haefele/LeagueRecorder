using System;

namespace LeagueRecorder.Shared.Abstractions.Summoners
{
    public class Summoner
    {
        public long SummonerId { get; set; }
        public Region Region { get; set; }

        public string Name { get; set; }

        public DateTimeOffset NextDateToCheckIfSummonerIsIngame { get; set; }
    }
}
