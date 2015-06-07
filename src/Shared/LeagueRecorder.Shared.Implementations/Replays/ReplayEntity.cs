using System;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.Replays;

namespace LeagueRecorder.Shared.Implementations.Replays
{
    public class ReplayEntity
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

        public virtual Replay AsReplay()
        {
            return new Replay
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

        public static ReplayEntity FromReplay(Replay replay)
        {
            return new ReplayEntity
            {
                Id = ToId(replay.GameId, replay.Region),
                GameId = replay.GameId,
                Region = replay.Region.ToString(),
                LeagueVersion = replay.LeagueVersion.ToString(),
                SpectatorVersion = replay.SpectatorVersion.ToString(),
                StartTime = replay.GameInformation.StartTime,
                EndTime = replay.GameInformation.EndTime,
                InterestScore = replay.GameInformation.InterestScore,
                GameLength = replay.GameInformation.GameLength,
                EncryptionKey = replay.ReplayInformation.EncryptionKey,
                CreateTime = replay.ReplayInformation.CreateTime,
                EndStartupChunkId = replay.ReplayInformation.EndStartupChunkId,
                StartGameChunkId = replay.ReplayInformation.StartGameChunkId,
                EndGameChunkId = replay.ReplayInformation.EndGameChunkId,
                EndGameKeyFrameId = replay.ReplayInformation.EndGameKeyFrameId,
                ChunkTimeInterval = replay.ReplayInformation.ChunkTimeInterval,
                KeyFrameTimeInterval = replay.ReplayInformation.KeyFrameTimeInterval,
                ClientAddedLag = replay.ReplayInformation.ClientAddedLag,
                DelayTime = replay.ReplayInformation.DelayTime
            };
        }
    }
}