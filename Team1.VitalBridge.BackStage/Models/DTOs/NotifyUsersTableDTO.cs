using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Dto
{
    public class NotifyUsersTableDTO
    {
        public int Id { get; set; } // 通知ID

        public int UserId { get; set; } // 使用者ID

        public string UserName { get; set; } // 使用者名稱

        public string UserEmail { get; set; } // 使用者電子郵件

        public string UserRoles { get; set; } // 使用者角色列表

    }
}
