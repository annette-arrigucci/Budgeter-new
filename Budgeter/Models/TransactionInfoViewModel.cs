using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Models
{
    public class TransactionInfoViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Account")]
        public int AccountId { get; set; }
        [StringLength(300, MinimumLength = 3)]
        public string Description { get; set; }
        [Display(Name = "Transaction date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime DateSpent { get; set; }
        public Decimal Amount { get; set; }
        public string Type { get; set; }
        [Display(Name = "Category")]
        public string Category { get; set; }
        [Display(Name = "Transaction by")]
        public string SpentByName { get; set; }
        [Display(Name = "Reconciled amount")]
        public Decimal ReconciledAmount { get; set; }
        [Display(Name = "Entered by")]
        public string EnteredByName { get; set; }
        [Display(Name = "Date entered")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime DateEntered { get; set; }
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
    }
}