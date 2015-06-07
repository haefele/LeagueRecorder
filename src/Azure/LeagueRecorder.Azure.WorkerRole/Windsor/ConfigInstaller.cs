using AppConfigFacility;
using AppConfigFacility.Azure;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LeagueRecorder.Shared.Abstractions;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace LeagueRecorder.Azure.WorkerRole.Windsor
{
    public class ConfigInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<AppConfigFacility.AppConfigFacility>(f => f.FromAzure());

            container.Register(Component.For<IConfig>().FromAppConfig(f => f.Computed(d => d.Url, this.GetUrl)));
        }

        private object GetUrl(IConfig config)
        {
            var endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HTTP"];
            return string.Format("{0}://{1}", endpoint.Protocol, endpoint.IPEndpoint);
        }
    }
}