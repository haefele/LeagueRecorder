﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.League;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Shared.Abstractions.Summoners;
using LeagueRecorder.Worker.Api.Extensions;
using LiteGuard;

namespace LeagueRecorder.Worker.Api.Controllers
{
    public class SummonersController : BaseController
    {
        private readonly ILeagueApiClient _leagueApiClient;
        private readonly ISummonerStorage _summonerStorage;

        public SummonersController([NotNull]ILeagueApiClient leagueApiClient, [NotNull]ISummonerStorage summonerStorage)
        {
            Guard.AgainstNullArgument("leagueApiClient", leagueApiClient);
            Guard.AgainstNullArgument("summonerStorage", summonerStorage);

            this._leagueApiClient = leagueApiClient;
            this._summonerStorage = summonerStorage;
        }

        [HttpPost]
        [Route("Summoners/{region}/{summonerName}")]
        public async Task<HttpResponseMessage> AddSummonerAsync(string region, string summonerName)
        {
            Region actualRegion = Region.FromString(region);

            if (actualRegion == null || string.IsNullOrWhiteSpace(summonerName))
                return this.Request.GetMessage(HttpStatusCode.BadRequest);

            Result<RiotSummoner> summoner = await this._leagueApiClient.GetSummonerBySummonerNameAsync(actualRegion, summonerName);

            if (summoner.IsError)
                return this.Request.GetMessageWithError(HttpStatusCode.InternalServerError, summoner.Message);

            var summonerToStore = new Summoner
            {
                Region = actualRegion,
                SummonerId = summoner.Data.Id,
                Name = summoner.Data.Name,
                NextDateToCheckIfSummonerIsIngame = DateTimeOffset.UtcNow
            };
            await this._summonerStorage.SaveSummonerAsync(summonerToStore);

            var result = new
            {
                Region = actualRegion.ToString(),
                SummonerId = summonerToStore.SummonerId,
                SummonerName = summonerToStore.Name
            };

            return this.Request.GetMessageWithObject(HttpStatusCode.Created, result);
        }

        [HttpPost]
        [Route("Summoners/{region}/Challengers")]
        public async Task<HttpResponseMessage> AddChallengersAsync(string region)
        {
            Region actualRegion = Region.FromString(region);

            if (actualRegion == null)
                return this.Request.GetMessage(HttpStatusCode.BadRequest);

            Result<IList<RiotSummoner>> challengerResult = await _leagueApiClient.GetChallengerSummonersAsync(actualRegion);

            if (challengerResult.IsError)
                return this.Request.GetMessageWithError(HttpStatusCode.InternalServerError, challengerResult.Message);

            foreach (RiotSummoner summoner in challengerResult.Data)
            {
                var summonerToStore = new Summoner
                {
                    Region = actualRegion,
                    SummonerId = summoner.Id,
                    Name = summoner.Name,
                    NextDateToCheckIfSummonerIsIngame = DateTimeOffset.UtcNow,
                };

                var saveResult = await this._summonerStorage.SaveSummonerAsync(summonerToStore);

                if (saveResult.IsError)
                    return this.Request.GetMessageWithError(HttpStatusCode.InternalServerError, saveResult.Message);
            }

            return this.Request.GetMessage(HttpStatusCode.Created);
        }
    }
}