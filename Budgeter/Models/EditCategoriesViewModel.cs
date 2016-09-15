using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class EditCategoriesViewModel
    {
        public int HouseholdId { get; set; }
        public CategoryCheckBox[] IncomeCategoriesToSelect { get; set; }
        public CategoryCheckBox[] ExpenseCategoriesToSelect { get; set; }
    }
}