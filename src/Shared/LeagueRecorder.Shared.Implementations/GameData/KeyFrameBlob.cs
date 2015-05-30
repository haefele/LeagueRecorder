using LeagueRecorder.Shared.Abstractions;

namespace LeagueRecorder.Shared.Implementations.GameData
{
    public class KeyFrameBlob
    {
        public static string ToBlobName(long gameId, Region region, int keyFrameId)
        {
            return string.Format("{0}/{1}/KeyFrames/{2}", region, gameId, keyFrameId);
        }
    }
}