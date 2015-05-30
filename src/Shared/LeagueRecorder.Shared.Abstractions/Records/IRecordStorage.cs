using System.IO;
using System.Threading.Tasks;
using LeagueRecorder.Shared.Abstractions.Results;

namespace LeagueRecorder.Shared.Abstractions.Records
{
    public interface IRecordStorage
    {
        Task<Result> SaveRecordAsync(Record record);
        Task<Result<Record>> GetRecordAsync(long gameId, Region region);
    }
}