using System;
using System.ComponentModel.DataAnnotations;

namespace ARBISTO_POS.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }

        public string Type { get; set; } // Kitchen / Payment / System

        public int? ReferenceId { get; set; } // OrderId etc

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}