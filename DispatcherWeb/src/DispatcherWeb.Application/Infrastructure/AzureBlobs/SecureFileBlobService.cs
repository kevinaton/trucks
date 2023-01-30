using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AboveGoal.Prospects.Import;
using Abp.Dependency;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public class SecureFileBlobService : ISecureFileBlobService, ISingletonDependency
    {
        private const string DefaultEncryptionKey = "<RSAKeyValue><Modulus>rIdDJ3Ea1kjr2/HSAdk7/q3NQvycQw9HZgrWXX+DSMuXOAN21+nU3H/889ENofwkOJuwDfUBsxL7tpk65Sxfgb0wwaa3CLQXJq5QHMhThgfcxZIFw28cT3P4WUj4SDj8EN9lt6JBkhZqIqOwDN88j2JdjDF5R0N5lUpfgSkjSTxbUh5FyPMTdpgZ35dynS0wCEj3SLA6flO9TGTJq/wOpPXu7AlknHgUJOr+Dc0qq063jd6RR8myhl3wA4nhHf/0F2LcxB+pdmLcB5vkc7CTSTYKLfN50rkPma2uyBqGP5d7NXQWWR2/I7FQyknAwS3O1jOH9SBOdaWiJ+2XRAP7lw==</Modulus><Exponent>AQAB</Exponent><P>3ctUz999kNNycz53rY3aLkvDWN98+69SDYt8FY67e7pk8cVgX/g9mCfCbwj2wpLhiqzIvT5ongZNQSeZ/uwkReb0N6mRa4Ygpus27eXllZdo+Zug30LBnipMOgOb4GcVy7MLGAjbytQytTAtdoabYu8TTPM+WlkY2B9lWpTnfEk=</P><Q>xyLctjICMIKVpNoOPjrCU1K19mXSq/oGTGgyTLXZ5NRMFYzWrbYRONHAvl8YCLZozyUPXvIr2Y80QLO6OoBYp7gTZtV1oJ2htWvKYQkw68R1+hvF64gHzvJjjg273JCPCWYQUEhHqmbIT2inCGntPeUEzrCbqtGTDjMOvy44+N8=</Q><DP>FMJRP9uVLxb7YVn5bZ8XSroAoLLaSdxQ+7+Qb5waZCxAAnkM2i6r5S/jfO0D25ZP0eD2MPBEbnX8iRqhyUBk7L3/Ia6XU88MA1OEoqOf525yftpYJk1jPeCLnep0jn5XhedOJIO/vq45yMenma5joNDrp4okzeQE9UWLAP1q5TE=</DP><DQ>UX/dINwatUvcftmXlrRpr7xcKHnH3Qa46TCD1Y1fnh2c/fzkJ7gqGD0QS1mT9ozhHFYokk0+0Q2g7xIfl63LmujlV+Lo/1FES4HQFCK02OlQ94nCWQEVYQcm446PYlfvkoMpDhJm8kCanpVQN0tTA0/lxcnWC/U14EvZLzl1q7U=</DQ><InverseQ>B2/qXwb21cxClROLaV1g5deYWVE7Jmi8QKeFVhk0oV7L3GCjR6UGotQLayi4ZxsExNgsSE6WO9KFDauXYT3WNRNN4i8vSnRb5hVYiagoJAyHS6smxdf9NDznbLWppB5PZjNeb8zI2mTxf1wBbEuyRY3c6mwqlVsg1ht7Wp9tET0=</InverseQ><D>YhXoZOUVt9vvU7UQiwKA7FLNTpclE679DZN9udxDvEAa8bpud2Q2I0IUgl1I3d3mjdRdbCHt3GwfdPl6lU374lu3+3CXwLf6LYCOhT6S1bFkn2JcKnbh5n3tJH2qwgy7qbAIOTVjB/X8U0MyVK8Z0/69ZnW+GHjRB8Int9u2duyNQxQXDuneOCsHB7D8MZ8ruXZYc8sUz0c4fPf+o7epo3l7HmWKDI6i5Gt6ILCxs5z92WbmbNJTdxoq+ms+slnmdMiTB19uGcqrrbDE7oVVSdToLW/gFqfzfhIvd5oYKvJV4+m6z0hJeqchfEDIsSEnngUPOMm344tFcaFHa95u0Q==</D></RSAKeyValue>";
        private const string SecureFilesContainerName = "securefiles";

        public void UploadSecureFile(Stream fileStream, Guid id, string fileName, IEnumerable<KeyValuePair<string, string>> metadataList = null)
        {
            var filesBlobContainer = GetBlobContainer();
            var path = $"{id}/{fileName}";
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(path);
            if(metadataList != null)
            {
                foreach(var metadataItem in metadataList)
                {
                    fileBlob.Metadata.Add(metadataItem);
                }
            }
            fileBlob.UploadFromStream(fileStream);
        }

        public async Task AddMetadataAsync(Guid id, string fileName, IEnumerable<KeyValuePair<string, string>> metadataList)
        {
            await AddMetadataAsync(GetBlobName(id, fileName), metadataList);
        }
        public async Task AddMetadataAsync(string blobName, IEnumerable<KeyValuePair<string, string>> metadataList)
        {
            CloudBlockBlob fileBlob = GetBlobContainer().GetBlockBlobReference(blobName);
            await fileBlob.FetchAttributesAsync();
            AddMetadataToBlob(fileBlob, metadataList);
            await fileBlob.SetMetadataAsync();
        }
        public void AddMetadata(string blobName, IEnumerable<KeyValuePair<string, string>> metadataList)
        {
            CloudBlockBlob fileBlob = GetBlobContainer().GetBlockBlobReference(blobName);
            fileBlob.FetchAttributes();
            AddMetadataToBlob(fileBlob, metadataList);
            fileBlob.SetMetadata();
        }
        public void AddMetadata(string blobName, string metadataKey, string metadataValue)
        {
            CloudBlockBlob fileBlob = GetBlobContainer().GetBlockBlobReference(blobName);
            fileBlob.FetchAttributes();
            AddKeyValueToMetadata(fileBlob.Metadata, metadataKey, metadataValue);
            fileBlob.SetMetadata();
        }

        public async Task AddChildBlob(string blobName, string childBlobName, string childContent)
        {
            string fullChildName = $"{blobName}/{childBlobName}";
            CloudBlockBlob fileBlob = GetBlobContainer().GetBlockBlobReference(fullChildName);
            await fileBlob.UploadTextAsync(childContent);
        }

        public string GetChildBlob(string blobName, string childBlobName)
        {
            string fullChildName = $"{blobName}/{childBlobName}";
            CloudBlockBlob fileBlob = GetBlobContainer().GetBlockBlobReference(fullChildName);
            return fileBlob.DownloadText();
        }

        public async Task<string> GetChildBlobAsync(string blobName, string childBlobName)
        {
            string fullChildName = $"{blobName}/{childBlobName}";
            CloudBlockBlob fileBlob = GetBlobContainer().GetBlockBlobReference(fullChildName);
            return await fileBlob.DownloadTextAsync();
        }

        public string[] GetSecureFileNames(Guid id)
        {
            var filesBlobContainer = GetBlobContainer();
            return filesBlobContainer.ListBlobs(id.ToString(), true)
                .Select(x => ((CloudBlockBlob)x).Name)
                .Where(name => !name.Substring(37).Contains("/"))
                .ToArray();
        }

        public byte[] GetSecureFile(Guid id, string fileName)
        {
            var path = $"{id}/{fileName}";
            return GetFromAzureBlob(path, SecureFilesContainerName).Content;
        }

        public AttachmentHelper.ContentWithType GetFromAzureBlob(string path, string containerName = null)
        {
            AttachmentHelper.ContentWithType contentWithType = new AttachmentHelper.ContentWithType();
            if(String.IsNullOrEmpty(path))
            {
                contentWithType.Content = new byte[0];
                return contentWithType;
            }
            var filesBlobContainer = GetBlobContainer();
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(path);
            if(!fileBlob.Exists())
            {
                contentWithType.Content = new byte[0];
                return contentWithType;
            }
            using(MemoryStream ms = new MemoryStream())
            {
                fileBlob.DownloadToStream(ms);
                contentWithType.Content = ms.ToArray();
                contentWithType.ContentType = fileBlob.Properties.ContentType;
                return contentWithType;
            }
        }

        private void AddMetadataToBlob(CloudBlockBlob fileBlob, IEnumerable<KeyValuePair<string, string>> metadataList)
        {
            foreach (var metadataItem in metadataList)
            {
                AddKeyValueToMetadata(fileBlob.Metadata, metadataItem.Key, metadataItem.Value);
            }
        }
        private void AddKeyValueToMetadata(IDictionary<string, string> blobMetadata, string key, string value)
        {
            if(blobMetadata.ContainsKey(key))
            {
                blobMetadata[key] = value;
            }
            else
            {
                blobMetadata.Add(key, value);
            }
        }

        public async Task<IDictionary<string, string>> GetMetadataAsync(Guid id, string fileName)
        {
            return await GetMetadataAsync(GetBlobName(id, fileName));
        }
        public async Task<IDictionary<string, string>> GetMetadataAsync(string blobName)
        {
            if(String.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException($"{nameof(blobName)} cannot be null or empty!");
            }
            CloudBlockBlob fileBlob = GetBlobContainer().GetBlockBlobReference(blobName);
            await fileBlob.FetchAttributesAsync();
            return fileBlob.Metadata;
        }

        public async Task DeleteSecureFilesAsync(Guid id)
        {
            string[] fileNames = GetSecureFileNames(id);
            foreach(var fileName in fileNames)
            {
                await DeleteSecureFileAsync(fileName /* fileName is path */);
            }
        }

        public async Task<bool> DeleteSecureFileAsync(Guid id, string fileName)
        {
            return await DeleteSecureFileAsync(GetBlobName(id, fileName));
        }
        public async Task<bool> DeleteSecureFileAsync(string blobName)
        {
            if(String.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException($"{nameof(blobName)} cannot be null or empty!");
            }
            var blobContainer = GetBlobContainer();
            return await DeleteBlobWithChildrenAsync(blobContainer, blobName);
        }

        private async Task<bool> DeleteBlobWithChildrenAsync(CloudBlobContainer blobContainer, string blobName)
        {
            bool result = false;
            foreach (string childBlobName in GetChildBlobNames(blobName))
            {
                result = await blobContainer.GetBlobReference(childBlobName).DeleteIfExistsAsync();
            }
            return result;
        }

        public void DeleteExpiredFiles(TimeSpan expirationPeriod)
        {
            var blobContainer = GetBlobContainer();
            var fileBlobs = blobContainer.ListBlobs(null, true, BlobListingDetails.Metadata);
            foreach(var blobItem in fileBlobs)
            {
                CloudBlockBlob fileBlob = (CloudBlockBlob)blobItem;
                string blobName = fileBlob.Name;
                if(fileBlob.Properties.LastModified?.Add(expirationPeriod) > DateTime.UtcNow)
                {
                    continue;
                }
                fileBlob.FetchAttributesAsync();
                //if (!fileBlob.Metadata.ContainsKey(ImportBlobMetadata.CampaignId))
                //{
                //    continue;
                //}
                //DeleteBlobWithChildren(blobContainer, blobName);
            }
        }

        //public void DeleteCampaignFiles(int[] campaignIds)
        //{
        //    var blobContainer = GetBlobContainer();
        //    var fileBlobs = blobContainer.ListBlobs(null, true, BlobListingDetails.Metadata);
        //    foreach(var blobItem in fileBlobs)
        //    {
        //        CloudBlockBlob fileBlob = (CloudBlockBlob)blobItem;
        //        string blobName = fileBlob.Name;
        //        fileBlob.FetchAttributesAsync();
        //        if (!fileBlob.Metadata.ContainsKey(ImportBlobMetadata.CampaignId))
        //        {
        //            continue;
        //        }
        //        int fileCampaignId = Int32.Parse(fileBlob.Metadata[ImportBlobMetadata.CampaignId]);
        //        if (campaignIds.Contains(fileCampaignId))
        //        {
        //            DeleteBlobWithChildren(blobContainer, blobName);
        //        }
        //    }
        //}

        private void DeleteBlobWithChildren(CloudBlobContainer blobContainer, string blobName)
        {
            foreach (string childBlobName in GetChildBlobNames(blobName))
            {
                blobContainer.GetBlobReference(childBlobName).DeleteIfExists();
            }
        }


        private string[] GetChildBlobNames(string blobName)
        {
            return GetBlobContainer().ListBlobs(blobName, true)
                .Select(x => ((CloudBlockBlob)x).Name)
                .ToArray();
        }

        //public async Task<string> GetLastSecureFileForCampaignAsync(int campaignId)
        //{
        //    var filesBlobContainer = GetBlobContainer();
        //    var blobs = filesBlobContainer.ListBlobs(null, true);
        //    string lastBlobName = null;
        //    DateTime lastBlobDateTime = DateTime.MinValue;
        //    foreach (var item in blobs)
        //    {
        //        CloudBlockBlob blob = (CloudBlockBlob)item;
        //        await blob.FetchAttributesAsync();
        //        if(
        //            blob.Metadata.Contains(new KeyValuePair<string, string>(ImportBlobMetadata.CampaignId, campaignId.ToString())) &&
        //            blob.Metadata.ContainsKey(ImportBlobMetadata.UtcDateTime)
        //        )
        //        {
        //            string blobDateTimeStringValue;
        //            if(blob.Metadata.TryGetValue(ImportBlobMetadata.UtcDateTime, out blobDateTimeStringValue))
        //            {
        //                DateTime blobDateTime = DateTime.ParseExact(blobDateTimeStringValue, "O", CultureInfo.InvariantCulture);
        //                if (blobDateTime > lastBlobDateTime)
        //                {
        //                    lastBlobDateTime = blobDateTime;
        //                    lastBlobName = blob.Name;
        //                }
        //            }
        //        }

        //    }
        //    return lastBlobName;
        //}

        private string GetBlobName(Guid id, string fileName)
        {
            return $"{id}/{fileName}";
        }

        private CloudBlobContainer GetBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(AttachmentHelper.StorageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RequireEncryption = true;
            blobClient.DefaultRequestOptions.EncryptionPolicy = new BlobEncryptionPolicy(GetBlobStorageKey(), null);
            var filesBlobContainer = blobClient.GetContainerReference(SecureFilesContainerName);
            if(!filesBlobContainer.Exists())
            {
                filesBlobContainer.CreateIfNotExists(BlobContainerPublicAccessType.Off);
            }
            return filesBlobContainer;
        }

        private IKey GetBlobStorageKey()
        {
            // using statement disposes rsaCsp before it is used
            //using (var rsaCsp = new RSACryptoServiceProvider(2048))
            var rsaCsp = new RSACryptoServiceProvider(2048);

            try
            {
                var key = Environment.GetEnvironmentVariable("APPSETTING_AttachmentsEncriptionKey");
                RSACryptoServiceProviderExtensions.FromXmlString(rsaCsp, key ?? AttachmentHelper._defaultEncryptionKey);
                return new RsaKey("BlobAttachmentStorageRsaKey", rsaCsp);
            }
            finally
            {
                rsaCsp.PersistKeyInCsp = false;
            }

        }


    }
}
