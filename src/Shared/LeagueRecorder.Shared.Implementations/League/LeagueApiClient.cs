using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Anotar.NLog;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.League;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Shared.Localization;
using LiteGuard;
using Newtonsoft.Json.Linq;

namespace LeagueRecorder.Shared.Implementations.League
{
    public class LeagueApiClient : ILeagueApiClient
    {
        #region Fields
        private readonly IConfig _config;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LeagueApiClient"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        public LeagueApiClient([NotNull]IConfig config)
        {
            Guard.AgainstNullArgument("config", config);

            this._config = config;
        }
        #endregion

        #region Methods
        public Task<Result<Version>> GetLeagueVersion(Region region)
        {
            Guard.AgainstNullArgument("region", region);

            return Result.CreateAsync(async () =>
            {
                HttpResponseMessage response = await this.GetClient()
                    .GetAsync(string.Format("api/lol/static-data/{0}/v1.2/versions", region.RiotApiPlatformId));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseJson = JArray.Parse(responseString);

                    return responseJson.Values().Select(f => Version.Parse(f.Value<string>())).Max();
                }
                else
                {
                    throw new ResultException(Messages.UnexpectedError);
                }
            });
        }
        public Task<Result<RiotSummoner>> GetSummonerBySummonerNameAsync(Region region, string summonerName)
        {
            Guard.AgainstNullArgument("region", region);
            Guard.AgainstNullArgument("summonerName", summonerName);

            return Result.CreateAsync(async () =>
            {
                HttpResponseMessage response = await this.GetClient(region)
                    .GetAsync(string.Format("api/lol/{0}/v1.4/summoner/by-name/{1}", region.RiotApiPlatformId, summonerName));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseJson = JObject.Parse(responseString).First.First;

                    var summoner = new RiotSummoner
                    {
                        Id = responseJson.Value<long>("id"),
                        Name = responseJson.Value<string>("name"),
                        Region = region.ToString()
                    };

                    return summoner;
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ResultException(Messages.SummonerNotFound);
                }
                else if (response.StatusCode == (HttpStatusCode)429)
                {
                    throw new ResultException(Messages.RateLimitExceeded);
                }
                else
                {
                    throw new ResultException(Messages.UnexpectedError);
                }
            });
        }
        public Task<Result<RiotSpectatorGameInfo>> GetCurrentGameAsync(Region region, long summonerId)
        {
            Guard.AgainstNullArgument("region", region);

            return Result.CreateAsync(async () =>
            {
                HttpResponseMessage response = await this.GetClient(region)
                       .GetAsync(string.Format("observer-mode/rest/consumer/getSpectatorGameInfo/{0}/{1}", region.SpectatorPlatformId, summonerId));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseJson = JObject.Parse(responseString);

                    var gameInfo = new RiotSpectatorGameInfo
                    {
                        GameId = responseJson.Value<long>("gameId"),
                        GameLength = TimeSpan.FromSeconds(responseJson.Value<int>("gameLength")),
                        Region = region.ToString(),
                        EncryptionKey = responseJson.Value<JObject>("observers").Value<string>("encryptionKey"),
                        Participants = responseJson.Value<JArray>("participants")
                            .OfType<JObject>()
                            .Select(f => new RiotGameParticipant
                                {
                                    ChampionId = f.Value<long>("championId"),
                                    SummonerId = f.Value<long>("summonerId")
                                })
                            .ToList()
                    };

                    return gameInfo;
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ResultException(Messages.SummonerNotInGame);
                }
                else if (response.StatusCode == (HttpStatusCode)429)
                {
                    throw new ResultException(Messages.RateLimitExceeded);
                }
                else
                {
                    throw new ResultException(Messages.UnexpectedError);
                }
            });
        }
        public Task<Result<IList<RiotSummoner>>> GetChallengerSummonersAsync(Region region)
        {
            Guard.AgainstNullArgument("region", region);

            return Result.CreateAsync(async () =>
            {
                var response = await this.GetClient(region)
                       .GetAsync(string.Format("/api/lol/{0}/v2.5/league/challenger?type=RANKED_SOLO_5x5", region.RiotApiPlatformId));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseJson = JObject.Parse(responseString).Value<JArray>("entries");

                    IList<RiotSummoner> result = responseJson
                        .OfType<JObject>()
                        .Select(item => new RiotSummoner
                        {
                            Id = item.Value<long>("playerOrTeamId"),
                            Name = item.Value<string>("playerOrTeamName"),
                            Region = region.ToString()
                        })
                        .ToList();

                    return result;
                }
                else if (response.StatusCode == (HttpStatusCode)429)
                {
                    throw new ResultException(Messages.RateLimitExceeded);
                }
                else
                {
                    throw new ResultException(Messages.UnexpectedError);
                }
            });
        }
        #endregion

        #region Private Methods
        private HttpClient GetClient([CanBeNull]Region region = null)
        {
            var client = HttpClientFactory.Create(new ApiKeyMessageHandler(this._config.RiotApiKey));
            client.BaseAddress = new Uri(string.Format("{0}://{1}.api.pvp.net/", Uri.UriSchemeHttps, region != null ? region.RiotApiPlatformId : "global"));

            return client;
        }
        #endregion
    }
}