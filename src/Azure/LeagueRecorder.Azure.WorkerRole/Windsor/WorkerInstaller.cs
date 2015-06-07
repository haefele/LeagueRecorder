using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LeagueRecorder.Worker.Api;
using LeagueRecorder.Worker.Recorder;
using LeagueRecorder.Worker.SummonerInGameFinder;

namespace LeagueRecorder.Azure.WorkerRole.Windsor
{
    public class WorkerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<SummonerInGameFinderWorker>().LifestyleSingleton(),
                Component.For<RecorderWorker>().LifestyleSingleton(),
                Component.For<ApiWorker>().LifestyleSingleton());
        }
    }
}