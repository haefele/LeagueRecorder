using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.Recordings;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Shared.Implementations.Extensions;
using LeagueRecorder.Shared.Localization;
using LiteGuard;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace LeagueRecorder.Shared.Implementations.Recordings
{
    public class RecordingQueue : IRecordingQueue
    {
        #region Fields
        private readonly CloudQueueClient _queueClient;
        private readonly IConfig _config;

        private readonly ConcurrentDictionary<RecordingRequest, CloudQueueMessage> _requestToQueueMessageMapping;
        #endregion

        #region Constructors
        public RecordingQueue([NotNull]CloudQueueClient queueClient, [NotNull]IConfig config)
        {
            Guard.AgainstNullArgument("queueClient", queueClient);
            Guard.AgainstNullArgument("config", config);

            this._queueClient = queueClient;
            this._config = config;

            this._requestToQueueMessageMapping = new ConcurrentDictionary<RecordingRequest, CloudQueueMessage>();
        }
        #endregion

        #region Methods
        public Task<Result> EnqueueAsync(RecordingRequest recording)
        {
            return Result.CreateAsync(async () =>
            {
                var queue = await this.GetQueueAsync();
                
                var message = new CloudQueueMessage(JsonConvert.SerializeObject(recording, LeagueJsonSerializerSettings.Get()));
                await queue.AddMessageAsync(message);
            });
        }

        public Task<Result<RecordingRequest>> DequeueAsync()
        {
            return Result.CreateAsync(async () =>
            {
                var queue = await this.GetQueueAsync();

                var message = await queue.GetMessageAsync();

                if (message == null)
                    return null;

                var actualData = JsonConvert.DeserializeObject<RecordingRequest>(message.AsString, LeagueJsonSerializerSettings.Get());
                this._requestToQueueMessageMapping.TryAdd(actualData, message);

                return actualData;
            });
        }
        public Task<Result> RemoveAsync(RecordingRequest request)
        {
            return Result.CreateAsync(async () =>
            {
                var queue = await this.GetQueueAsync();

                CloudQueueMessage message;
                if (this._requestToQueueMessageMapping.TryGetValue(request, out message))
                {
                    await queue.DeleteMessageAsync(message);
                }
            });
        }
        #endregion

        #region Private Methods
        private async Task<CloudQueue> GetQueueAsync()
        {
            var queue = this._queueClient.GetQueueReference(this._config.RecordingQueueName);
            await queue.CreateIfNotExistsAsync();

            return queue;
        }
        #endregion
    }
}