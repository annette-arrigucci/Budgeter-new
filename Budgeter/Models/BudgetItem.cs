using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [Required]
        [Display(Name = "Budget")]
        public int BudgetId { get; set; }
        [Required]
        [Range(typeof(decimal), "0.00","1000000.00")]
        public Decimal Amount { get; set; }
        [Range(0,365)]
        [Display(Name = "Annual Frequency")]
        public int AnnualFrequency { get; set; }
    }
}