using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Anotar.NLog;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.League;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Shared.Localization;
using Newtonsoft.Json.Linq;

namespace LeagueRecorder.Shared.Implementations.League
{
    public class LeagueSpectatorApiClient : ILeagueSpectatorApiClient
    {
        public Task<Result<Version>> GetSpectatorVersion(Region region)
        {
            return Result.CreateAsync(async () =>
            {
                var response = await this.GetClient(region)
                       .GetAsync("observer-mode/rest/consumer/version");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var version = Version.Parse(responseString);

                    return version;
                }
                else
                {
                    throw new ResultException(Messages.UnexpectedError);
                }
            });
        }

        public Task<Result<RiotGameMetaData>> GetGameMetaData(Region region, long gameId)
        {
            return Result.CreateAsync(async () =>
            {
                HttpResponseMessage response = await this.GetClient(region)
                       .GetAsync(string.Format("observer-mode/rest/consumer/getGameMetaData/{0}/{1}/1/token", region.SpectatorPlatformId, gameId));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseJson = JObject.Parse(responseString);

                    var gameMetaData = new RiotGameMetaData
                    {
                        EndGameChunkId = responseJson.Value<int>("endGameChunkId"),
                        EndGameKeyFrameId = responseJson.Value<int>("endGameKeyFrameId"),
                        EndStartupChunkId = responseJson.Value<int>("endStartupChunkId"),
                        GameId = responseJson.Value<JObject>("gameKey").Value<long>("gameId"),
                        GameLength = TimeSpan.FromMilliseconds(responseJson.Value<int>("gameLength")),
                        StartGameChunkId = responseJson.Value<int>("startGameChunkId"),
                        Region = region.ToString(),
                        ChunkTimeInterval = TimeSpan.FromMilliseconds(responseJson.Value<int>("chunkTimeInterval")),
                        ClientAddedLag = TimeSpan.FromMilliseconds(responseJson.Value<int>("clientAddedLag")),
                        CreateTime = DateTime.Parse(responseJson.Value<string>("createTime")),
                        DelayTime = TimeSpan.FromMilliseconds(responseJson.Value<int>("delayTime")),
                        EndTime = DateTime.Parse(responseJson.Value<string>("endTime")),
                        InterestScore = responseJson.Value<int>("interestScore"),
                        KeyFrameTimeInterval = TimeSpan.FromMilliseconds(responseJson.Value<int>("keyFrameTimeInterval")),
                        StartTime = DateTime.Parse(responseJson.Value<string>("startTime"))
                    };

                    return gameMetaData;
                }
                else
                {
                    throw new ResultException(Messages.UnexpectedError);
                }
            });
        }

        public Task<Result<RiotLastGameInfo>> GetLastGameInfo(Region region, long gameId)
        {
            return Result.CreateAsync(async () =>
            {
                var response = await this.GetClient(region)
                       .GetAsync(string.Format("observer-mode/rest/consumer/getLastChunkInfo/{0}/{1}/1/token", region.SpectatorPlatformId, gameId));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseJson = JObject.Parse(responseString);

                    var lastChunkInfo = new RiotLastGameInfo
                    {
                        CurrentChunkId = responseJson.Value<int>("chunkId"),
                        CurrentKeyFrameId = responseJson.Value<int>("keyFrameId"),
                        OriginalJsonResponse = responseString,
                        EndGameChunkId = responseJson.Value<int>("endGameChunkId")
                    };

                    return lastChunkInfo;
                }
                else
                {
                    throw new ResultException(Messages.UnexpectedError);
                }
            });
        }

        public Task<Result<RiotChunk>> GetChunk(Region region, long gameId, int chunkId)
        {
            return Result.CreateAsync(async () =>
            {
                HttpResponseMessage response = await this.GetClient(region)
                       .GetAsync(string.Format("observer-mode/rest/consumer/getGameDataChunk/{0}/{1}/{2}/token", region.SpectatorPlatformId, gameId, chunkId));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseData = await response.Content.ReadAsByteArrayAsync();

                    var chunk = new RiotChunk
                    {
                        Id = chunkId,
                        Data = responseData
                    };

                    return chunk;
                }
                else
                {
                    throw new ResultException(Messages.UnexpectedError);
                }
            });
        }

        public Task<Result<RiotKeyFrame>> GetKeyFrame(Region region, long gameId, int keyFrameId)
        {
            return Result.CreateAsync(async () =>
            {
                HttpResponseMessage response = await this.GetClient(region)
                       .GetAsync(string.Format("observer-mode/rest/consumer/getKeyFrame/{0}/{1}/{2}/token", region.SpectatorPlatformId, gameId, keyFrameId));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseData = await response.Content.ReadAsByteArrayAsync();

                    var keyFrame = new RiotKeyFrame
                    {
                        Id = keyFrameId,
                        Data = responseData
                    };

                    return keyFrame;
                }
                else
                {
                    throw new ResultException(Messages.UnexpectedError);
                }
            });
        }

        private HttpClient GetClient(Region region)
        {
            var client = HttpClientFactory.Create();
            client.BaseAddress = new Uri(string.Format("{0}://{1}:{2}/", Uri.UriSchemeHttp, region.SpectatorUrl, region.SpectatorPort));

            return client;
        }
    }
}