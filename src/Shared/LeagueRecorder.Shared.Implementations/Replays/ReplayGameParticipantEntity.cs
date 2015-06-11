using System;

namespace LeagueRecorder.Shared.Implementations.Replays
{
    public class ReplayGameParticipantEntity
    {
        public virtual Guid Id { get; set; }

        public virtual long SummonerId { get; set; }
        public virtual string SummonerName { get; set; }
        public virtual long ChampionId { get; set; }
        public virtual long TeamId { get; set; }
    }
}