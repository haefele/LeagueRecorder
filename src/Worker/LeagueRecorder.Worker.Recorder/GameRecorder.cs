using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Anotar.NLog;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.GameData;
using LeagueRecorder.Shared.Abstractions.League;
using LeagueRecorder.Shared.Abstractions.Recordings;
using LeagueRecorder.Shared.Abstractions.Replays;
using LeagueRecorder.Shared.Abstractions.Results;
using LiteGuard;

namespace LeagueRecorder.Worker.Recorder
{
    /// <summary>
    /// This class is responsible for recording a single game.
    /// </summary>
    public class GameRecorder : IDisposable
    {
        #region Fields
        private readonly RecordingRequest _recording;
        private readonly ILeagueApiClient _leagueApiClient;
        private readonly ILeagueSpectatorApiClient _leagueSpectatorApiClient;
        private readonly IRecordingStorage _recordingStorage;
        private readonly IGameDataStorage _gameDataStorage;
        private readonly IReplayStorage _replayStorage;
        private readonly IConfig _config;

        private readonly Timer _timer;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the recording request that is beeing recorded here.
        /// </summary>
        public RecordingRequest RecordingRequest
        {
            get { return this._recording; }
        }
        /// <summary>
        /// Gets the current state of the game recorder.
        /// </summary>
        public GameRecorderState State { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GameRecorder" /> class.
        /// </summary>
        /// <param name="recording">The recording.</param>
        /// <param name="leagueApiClient">The league API client.</param>
        /// <param name="leagueSpectatorApiClient">The league spectator API client.</param>
        /// <param name="recordingStorage">The recording storage.</param>
        /// <param name="gameDataStorage">The game data storage.</param>
        /// <param name="replayStorage">The record storage.</param>
        /// <param name="config">The configuration.</param>
        public GameRecorder([NotNull]RecordingRequest recording, [NotNull]ILeagueApiClient leagueApiClient, [NotNull]ILeagueSpectatorApiClient leagueSpectatorApiClient, [NotNull]IRecordingStorage recordingStorage, [NotNull]IGameDataStorage gameDataStorage, [NotNull]IReplayStorage replayStorage, [NotNull]IConfig config)
        {
            Guard.AgainstNullArgument("recording", recording);
            Guard.AgainstNullArgument("leagueApiClient", leagueApiClient);
            Guard.AgainstNullArgument("leagueSpectatorApiClient", leagueSpectatorApiClient);
            Guard.AgainstNullArgument("recordingStorage", recordingStorage);
            Guard.AgainstNullArgument("gameDataStorage", gameDataStorage);
            Guard.AgainstNullArgument("ReplayStorage", replayStorage);
            Guard.AgainstNullArgument("config", config);

            this._recording = recording;
            this._leagueApiClient = leagueApiClient;
            this._leagueSpectatorApiClient = leagueSpectatorApiClient;
            this._recordingStorage = recordingStorage;
            this._gameDataStorage = gameDataStorage;
            this._replayStorage = replayStorage;
            this._config = config;
            
            this._timer = new Timer();
            this._timer.Interval = TimeSpan.FromSeconds(20).TotalMilliseconds;
            this._timer.Elapsed += this.TimerOnElapsed;
            
            this._timer.Start();

            this.State = GameRecorderState.Running;
        }
        #endregion

        #region Private Methods
        private async void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                if (this.State == GameRecorderState.Running)
                    this._timer.Stop();

                Result<Recording> recordingResult = await this._recordingStorage.GetRecordingAsync(this.RecordingRequest.GameId, this.RecordingRequest.Region);
                if (recordingResult.IsError)
                {
                    LogTo.Error("Error while loading the recording {0} {1}: {2}", this.RecordingRequest.Region, this.RecordingRequest.GameId, recordingResult.Message);

                    this.TrySetState(GameRecorderState.Error);
                    return;
                }
                else
                {
                    LogTo.Debug("Updating recording {0} {1}.", this.RecordingRequest.Region, this.RecordingRequest.GameId);

                    await this.LoadLeagueVersionAsync(recordingResult.Data);
                    await this.LoadLeagueSpectatorVersionAsync(recordingResult.Data);

                    Result<RiotLastGameInfo> lastGameInfoResult = await this._leagueSpectatorApiClient.GetLastGameInfo(this.RecordingRequest.Region, this.RecordingRequest.GameId);
                    if (lastGameInfoResult.IsError)
                    {
                        LogTo.Error("Error while retrieving the last-game-info for recording {0} {1}: {2}", this.RecordingRequest.Region, this.RecordingRequest.GameId, lastGameInfoResult.Message);

                        recordingResult.Data.ErrorCount++;
                    }
                    else
                    {
                        var loadChunkTask = this.LoadChunksAsync(recordingResult.Data, lastGameInfoResult.Data);
                        var loadKeyFrameTask = this.LoadKeyFramesAsync(recordingResult.Data, lastGameInfoResult.Data);

                        await Task.WhenAll(loadChunkTask, loadKeyFrameTask);

                        await this.LoadEndGameDataAsync(recordingResult.Data, lastGameInfoResult.Data);
                    }

                    if (recordingResult.Data.EndGameChunkId.HasValue &&
                        recordingResult.Data.EndGameKeyFrameId.HasValue &&
                        recordingResult.Data.LatestChunkId == recordingResult.Data.EndGameChunkId.Value &&
                        recordingResult.Data.LatestKeyFrameId == recordingResult.Data.EndGameKeyFrameId.Value)
                    {
                        LogTo.Info("The recording {0} {1} has finished. Yeah!", this.RecordingRequest.Region, this.RecordingRequest.GameId);

                        await this.SaveReplayAsync(recordingResult.Data);
                        await this._recordingStorage.DeleteRecordingAsync(recordingResult.Data.GameId, recordingResult.Data.Region);

                        this.TrySetState(GameRecorderState.Finished);
                        return;
                    }

                    if (recordingResult.Data.ErrorCount > this._config.RecordingMaxErrorCount)
                    {
                        LogTo.Info("The recording {0} {1} exceeded the error count of {2}. Deleting it.", this.RecordingRequest.Region, this.RecordingRequest.GameId, this._config.RecordingMaxErrorCount);

                        await this.DeleteRecordingWithAllGameDataAsync(recordingResult.Data);
                        this.TrySetState(GameRecorderState.Error);

                        return;
                    }

                    Result saveResult = await this._recordingStorage.SaveRecordingAsync(recordingResult.Data);
                    if (saveResult.IsError)
                    {
                        LogTo.Error("Error while saving the recording {0} {1}: {2}", this.RecordingRequest.Region, this.RecordingRequest.GameId, saveResult.Message);
                    }
                }
            }
            catch (Exception exception)
            {
                LogTo.ErrorException("Exception while recording game.", exception);
            }
            finally
            {
                if (this.State == GameRecorderState.Running)
                    this._timer.Start();
            }
        }

