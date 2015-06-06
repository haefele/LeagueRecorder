using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Anotar.NLog;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.League;
using LeagueRecorder.Shared.Abstractions.Recordings;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Shared.Abstractions.Summoners;
using LiteGuard;

namespace LeagueRecorder.Worker.SummonerInGameFinder
{
    public class SummonerInGameFinderWorker : IWorker
    {
        private readonly ILeagueApiClient _apiClient;
        private readonly ISummonerStorage _summonerStorage;
        private readonly IRecordingQueue _recordingQueue;
        private readonly IRecordingStorage _recordingStorage;
        private readonly IConfig _config;

        public SummonerInGameFinderWorker([NotNull]ILeagueApiClient apiClient, [NotNull]ISummonerStorage summonerStorage, [NotNull]IRecordingQueue recordingQueue, [NotNull]IRecordingStorage recordingStorage, [NotNull]IConfig config)
        {
            Guard.AgainstNullArgument("apiClient", apiClient);
            Guard.AgainstNullArgument("summonerStorage", summonerStorage);
            Guard.AgainstNullArgument("recordingQueue", recordingQueue);
            Guard.AgainstNullArgument("recordingStorage", recordingStorage);
            Guard.AgainstNullArgument("config", config);

            this._apiClient = apiClient;
            this._summonerStorage = summonerStorage;
            this._recordingQueue = recordingQueue;
            this._recordingStorage = recordingStorage;
            this._config = config;
        }

        public Task StartAsync()
        {
            return Task.FromResult((object)null);
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                Region[] availableRegions = this.GetAvailableRegions();
                Result<IList<Summoner>> summonersToCheck = await this._summonerStorage.GetSummonersForInGameCheckAsync(availableRegions);

                if (summonersToCheck.IsError)
                { 
                    LogTo.Debug("Error while retrieving the summoners to check if they are ingame: {0}", summonersToCheck.Message);
                }
                else
                {
                    var tasks = summonersToCheck.Data.Select(this.CheckIfSummonerIsIngameAsync);
                    await Task.WhenAll(tasks);
                }

                await Task.Delay(TimeSpan.FromSeconds(this._config.IntervalToCheckIfSummonersAreIngame));
            }
        }

        private async Task CheckIfSummonerIsIngameAsync(Summoner summoner)
        {
            summoner.NextDateToCheckIfSummonerIsIngame = DateTimeOffset.UtcNow.AddSeconds(this._config.IntervalToCheckIfOneSummonerIsIngame);

            var saveSummonerResult = await this._summonerStorage.SaveSummonerAsync(summoner);
            if (saveSummonerResult.IsError)
            {
                LogTo.Error("Error while saving the summoner {0} {1}: {2}", summoner.Region, summoner.SummonerId, saveSummonerResult.Message);
                return;
            }

            var currentGameResult = await this._apiClient.GetCurrentGameAsync(summoner.Region, summoner.SummonerId);
            if (currentGameResult.IsError)
            {
                LogTo.Debug("Failed to check if summoner {0} {1} is ingame: {2}", summoner.Region, summoner.SummonerId, currentGameResult.Message);
                return;
            }

            var isGameRecordingResult = await this._recordingStorage.IsGameRecording(currentGameResult.Data.GameId, summoner.Region);
            if (isGameRecordingResult.IsSuccess)
            {
                LogTo.Info("The game {0} {1} is already beeing recorded.", summoner.Region, currentGameResult.Data.GameId);
                return;
            }

            var recording = new Recording
            {
                GameId = currentGameResult.Data.GameId,
                Region = summoner.Region,
                EncryptionKey = currentGameResult.Data.EncryptionKey
            };

            var saveResult = await this._recordingStorage.SaveNewRecordingAsync(recording);
            if (saveResult.IsError)
            {
                LogTo.Error("Error while saving a recording: {0}", saveResult.Message);
                return;
            }

            var recordingRequest = new RecordingRequest
            {
                GameId = recording.GameId,
                Region = recording.Region
            };

            var queueResult = await this._recordingQueue.EnqueueAsync(recordingRequest);
            if (queueResult.IsError)
            {
                LogTo.Error("Error while saving a recording request in the queue: {0}", queueResult.Message);
                return;
            }

            LogTo.Info("Recording game {0} {1}.", recording.Region, recording.GameId);
        }

        private Region[] GetAvailableRegions()
        {
            return Region.All;
        }
    }
}
