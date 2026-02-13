using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartERP.Models.Entities
{
    public class InventoryAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Inventory")]
        public int InventoryId { get; set; }

        public Inventory? Inventory { get; set; }

        [Required]
        [ForeignKey("AssignedToUser")]
        public int AssignedToUserId { get; set; }

        public User? AssignedToUser { get; set; }

        [Required]
        public int QuantityAssigned { get; set; }

        [Required]
        public DateTime AssignmentDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string Remarks { get; set; } = string.Empty;

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public User? CreatedByUser { get; set; }
    }
}
