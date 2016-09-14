using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Account")]
        public int AccountId { get; set; }
        [Required]
        [StringLength(300, MinimumLength = 3)]
        public string Description { get; set; }
        public DateTime DateEntered { get; set; }
        [Required]
        public DateTime DateSpent { get; set; }
        [Required]
        public Decimal Amount { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [Required]
        [Display(Name = "Entered by")]
        public string EnteredById { get; set; }
        [Required]
        [Display(Name = "Spent by")]
        public string SpentById { get; set; }
        [Display(Name = "Reconciled")]
        public bool IsReconciled { get; set; }
        [Display(Name = "Reconciled Amount")]
        public Decimal ReconciledAmount { get; set; }
        public bool IsActive { get; set; }
    }
}