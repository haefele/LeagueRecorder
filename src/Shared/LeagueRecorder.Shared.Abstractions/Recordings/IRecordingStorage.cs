using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LeagueRecorder.Shared.Abstractions.Results;

namespace LeagueRecorder.Shared.Abstractions.Recordings
{
    public interface IRecordingStorage
    {
        Task<Result> SaveRecordingAsync(Recording recording);
        Task<Result> SaveChunkAsync(long gameId, Region region, int chunkId, Stream chunk);
        Task<Result> SaveKeyFrameAsync(long gameId, Region region, int keyFrameId, Stream keyFrame);

        Task<Result<Recording>> GetRecordingAsync(long gameId, Region region);
        Task<Result<Stream>> GetChunkAsync(long gameId, Region region, int chunkId);
        Task<Result<Stream>> GetKeyFrameAsync(long gameId, Region region, int keyFrameId);

        Task<Result<IList<Recording>>> GetFinishedRecordingsAsync();
    }
}