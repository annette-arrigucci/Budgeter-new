using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class EditCategoriesViewModel
    {
        public int HouseholdId { get; set; }
        [Display(Name="Income")]
        public CategoryCheckBox[] IncomeCategoriesToSelect { get; set; }
        [Display(Name = "Expense")]
        public CategoryCheckBox[] ExpenseCategoriesToSelect { get; set; }
    }
}