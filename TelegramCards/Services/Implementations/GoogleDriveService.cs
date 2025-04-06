using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using TelegramCards.Services.interfaces;

namespace TelegramCards.Services.Implementations;

public class GoogleDriveService : IFileDriveService
{
    private readonly string[] _scopes = { DriveService.Scope.DriveFile };
    private readonly string _applicationName = "cardsholder";
    private readonly string _credentialsPath = "credentials.json";
    private readonly string _folderId = "1sqls9luquX3QURDwrar7--IfJnzgsADe";
    
    public async Task<bool> ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
            return false;

        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        if (Array.IndexOf(allowedMimeTypes, file.ContentType) == -1)
            return false;

        if (file.Length > 10 * 1024 * 1024)
            return false;

        return true;
    }

    public async Task<string> UploadFileToDrive(IFormFile file)
    {
        UserCredential credential;

        await using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                _scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("token.json", true));
        }

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName,
        });

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = file.FileName,
            Parents = new[] { _folderId }
        };

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var request = service.Files.Create(fileMetadata, memoryStream, file.ContentType);
            request.Fields = "id, webViewLink";

            var result = await request.UploadAsync(CancellationToken.None);
            if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw new Exception("Upload failed: " + result.Exception?.Message);

            var uploadedFile = request.ResponseBody;
            return uploadedFile.WebViewLink ?? $"https://drive.google.com/file/d/{uploadedFile.Id}/view";
        }
    }
}