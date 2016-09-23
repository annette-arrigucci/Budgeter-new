using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class SpendingCategory
    {
        public string CategoryName { get; set; }
        public Decimal BudgetTotal { get; set; }
        public Decimal TransactionTotal { get; set; }
    }
}