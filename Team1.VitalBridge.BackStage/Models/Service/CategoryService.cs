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
            try
            {
                _repository.CreateCategory(name);

            }
            catch(Exception ex)
            {
                throw ex;
            }
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
            try
            {
                _repository.UpdateCategory(id, category);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void setEnable(int id,bool enable)
        {
            var category = _repository.getCategoryById(id);
            category.Enable = enable;
            UpdateCategory(id,category);
        }

        public void setName(int id, string name)
        {
            var category = _repository.getCategoryById(id);
            category.Name = name;
            UpdateCategory(id, category);
        }
    }
}
