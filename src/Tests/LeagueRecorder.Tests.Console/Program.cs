using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.GameData;
using LeagueRecorder.Shared.Abstractions.League;
using LeagueRecorder.Shared.Abstractions.Replays;
using LeagueRecorder.Shared.Abstractions.Summoners;
using LeagueRecorder.Shared.Implementations.GameData;
using LeagueRecorder.Shared.Implementations.League;
using LeagueRecorder.Shared.Implementations.Recordings;
using LeagueRecorder.Shared.Implementations.Replays;
using LeagueRecorder.Shared.Implementations.Summoners;
using LeagueRecorder.Worker.Api;
using LeagueRecorder.Worker.Api.Controllers;
using LeagueRecorder.Worker.SummonerInGameFinder;
using Microsoft.WindowsAzure.Storage;
using NHibernate.Tool.hbm2ddl;
using LeagueRecorder.Worker.Recorder;

namespace LeagueRecorder.Tests.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse("");

            var sessionFactory = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012.ConnectionString(f => f.Is("")))
                .Mappings(f => f.FluentMappings.AddFromAssemblyOf<SummonerEntity>())
                .BuildSessionFactory();

            var config = new InMemoryConfig();

            var apiClient = new LeagueApiClient(config);
            var spectatorApiClient = new LeagueSpectatorApiClient();
            var summonerStorage = new SummonerStorage(sessionFactory, config);
            var recordStorage = new ReplayStorage(sessionFactory);
            var recordingQueue = new RecordingQueue(cloudStorageAccount.CreateCloudQueueClient(), config);
            var recordingStorage = new RecordingStorage(cloudStorageAccount.CreateCloudTableClient(), config);
            var gameDataStorage = new GameDataStorage(cloudStorageAccount.CreateCloudBlobClient(), config);

            var container = new WindsorContainer();
            container.Register(
                Component.For<IReplayStorage>().Instance(recordStorage).LifestyleSingleton(),
                Component.For<ISummonerStorage>().Instance(summonerStorage).LifestyleSingleton(),
                Component.For<ILeagueApiClient>().Instance(apiClient).LifestyleSingleton(),
                Component.For<IGameDataStorage>().Instance(gameDataStorage).LifestyleSingleton());
            container.Register(Classes
                .FromAssemblyContaining<BaseController>()
                .BasedOn<BaseController>()
                .WithServiceSelf()
                .LifestyleScoped());

            var finder = new SummonerInGameFinderWorker(apiClient, summonerStorage, recordingQueue, recordingStorage, config);
            finder.StartAsync().Wait();

            var recorder = new RecorderWorker(recordingQueue, apiClient, spectatorApiClient, recordingStorage, gameDataStorage, recordStorage, config);
            recorder.StartAsync().Wait();

            var api = new ApiWorker(container, config);
            api.StartAsync().Wait();

            var tokenSource = new CancellationTokenSource();
            
            var task1 = finder.RunAsync(tokenSource.Token);
            var task2 = recorder.RunAsync(tokenSource.Token);
            var task3 = recorder.RunAsync(tokenSource.Token);

            System.Console.ReadLine();
            System.Console.WriteLine("Stopping");

            tokenSource.Cancel();

            Task.WaitAll(task1, task2, task3);
        }
    }

    public class InMemoryConfig : IConfig
    {
        public string GameDataContainerName
        {
            get { return "games"; }
        }

        public string RecordingTableName
        {
            get { return "recordings"; }
        }

        public string RecordingQueueName
        {
            get { return "recordings"; }
        }

        public int CountOfSummonersToCheckIfIngameAtOnce
        {
            get { return 50; }
        }

        public string RiotApiKey
        {
            get { return ""; }
        }

        public int IntervalToCheckIfOneSummonerIsIngame
        {
            get { return 300; }
        }

        public int IntervalToCheckIfSummonersAreIngame
        {
            get { return 5; }
        }
        
        public int RecordingMaxErrorCount
        {
            get { return 10; }
        }

        public string Url
        {
            get { return "http://localhost/"; }
        }

        public bool CompressResponses
        {
            get { return true; }
        }
    }
}
