using System;
using System.IO;
using System.Security.Cryptography;
using Castle.Core.Logging;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;



namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public class AttachmentHelper
    {
        public static string _defaultEncryptionKey = "<RSAKeyValue><Modulus>rIdDJ3Ea1kjr2/HSAdk7/q3NQvycQw9HZgrWXX+DSMuXOAN21+nU3H/889ENofwkOJuwDfUBsxL7tpk65Sxfgb0wwaa3CLQXJq5QHMhThgfcxZIFw28cT3P4WUj4SDj8EN9lt6JBkhZqIqOwDN88j2JdjDF5R0N5lUpfgSkjSTxbUh5FyPMTdpgZ35dynS0wCEj3SLA6flO9TGTJq/wOpPXu7AlknHgUJOr+Dc0qq063jd6RR8myhl3wA4nhHf/0F2LcxB+pdmLcB5vkc7CTSTYKLfN50rkPma2uyBqGP5d7NXQWWR2/I7FQyknAwS3O1jOH9SBOdaWiJ+2XRAP7lw==</Modulus><Exponent>AQAB</Exponent><P>3ctUz999kNNycz53rY3aLkvDWN98+69SDYt8FY67e7pk8cVgX/g9mCfCbwj2wpLhiqzIvT5ongZNQSeZ/uwkReb0N6mRa4Ygpus27eXllZdo+Zug30LBnipMOgOb4GcVy7MLGAjbytQytTAtdoabYu8TTPM+WlkY2B9lWpTnfEk=</P><Q>xyLctjICMIKVpNoOPjrCU1K19mXSq/oGTGgyTLXZ5NRMFYzWrbYRONHAvl8YCLZozyUPXvIr2Y80QLO6OoBYp7gTZtV1oJ2htWvKYQkw68R1+hvF64gHzvJjjg273JCPCWYQUEhHqmbIT2inCGntPeUEzrCbqtGTDjMOvy44+N8=</Q><DP>FMJRP9uVLxb7YVn5bZ8XSroAoLLaSdxQ+7+Qb5waZCxAAnkM2i6r5S/jfO0D25ZP0eD2MPBEbnX8iRqhyUBk7L3/Ia6XU88MA1OEoqOf525yftpYJk1jPeCLnep0jn5XhedOJIO/vq45yMenma5joNDrp4okzeQE9UWLAP1q5TE=</DP><DQ>UX/dINwatUvcftmXlrRpr7xcKHnH3Qa46TCD1Y1fnh2c/fzkJ7gqGD0QS1mT9ozhHFYokk0+0Q2g7xIfl63LmujlV+Lo/1FES4HQFCK02OlQ94nCWQEVYQcm446PYlfvkoMpDhJm8kCanpVQN0tTA0/lxcnWC/U14EvZLzl1q7U=</DQ><InverseQ>B2/qXwb21cxClROLaV1g5deYWVE7Jmi8QKeFVhk0oV7L3GCjR6UGotQLayi4ZxsExNgsSE6WO9KFDauXYT3WNRNN4i8vSnRb5hVYiagoJAyHS6smxdf9NDznbLWppB5PZjNeb8zI2mTxf1wBbEuyRY3c6mwqlVsg1ht7Wp9tET0=</InverseQ><D>YhXoZOUVt9vvU7UQiwKA7FLNTpclE679DZN9udxDvEAa8bpud2Q2I0IUgl1I3d3mjdRdbCHt3GwfdPl6lU374lu3+3CXwLf6LYCOhT6S1bFkn2JcKnbh5n3tJH2qwgy7qbAIOTVjB/X8U0MyVK8Z0/69ZnW+GHjRB8Int9u2duyNQxQXDuneOCsHB7D8MZ8ruXZYc8sUz0c4fPf+o7epo3l7HmWKDI6i5Gt6ILCxs5z92WbmbNJTdxoq+ms+slnmdMiTB19uGcqrrbDE7oVVSdToLW/gFqfzfhIvd5oYKvJV4+m6z0hJeqchfEDIsSEnngUPOMm344tFcaFHa95u0Q==</D></RSAKeyValue>";
        public static string StorageConnectionString; // Should be set at startup

        private static string SecureFilesContainerName = "securefiles";
        private static string ReportFilesContainerName = "reportfiles";
        private static string DefaultContainerName = "attachments";

        private static CloudBlobContainer GetBlobContainer(string containerName = null)
        {
            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            //blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);
            blobClient.DefaultRequestOptions.RequireEncryption = true;
            blobClient.DefaultRequestOptions.EncryptionPolicy = new BlobEncryptionPolicy(GetBlobStorageKey(), null);
            var filesBlobContainer = blobClient.GetContainerReference(containerName ?? DefaultContainerName);
            if (!filesBlobContainer.Exists())
            {
                filesBlobContainer.CreateIfNotExists(BlobContainerPublicAccessType.Off);
            }
            return filesBlobContainer;
        }

        public static string UploadToAzureBlob(byte[] file, string userId, string contentType = null)
        {
            var filesBlobContainer = GetBlobContainer();
            var fileId = Guid.NewGuid();
            var path = $"{userId}/{fileId}";
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(path);

            fileBlob.Properties.ContentType = contentType;
            fileBlob.UploadFromByteArray(file, 0, file.Length, AccessCondition.GenerateIfNotExistsCondition());
            return path;
        }

        public static Guid UploadToAzureBlob(byte[] file, int ownerId, string contentType = null, string containerName = null)
        {
            var filesBlobContainer = GetBlobContainer(containerName);
            var fileId = Guid.NewGuid();
            var path = $"{ownerId}/{fileId}";
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(path);

            fileBlob.Properties.ContentType = contentType;
            fileBlob.UploadFromByteArray(file, 0, file.Length, AccessCondition.GenerateIfNotExistsCondition());
            return fileId;
        }

        public static void UploadReportFile(Stream fileStream, string userId, string fileToken)
        {
            var filesBlobContainer = GetBlobContainer(ReportFilesContainerName);
            var path = $"{userId}/{fileToken}";
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(path);
            fileBlob.UploadFromStream(fileStream);
        }
        public static byte[] GetReportFile(string userId, string fileToken)
        {
            var path = $"{userId}/{fileToken}";
            return GetFromAzureBlob(path, ReportFilesContainerName).Content;
        }
        public static void DeleteReportFile(string userId, string fileToken)
        {
            var path = $"{userId}/{fileToken}";
            DeleteFromAzureBlob(path, ReportFilesContainerName);
        }
        public static void DeleteExpitedReportFiles(TimeSpan expirationPeriod, ILogger logger)
        {
            var filesBlobContainer = GetBlobContainer(ReportFilesContainerName);
            var fileBlobs = filesBlobContainer.ListBlobs(null, true, BlobListingDetails.Metadata);
            foreach (var blobItem in fileBlobs)
            {
                CloudBlockBlob fileBlob = (CloudBlockBlob)blobItem;
                string fileName = fileBlob.Name;
                //CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(fileName);
                logger.Debug($"{fileName}, LastModified={fileBlob.Properties.LastModified}");
                if (fileBlob.Properties.LastModified?.Add(expirationPeriod) > DateTime.UtcNow)
                {
                    logger.Debug($"File {fileName} doesn't expired yet, skip");
                    continue;
                }
                logger.Debug($"Deleting file {fileName}");
                DeleteFromAzureBlob(fileName /* fileName is path */, ReportFilesContainerName);
            }
        }

        public static string UploadToAzureBlob(Stream fileStream, string userId)
        {
            var fileBlob = GetBlockBlob(userId);

            fileBlob.UploadFromStream(fileStream, AccessCondition.GenerateIfNotExistsCondition());
            return fileBlob.Name;
        }

        private static CloudBlockBlob GetBlockBlob(string userId)
        {
            var filesBlobContainer = GetBlobContainer();
            var fileId = Guid.NewGuid();
            var path = $"{userId}/{fileId}";
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(path);
            return fileBlob;
        }

        public static ContentWithType GetFromAzureBlob(string path, string containerName = null)
        {
            ContentWithType contentWithType = new ContentWithType();
            if (String.IsNullOrEmpty(path))
            {
                contentWithType.Content = new byte[0];
                return contentWithType;
            }
            var filesBlobContainer = GetBlobContainer(containerName);
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(path);
            if (!fileBlob.Exists())
            {
                contentWithType.Content = new byte[0];
                return contentWithType;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                fileBlob.DownloadToStream(ms);
                contentWithType.Content = ms.ToArray();
                contentWithType.ContentType = fileBlob.Properties.ContentType;
                return contentWithType;
            }
        }

        public static void DeleteFromAzureBlob(string path, string containerName = null)
        {
            if (String.IsNullOrEmpty(path)) return;
            var filesBlobContainer = GetBlobContainer(containerName);
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(path);
            fileBlob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }

        private static IKey GetBlobStorageKey()
        {
            var rsaCsp = new RSACryptoServiceProvider(2048);

            try
            {


                var key = Environment.GetEnvironmentVariable("APPSETTING_AttachmentsEncriptionKey");
                //rsaCsp.FromXmlString(key ?? _defaultEncryptionKey);
                RSACryptoServiceProviderExtensions.FromXmlString(rsaCsp, key ?? _defaultEncryptionKey);

                return new RsaKey("BlobAttachmentStorageRsaKey", rsaCsp);
            }
            finally
            {
                rsaCsp.PersistKeyInCsp = false;
            }

        }

        public class ContentWithType
        {
            public byte[] Content { get; set; }
            public string ContentType { get; set; }
        }

        public static Stream GetStreamFromAzureBlob(string path)
        {
            if (String.IsNullOrEmpty(path)) return null;
            var filesBlobContainer = GetBlobContainer(SecureFilesContainerName);
            CloudBlockBlob fileBlob = filesBlobContainer.GetBlockBlobReference(path);
            //MemoryStream ms = new MemoryStream();
            //fileBlob.DownloadToStream(ms);
            //ms.Position = 0;
            //return ms;
            return fileBlob.OpenRead();
        }
    }
}
