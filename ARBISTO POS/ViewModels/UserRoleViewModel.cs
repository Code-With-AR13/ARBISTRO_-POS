using ARBISTO_POS.Models;
using System.ComponentModel.DataAnnotations;

namespace ARBISTO_POS.ViewModels
{
    public class UserRoleViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        // Selected permission IDs from checkboxes
        public List<int> SelectedPermissions { get; set; } = new();
        public ICollection<UserRole> UsersRole { get; set; } = new List<UserRole>();
    }
}
