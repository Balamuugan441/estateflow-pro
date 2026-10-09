namespace Brokerage.Models.DTOs.Admin;

// Carries the target active or inactive state selected by the administrator.
public class AdminUserStatusRequest
{
    public bool IsActive { get; set; }
}