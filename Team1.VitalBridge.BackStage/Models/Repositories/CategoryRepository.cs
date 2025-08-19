using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;

namespace Team1.VitalBridge.BackStage.Models.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            this._context = context;
        }

        public void CreateCategory(string name)
        {
            if(!checkName(name))
            {
                throw new Exception("名稱已存在");
            }
            NotifysCategory Category = new NotifysCategory
            {
                Name = name,
                Enable = true,
            };
            _context.NotifysCategories.Add(Category);
            _context.SaveChanges();
        }
        private bool checkName(string name)
        {
            var category = _context.NotifysCategories.FirstOrDefault(c => c.Name == name);
            return category == null;
        }

        public void deleteCategoryById(int id)
        {
            var category = _context.NotifysCategories.Find(id);
            _context.NotifysCategories.Remove(category);
            _context.SaveChanges();
        }

        public List<NotifysCategory> getAllCategories()
        {
            var Categories = _context.NotifysCategories.ToList();
            return Categories;
        }

        public NotifysCategory getCategoryById(int id)
        {
            var Category = _context.NotifysCategories.Find(id);
            return Category;
        }

        public void UpdateCategory(int id,NotifysCategory newCategory)
        {
            var category = _context.NotifysCategories.Find(id);
            if (category.Name!=newCategory.Name&&!checkName(newCategory.Name))
            {
                throw new Exception("名稱已存在");
            }
            category.Name = newCategory.Name;
            category.Enable = newCategory.Enable;
            _context.Update(category);
            _context.SaveChanges();

        }
    }
}
