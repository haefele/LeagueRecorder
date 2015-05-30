using LeagueRecorder.Shared.Abstractions;

namespace LeagueRecorder.Shared.Implementations.GameData
{
    internal class ChunkBlob
    {
        public static string ToBlobName(long gameId, Region region, int chunkId)
        {
            return string.Format("{0}/{1}/Chunks/{2}", region, gameId, chunkId);
        }
    }
}