        private async Task LoadLeagueVersionAsync(Recording recording)
        {
            if (recording.LeagueVersion == null)
            {
                LogTo.Debug("The league version of recording {0} {1} is missing.", recording.Region, recording.GameId);

                Result<Version> versionResult = await this._leagueApiClient.GetLeagueVersion(recording.Region);

                if (versionResult.IsError)
                {
                    LogTo.Error("Error while retrieving the league version for recording {0} {1}: {2}", recording.Region, recording.GameId, versionResult.Message);

                    recording.ErrorCount++;
                    return;
                }
                
                LogTo.Debug("The league version of recording {0} {1} is {2}.", recording.Region, recording.GameId, versionResult.Data);
                recording.LeagueVersion = versionResult.Data;
            }
        }
        
        private async Task LoadLeagueSpectatorVersionAsync(Recording recording)
        {
            if (recording.SpectatorVersion == null)
            {
                LogTo.Debug("The spectator version of recording {0} {1} is missing.", recording.Region, recording.GameId);

                Result<Version> spectatorVersionResult = await this._leagueSpectatorApiClient.GetSpectatorVersion(recording.Region);

                if (spectatorVersionResult.IsError)
                {
                    LogTo.Error("Error while retrieving the spectator version for recording {0} {1}: {2}", recording.Region, recording.GameId, spectatorVersionResult.Message);

                    recording.ErrorCount++;
                    return;
                }

                LogTo.Debug("The spectator version of recording {0} {1} is {2}.", recording.Region, recording.GameId, spectatorVersionResult.Data);
                recording.SpectatorVersion = spectatorVersionResult.Data;
            }
        }

