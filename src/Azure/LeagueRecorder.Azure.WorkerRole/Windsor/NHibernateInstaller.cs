using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Implementations.Summoners;
using NHibernate;

namespace LeagueRecorder.Azure.WorkerRole.Windsor
{
    public class NHibernateInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<ISessionFactory>().UsingFactoryMethod((kernel, context) => this.CreateSessionFactory(kernel)).LifestyleSingleton(),
                Component.For<ISession>().UsingFactoryMethod((kernel, context) => kernel.Resolve<ISessionFactory>().OpenSession()).LifestyleTransient());
        }

        private ISessionFactory CreateSessionFactory(IKernel kernel)
        {
            var config = kernel.Resolve<IConfig>();

            var sessionFactory = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012.ConnectionString(f => f.Is(config.SqlServerConnectionString)))
                .Mappings(f => f.FluentMappings.AddFromAssemblyOf<SummonerEntity>())
                .BuildSessionFactory();

            return sessionFactory;
        }
    }
}