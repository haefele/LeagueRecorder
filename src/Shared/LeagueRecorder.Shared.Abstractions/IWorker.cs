using System.Threading;
using System.Threading.Tasks;

namespace LeagueRecorder.Shared.Abstractions
{
    public interface IWorker
    {
        Task StartAsync();

        Task RunAsync(CancellationToken token);
    }
}