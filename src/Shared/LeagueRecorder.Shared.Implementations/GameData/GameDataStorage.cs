using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.GameData;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Shared.Localization;
using LiteGuard;
using Microsoft.WindowsAzure.Storage.Blob;

namespace LeagueRecorder.Shared.Implementations.GameData
{
    public class GameDataStorage : IGameDataStorage
    {
        #region Fields
        private readonly CloudBlobClient _blobClient;
        private readonly IConfig _config;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GameDataStorage"/> class.
        /// </summary>
        /// <param name="blobClient">The BLOB client.</param>
        /// <param name="config">The configuration.</param>
        public GameDataStorage([NotNull]CloudBlobClient blobClient, [NotNull]IConfig config)
        {
            Guard.AgainstNullArgument("blobClient", blobClient);
            Guard.AgainstNullArgument("config", config);

            this._blobClient = blobClient;
            this._config = config;
        }
        #endregion

        #region Methods
        public Task<Result> SaveChunkAsync(long gameId, Region region, int chunkId, Stream chunk)
        {
            return Result.CreateAsync(async () =>
            {
                CloudBlobContainer recordingContainer = await this.GetRecordingContainerAsync();
                CloudBlockBlob chunkReference = recordingContainer.GetBlockBlobReference(ChunkBlob.ToBlobName(gameId, region, chunkId));

                await chunkReference.UploadFromStreamAsync(chunk);
            });
        }

        public Task<Result> SaveKeyFrameAsync(long gameId, Region region, int keyFrameId, Stream keyFrame)
        {
            return Result.CreateAsync(async () =>
            {
                CloudBlobContainer recordingContainer = await this.GetRecordingContainerAsync();
                CloudBlockBlob keyFrameReference = recordingContainer.GetBlockBlobReference(KeyFrameBlob.ToBlobName(gameId, region, keyFrameId));

                await keyFrameReference.UploadFromStreamAsync(keyFrame);
            });
        }

        public Task<Result<Stream>> GetChunkAsync(long gameId, Region region, int chunkId)
        {
            return Result.CreateAsync(async () =>
            {
                CloudBlobContainer recordingContainer = await this.GetRecordingContainerAsync();
                CloudBlockBlob chunkReference = recordingContainer.GetBlockBlobReference(ChunkBlob.ToBlobName(gameId, region, chunkId));

                if (await chunkReference.ExistsAsync() == false)
                    throw new ResultException(Messages.ChunkDoesNotExist);

                return await chunkReference.OpenReadAsync();
            });
        }

        public Task<Result<Stream>> GetKeyFrameAsync(long gameId, Region region, int keyFrameId)
        {
            return Result.CreateAsync(async () =>
            {
                CloudBlobContainer recordingContainer = await this.GetRecordingContainerAsync();
                CloudBlockBlob keyFrameReference = recordingContainer.GetBlockBlobReference(KeyFrameBlob.ToBlobName(gameId, region, keyFrameId));

                if (await keyFrameReference.ExistsAsync() == false)
                    throw new ResultException(Messages.KeyFrameDoesNotExist);

                return await keyFrameReference.OpenReadAsync();
            });
        }
        #endregion

        #region Private Methods
        private async Task<CloudBlobContainer> GetRecordingContainerAsync()
        {
            var container = this._blobClient.GetContainerReference(this._config.GameDataContainerName);
            await container.CreateIfNotExistsAsync();

            return container;
        }
        #endregion
    }
}