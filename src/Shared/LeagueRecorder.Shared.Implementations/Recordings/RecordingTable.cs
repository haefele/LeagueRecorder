using System;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.Recordings;
using Microsoft.WindowsAzure.Storage.Table;

namespace LeagueRecorder.Shared.Implementations.Recordings
{
    internal class RecordingTable : TableEntity
    {
        #region Constructors
        public RecordingTable()
        {
        }

        public RecordingTable(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }
        #endregion

        #region Properties
        public int ErrorCount { get; set; }
        public bool HasFinished { get; set; }
        public int LatestChunkId { get; set; }
        public int LatestKeyFrameId { get; set; }

        public string LeagueVersion { get; set; }
        public string SpectatorVersion { get; set; }
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
        #endregion

        public static string ToPartitionKey(Region region)
        {
            return region.ToString();
        }

        public static string ToRowKey(long gameId)
        {
            return gameId.ToString();
        }

        public static RecordingTable FromRecording(Recording recording)
        {
            return new RecordingTable(ToPartitionKey(recording.Region), ToRowKey(recording.GameId))
            {
                ErrorCount = recording.ErrorCount,
                HasFinished = recording.HasFinished,
                LatestChunkId = recording.LatestChunkId,
                LatestKeyFrameId = recording.LatestKeyFrameId,
                LeagueVersion = recording.LeagueVersion.ToString(),
                SpectatorVersion = recording.SpectatorVersion.ToString(),
                EncryptionKey = recording.EncryptionKey,
                CreateTime = recording.CreateTime,
                StartTime = recording.StartTime,
                EndTime = recording.EndTime,
                GameLength = recording.GameLength,
                EndStartupChunkId = recording.EndStartupChunkId,
                StartGameChunkId = recording.StartGameChunkId,
                EndGameChunkId = recording.EndGameChunkId,
                EndGameKeyFrameId = recording.EndGameKeyFrameId,
                ChunkTimeInterval = recording.ChunkTimeInterval,
                KeyFrameTimeInterval = recording.KeyFrameTimeInterval,
                ClientAddedLag = recording.ClientAddedLag,
                DelayTime = recording.DelayTime,
                InterestScore = recording.InterestScore
            };
        }

        public Recording AsRecording()
        {
            return new Recording
            {
                Region = Region.FromString(this.PartitionKey),
                GameId = long.Parse(this.RowKey),
                ErrorCount = this.ErrorCount,
                HasFinished = this.HasFinished,
                LatestChunkId = this.LatestChunkId,
                LatestKeyFrameId = this.LatestKeyFrameId,
                LeagueVersion = Version.Parse(this.LeagueVersion),
                SpectatorVersion = Version.Parse(this.SpectatorVersion),
                EncryptionKey = this.EncryptionKey,
                CreateTime = this.CreateTime,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                GameLength = this.GameLength,
                EndStartupChunkId = this.EndStartupChunkId,
                StartGameChunkId = this.StartGameChunkId,
                EndGameChunkId = this.EndGameChunkId,
                EndGameKeyFrameId = this.EndGameKeyFrameId,
                ChunkTimeInterval = this.ChunkTimeInterval,
                KeyFrameTimeInterval = this.KeyFrameTimeInterval,
                ClientAddedLag = this.ClientAddedLag,
                DelayTime = this.DelayTime,
                InterestScore = this.InterestScore
            };
        }
    }
}