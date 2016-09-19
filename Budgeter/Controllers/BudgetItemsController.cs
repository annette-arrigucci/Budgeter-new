using Budgeter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Controllers
{
    public class BudgetItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BudgetItem
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BudgetId,Type,CategoryName,Description,Amount,IsRepeating")] BudgetItemCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var bItem = new BudgetItem();
                var householdId = User.Identity.GetHouseholdId();
                bItem.BudgetId = model.BudgetId;
                //check if the category exists
                if (db.Categories.Any(x => x.Name == model.CategoryName)){                   
                    var category = db.Categories.First(x => x.Name == model.CategoryName);
                    //check if this is one of the household's budget categories
                    var assignment = db.CategoryHouseholds.Where(x => x.CategoryId == category.Id).Where(x => x.HouseholdId == householdId).ToList();
                    if(assignment.Count > 0)
                    {
                        bItem.CategoryId = category.Id;
                    }        
                }
                else
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Category not found" });
                }
                bItem.Description = model.Description;
                bItem.Type = model.Type;
                bItem.Amount = model.Amount;
                bItem.IsRepeating = model.IsRepeating;
                db.BudgetItems.Add(bItem);
                db.SaveChanges();

                //    var transaction = new Transaction();
                //    transaction.AccountId = model.AccountId;
                //    transaction.Description = model.Description;
                //    transaction.DateEntered = DateTime.Now;
                //    transaction.DateSpent = model.DateSpent;
                //    transaction.Amount = model.Amount;
                //    transaction.Type = model.Type;
                //    transaction.CategoryId = model.SelectedCategory;
                //    transaction.EnteredById = User.Identity.GetUserId();
                //    transaction.SpentById = model.SelectedUser;
                //    transaction.IsActive = true;
                //    if (model.Amount == model.ReconciledAmount)
                //    {
                //        transaction.IsReconciled = true;
                //    }
                //    else transaction.IsReconciled = false;
                //    transaction.ReconciledAmount = model.ReconciledAmount;

                //    db.Transactions.Add(transaction);
                //    db.SaveChanges();

                //    var account = db.Accounts.Find(model.AccountId);

                //    //update the account balance
                //    account.UpdateAccountBalance();
                //    //update the reconciled balance
                //    account.UpdateReconciledAccountBalance();
                return RedirectToAction("Details", "Budgets", new { id = model.BudgetId });
            }
            return RedirectToAction("Details", "Budgets", new { id = model.BudgetId });
          }
    }
}