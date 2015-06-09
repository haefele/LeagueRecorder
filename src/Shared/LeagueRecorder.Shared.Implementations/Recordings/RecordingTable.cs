using System;
using System.Collections;
using System.Collections.Generic;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.Recordings;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

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
        public long? GameLength { get; set; }
        public int? EndStartupChunkId { get; set; }
        public int? StartGameChunkId { get; set; }
        public int? EndGameChunkId { get; set; }
        public int? EndGameKeyFrameId { get; set; }
        public long? ChunkTimeInterval { get; set; }
        public long? KeyFrameTimeInterval { get; set; }
        public long? ClientAddedLag { get; set; }
        public long? DelayTime { get; set; }
        public int? InterestScore { get; set; }
        public string ParticipantsJson { get; set; }
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
                LatestChunkId = recording.LatestChunkId,
                LatestKeyFrameId = recording.LatestKeyFrameId,
                LeagueVersion = recording.LeagueVersion != null ? recording.LeagueVersion.ToString() : null,
                SpectatorVersion = recording.SpectatorVersion != null ? recording.SpectatorVersion.ToString() : null,
                EncryptionKey = recording.EncryptionKey,
                CreateTime = recording.CreateTime,
                StartTime = recording.StartTime,
                EndTime = recording.EndTime,
                GameLength = recording.GameLength.HasValue ? recording.GameLength.Value.Ticks : (long?)null,
                EndStartupChunkId = recording.EndStartupChunkId,
                StartGameChunkId = recording.StartGameChunkId,
                EndGameChunkId = recording.EndGameChunkId,
                EndGameKeyFrameId = recording.EndGameKeyFrameId,
                ChunkTimeInterval = recording.ChunkTimeInterval.HasValue ? recording.ChunkTimeInterval.Value.Ticks : (long?)null,
                KeyFrameTimeInterval = recording.KeyFrameTimeInterval.HasValue ? recording.KeyFrameTimeInterval.Value.Ticks : (long?)null,
                ClientAddedLag = recording.ClientAddedLag.HasValue ? recording.ClientAddedLag.Value.Ticks : (long?)null,
                DelayTime = recording.DelayTime.HasValue ? recording.DelayTime.Value.Ticks : (long?)null,
                InterestScore = recording.InterestScore,
                ParticipantsJson = JsonConvert.SerializeObject(recording.Participants)
            };
        }

        public Recording AsRecording()
        {
            return new Recording
            {
                Region = Region.FromString(this.PartitionKey),
                GameId = long.Parse(this.RowKey),
                ErrorCount = this.ErrorCount,
                LatestChunkId = this.LatestChunkId,
                LatestKeyFrameId = this.LatestKeyFrameId,
                LeagueVersion = this.LeagueVersion != null ? Version.Parse(this.LeagueVersion) : null,
                SpectatorVersion = this.SpectatorVersion != null ? Version.Parse(this.SpectatorVersion) : null,
                EncryptionKey = this.EncryptionKey,
                CreateTime = this.CreateTime,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                GameLength = this.GameLength.HasValue ? TimeSpan.FromTicks(this.GameLength.Value) : (TimeSpan?)null,
                EndStartupChunkId = this.EndStartupChunkId,
                StartGameChunkId = this.StartGameChunkId,
                EndGameChunkId = this.EndGameChunkId,
                EndGameKeyFrameId = this.EndGameKeyFrameId,
                ChunkTimeInterval = this.ChunkTimeInterval.HasValue ? TimeSpan.FromTicks(this.ChunkTimeInterval.Value) : (TimeSpan?)null,
                KeyFrameTimeInterval = this.KeyFrameTimeInterval.HasValue ? TimeSpan.FromTicks(this.KeyFrameTimeInterval.Value) : (TimeSpan?)null,
                ClientAddedLag = this.ClientAddedLag.HasValue ? TimeSpan.FromTicks(this.ClientAddedLag.Value) : (TimeSpan?)null,
                DelayTime = this.DelayTime.HasValue ? TimeSpan.FromTicks(this.DelayTime.Value) : (TimeSpan?)null,
                InterestScore = this.InterestScore,
                Participants = JsonConvert.DeserializeObject<IList<RecordingGameParticipant>>(this.ParticipantsJson)
            };
        }
    }
}