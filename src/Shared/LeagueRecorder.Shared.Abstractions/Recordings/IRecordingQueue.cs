using System.Threading.Tasks;
using LeagueRecorder.Shared.Abstractions.Results;

namespace LeagueRecorder.Shared.Abstractions.Recordings
{
    public interface IRecordingQueue
    {
        Task<Result> EnqueueAsync(RecordingRequest recording);

        Task<Result<RecordingRequest>> DequeueAsync();

        Task<Result> RemoveAsync(RecordingRequest request);
    }
}