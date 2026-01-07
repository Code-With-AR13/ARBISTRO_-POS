namespace ARBISTO_POS.Models
{
    public class UserPermission
    {

        public int Id { get; set; }
        public string Name { get; set; }       // e.g. "Manage Users"
        public string Category { get; set; }   // e.g. "Users area"
        public string? Description { get; set; }

        public ICollection<UserRolePermission> UserRolePermissions { get; set; }
    }
}
