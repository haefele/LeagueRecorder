using System.Threading.Tasks;
using LeagueRecorder.Shared.Abstractions.Results;

namespace LeagueRecorder.Shared.Abstractions.Replays
{
    public interface IReplayStorage
    {
        Task<Result> SaveReplayAsync(Replay replay);
        Task<Result<Replay>> GetReplayAsync(long gameId, Region region);
    }
}