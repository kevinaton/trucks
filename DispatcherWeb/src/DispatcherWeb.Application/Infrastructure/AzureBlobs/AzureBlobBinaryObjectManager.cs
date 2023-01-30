using System;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Storage;
using Castle.Core.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public class AzureBlobBinaryObjectManager : IBinaryObjectManager, ITransientDependency
    {
        private const string BinaryObjectsContainerName = "binaryobjects";
        private const string TenantId = "TenantId";

        public ILogger Logger { get; set; }

        public AzureBlobBinaryObjectManager()
        {
            Logger = NullLogger.Instance;
        }

        public async Task<BinaryObject> GetOrNullAsync(Guid id)
        {
            var filesBlobContainer = GetBlobContainer();
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference($"{id}");
            if (!await fileBlob.ExistsAsync())
            {
                Logger.Error($"The blob with id={id} doesn't exist.");
                return null;
            }
            fileBlob.FetchAttributes();
            byte[] fileBytes = new byte[fileBlob.Properties.Length];
            fileBlob.DownloadToByteArray(fileBytes, 0);
            await fileBlob.FetchAttributesAsync();
            int? tenantId = null;
            if (fileBlob.Metadata.ContainsKey(TenantId) && Int32.TryParse(fileBlob.Metadata[TenantId], out int parsedTenantId))
            {
                tenantId = parsedTenantId;
            }
            BinaryObject binaryObject = new BinaryObject(tenantId, fileBytes) { Id = id };
            return binaryObject;
        }

        public Task SaveAsync(BinaryObject file)
        {
            var filesBlobContainer = GetBlobContainer();
            var path = $"{file.Id}";
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(path);
            if (file.TenantId.HasValue)
            {
                fileBlob.Metadata.Add(TenantId, file.TenantId.Value.ToString());
            }
            return fileBlob.UploadFromByteArrayAsync(file.Bytes, 0, file.Bytes.Length);
        }

        public async Task DeleteAsync(Guid id)
        {
            var filesBlobContainer = GetBlobContainer();
            await filesBlobContainer.GetBlobReference($"{id}").DeleteIfExistsAsync();
        }

        private CloudBlobContainer GetBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(AttachmentHelper.StorageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var filesBlobContainer = blobClient.GetContainerReference(BinaryObjectsContainerName);
            if (!filesBlobContainer.Exists())
            {
                filesBlobContainer.CreateIfNotExists(BlobContainerPublicAccessType.Off);
            }
            return filesBlobContainer;
        }

    }
}
