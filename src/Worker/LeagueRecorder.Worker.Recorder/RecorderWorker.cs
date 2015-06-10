using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public class RecorderWorker : IWorker
    {
        private readonly IRecordingQueue _recordingQueue;
        private readonly ILeagueApiClient _leagueApiClient;
        private readonly ILeagueSpectatorApiClient _spectatorApiClient;
        private readonly IRecordingStorage _recordingStorage;
        private readonly IGameDataStorage _gameDataStorage;
        private readonly IReplayStorage _replayStorage;
        private readonly IConfig _config;

        private readonly ConcurrentDictionary<GameRecorder, object> _gameRecorders;

        public RecorderWorker([NotNull]IRecordingQueue recordingQueue, [NotNull]ILeagueApiClient leagueApiClient, [NotNull]ILeagueSpectatorApiClient spectatorApiClient, [NotNull]IRecordingStorage recordingStorage, [NotNull]IGameDataStorage gameDataStorage, [NotNull]IReplayStorage replayStorage, [NotNull]IConfig config)
        {
            Guard.AgainstNullArgument("recordingQueue", recordingQueue);
            Guard.AgainstNullArgument("leagueApiClient", leagueApiClient);
            Guard.AgainstNullArgument("spectatorApiClient", spectatorApiClient);
            Guard.AgainstNullArgument("recordingStorage", recordingStorage);
            Guard.AgainstNullArgument("gameDataStorage", gameDataStorage);
            Guard.AgainstNullArgument("ReplayStorage", replayStorage);
            Guard.AgainstNullArgument("config", config);

            this._recordingQueue = recordingQueue;
            this._leagueApiClient = leagueApiClient;
            this._spectatorApiClient = spectatorApiClient;
            this._recordingStorage = recordingStorage;
            this._gameDataStorage = gameDataStorage;
            this._replayStorage = replayStorage;
            this._config = config;

            this._gameRecorders = new ConcurrentDictionary<GameRecorder, object>();
        }

        public Task StartAsync()
        {
            return Task.FromResult((object)null);
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                Result<RecordingRequest> recording = await this._recordingQueue.DequeueAsync();

                if (recording.IsError)
                {
                    LogTo.Error("Error while reading from the queue: {0}", recording.Message);

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                else
                {
                    var gameRecorder = new GameRecorder(recording.Data, this._leagueApiClient, this._spectatorApiClient, this._recordingStorage, this._gameDataStorage, this._replayStorage, this._config);
                    this._gameRecorders.TryAdd(gameRecorder, null);

                    await this._recordingQueue.RemoveAsync(recording.Data);

                    LogTo.Info("Recording {0} {1}.", recording.Data.Region, recording.Data.GameId);
                }

                foreach (var recorder in this._gameRecorders.Keys)
                {
                    if (recorder.State.HasEnded())
                    {
                        object output;
                        this._gameRecorders.TryRemove(recorder, out output);

                        recorder.Dispose();
                    }
                }
            }

            foreach (var runningRecording in this._gameRecorders.Keys)
            {
                if (runningRecording.State == GameRecorderState.Running)
                    await this._recordingQueue.EnqueueAsync(runningRecording.RecordingRequest);

                object output;
                this._gameRecorders.TryRemove(runningRecording, out output);
            }
        }
    }
}