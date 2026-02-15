using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartERP.Models.Entities
{
    public class Billing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string BillNumber { get; set; } = string.Empty; // Unique bill identifier

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        public int BillingMonth { get; set; } // 1-12

        [Required]
        public int BillingYear { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BillAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousDue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // "Pending", "Paid", "Partial", "Overdue"

        public DateTime BillDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // "Cash", "UPI", "Bank Transfer", etc.

        [MaxLength(100)]
        public string TransactionReference { get; set; } = string.Empty;

        public int? RecoveryPersonId { get; set; }

        [ForeignKey("RecoveryPersonId")]
        public RecoveryPerson? RecoveryPerson { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastModifiedDate { get; set; }

        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public User? CreatedByUser { get; set; }

        public int? LastModifiedBy { get; set; }

        [ForeignKey("LastModifiedBy")]
        public User? LastModifiedByUser { get; set; }
    }
}
