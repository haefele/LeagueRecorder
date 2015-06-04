using System;

namespace LeagueRecorder.Shared.Abstractions.Recordings
{
    public class Recording
    {
        public long GameId { get; set; }
        public Region Region { get; set; }

        public int ErrorCount { get; set; }
        public int LatestChunkId { get; set; }
        public int LatestKeyFrameId { get; set; }

        public Version LeagueVersion { get; set; }
        public Version SpectatorVersion { get; set; }
        public string EncryptionKey { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? GameLength { get; set; }
        public int? EndStartupChunkId { get; set; }
        public int? StartGameChunkId { get; set; }
        public int? EndGameChunkId { get; set; }
        public int? EndGameKeyFrameId { get; set; }
        public TimeSpan? ChunkTimeInterval { get; set; }
        public TimeSpan? KeyFrameTimeInterval { get; set; }
        public TimeSpan? ClientAddedLag { get; set; }
        public TimeSpan? DelayTime { get; set; }
        public int? InterestScore { get; set; }
    }
}