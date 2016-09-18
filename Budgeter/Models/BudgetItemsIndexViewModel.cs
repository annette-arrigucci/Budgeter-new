using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class BudgetItemsIndexViewModel
    {
        public List<BudgetItem> ExpenseItems { get; set; }
        public List<BudgetItem> IncomeItems { get; set; }
    }
}