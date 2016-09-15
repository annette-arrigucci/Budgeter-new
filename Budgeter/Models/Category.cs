using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [StringLength(160, MinimumLength=3)]
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsDefault { get; set; }

        public Category()
        {
            this.CategoryHouseholds = new HashSet<CategoryHousehold>();
            this.BudgetItems = new HashSet<BudgetItem>();
            this.Transactions = new HashSet<Transaction>();
        }

        public virtual ICollection<CategoryHousehold> CategoryHouseholds { get; set; }
        public virtual ICollection<BudgetItem> BudgetItems { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}