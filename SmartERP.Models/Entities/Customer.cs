using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartERP.Models.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CustomerCode { get; set; } = string.Empty; // Unique customer identifier

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PinCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string PackageType { get; set; } = string.Empty; // e.g., "Basic", "Premium", "Custom"

        [Column(TypeName = "decimal(18,2)")]
        public decimal PackageAmount { get; set; }

        [MaxLength(100)]
        public string ConnectionType { get; set; } = string.Empty; // e.g., "Fiber", "Cable", etc.

        public DateTime ConnectionDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string AdditionalDetails { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingBalance { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastModifiedDate { get; set; }

        public int CreatedBy { get; set; }

        public int? LastModifiedBy { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
