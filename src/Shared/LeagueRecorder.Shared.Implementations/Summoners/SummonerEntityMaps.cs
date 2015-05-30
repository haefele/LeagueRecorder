using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Mapping;
using LeagueRecorder.Shared.Implementations.Extensions;

namespace LeagueRecorder.Shared.Implementations.Summoners
{
    public class SummonerEntityMaps : ClassMap<SummonerEntity>
    {
        public SummonerEntityMaps()
        {
            Table("Summoners");

            Id(f => f.Id).GeneratedBy.Assigned();

            Map(f => f.SummonerId).Not.Nullable().MaxLength();
            Map(f => f.Region).Not.Nullable().MaxLength();

            Map(f => f.Name).Not.Nullable().MaxLength();

            Map(f => f.NextDateToCheckIfSummonerIsIngame).Not.Nullable();
        }
    }
}