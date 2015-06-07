using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.GameData;
using LeagueRecorder.Shared.Abstractions.Recordings;
using LeagueRecorder.Shared.Abstractions.Records;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Worker.Api.Extensions;
using LiteGuard;
using Newtonsoft.Json;

namespace LeagueRecorder.Worker.Api.Controllers
{
    public class RiotApiController : BaseController
    {
        private readonly IRecordStorage _recordingStorage;
        private readonly IGameDataStorage _gameDataStorage;

        public RiotApiController([NotNull]IRecordStorage recordingStorage, [NotNull]IGameDataStorage gameDataStorage)
        {
            Guard.AgainstNullArgument("RecordingStorage", recordingStorage);
            Guard.AgainstNullArgument("gameDataStorage", gameDataStorage);

            this._recordingStorage = recordingStorage;
            this._gameDataStorage = gameDataStorage;
        }

        [HttpGet]
        [Route("observer-mode/rest/consumer/version")]
        public HttpResponseMessage GetVersion()
        {
            var message = this.Request.GetMessage(HttpStatusCode.OK);
            message.Content = new StringContent("1.82.89");

            return message;
        }

        [HttpGet]
        [Route("observer-mode/rest/consumer/getGameMetaData/{spectatorRegionId}/{gameId}/{token}/token")]
        public async Task<HttpResponseMessage> GetGameMetaData(string spectatorRegionId, long gameId)
        {
            var region = Region.FromString(spectatorRegionId);

            Result<Record> recordingResult = await this._recordingStorage.GetRecordAsync(gameId, region);

            if (recordingResult.IsError)
                return this.Request.GetMessage(HttpStatusCode.NotFound);

            this.RememberToReturnStartupChunkInfo(this.Request, recordingResult.Data);

            var metaData = new
            {
                gameKey = new
                {
                    gameId = recordingResult.Data.GameId,
                    PlatformID = region.SpectatorPlatformId
                },
                gameServerAddress = string.Empty,
                port = 0,
                encryptionKey = string.Empty,
                chunkTimeInterval = recordingResult.Data.ReplayInformation.ChunkTimeInterval.TotalMilliseconds,
                startTime = recordingResult.Data.GameInformation.StartTime.ToLeagueTime(),
                endTime = recordingResult.Data.GameInformation.EndTime.ToLeagueTime(),
                gameEnded = false,
                lastChunkId = -1,
                lastKeyFrameId = -1,
                endStartupChunkId = 0,
                delayTime = recordingResult.Data.ReplayInformation.DelayTime.TotalMilliseconds,
                pendingAvailableChunkInfo = (object)null,
                pendingAvailableKeyFrameInfo = (object)null,
                keyFrameTimeInterval = recordingResult.Data.ReplayInformation.KeyFrameTimeInterval.TotalMilliseconds,
                decodedEncryptionKey = string.Empty,
                startGameChunkId = 0,
                gameLength = 0,
                clientAddedLag = recordingResult.Data.ReplayInformation.ChunkTimeInterval.TotalMilliseconds,
                clientBackFetchingEnabled = false,
                clientBackFetchingFreq = 1000,
                interestScore = recordingResult.Data.GameInformation.InterestScore,
                featuredGame = false,
                createTime = recordingResult.Data.ReplayInformation.CreateTime.ToLeagueTime(),
                endGameChunkId = -1,
                endGameKeyFrameId = -1,
            };

            var result = this.Request.GetMessage(HttpStatusCode.OK);
            result.Content = new StringContent(JsonConvert.SerializeObject(metaData), Encoding.UTF8, "application/json");

            return result;
        }

