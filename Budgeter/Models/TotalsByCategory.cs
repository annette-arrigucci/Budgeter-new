using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class TotalsByCategory
    {
        public string CategoryName { get; set; }
        public Decimal BudgetTotal { get; set; }
        public Decimal TransactionTotal { get; set; }
    }
}