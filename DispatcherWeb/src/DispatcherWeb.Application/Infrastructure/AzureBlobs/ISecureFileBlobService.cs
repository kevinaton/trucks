using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
	public interface ISecureFileBlobService
	{
		void UploadSecureFile(Stream fileStream, Guid id, string fileName, IEnumerable<KeyValuePair<string, string>> metadataList = null);
		Task AddMetadataAsync(Guid id, string fileName, IEnumerable<KeyValuePair<string, string>> metadataList);
		Task AddMetadataAsync(string blobName, IEnumerable<KeyValuePair<string, string>> metadataList);
		Task<IDictionary<string, string>> GetMetadataAsync(string blobName);
		Task<IDictionary<string, string>> GetMetadataAsync(Guid id, string fileName);
		void AddMetadata(string blobName, IEnumerable<KeyValuePair<string, string>> metadataList);
		void AddMetadata(string blobName, string metadataKey, string metadataValue);
		Task<bool> DeleteSecureFileAsync(string blobName);
		//Task<string> GetLastSecureFileForCampaignAsync(int campaignId);
		Task AddChildBlob(string blobName, string childBlobName, string childContent);
		string GetChildBlob(string blobName, string childBlobName);
		Task<string> GetChildBlobAsync(string blobName, string childBlobName);
		string[] GetSecureFileNames(Guid id);
		Task DeleteSecureFilesAsync(Guid id);
		Task<bool> DeleteSecureFileAsync(Guid id, string fileName);
		byte[] GetSecureFile(Guid id, string fileName);
		AttachmentHelper.ContentWithType GetFromAzureBlob(string path, string containerName = null);
		void DeleteExpiredFiles(TimeSpan expirationPeriod);
		//void DeleteCampaignFiles(int[] campaignIds);
	}
}