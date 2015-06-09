using FluentNHibernate.Mapping;

namespace LeagueRecorder.Shared.Implementations.Replays
{
    public class ReplayGameParticipantEntityMaps : ClassMap<ReplayGameParticipantEntity>
    {
        public ReplayGameParticipantEntityMaps()
        {
            Table("ReplayGameParticipant");

            Id(f => f.Id).GeneratedBy.Guid();

            Map(f => f.SummonerId).Not.Nullable();
            Map(f => f.ChampionId).Not.Nullable();
        }
    }
}