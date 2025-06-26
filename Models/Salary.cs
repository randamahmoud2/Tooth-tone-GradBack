using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalManagementAPI.Models
{
    public class Salary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReceptionistId { get; set; }

        [Required]
        [StringLength(50)]
        public string Month { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowances { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // "Paid" or "Pending"
    }
}