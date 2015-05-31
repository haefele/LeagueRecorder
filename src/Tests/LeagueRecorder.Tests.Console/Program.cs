using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Implementations.League;
using LeagueRecorder.Shared.Implementations.Recordings;
using LeagueRecorder.Shared.Implementations.Summoners;
using LeagueRecorder.Worker.SummonerInGameFinder;
using Microsoft.WindowsAzure.Storage;
using NHibernate.Tool.hbm2ddl;

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
            var summonerStorage = new SummonerStorage(sessionFactory, config);
            var recordingQueue = new RecordingQueue(cloudStorageAccount.CreateCloudQueueClient(), config);
            var recordingStorage = new RecordingStorage(cloudStorageAccount.CreateCloudTableClient(), config);

            var finder = new SummonerInGameFinderWorker(apiClient, summonerStorage, recordingQueue, recordingStorage, config);

            finder.StartAsync().Wait();

            var tokenSource = new CancellationTokenSource();
            finder.RunAsync(tokenSource.Token).Wait();
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
    }
}
