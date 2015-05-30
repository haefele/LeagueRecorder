using System.Threading;
using System.Threading.Tasks;

namespace LeagueRecorder.Shared.Abstractions
{
    public interface IWorker
    {
        void OnStart();

        Task Run(CancellationToken token);

        void OnStop();
    }
}