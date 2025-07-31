using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Interface
{
    public interface ICategoryRepository
    {
        public NotifysCategory getCategoryById(int id);

        public void deleteCategoryById(int id);

        public List<NotifysCategory> getAllCategories();

        public void CreateCategory(string name);

        public void UpdateCategory(int id, NotifysCategory category);
    }
}
