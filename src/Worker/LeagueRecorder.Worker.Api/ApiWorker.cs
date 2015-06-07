using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using Anotar.NLog;
using Castle.Windsor;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Worker.Api.Configuration;
using LeagueRecorder.Worker.Api.Windsor;
using LiteGuard;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using NLog.Config;
using Owin;

namespace LeagueRecorder.Worker.Api
{
    public class ApiWorker : IWorker
    {
        #region Fields
        private readonly IWindsorContainer _windsorContainer;
        private readonly IConfig _config;
        private IDisposable _webApp;
        #endregion

        #region Constructors
        public ApiWorker([NotNull]IWindsorContainer windsorContainer, [NotNull]IConfig config)
        {
            Guard.AgainstNullArgument("windsorContainer", windsorContainer);
            Guard.AgainstNullArgument("config", config);

            this._windsorContainer = windsorContainer;
            this._config = config;
        }
        #endregion

        #region Methods
        public Task StartAsync()
        {
            var startOptions = new StartOptions(this._config.Url);
            this._webApp = WebApp.Start(startOptions, this.StartHttpApi);

            return Task.FromResult((object)null);
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            if (this._webApp != null)
                this._webApp.Dispose();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Configures the web application.
        /// </summary>
        /// <param name="appBuilder">The application builder.</param>
        private void StartHttpApi(IAppBuilder appBuilder)
        {
            this.UseCors(appBuilder);
            this.UseWebApi(appBuilder);
        }
        /// <summary>
        /// Instructs the http api to allow all CORS requests.
        /// </summary>
        /// <param name="appBuilder">The application builder.</param>
        private void UseCors(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(CorsOptions.AllowAll);
        }
        /// <summary>
        /// Instructs the http api to use ASP.NET WebAPI. 
        /// </summary>
        /// <param name="appBuilder">The application builder.</param>
        private void UseWebApi(IAppBuilder appBuilder)
        {
            var httpConfiguration = new HttpConfiguration();

            this.ConfigureWindsor(httpConfiguration);
            this.ConfigureFilters(httpConfiguration);
            this.ConfigureMessageHandlers(httpConfiguration);
            this.ConfigureServices(httpConfiguration);
            this.ConfigureRoutes(httpConfiguration);

            appBuilder.UseWebApi(httpConfiguration);
        }
        /// <summary>
        /// Configures the castle windsor container.
        /// </summary>
        /// <param name="httpConfiguration">The HTTP configuration.</param>
        private void ConfigureWindsor(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.DependencyResolver = new WindsorResolver(this._windsorContainer);
        }
        /// <summary>
        /// Configures the default filters.
        /// </summary>
        /// <param name="httpConfiguration">The HTTP configuration.</param>
        private void ConfigureFilters(HttpConfiguration httpConfiguration)
        {
        }
        /// <summary>
        /// Configures the default message handlers.
        /// </summary>
        /// <param name="httpConfiguration">The HTTP configuration.</param>
        private void ConfigureMessageHandlers(HttpConfiguration httpConfiguration)
        {
            if (this._config.CompressResponses)
            {
                httpConfiguration.MessageHandlers.Add(new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));
            }
        }
        /// <summary>
        /// Configures the ASP.NET WebAPI services.
        /// </summary>
        /// <param name="httpConfiguration">The HTTP configuration.</param>
        private void ConfigureServices(HttpConfiguration httpConfiguration)
        {
            LogTo.Debug("Replacing some WebAPI services. (IAssembliesResolver, IExceptionHandler, IExceptionLogger)");

            httpConfiguration.Services.Replace(typeof(IAssembliesResolver), new LeagueRecorderAssembliesResolver());
            httpConfiguration.Services.Replace(typeof(IExceptionHandler), new LeagueRecorderExceptionHandler());
            httpConfiguration.Services.Replace(typeof(IExceptionLogger), new LeagueRecorderExceptionLogger());
        }
        /// <summary>
        /// Configures the routes.
        /// </summary>
        /// <param name="httpConfiguration">The HTTP configuration.</param>
        private void ConfigureRoutes(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.MapHttpAttributeRoutes();

            httpConfiguration.Routes.MapHttpRoute(
                name: "DefaultRoute",
                routeTemplate: "{*uri}",
                defaults: new { controller = "Default", uri = RouteParameter.Optional });
        }
        #endregion
    }
}