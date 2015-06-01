using System.IO;
using System.Threading.Tasks;
using Anotar.NLog;
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
        public Task<Result> SaveChunkAsync(long gameId, Region region, int chunkId, byte[] chunk)
        {
            return Result.CreateAsync(async () =>
            {
                CloudBlobContainer recordingContainer = await this.GetRecordingContainerAsync();
                CloudBlockBlob chunkReference = recordingContainer.GetBlockBlobReference(ChunkBlob.ToBlobName(gameId, region, chunkId));

                await chunkReference.UploadFromByteArrayAsync(chunk, 0, chunk.Length);

                LogTo.Debug("Uploaded chunk {0} for recording {1} {2}.", chunkId, region, gameId);
            });
        }

        public Task<Result> SaveKeyFrameAsync(long gameId, Region region, int keyFrameId, byte[] keyFrame)
        {
            return Result.CreateAsync(async () =>
            {
                CloudBlobContainer recordingContainer = await this.GetRecordingContainerAsync();
                CloudBlockBlob keyFrameReference = recordingContainer.GetBlockBlobReference(KeyFrameBlob.ToBlobName(gameId, region, keyFrameId));

                await keyFrameReference.UploadFromByteArrayAsync(keyFrame, 0, keyFrame.Length);

                LogTo.Debug("Uploaded keyframe {0} for recording {1} {2}.", keyFrameId, region, gameId);
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

        public Task<Result> DeleteChunksAsync(long gameId, Region region, int latestChunkId)
        {
            return Result.CreateAsync(async () =>
            {
                CloudBlobContainer recordingContainer = await this.GetRecordingContainerAsync();

                for (int chunkId = 1; chunkId <= latestChunkId; chunkId++)
                {
                    CloudBlockBlob chunkReference = recordingContainer.GetBlockBlobReference(ChunkBlob.ToBlobName(gameId, region, chunkId));

                    if (await chunkReference.DeleteIfExistsAsync() == false)
                        throw new ResultException(Messages.ErrorWhileDeletingChunk);
                }
            });
        }

        public Task<Result> DeleteKeyFramesAsync(long gameId, Region region, int latestKeyFrameId)
        {
            return Result.CreateAsync(async () =>
            {
                CloudBlobContainer recordingContainer = await this.GetRecordingContainerAsync();

                for (int keyFrameId = 1; keyFrameId <= latestKeyFrameId; keyFrameId++)
                {
                    CloudBlockBlob keyFrameReference = recordingContainer.GetBlockBlobReference(KeyFrameBlob.ToBlobName(gameId, region, keyFrameId));

                    if (await keyFrameReference.DeleteIfExistsAsync() == false)
                        throw new ResultException(Messages.ErrorWhileDeletingKeyFrame);
                }
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