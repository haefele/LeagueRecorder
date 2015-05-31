using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.Extensions;
using LeagueRecorder.Shared.Abstractions.Recordings;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Shared.Localization;
using LiteGuard;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;

namespace LeagueRecorder.Shared.Implementations.Recordings
{
    public class RecordingStorage : IRecordingStorage
    {
        #region Fields
        private readonly CloudTableClient _tableClient;
        private readonly IConfig _config;
        #endregion

        #region Constructors
        public RecordingStorage([NotNull]CloudTableClient tableClient, [NotNull]IConfig config)
        {
            Guard.AgainstNullArgument("tableClient", tableClient);
            Guard.AgainstNullArgument("config", config);

            this._tableClient = tableClient;
            this._config = config;
        }
        #endregion

        #region Methods
        public Task<Result> IsGameRecording(long gameId, Region region)
        {
            return Result.CreateAsync(async () =>
            {
                CloudTable recordingTable = await this.GetRecordingTableAsync();

                TableOperation operation = TableOperation.Retrieve<RecordingTable>(RecordingTable.ToPartitionKey(region), RecordingTable.ToRowKey(gameId));
                TableResult result = await recordingTable.ExecuteAsync(operation);

                if (result.Result == null)
                    throw new ResultException(Messages.RecordingDoesNotExist);
            });
        }

        public Task<Result> SaveRecordingAsync(Recording recording)
        {
            return Result.CreateAsync(async () =>
            {
                Result isRecordingResult = await this.IsGameRecording(recording.GameId, recording.Region);

                if (isRecordingResult.IsError)
                    throw new ResultException(isRecordingResult.Message);

                CloudTable recordingTable = await this.GetRecordingTableAsync();

                TableOperation operation = TableOperation.Insert(RecordingTable.FromRecording(recording));
                TableResult result = await recordingTable.ExecuteAsync(operation);

                if (result.Etag == null)
                    throw new ResultException(Messages.RecordingInsertFailed);
            });
        }

        public Task<Result<Recording>> GetRecordingAsync(long gameId, Region region)
        {
            return Result.CreateAsync(async () =>
            {
                CloudTable recordingTable = await this.GetRecordingTableAsync();

                TableOperation operation = TableOperation.Retrieve<RecordingTable>(RecordingTable.ToPartitionKey(region), RecordingTable.ToRowKey(gameId));
                TableResult result = await recordingTable.ExecuteAsync(operation);

                if (result.Result == null)
                    throw new ResultException(Messages.RecordingDoesNotExist);

                var tableEntry = (RecordingTable)result.Result;
                return tableEntry.AsRecording();
            });
        }
        
        public Task<Result<IList<Recording>>> GetFinishedRecordingsAsync()
        {
            return Result.CreateAsync(async () =>
            {
                CloudTable recordingTable = await this.GetRecordingTableAsync();

                IList<Recording> result = new List<Recording>();
                TableContinuationToken tableContinuationToken = null;

                do
                {
                    var segment = await recordingTable
                        .CreateQuery<RecordingTable>()
                        .Where(f => f.HasFinished)
                        .AsTableQuery()
                        .ExecuteSegmentedAsync(tableContinuationToken);

                    tableContinuationToken = segment.ContinuationToken;

                    result.AddRange(segment.Results.Select(f => f.AsRecording()));

                } while (tableContinuationToken != null);

                return result;
            });
        }
        #endregion

        #region Private Methods
        private async Task<CloudTable> GetRecordingTableAsync()
        {
            var table = this._tableClient.GetTableReference(this._config.RecordingTableName);
            await table.CreateIfNotExistsAsync();

            return table;
        }
        #endregion
    }
}