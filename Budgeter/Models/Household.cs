using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class Household
    {
        public int Id { get; set; }
        [Required]
        [StringLength(160, MinimumLength=3)]
        public string Name { get; set; }
        public string Code { get; set; }

        public Household()
        {
            this.Budgets = new HashSet<Budget>();
            this.CategoryHouseholds = new HashSet<CategoryHousehold>();
            this.Accounts = new HashSet<Account>();
            this.Users = new HashSet<ApplicationUser>();
        }

        public virtual ICollection<Budget> Budgets { get; set; }
        public virtual ICollection<CategoryHousehold> CategoryHouseholds {get; set;}
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}