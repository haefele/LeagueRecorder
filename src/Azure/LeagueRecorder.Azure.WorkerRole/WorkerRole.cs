using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using Castle.Windsor.Installer;
using LeagueRecorder.Worker.Api;
using LeagueRecorder.Worker.Recorder;
using LeagueRecorder.Worker.SummonerInGameFinder;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace LeagueRecorder.Azure.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private CancellationTokenSource _cancellationTokenSource;

        private SummonerInGameFinderWorker _summonerInGameFinderWorker;
        private RecorderWorker _recorderWorker;
        private ApiWorker _apiWorker;

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 256;

            var container = new WindsorContainer();
            container.Install(FromAssembly.This());

            this._cancellationTokenSource = new CancellationTokenSource();

            this._summonerInGameFinderWorker = container.Resolve<SummonerInGameFinderWorker>();
            this._recorderWorker = container.Resolve<RecorderWorker>();
            this._apiWorker = container.Resolve<ApiWorker>();

            Task.WaitAll(
                this._summonerInGameFinderWorker.StartAsync(),
                this._recorderWorker.StartAsync(),
                this._apiWorker.StartAsync());

            return true;
        }

        public override void OnStop()
        {
            this._cancellationTokenSource.Cancel();
        }

        public override void Run()
        {
            Task.WaitAll(
                this._summonerInGameFinderWorker.RunAsync(this._cancellationTokenSource.Token),
                this._recorderWorker.RunAsync(this._cancellationTokenSource.Token),
                this._apiWorker.RunAsync(this._cancellationTokenSource.Token));
        }
    }
}
