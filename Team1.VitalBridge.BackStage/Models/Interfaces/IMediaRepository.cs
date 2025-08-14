using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IMediaRepository
    {
        Task<IEnumerable<Media>> GetAllAsync();
        Task<Media?> GetByIdAsync(int id);
        Task AddAsync(Media media);
        Task UpdateAsync(Media media);
        Task DeleteAsync(int id);
        Task<IEnumerable<Media?>> GetByContentIdAsync(int contentId);
        Task<IEnumerable<Media>> GetByTypeAsync(string type);

    }
}
