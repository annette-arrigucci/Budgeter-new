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
        [Display(Name = "Repeats every month")]
        public bool IsRepeating { get; set; }
        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }
        //this is to designate the original repeating item
        public bool IsOriginal { get; set; }
        //this is to designate whether the original repeating item is active or will not be repeated on future budgets
        public bool RepeatActive { get; set; }
        //if this item is a copy, store the ID of the BudgetItem
        public int? IsCopyOf { get; set; }
    }
}