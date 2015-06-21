using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Anotar.NLog;
using Castle.Windsor;
using Castle.Windsor.Installer;
using LeagueRecorder.Worker.Api;
using LeagueRecorder.Worker.Recorder;
using LeagueRecorder.Worker.SummonerInGameFinder;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using NLog;

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
            try
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
            catch (Exception exception)
            {
                LogTo.ErrorException("Exception while starting the cloud service.", exception);
                LogManager.Flush();

                throw;
            }
        }

        public override void OnStop()
        {
            try
            {
                this._cancellationTokenSource.Cancel();
            }
            catch (Exception exception)
            {
                LogTo.ErrorException("Exception while stopping the cloud service.", exception);
                LogManager.Flush();

                throw;
            }
        }

        public override void Run()
        {
            try
            {
                Task.WaitAll(
                    this._summonerInGameFinderWorker.RunAsync(this._cancellationTokenSource.Token),
                    this._recorderWorker.RunAsync(this._cancellationTokenSource.Token),
                    this._apiWorker.RunAsync(this._cancellationTokenSource.Token));
            }
            catch (Exception exception)
            {
                LogTo.ErrorException("Exception while running the cloud service.", exception);
                LogManager.Flush();

                throw;
            }
        }
    }
}
