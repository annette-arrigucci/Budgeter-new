using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class BudgetItemCreateViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Budget")]
        public int BudgetId { get; set; }
        [Display(Name = "Category")]
        public string[] IncomeCategories { get; set; }
        [Display(Name = "Category")]
        public string[] ExpenseCategories { get; set; }
        [Required]
        [Display(Name = "Category")]
        public string CategoryName { get; set; }
        [Required]
        [Range(typeof(decimal), "0.00", "1000000.00")]
        public Decimal Amount { get; set; }
        [Display(Name = "Repeats every month")]
        public bool IsRepeating { get; set; }
    }
}