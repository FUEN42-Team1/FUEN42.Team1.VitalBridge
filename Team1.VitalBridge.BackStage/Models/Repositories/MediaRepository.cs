using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly AppDbContext _context;

        public MediaRepository(AppDbContext context)
        {
            this._context = context;
        }

        public async Task AddAsync(Media media)
        {
            if (media == null)
            {
                throw new ArgumentNullException(nameof(media), "Media cannot be null");
            }
            // date
            media.UpdatedAt = DateTime.Now;
            media.CreatedAt = DateTime.Now;

            // Add the media to the context
            _context.Medias.Add(media);
            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id) 
        {
            var media = await _context.Medias.FindAsync(id);
            if (media == null)
            {
                throw new KeyNotFoundException($"Media with ID {id} not found.");
            }
            _context.Medias.Remove(media);
            await _context.SaveChangesAsync();
        }

        

        public async Task<IEnumerable<Media>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Media?>> GetByContentIdAsync(int contentId)
        {
            var medias = await _context.Medias
                .Where(m => m.ContentId == contentId)
                .ToListAsync();
            return medias;
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
