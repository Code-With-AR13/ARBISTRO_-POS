namespace ARBISTO_POS.Models
{
    public class UserRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<UserRolePermission> UserRolePermissions { get; set; }
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}
