using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeagueRecorder.Shared.Abstractions.Results;

namespace LeagueRecorder.Shared.Abstractions.Summoners
{
    public interface ISummonerStorage
    {
        Task<Result> SaveSummonerAsync(Summoner summoner);

        Task<Result<IList<Summoner>>> GetSummonersForInGameCheckAsync(Region[] availableRegions);
    }
}