using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Repositories
{
    public class MediaRepository : IMediaRepository
    {
       
        public async Task AddAsync(Media media)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Media>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Media?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Media>> GetByTypeAsync(string type)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(Media media)
        {
            throw new NotImplementedException();
        }
    }
}
