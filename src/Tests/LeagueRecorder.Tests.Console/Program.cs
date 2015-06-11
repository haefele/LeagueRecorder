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
using Microsoft.WindowsAzure.Storage.Blob;
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
                .ExposeConfiguration(f => new SchemaUpdate(f).Execute(true, true))
                .BuildSessionFactory();
            
            var config = new InMemoryConfig();

            var apiClient = new LeagueApiClient(config);
            var spectatorApiClient = new LeagueSpectatorApiClient();
            var summonerStorage = new SummonerStorage(sessionFactory, config);
            var recordStorage = new ReplayStorage(sessionFactory);
            var recordingQueue = new RecordingQueue(cloudStorageAccount.CreateCloudQueueClient(), config);
            var recordingStorage = new RecordingStorage(cloudStorageAccount.CreateCloudTableClient(), config);
            var gameDataStorage = new GameDataStorage(cloudStorageAccount.CreateCloudBlobClient(), config);

            var version = apiClient.GetLeagueVersion(Region.EuropeWest).Result;
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

        public string SqlServerConnectionString
        {
            get { return string.Empty; }
        }

        public string AzureStorageConnectionString
        {
            get { return string.Empty; }
        }

        public bool RecordGames
        {
            get { return true; }
        }
    }
}
