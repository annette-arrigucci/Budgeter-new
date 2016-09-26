using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Budgeter.Models;
using Microsoft.AspNet.Identity;

namespace Budgeter.Controllers
{
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            //check if the user is authorized to view this account
            var helper = new AccountUserHelper();
            var user = User.Identity.GetUserId();
            if (helper.CanUserAccessAccount(user, (int)id) == false)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
            }

            var catHelper = new CategoryHouseholdHelper();
            var householdId = User.Identity.GetHouseholdId();
            var createTransactionModel = new TransactionCreateViewModel();
            createTransactionModel.AccountId = account.Id;
            var categories = db.Categories.ToList();
            var incomeCategories = catHelper.GetIncomeCategories((int)householdId);
            var expenseCategories = catHelper.GetExpenseCategories((int)householdId);
            createTransactionModel.IncomeCategoryList = new SelectList(incomeCategories,"Id","Name");
            createTransactionModel.ExpenseCategoryList = new SelectList(expenseCategories, "Id", "Name");
            var householdUsers = db.Users.Where(x => x.HouseholdId == (int)householdId).ToList();
            createTransactionModel.HouseholdUsersList = new SelectList(householdUsers,"Id","DisplayName");
            //pass a model to create a new transaction through the ViewBag
            ViewBag.CreateModel = createTransactionModel;

            //get all the transactions for this account
            var transactions = db.Transactions.Where(x => x.AccountId == account.Id).Where(y => y.IsActive == true).ToList();
            //transform the transactions so we can show them in the index page
            var transactionsToShow = new List<TransactionsIndexViewModel>();
            foreach(var t in transactions)
            {
                var transToShow = new TransactionsIndexViewModel(t);
                transactionsToShow.Add(transToShow);
            }

            //pass the account Id to the model
            ViewBag.AccountId = account.Id;
            //get the account name and put it in the ViewBag
            ViewBag.AccountName = account.Name;
            //update the account balance
            account.UpdateAccountBalance();
            //update the reconciled balance
            account.UpdateReconciledAccountBalance();
            //pass the balances in the ViewBag
            ViewBag.Balance = account.Balance;
            ViewBag.Reconciled = account.ReconciledBalance;
            ViewBag.StartingBalance = account.StartingBalance;

            return View(transactionsToShow);
        }

        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult VoidedTransactions(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            //check if the user is authorized to view this account
            var helper = new AccountUserHelper();
            var user = User.Identity.GetUserId();
            if (helper.CanUserAccessAccount(user, (int)id) == false)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
            }

            //get all the voided transactions for this account
            var transactions = db.Transactions.Where(x => x.AccountId == account.Id).Where(y => y.IsActive == false).ToList();
            //transform the transactions so we can show them in the index page
            var transactionsToShow = new List<TransactionsIndexViewModel>();
            foreach (var t in transactions)
            {
                var transToShow = new TransactionsIndexViewModel(t);
                transactionsToShow.Add(transToShow);
            }

            //pass the account Id to the model
            ViewBag.AccountId = account.Id;
            //get the account name and put it in the ViewBag
            ViewBag.AccountName = account.Name;

            return View(transactionsToShow);
        }


        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AccountId,Description,DateSpent,Amount,Type,SelectedIncomeCategory,SelectedExpenseCategory,SelectedUser,ReconciledAmount")] TransactionCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var transaction = new Transaction();
                transaction.AccountId = model.AccountId;
                transaction.Description = model.Description;
                transaction.DateEntered = DateTime.Now;
                //check if the date entered has a valid year - within one year before or after the transaction was recorded
                if((model.DateSpent.Year < (transaction.DateEntered.Year - 1))||(model.DateSpent.Year > (transaction.DateEntered.Year + 1)))
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Transaction date out of range" });
                }
                else
                {
                    transaction.DateSpent = model.DateSpent;
                }              
                transaction.Amount = model.Amount;
                transaction.Type = model.Type;
                //transaction.CategoryId = model.SelectedCategory;
                //If income was selected, make sure the selected income property is not null
                if(model.Type == "Income")
                {
                    if (model.SelectedIncomeCategory == null)
                    {
                        return RedirectToAction("Index", "Errors", new { errorMessage = "Error in transaction category" });
                    }
                    transaction.CategoryId = (int)model.SelectedIncomeCategory;
                }
                //If expense was selected, make sure the selected income property is not null
                if (model.Type == "Expense")
                {
                    if(model.SelectedExpenseCategory == null)
                    {
                        return RedirectToAction("Index", "Errors", new { errorMessage = "Error in transaction category" });
                    }
                    transaction.CategoryId = (int)model.SelectedExpenseCategory;
                }
                transaction.EnteredById = User.Identity.GetUserId();
                transaction.SpentById = model.SelectedUser;
                transaction.IsActive = true;
                if (model.Amount == model.ReconciledAmount)
                {
                    transaction.IsReconciled = true;
                }
                else transaction.IsReconciled = false;
                transaction.ReconciledAmount = model.ReconciledAmount;

                db.Transactions.Add(transaction);
                db.SaveChanges();

                var account = db.Accounts.Find(model.AccountId);

                //update the account balance
                account.UpdateAccountBalance();
                //update the reconciled balance
                account.UpdateReconciledAccountBalance();
                return RedirectToAction("Index", new { id = model.AccountId });
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "Error in recording transaction" });
        }

        public ActionResult GetCreateView(int accountId)
        {     
            //create the object here
            if (accountId == null)
            {
                RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            var createTransactionModel = new TransactionCreateViewModel();
            createTransactionModel.AccountId = accountId;
            var categories = db.Categories.ToList();

            var catHelper = new CategoryHouseholdHelper();
            var householdId = User.Identity.GetHouseholdId();
            
            var incomeCategories = catHelper.GetIncomeCategories((int)householdId);
            var expenseCategories = catHelper.GetExpenseCategories((int)householdId);
            createTransactionModel.IncomeCategoryList = new SelectList(incomeCategories, "Id", "Name");
            createTransactionModel.ExpenseCategoryList = new SelectList(expenseCategories, "Id", "Name");

            var householdUsers = db.Users.Where(x => x.HouseholdId == (int)householdId).ToList();
            createTransactionModel.HouseholdUsersList = new SelectList(householdUsers, "Id", "DisplayName");
            return PartialView("_CreateTransaction", createTransactionModel);
        }

        public ActionResult GetView(int transactionId, string viewName)
        {
            object model = null;
            if (viewName == "_EditTransaction")
            {
                if (transactionId == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
                }
                Transaction transaction = db.Transactions.Find(transactionId);
                if (transaction == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
                }
                var tModel = new TransactionEditViewModel();
                tModel.Id = transaction.Id;
                tModel.AccountId = transaction.AccountId;
                tModel.Description = transaction.Description;
                tModel.DateSpent = transaction.DateSpent;
                tModel.Amount = transaction.Amount;
                tModel.Type = transaction.Type;
                         
                var householdId = User.Identity.GetHouseholdId();
                var householdUsers = db.Users.Where(x => x.HouseholdId == (int)householdId).ToList();
                tModel.HouseholdUsersList = new SelectList(householdUsers, "Id", "DisplayName", transaction.SpentById);

                //send categories to dropdown depending on whether transaction is of type income or expense
                var categories = new List<Category>();
                var catHelper = new CategoryHouseholdHelper();
                if(tModel.Type == "Income")
                {
                    categories = catHelper.GetIncomeCategories((int)householdId);
                }
                if (tModel.Type == "Expense")
                {
                    categories = catHelper.GetExpenseCategories((int)householdId);
                }
                tModel.SelectedCategory = transaction.CategoryId;
                tModel.CategoryList = new SelectList(categories, "Id", "Name", tModel.SelectedCategory);

                tModel.ReconciledAmount = transaction.ReconciledAmount;
                tModel.EnteredById = transaction.EnteredById;
                var user = db.Users.Find(transaction.EnteredById);
                tModel.EnteredByName = user.DisplayName;
                tModel.DateEntered = transaction.DateEntered;
                tModel.IsActive = transaction.IsActive;
                model = tModel;
            }
            else if(viewName == "_TransactionDetails" || viewName == "_DeleteTransaction" || viewName == "_VoidTransaction" || viewName == "_RestoreTransaction")
            {
                if (transactionId == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
                }
                Transaction transaction = db.Transactions.Find(transactionId);
                if (transaction == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
                }
                var infoModel = new TransactionInfoViewModel();
                infoModel.Id = transaction.Id;
                infoModel.AccountId = transaction.AccountId;
                infoModel.Description = transaction.Description;
                infoModel.DateSpent = transaction.DateSpent;
                infoModel.Amount = transaction.Amount;
                infoModel.Type = transaction.Type;
                var category = db.Categories.Find(transaction.CategoryId);
                infoModel.Category = category.Name;
                var user = db.Users.Find(transaction.SpentById);
                infoModel.SpentByName = user.DisplayName;
                infoModel.ReconciledAmount = transaction.ReconciledAmount;
                var enteredBy = db.Users.Find(transaction.EnteredById);
                infoModel.EnteredByName = enteredBy.DisplayName;
                infoModel.DateEntered = transaction.DateEntered;
                infoModel.IsActive = transaction.IsActive;
                model = infoModel;
            }          
            return PartialView(viewName, model);
        }

        // POST: Form posted in partial view
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,AccountId,Description,DateEntered,DateSpent,Amount,Type,SelectedCategory,EnteredById,SelectedUser,ReconciledAmount,IsActive")] TransactionEditViewModel tevModel)
        {
            if (ModelState.IsValid)
            {
                Transaction transaction = db.Transactions.Find(tevModel.Id);
                if (transaction == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
                }
                transaction.Id = tevModel.Id;
                transaction.AccountId = tevModel.AccountId;
                transaction.Description = tevModel.Description;
                transaction.DateEntered = tevModel.DateEntered;
                //check if the date entered has a valid year - within one year before or after the transaction was recorded
                if ((tevModel.DateSpent.Year < (transaction.DateEntered.Year - 1)) || (tevModel.DateSpent.Year > (transaction.DateEntered.Year + 1)))
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Transaction date out of range" });
                }
                else
                {
                    transaction.DateSpent = tevModel.DateSpent;
                }
                transaction.DateSpent = tevModel.DateSpent;
                transaction.Amount = tevModel.Amount;
                transaction.Type = tevModel.Type;
                transaction.CategoryId = tevModel.SelectedCategory;
                transaction.EnteredById = tevModel.EnteredById;
                transaction.SpentById = tevModel.SelectedUser;
                if (transaction.Amount == tevModel.ReconciledAmount)
                {
                    transaction.IsReconciled = true;
                }
                else transaction.IsReconciled = false;
                transaction.ReconciledAmount = tevModel.ReconciledAmount;
                transaction.IsActive = tevModel.IsActive;

                //update the transaction in the database
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                var account = db.Accounts.Find(tevModel.AccountId);

                //update the account balance in the database
                account.UpdateAccountBalance();
                //update the reconciled balance
                account.UpdateReconciledAccountBalance();
                db.SaveChanges();
                return RedirectToAction("Index", new { id = tevModel.AccountId });
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "Error in editing transaction" });
        }

        // POST: Form posted in partial view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (ModelState.IsValid)
            {
                Transaction transaction = db.Transactions.Find(id);
                var account = db.Accounts.Find(transaction.AccountId);
                if (account == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
                }
                db.Transactions.Remove(transaction);
                db.SaveChanges();

                //update the account balance in the database
                account.UpdateAccountBalance();
                //update the reconciled balance
                account.UpdateReconciledAccountBalance();
                db.SaveChanges();
                return RedirectToAction("Index", new { id = account.Id });
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "Error in deleting transaction" });
        }

        // POST: Form posted in partial view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VoidConfirmed(int id)
        {
            if (ModelState.IsValid)
            {
                Transaction transaction = db.Transactions.Find(id);
                if(transaction.IsActive == false)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Transaction has already been voided" });
                }
                var account = db.Accounts.Find(transaction.AccountId);
                if (account == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
                }

                transaction.IsActive = false;
                //update the transaction's status in the database
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                //update the account balance in the database
                account.UpdateAccountBalance();
                //update the reconciled balance
                account.UpdateReconciledAccountBalance();
                db.SaveChanges();
                return RedirectToAction("Index", new { id = account.Id });
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "Error in voiding transaction" });
        }

        // POST: Form posted in partial view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreConfirmed(int id)
        {
            if (ModelState.IsValid)
            {
                Transaction transaction = db.Transactions.Find(id);
                if (transaction.IsActive == true)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Transaction is already active" });
                }
                var account = db.Accounts.Find(transaction.AccountId);
                if (account == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
                }

                transaction.IsActive = true;
                //update the transaction's status in the database
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                //update the account balance in the database
                account.UpdateAccountBalance();
                //update the reconciled balance
                account.UpdateReconciledAccountBalance();
                db.SaveChanges();
                return RedirectToAction("Index", new { id = account.Id });
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "Error in voiding transaction" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        
    }
}
