using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class TransactionsIndexViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Account")]
        public string Account { get; set; }
        public string Description { get; set; }
        [Display(Name = "Date")]
        public DateTime DateSpent { get; set; }
        public Decimal Amount { get; set; }
        public string Type { get; set; }
        [Display(Name = "Category")]
        public string Category { get; set; }
        [Display(Name = "Entered by")]
        public string EnteredBy { get; set; }
        [Display(Name = "Spent by")]
        public string SpentBy { get; set; }
        [Display(Name = "Reconciled")]
        public bool IsReconciled { get; set; }
        [Display(Name = "Reconciled")]
        public Decimal ReconciledAmount { get; set; }

        public TransactionsIndexViewModel()
        {

        }
        public TransactionsIndexViewModel(Transaction trans)
        {
            var db = new ApplicationDbContext();
            this.Id = trans.Id;
            var account = db.Accounts.Find(trans.AccountId);
            this.Account = account.Name;
            this.Description = trans.Description;
            this.DateSpent = trans.DateSpent;
            this.Amount = trans.Amount;
            this.Type = trans.Type;
            var category = db.Categories.Find(trans.CategoryId);
            this.Category = category.Name;
            var enteredBy = db.Users.Find(trans.EnteredById);
            this.EnteredBy = enteredBy.FirstName + " " + enteredBy.LastName;
            this.IsReconciled = trans.IsReconciled;
            this.ReconciledAmount = trans.ReconciledAmount;
            var spentBy = db.Users.Find(trans.SpentById);
            this.SpentBy = spentBy.DisplayName;
        }
    }
}