        private async Task LoadChunksAsync(Recording recording, RiotLastGameInfo lastGameInfo)
        {
            while (recording.LatestChunkId < lastGameInfo.CurrentChunkId)
            {
                int nextChunkId = recording.LatestChunkId + 1;

                LogTo.Debug("Downloading chunk {0} for recording {1} {2}.", nextChunkId, recording.Region, recording.GameId);
                Result<RiotChunk> chunkResult = await this._leagueSpectatorApiClient.GetChunk(recording.Region, recording.GameId, nextChunkId);

                if (chunkResult.IsError)
                {
                    LogTo.Error("Error while downloading chunk {0} for recording {1} {2}: {3}", nextChunkId, recording.Region, recording.GameId, chunkResult.Message);

                    recording.ErrorCount++;
                    return;
                }

                Result saveChunkResult = await this._gameDataStorage.SaveChunkAsync(recording.GameId, recording.Region, chunkResult.Data.Id, chunkResult.Data.Data);
                if (saveChunkResult.IsError)
                {
                    LogTo.Error("Error while saving chunk {0} for recording {1} {2}: {3}", nextChunkId, recording.Region, recording.GameId, saveChunkResult.Message);
                    return;
                }

                recording.LatestChunkId = nextChunkId;
            }
        }
        
        private async Task LoadKeyFramesAsync(Recording recording, RiotLastGameInfo lastGameInfo)
        {
            while (recording.LatestKeyFrameId < lastGameInfo.CurrentKeyFrameId)
            {
                int nextKeyFrameId = recording.LatestKeyFrameId + 1;
                
                LogTo.Debug("Downloading keyframe {0} for recording {1} {2}.", nextKeyFrameId, recording.Region, recording.GameId);
                Result<RiotKeyFrame> keyFrameResult = await this._leagueSpectatorApiClient.GetKeyFrame(recording.Region, recording.GameId, nextKeyFrameId);

                if (keyFrameResult.IsError)
                {
                    LogTo.Error("Error while downloading keyframe {0} for recording {1} {2}: {3}", nextKeyFrameId, recording.Region, recording.GameId, keyFrameResult.Message);

                    recording.ErrorCount++;
                    return;
                }

                Result saveKeyFrameResult = await this._gameDataStorage.SaveKeyFrameAsync(recording.GameId, recording.Region, keyFrameResult.Data.Id, keyFrameResult.Data.Data);
                if (saveKeyFrameResult.IsError)
                {
                    LogTo.Error("Error while saving keyframe {0} for recording {1} {2}: {3}", nextKeyFrameId, recording.Region, recording.GameId, saveKeyFrameResult.Message);
                    return;
                }

                recording.LatestKeyFrameId = nextKeyFrameId;
            }
        }
        
