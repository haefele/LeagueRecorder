using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Implementations.GameData;
using LeagueRecorder.Shared.Implementations.League;
using LeagueRecorder.Shared.Implementations.Recordings;
using LeagueRecorder.Shared.Implementations.Records;
using LeagueRecorder.Shared.Implementations.Summoners;
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
            var recordStorage = new RecordStorage(sessionFactory);
            var recordingQueue = new RecordingQueue(cloudStorageAccount.CreateCloudQueueClient(), config);
            var recordingStorage = new RecordingStorage(cloudStorageAccount.CreateCloudTableClient(), config);
            var gameDataStorage = new GameDataStorage(cloudStorageAccount.CreateCloudBlobClient(), config);

            var finder = new SummonerInGameFinderWorker(apiClient, summonerStorage, recordingQueue, recordingStorage, config);
            finder.StartAsync().Wait();

            var recorder = new RecorderWorker(recordingQueue, apiClient, spectatorApiClient, recordingStorage, gameDataStorage, recordStorage, config);
            recorder.StartAsync().Wait();
                        
            var tokenSource = new CancellationTokenSource();
            
            finder.RunAsync(tokenSource.Token);
            recorder.RunAsync(tokenSource.Token);

            System.Console.ReadLine();
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
            get { return 20; }
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
    }
}
