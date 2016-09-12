﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class Budget
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Name is required")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Household")]
        public string HouseholdId { get; set; }

        public Budget()
        {
            this.BudgetItems = new HashSet<BudgetItem>();
        }

        public virtual ICollection<BudgetItem> BudgetItems { get; set;}
    }
}