using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class AddCategoryViewModel
    {
        public string CategoryName { get; set; }
        public int HouseholdId { get; set; }
        public string Type { get; set; }
    }
}