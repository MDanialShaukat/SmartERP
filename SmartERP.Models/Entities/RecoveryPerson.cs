using System;
using System.ComponentModel.DataAnnotations;

namespace SmartERP.Models.Entities
{
    public class RecoveryPerson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string PersonName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastModifiedDate { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastModifiedBy { get; set; }
    }
}
