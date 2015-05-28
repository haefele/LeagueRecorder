using System.IO;
using System.Threading.Tasks;
using LeagueRecorder.Shared.Abstractions.Results;

namespace LeagueRecorder.Shared.Abstractions.Records
{
    public interface IRecordStorage
    {
        Task<Result> SaveRecordAsync(Record record);
        Task<Result> SaveChunkAsync(long gameId, Region region, int chunkId, Stream chunk);
        Task<Result> SaveKeyFrameAsync(long gameId, Region region, int keyFrameId, Stream keyFrame);

        Task<Result<Record>> GetRecordAsync(long gameId, Region region);
        Task<Result<Stream>> GetChunkAsync(long gameId, Region region, int chunkId);
        Task<Result<Stream>> GetKeyFrameAsync(long gameId, Region region, int chunkId);
    }
}