using Amazon.S3;
using Amazon.S3.Model;
using TelegramCards.Services.interfaces;

namespace TelegramCards.Services.Implementations;
public class S3DriveService : IFileDriveService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicUrl;

    public S3DriveService(IConfiguration config)
    {
        _bucketName = config["S3:BucketName"];
        _publicUrl = config["S3:PublicUrl"] ?? 
                     $"http://{config["S3:Endpoint"]}/{_bucketName}";
        var s3Config = new AmazonS3Config
        {
            ServiceURL = $"http://{config["S3:Endpoint"]}",
            ForcePathStyle = true,
            UseHttp = true
        };

        _s3Client = new AmazonS3Client(
            config["S3:AccessKey"], 
            config["S3:SecretKey"], 
            s3Config);
    }

    public async Task<string> UploadFileToDrive(IFormFile file)
    {
        try
        {
            string filename = $"file/{Guid.NewGuid()}_{file.FileName}";
            using (var stream = file.OpenReadStream())
            {
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = filename,
                    InputStream = stream,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.PublicRead
                };

                await _s3Client.PutObjectAsync(request);
            }

            return $"{_publicUrl}/{filename}";
        }
        catch (AmazonS3Exception ex) 
        { 
            Console.WriteLine(ex.ToString());
            return "";
        }

    }

    public async Task<bool> ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        var allowedExtensions = new[] { ".png" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
            return false;

        var allowedMimeTypes = new[] { "image/png"};
        if (Array.IndexOf(allowedMimeTypes, file.ContentType) == -1)
            return false;

        if (file.Length > 10 * 1024 * 1024)
            return false;

        return true;
    }
}
