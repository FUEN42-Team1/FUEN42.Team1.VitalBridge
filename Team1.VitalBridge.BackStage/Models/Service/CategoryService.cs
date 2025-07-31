using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;

namespace Team1.VitalBridge.BackStage.Models.Service
{
    public class CategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            this._repository = repository;
        }

        public void CreateCategory(string name)
        {
            _repository.CreateCategory(name);
        }

        public void deleteCategoryById(int id)
        {
            _repository.deleteCategoryById(id);
        }

        public List<NotifysCategory> getAllCategories()
        {
            return _repository.getAllCategories();
        }

        public NotifysCategory getCategoryById(int id)
        {
            return _repository.getCategoryById(id);
        }

        public void UpdateCategory(int id, NotifysCategory category)
        {
            _repository.UpdateCategory(id,category);
        }

        public void setEnable(int id,bool enable)
        {
            var category = _repository.getCategoryById(id);
            category.Enable = enable;
            _repository.UpdateCategory(id,category);
        }

        public void setName(int id, string name)
        {
            var category = _repository.getCategoryById(id);
            category.Name = name;
            _repository.UpdateCategory(id, category);
        }
    }
}
