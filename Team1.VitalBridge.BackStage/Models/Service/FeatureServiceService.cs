using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Team1.VitalBridge.BackStage.Models.Service
{
    public class FeatureServiceService
    {
        private readonly AppDbContext _context;

        public FeatureServiceService(AppDbContext context)
        {
            _context = context;
        }

        public List<FeatureServiceViewModel> GetAll()
        {
            return _context.FeatureServices
                .Include(x => x.File)
                .Select(x => new FeatureServiceViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImageUrl = x.File.FileName, // 修正：使用 FileName 屬性
                    IsActive = x.IsActive
                })
                .ToList();
        }

        public void Create(FeatureServiceViewModel vm, int fileId)
        {
            var model = new FeatureService
            {
                Name = vm.Name,
                FileId = fileId, // 將 FileId 存入
                IsActive = true // 新增時預設為啟用
            };
            _context.FeatureServices.Add(model);
            _context.SaveChanges();
        }

        public void ToggleStatus(int id)
        {
            var service = _context.FeatureServices.Find(id);
            if (service != null)
            {
                service.IsActive = !service.IsActive;
                _context.SaveChanges();
            }
        }

        public FeatureServiceViewModel GetById(int id)
        {
            var service = _context.FeatureServices.Include(x => x.File).SingleOrDefault(x => x.Id == id);
            if (service == null)
            {
                return null;
            }

            return new FeatureServiceViewModel
            {
                Id = service.Id,
                Name = service.Name,
                ImageUrl = service.File.FileName, // 修正：使用 FileName 屬性
                IsActive = service.IsActive
            };
        }

        public void Update(FeatureServiceViewModel vm, int newFileId)
        {
            var service = _context.FeatureServices.Find(vm.Id);
            if (service != null)
            {
                service.Name = vm.Name;
                service.FileId = newFileId; // 更新 FileId
                service.IsActive = vm.IsActive;
                _context.SaveChanges();
            }
        }
    }
}