using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Models
{
    public class BudgetItemViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Budget")]
        public int BudgetId { get; set; }
        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; }
        [Display(Name = "Category")]
        public string CategoryName { get; set; }
        [Display(Name = "Category")]
        public SelectList IncomeCategoryList { get; set; }
        [Display(Name = "Category")]
        public SelectList ExpenseCategoryList { get; set; }
        //[Required(ErrorMessage = "No type selected")]
        //[Display(Name = "Category")]
        //public int SelectedCategory { get; set; }
        [Display(Name = "Category")]
        public int? SelectedIncomeCategory { get; set; }
        [Display(Name = "Category")]
        public int? SelectedExpenseCategory { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Required]
        [Range(typeof(decimal), "0.00", "1000000.00")]
        public Decimal Amount { get; set; }
        [Display(Name = "Repeats every month")]
        public bool IsRepeating { get; set; }
    }
}