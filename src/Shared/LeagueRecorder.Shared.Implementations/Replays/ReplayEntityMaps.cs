using FluentNHibernate.Mapping;
using LeagueRecorder.Shared.Implementations.Extensions;

namespace LeagueRecorder.Shared.Implementations.Replays
{
    public class ReplayEntityMaps : ClassMap<ReplayEntity>
    {
        public ReplayEntityMaps()
        {
            Table("Replays");

            Id(f => f.Id).GeneratedBy.Assigned();

            Map(f => f.GameId).Not.Nullable();
            Map(f => f.Region).Not.Nullable().MaxLength();

            Map(f => f.LeagueVersion).Not.Nullable().MaxLength();
            Map(f => f.SpectatorVersion).Not.Nullable().MaxLength();

            Map(f => f.StartTime).Not.Nullable();
            Map(f => f.EndTime).Not.Nullable();
            Map(f => f.InterestScore).Not.Nullable();
            Map(f => f.GameLength).Not.Nullable();

            Map(f => f.EncryptionKey).Not.Nullable();

            Map(f => f.CreateTime).Not.Nullable();

            Map(f => f.EndStartupChunkId).Not.Nullable();
            Map(f => f.StartGameChunkId).Not.Nullable();
            Map(f => f.EndGameChunkId).Not.Nullable();
            Map(f => f.EndGameKeyFrameId).Not.Nullable();

            Map(f => f.ChunkTimeInterval).Not.Nullable();
            Map(f => f.KeyFrameTimeInterval).Not.Nullable();
            Map(f => f.ClientAddedLag).Not.Nullable();
            Map(f => f.DelayTime).Not.Nullable();
        }
    }
}