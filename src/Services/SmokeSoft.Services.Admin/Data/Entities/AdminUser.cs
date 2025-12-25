using SmokeSoft.Infrastructure.Data.Entities;

namespace SmokeSoft.Services.Admin.Data.Entities;

public class AdminUser : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin"; // Admin, SuperAdmin
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
}
