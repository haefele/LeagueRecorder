using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using LeagueRecorder.Shared.Abstractions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace LeagueRecorder.Azure.WorkerRole.Windsor
{
    public class AzureInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<CloudQueueClient>().UsingFactoryMethod((kernel, context) => this.CreateQueueClient(kernel)).LifestyleSingleton(),
                Component.For<CloudTableClient>().UsingFactoryMethod((kernel, context) => this.CreateTableClient(kernel)).LifestyleSingleton(),
                Component.For<CloudBlobClient>().UsingFactoryMethod((kernel, context) => this.CreateBlobClient(kernel)).LifestyleSingleton());

        }

        private CloudStorageAccount GetStorageAccount(IKernel kernel)
        {
            var config = kernel.Resolve<IConfig>();

            return CloudStorageAccount.Parse(config.AzureStorageConnectionString);
        }

        private CloudBlobClient CreateBlobClient(IKernel kernel)
        {
            var storageAccount = this.GetStorageAccount(kernel);
            return storageAccount.CreateCloudBlobClient();
        }

        private CloudTableClient CreateTableClient(IKernel kernel)
        {
            var storageAccount = this.GetStorageAccount(kernel);
            return storageAccount.CreateCloudTableClient();
        }

        private CloudQueueClient CreateQueueClient(IKernel kernel)
        {
            var storageAccount = this.GetStorageAccount(kernel);
            return storageAccount.CreateCloudQueueClient();
        }
    }
}