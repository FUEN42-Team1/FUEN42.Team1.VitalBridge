using System.ComponentModel.DataAnnotations;

public sealed class AdminInstitutionUserListItemVM
{
    [Display(Name = "會員編號")]
    public string UserId { get; init; } = default!;

    [Display(Name = "姓名")]
    public string Name { get; init; } = default!;

    [Display(Name = "電子信箱")]
    public string? Email { get; init; }

    [Display(Name = "聯絡電話")]
    public string? Phone { get; init; }

    public int? InstitutionId { get; init; }

    [Display(Name = "機構編號")]
    public string? InstitutionCode { get; init; }

    [Display(Name = "機構名稱")]
    public string? InstitutionName { get; init; }

    [Display(Name = "狀態")]
    public string Status { get; init; } = "active";

    [Display(Name = "最後登入")]
    public DateTime? LastLoginAt { get; init; }

    // 角色（顯示用字串即可，最簡）
    [Display(Name = "身分")]
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
