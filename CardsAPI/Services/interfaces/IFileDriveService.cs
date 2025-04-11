namespace TelegramCards.Services.interfaces;

public interface IFileDriveService
{
    Task<bool> ValidateImage(IFormFile file);
    Task<string> UploadFileToDrive(IFormFile file);
}