        [HttpGet]
        [Route("observer-mode/rest/consumer/getLastChunkInfo/{spectatorRegionId}/{gameId}/{token}/token")]
        public async Task<HttpResponseMessage> GetLastChunkInfo(string spectatorRegionId, long gameId)
        {
            var region = Region.FromString(spectatorRegionId);

            Result<Record> recordingResult = await this._recordingStorage.GetRecordAsync(gameId, region);

            if (recordingResult.IsError)
                return this.Request.GetMessage(HttpStatusCode.NotFound);

            var returnStartupChunkInfo = this.ShouldReturnStartupChunkInfo(this.Request, recordingResult.Data);

            var lastChunkInfo = new
            {
                chunkId = returnStartupChunkInfo
                    ? recordingResult.Data.ReplayInformation.StartGameChunkId
                    : recordingResult.Data.ReplayInformation.EndGameChunkId,
                availableSince = 30000,
                nextAvailableChunk = returnStartupChunkInfo
                    ? 1000
                    : 0,
                keyFrameId = returnStartupChunkInfo
                    ? 1
                    : recordingResult.Data.ReplayInformation.EndGameKeyFrameId,
                nextChunkId = returnStartupChunkInfo
                    ? recordingResult.Data.ReplayInformation.StartGameChunkId
                    : recordingResult.Data.ReplayInformation.EndGameChunkId,
                endStartupChunkId = recordingResult.Data.ReplayInformation.EndStartupChunkId,
                startGameChunkId = recordingResult.Data.ReplayInformation.StartGameChunkId,
                endGameChunkId = returnStartupChunkInfo
                    ? 0
                    : recordingResult.Data.ReplayInformation.EndGameChunkId,
                duration = 30000
            };

            var result = this.Request.GetMessage(HttpStatusCode.OK);
            result.Content = new StringContent(JsonConvert.SerializeObject(lastChunkInfo), Encoding.UTF8, "application/json");

            return result;
        }

        [HttpGet]
        [Route("observer-mode/rest/consumer/getGameDataChunk/{spectatorRegionId}/{gameId}/{chunkId}/token")]
        public async Task<HttpResponseMessage> GetChunk(string spectatorRegionId, long gameId, int chunkId)
        {
            var region = Region.FromString(spectatorRegionId);

            Result<Record> recordingResult = await this._recordingStorage.GetRecordAsync(gameId, region);

            if (recordingResult.IsError)
                return this.Request.GetMessage(HttpStatusCode.NotFound);

            Result<Stream> chunkResult = await this._gameDataStorage.GetChunkAsync(gameId, region, chunkId);

            if (chunkResult.IsError)
                return this.Request.GetMessage(HttpStatusCode.NotFound);

            var result = this.Request.GetMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(chunkResult.Data);

            return result;
        }

        [HttpGet]
        [Route("observer-mode/rest/consumer/getKeyFrame/{spectatorRegionId}/{gameId}/{keyFrameId}/token")]
        public async Task<HttpResponseMessage> GetKeyFrame(string spectatorRegionId, long gameId, int keyFrameId)
        {
            var region = Region.FromString(spectatorRegionId);

            Result<Record> recordingResult = await this._recordingStorage.GetRecordAsync(gameId, region);

            if (recordingResult.IsError)
                return this.Request.GetMessage(HttpStatusCode.NotFound);

            Result<Stream> keyFrameResult = await this._gameDataStorage.GetKeyFrameAsync(gameId, region, keyFrameId);

            if (keyFrameResult.IsError)
                return this.Request.GetMessage(HttpStatusCode.NotFound);

            var result = this.Request.GetMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(keyFrameResult.Data);

            return result;
        }

        #region Private Methods
        private static readonly Dictionary<Tuple<Region, long, string>, int> _clientsThatNeedStartupChunkInfo = new Dictionary<Tuple<Region, long, string>, int>();

        private void RememberToReturnStartupChunkInfo(HttpRequestMessage request, Record recording)
        {
            var clientIp = request.GetOwinContext().Request.RemoteIpAddress;
            var client = Tuple.Create(recording.Region, recording.GameId, clientIp);

            _clientsThatNeedStartupChunkInfo[client] = 0;
        }

        private bool ShouldReturnStartupChunkInfo(HttpRequestMessage request, Record recording)
        {
            var clientIp = request.GetOwinContext().Request.RemoteIpAddress;
            var client = Tuple.Create(recording.Region, recording.GameId, clientIp);

            bool returnStartupChunkInfo = _clientsThatNeedStartupChunkInfo.ContainsKey(client);

            if (returnStartupChunkInfo)
            {
                _clientsThatNeedStartupChunkInfo[client]++;

                double amountOfChunksTheLoadingScreenTook = recording.ReplayInformation.StartGameChunkId - recording.ReplayInformation.EndStartupChunkId - 1;
                int estimatedTimeInSecondsOnLoadingScreen = (int)(amountOfChunksTheLoadingScreenTook * recording.ReplayInformation.ChunkTimeInterval.TotalSeconds);

                if (_clientsThatNeedStartupChunkInfo[client] >= estimatedTimeInSecondsOnLoadingScreen)
                {
                    _clientsThatNeedStartupChunkInfo.Remove(client);
                }
            }
            return returnStartupChunkInfo;
        }
        #endregion
    }
}