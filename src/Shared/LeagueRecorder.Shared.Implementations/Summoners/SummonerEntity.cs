using System;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.Summoners;

using RegionEnum = LeagueRecorder.Shared.Abstractions.Region;

namespace LeagueRecorder.Shared.Implementations.Summoners
{
    public class SummonerEntity
    {
        public virtual string Id { get; set; }

        public virtual long SummonerId { get; set; }
        public virtual string Region { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTimeOffset NextDateToCheckIfSummonerIsIngame { get; set; }

        public static string ToId(long summonerId, Region region)
        {
            return string.Format("{0}/{1}", region, summonerId);
        }

        public static SummonerEntity FromSummoner(Summoner summoner)
        {
            return new SummonerEntity
            {
                Id = ToId(summoner.SummonerId, summoner.Region),
                SummonerId = summoner.SummonerId,
                Region = summoner.Region.ToString(),
                Name = summoner.Name,
                NextDateToCheckIfSummonerIsIngame = summoner.NextDateToCheckIfSummonerIsIngame
            };
        }

        public virtual Summoner AsSummoner()
        {
            return new Summoner
            {
                SummonerId = this.SummonerId,
                Region = RegionEnum.FromString(this.Region),
                Name = this.Name,
                NextDateToCheckIfSummonerIsIngame = this.NextDateToCheckIfSummonerIsIngame
            };
        }
    }
}