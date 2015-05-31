using System;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.Records;

namespace LeagueRecorder.Shared.Implementations.Records
{
    public class RecordEntity
    {
        public virtual string Id { get; set; }

        public virtual long GameId { get; set; }
        public virtual string Region { get; set; }

        public virtual string LeagueVersion { get; set; }
        public virtual string SpectatorVersion { get; set; }

        public virtual DateTime StartTime { get; set; }
        public virtual DateTime EndTime { get; set; }
        public virtual int InterestScore { get; set; }
        public virtual TimeSpan GameLength { get; set; }

        public virtual string EncryptionKey { get; set; }

        public virtual DateTime CreateTime { get; set; }

        public virtual int EndStartupChunkId { get; set; }
        public virtual int StartGameChunkId { get; set; }
        public virtual int EndGameChunkId { get; set; }
        public virtual int EndGameKeyFrameId { get; set; }

        public virtual TimeSpan ChunkTimeInterval { get; set; }
        public virtual TimeSpan KeyFrameTimeInterval { get; set; }
        public virtual TimeSpan ClientAddedLag { get; set; }
        public virtual TimeSpan DelayTime { get; set; }

        public static string ToId(long gameId, Region region)
        {
            return string.Format("{0}/{1}", region, gameId);
        }

        public virtual Record AsRecord()
        {
            return new Record
            {
                GameId = this.GameId,
                Region = Abstractions.Region.FromString(this.Region),
                LeagueVersion = Version.Parse(this.LeagueVersion),
                SpectatorVersion = Version.Parse(this.SpectatorVersion),
                GameInformation = new GameInformation
                {
                    StartTime = this.StartTime,
                    EndTime = this.EndTime,
                    InterestScore = this.InterestScore,
                    GameLength = this.GameLength
                },
                ReplayInformation = new ReplayInformation
                {
                    EncryptionKey = this.EncryptionKey,
                    CreateTime = this.CreateTime,
                    EndStartupChunkId = this.EndStartupChunkId,
                    StartGameChunkId = this.StartGameChunkId,
                    EndGameChunkId = this.EndGameChunkId,
                    EndGameKeyFrameId = this.EndGameKeyFrameId,
                    ChunkTimeInterval = this.ChunkTimeInterval,
                    KeyFrameTimeInterval = this.KeyFrameTimeInterval,
                    ClientAddedLag = this.ClientAddedLag,
                    DelayTime = this.DelayTime
                }
            };
        }

        public static RecordEntity FromRecord(Record record)
        {
            return new RecordEntity
            {
                Id = ToId(record.GameId, record.Region),
                GameId = record.GameId,
                Region = record.Region.ToString(),
                LeagueVersion = record.LeagueVersion.ToString(),
                SpectatorVersion = record.SpectatorVersion.ToString(),
                StartTime = record.GameInformation.StartTime,
                EndTime = record.GameInformation.EndTime,
                InterestScore = record.GameInformation.InterestScore,
                GameLength = record.GameInformation.GameLength,
                EncryptionKey = record.ReplayInformation.EncryptionKey,
                CreateTime = record.ReplayInformation.CreateTime,
                EndStartupChunkId = record.ReplayInformation.EndStartupChunkId,
                StartGameChunkId = record.ReplayInformation.StartGameChunkId,
                EndGameChunkId = record.ReplayInformation.EndGameChunkId,
                EndGameKeyFrameId = record.ReplayInformation.EndGameKeyFrameId,
                ChunkTimeInterval = record.ReplayInformation.ChunkTimeInterval,
                KeyFrameTimeInterval = record.ReplayInformation.KeyFrameTimeInterval,
                ClientAddedLag = record.ReplayInformation.ClientAddedLag,
                DelayTime = record.ReplayInformation.DelayTime
            };
        }
    }
}