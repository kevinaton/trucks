using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Extensions;
using Abp.UI;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class BinaryObjectExtensions
    {
        public static async Task<string> GetImageAsBase64StringAsync(this IBinaryObjectManager binaryObjectManager, Guid? id)
        {
            if (id != null)
            {
                return await binaryObjectManager.GetImageAsBase64StringAsync(id.Value);
            }
            return null;
        }

        public static async Task<string> GetImageAsBase64StringAsync(this IBinaryObjectManager binaryObjectManager, Guid id)
        {
            var logoObject = await binaryObjectManager.GetOrNullAsync(id);
            if (logoObject?.Bytes?.Length > 0)
            {
                return "base64:" + Convert.ToBase64String(logoObject.Bytes);
            }
            return null;
        }

        public static async Task<Guid?> UploadDataUriStringAsync(this IBinaryObjectManager binaryObjectManager, string dataUri, int? tenantId, int? maxSizeInBytes = null)
        {
            if (dataUri.IsNullOrEmpty() || !dataUri.Contains(",")) //"data:" input strings (without the content) should be skipped
            {
                return null;
            }
            //var dataUri = "data:image/png;base64,iVBORw0K...";
            var encodedImage = dataUri.Split(",").LastOrDefault();
            if (encodedImage.IsNullOrEmpty())
            {
                return null;
            }
            var decodedImage = Convert.FromBase64String(encodedImage);
            return await binaryObjectManager.UploadByteArrayAsync(decodedImage, tenantId, maxSizeInBytes);
        }

        public static async Task<Guid?> UploadByteArrayAsync(this IBinaryObjectManager binaryObjectManager, byte[] byteArray, int? tenantId, int? maxSizeInBytes = null)
        {
            if (maxSizeInBytes != null && byteArray.Length > maxSizeInBytes)
            {
                throw new UserFriendlyException("Size of uploaded image cannot exceed " + maxSizeInBytes + " bytes");
            }

            var storedFile = new BinaryObject(tenantId, byteArray);
            await binaryObjectManager.SaveAsync(storedFile);

            return storedFile.Id;
        }
    }
}
