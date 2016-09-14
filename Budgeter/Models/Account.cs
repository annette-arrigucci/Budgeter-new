using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class Account
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Household")]
        public int HouseholdId { get; set; }
        [Required]
        [StringLength(160, MinimumLength = 3)]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public Decimal StartingBalance { get; set; }
        [Required]
        public Decimal Balance { get; set; }
        [Display(Name = "Reconciled Balance")]
        public Decimal ReconciledBalance { get; set; }
        public bool IsActive { get; set; }
        private ApplicationDbContext db;

        public Account()
        {
            this.Transactions = new HashSet<Transaction>();
            db = new ApplicationDbContext();
        }

        public virtual ICollection<Transaction> Transactions { get; set; }

        public void UpdateAccountBalance()
        {
            //find the transactions for this account - include only active (not voided) transactions
            var transactions = db.Transactions.Where(x => x.AccountId == this.Id).Where(x => x.IsActive == true).ToList();
            var account = db.Accounts.Find(this.Id);
            Decimal total = 0;
            total += StartingBalance;
            foreach (var t in transactions)
            {
                if (t.Type.Equals("Income"))
                {
                    total += t.Amount;
                }
                else if (t.Type.Equals("Expense"))
                {
                    total -= t.Amount;
                }       
            }
            account.Balance = total;
            db.Entry(account).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void UpdateReconciledAccountBalance()
        {
            //find the transactions for this account - include only active (not voided) transactions
            var transactions = db.Transactions.Where(x => x.AccountId == this.Id).Where(x => x.IsActive == true).ToList();
            var account = db.Accounts.Find(this.Id);
            //computing reconciled balance the same way we compute the regular balance
            Decimal recTotal = 0;
            recTotal += StartingBalance;
            foreach (var t in transactions)
            {
                if (t.Type.Equals("Income"))
                {
                    recTotal += t.ReconciledAmount;
                }
                else if (t.Type.Equals("Expense"))
                {
                    recTotal -= t.ReconciledAmount;
                }
            }
            account.ReconciledBalance = recTotal;
            db.Entry(account).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}