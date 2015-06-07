using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LeagueRecorder.Shared.Abstractions.GameData;
using LeagueRecorder.Shared.Abstractions.League;
using LeagueRecorder.Shared.Abstractions.Recordings;
using LeagueRecorder.Shared.Abstractions.Replays;
using LeagueRecorder.Shared.Abstractions.Summoners;
using LeagueRecorder.Shared.Implementations.GameData;
using LeagueRecorder.Shared.Implementations.League;
using LeagueRecorder.Shared.Implementations.Recordings;
using LeagueRecorder.Shared.Implementations.Replays;
using LeagueRecorder.Shared.Implementations.Summoners;

namespace LeagueRecorder.Azure.WorkerRole.Windsor
{
    public class ServiceInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IGameDataStorage>().ImplementedBy<GameDataStorage>().LifestyleSingleton(),
                Component.For<ILeagueApiClient>().ImplementedBy<LeagueApiClient>().LifestyleSingleton(),
                Component.For<ILeagueSpectatorApiClient>().ImplementedBy<LeagueSpectatorApiClient>().LifestyleSingleton(),
                Component.For<IRecordingQueue>().ImplementedBy<RecordingQueue>().LifestyleSingleton(),
                Component.For<IRecordingStorage>().ImplementedBy<RecordingStorage>().LifestyleSingleton(),
                Component.For<IReplayStorage>().ImplementedBy<ReplayStorage>().LifestyleSingleton(),
                Component.For<ISummonerStorage>().ImplementedBy<SummonerStorage>().LifestyleSingleton()
            );
        }
    }
}