using System.ComponentModel.DataAnnotations;

namespace ARBISTO_POS.Models
{
    public class AppSetttingPrinter
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string KitchenPrinter { get; set; }
    }
}
