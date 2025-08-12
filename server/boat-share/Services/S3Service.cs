using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;

namespace boat_share.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IAmazonS3 s3Client, string bucketName)
        {
            _s3Client = s3Client;
            _bucketName = bucketName;
        }

        // Upload file to S3
        public async Task UploadFileAsync(string key, Stream fileStream)
        {
            var uploadRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream
            };

            await _s3Client.PutObjectAsync(uploadRequest);
        }

        // Download file from S3
        public async Task<Stream> DownloadFileAsync(string key)
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(getRequest);
            return response.ResponseStream;
        }

        // Delete file from S3
        public async Task DeleteFileAsync(string key)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
    }
}
