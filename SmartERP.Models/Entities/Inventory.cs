using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartERP.Models.Entities
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // e.g., "Cable", "Equipment", etc.

        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty; // e.g., "Meter", "Piece", "Box"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        [Required]
        public int QuantityPurchased { get; set; }

        [Required]
        public int QuantityUsed { get; set; }

        [Required]
        public int QuantityRemaining { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPurchaseAmount { get; set; }

        [MaxLength(200)]
        public string Supplier { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastModifiedDate { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public User? CreatedByUser { get; set; }

        public int? LastModifiedBy { get; set; }

        [ForeignKey("LastModifiedBy")]
        public User? LastModifiedByUser { get; set; }
    }
}
