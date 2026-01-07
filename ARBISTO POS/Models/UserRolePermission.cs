namespace ARBISTO_POS.Models
{
    public class UserRolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        public UserRole Role { get; set; }
        public UserPermission UserPermission { get; set; }
    }
}
