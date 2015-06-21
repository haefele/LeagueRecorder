using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NLog.Common;
using NLog.Targets;

namespace LeagueRecorder.Shared.Implementations.LoggingTargets
{
    [Target("AzureTableStorage")]
    public class AzureTableStorageTarget : TargetWithLayout
    {
        #region Fields
        private CloudTable _table;
        #endregion

        #region Properties
        [Required]
        public string ConnectionStringKey { get; set; }
        [Required]
        public string TableName { get; set; }
        #endregion

        #region Methods
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(this.ConnectionStringKey));
            var tableClient = storageAccount.CreateCloudTableClient();

            this._table = tableClient.GetTableReference(this.TableName);
            this._table.CreateIfNotExists();
        }
        
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            var logEntity = new LogEntity(logEvent.LogEvent, this.Layout.Render(logEvent.LogEvent));
            this._table.Execute(TableOperation.Insert(logEntity));
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            try
            {
                foreach (var group in logEvents.GroupBy(f => f.LogEvent.LoggerName))
                {
                    var batch = new TableBatchOperation();

                    foreach (AsyncLogEventInfo logEvent in group)
                    {
                        var logEntity = new LogEntity(logEvent.LogEvent, this.Layout.Render(logEvent.LogEvent));
                        batch.Add(TableOperation.Insert(logEntity));
                    }

                    this._table.ExecuteBatch(batch);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion
    }
}