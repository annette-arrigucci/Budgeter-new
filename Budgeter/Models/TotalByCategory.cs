using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class TotalByCategory
    {
        public string CategoryName { get; set; }
        public Decimal Total { get; set; }
    }
}