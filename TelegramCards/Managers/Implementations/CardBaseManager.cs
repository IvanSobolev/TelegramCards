using TelegramCards.Managers.Interfaces;
using TelegramCards.Models.DTO;
using TelegramCards.Models.Entitys;
using TelegramCards.Models.Enum;
using TelegramCards.Repositories.Interfaces;
using TelegramCards.Services.interfaces;

namespace TelegramCards.Managers.Implementations;

public class CardBaseManager (ICardBaseRepository cardBaseRepository, IFileDriveService fileDriveService) : ICardBaseManager
{
    private readonly ICardBaseRepository _cardBaseRepository = cardBaseRepository;
    private readonly IFileDriveService _fileDrive = fileDriveService;
    
    /// <inheritdoc/>
    public async Task<CardBaseOutputDto?> AddNewCardBaseAsync(AddCardBaseDto cardBaseDto)
    {
        if (!await _fileDrive.ValidateImage(cardBaseDto.File))
        {
            return null;
        }

        string fileUrl = await _fileDrive.UploadFileToDrive(cardBaseDto.File);
        
        CardBase? cardBase = await _cardBaseRepository.AddNewCardBaseAsync(cardBaseDto.AdminId, cardBaseDto.Rarity,
            fileUrl, cardBaseDto.PointsNumber);

        if (cardBase == null)
        {
            return null;
        }

        return new CardBaseOutputDto
        {
            RarityLevel = cardBase.RarityLevel, Id = cardBase.Id,
            CardPhotoUrl = cardBase.CardPhotoUrl, Points = cardBase.Points
        };
    }

    /// <inheritdoc/>
    public async Task<GetAllCardBaseDto> GetCardBasesAsync(long adminId, int page, int pageSize)
    {
        return await _cardBaseRepository.GetCardBasesAsync(adminId, page, pageSize);
    }
}