namespace Team1.VitalBridge.BackStage.Models.ViewModels.Admin
{
    public class AdminEditRolesVM
    {
        public string UserId { get; set; }

        public List<RoleOptionVM> AllRoles { get; set; } = new(); // 系統中可選的管理員角色
        public List<string> SelectedRoles { get; set; } = new();  // 目前擁有的角色Code


    }


    public class RoleOptionVM
    {
        public string Code { get; set; } // 例如：MainAdmin、Admin、ContentMaintainer...
        public string Name { get; set; } // 例如：主要管理員、一般管理員、資料維護
    }
}
