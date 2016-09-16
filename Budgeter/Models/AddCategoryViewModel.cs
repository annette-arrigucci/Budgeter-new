using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class AddCategoryViewModel
    {
        [Required(ErrorMessage = "No category entered")]
        public string CategoryName { get; set; }
        public int HouseholdId { get; set; }
        public string Type { get; set; }
    }
}