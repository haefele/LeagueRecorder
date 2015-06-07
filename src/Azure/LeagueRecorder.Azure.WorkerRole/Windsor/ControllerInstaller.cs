using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LeagueRecorder.Worker.Api.Controllers;

namespace LeagueRecorder.Azure.WorkerRole.Windsor
{
    public class ControllerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromAssemblyContaining<BaseController>()
                .BasedOn<BaseController>()
                .WithServiceSelf()
                .LifestyleScoped());
        }
    }
}