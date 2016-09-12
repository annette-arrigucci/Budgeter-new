using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class EmailViewModel
    {
        public string Email { get; set; }
        public int? HouseholdId { get; set; }
    }
}