using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class LeaveHouseholdViewModel
    {
        public string UserId { get; set; }
        public int? HouseholdId { get; set; }
        public bool HasAgreedToLeave { get; set; }
    }
}