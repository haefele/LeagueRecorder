using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions.Results;

namespace LeagueRecorder.Shared.Abstractions.GameData
{
    public interface IGameDataStorage
    {
        Task<Result> SaveChunkAsync(long gameId, [NotNull]Region region, int chunkId, [NotNull]byte[] chunk);
        Task<Result> SaveKeyFrameAsync(long gameId, [NotNull]Region region, int keyFrameId, [NotNull]byte[] keyFrame);

        Task<Result<Stream>> GetChunkAsync(long gameId, [NotNull]Region region, int chunkId);
        Task<Result<Stream>> GetKeyFrameAsync(long gameId, [NotNull]Region region, int keyFrameId);

        Task<Result> DeleteChunksAsync(long gameId, [NotNull]Region region, int latestChunkId);
        Task<Result> DeleteKeyFramesAsync(long gameId, [NotNull] Region region, int latestKeyFrameId);
    }
}