        private async Task LoadEndGameDataAsync(Recording recording, RiotLastGameInfo lastGameInfo)
        {
            if (lastGameInfo.EndGameChunkId > 0)
            {
                LogTo.Debug("The recording {0} {1} has finished in chunk {2}.", recording.Region, recording.GameId, lastGameInfo.EndGameChunkId);

                Result<RiotGameMetaData> metaDataResult = await this._leagueSpectatorApiClient.GetGameMetaData(recording.Region, recording.GameId);

                if (metaDataResult.IsError)
                {
                    LogTo.Error("Error while retrieving the game meta-data for recording {0} {1}: {2}", recording.Region, recording.GameId, metaDataResult.Message);

                    recording.ErrorCount++;
                    return;
                }

                recording.CreateTime = metaDataResult.Data.CreateTime;
                recording.StartTime = metaDataResult.Data.StartTime;
                recording.EndTime = metaDataResult.Data.EndTime;
                recording.GameLength = metaDataResult.Data.GameLength;
                recording.EndStartupChunkId = metaDataResult.Data.EndStartupChunkId;
                recording.StartGameChunkId = metaDataResult.Data.StartGameChunkId;
                recording.EndGameChunkId = metaDataResult.Data.EndGameChunkId;
                recording.EndGameKeyFrameId = metaDataResult.Data.EndGameKeyFrameId;
                recording.ChunkTimeInterval = metaDataResult.Data.ChunkTimeInterval;
                recording.KeyFrameTimeInterval = metaDataResult.Data.KeyFrameTimeInterval;
                recording.ClientAddedLag = metaDataResult.Data.ClientAddedLag;
                recording.DelayTime = metaDataResult.Data.DelayTime;
                recording.InterestScore = metaDataResult.Data.InterestScore;
            }
        }
        
        private async Task DeleteRecordingWithAllGameDataAsync(Recording recording)
        {
            Result deleteChunksResult = await this._gameDataStorage.DeleteChunksAsync(this.RecordingRequest.GameId, this.RecordingRequest.Region, recording.LatestChunkId);
            if (deleteChunksResult.IsError)
            {
                LogTo.Error("Error while deleting the chunks of recording {0} {1}: {2}", this.RecordingRequest.Region, this.RecordingRequest.GameId, deleteChunksResult.Message);
                return;
            }

            Result deleteKeyFramesResult = await this._gameDataStorage.DeleteKeyFramesAsync(this.RecordingRequest.GameId, this.RecordingRequest.Region, recording.LatestKeyFrameId);
            if (deleteKeyFramesResult.IsError)
            {
                LogTo.Error("Error while deleting the keyframes of recording {0} {1}: {2}", this.RecordingRequest.Region, this.RecordingRequest.GameId, deleteKeyFramesResult.Message);
                return;
            }

            Result deleteResult = await this._recordingStorage.DeleteRecordingAsync(this.RecordingRequest.GameId, this.RecordingRequest.Region);
            if (deleteResult.IsError)
            {
                LogTo.Error("Error while deleting the recording {0} {1}: {2}", this.RecordingRequest.Region, this.RecordingRequest.GameId, deleteResult.Message);
            }
        }
        
        private async Task SaveReplayAsync(Recording recording)
        {
            var record = new Replay
            {
                GameId = recording.GameId,
                Region = recording.Region,
                LeagueVersion = recording.LeagueVersion,
                SpectatorVersion = recording.SpectatorVersion,
                GameInformation = new GameInformation
                {
                    EndTime = recording.EndTime.Value,
                    GameLength = recording.GameLength.Value,
                    InterestScore = recording.InterestScore.Value,
                    StartTime = recording.StartTime.Value
                },
                ReplayInformation = new ReplayInformation
                {
                    ChunkTimeInterval = recording.ChunkTimeInterval.Value,
                    ClientAddedLag = recording.ClientAddedLag.Value,
                    CreateTime = recording.CreateTime.Value,
                    DelayTime = recording.DelayTime.Value,
                    EncryptionKey = recording.EncryptionKey,
                    EndGameChunkId = recording.EndGameChunkId.Value,
                    EndGameKeyFrameId = recording.EndGameKeyFrameId.Value,
                    EndStartupChunkId = recording.EndStartupChunkId.Value,
                    KeyFrameTimeInterval = recording.KeyFrameTimeInterval.Value,
                    StartGameChunkId = recording.StartGameChunkId.Value
                }
            };

            var saveResult = await this._replayStorage.SaveReplayAsync(record);

            if (saveResult.IsError)
            {
                LogTo.Error("Error while saving finished record {0} {1}: {2}", recording.Region, recording.GameId, saveResult.Message);
            }
        }

        private void TrySetState(GameRecorderState state)
        {
            if (this.State.HasEnded())
                return;

            this.State = state;
        }
        #endregion

        #region Implementation of IDisposable
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this._timer.Dispose();
        }
        #endregion
    }
}