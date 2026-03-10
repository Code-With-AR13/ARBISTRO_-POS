using System.ComponentModel.DataAnnotations;

namespace ARBISTO_POS.Models
{
    public class AppSettting
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string KitchenPrinter { get; set; }
        public string SingleTableMultiOrder { get; set; } = "No";
    }
}
