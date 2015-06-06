using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions.Results;

namespace LeagueRecorder.Shared.Abstractions.Recordings
{
    public interface IRecordingStorage
    {
        Task<Result> IsGameRecording(long gameId, [NotNull]Region region);

        Task<Result> SaveNewRecordingAsync([NotNull] Recording recording);
        Task<Result> SaveRecordingAsync([NotNull]Recording recording);
        Task<Result<Recording>> GetRecordingAsync(long gameId, [NotNull]Region region);

        Task<Result<IList<Recording>>> GetFinishedRecordingsAsync();
        Task<Result> DeleteRecordingAsync(long gameId, [NotNull]Region region);
    }
}