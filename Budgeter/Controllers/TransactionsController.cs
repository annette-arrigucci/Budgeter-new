﻿using System;
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
            
            var createTransactionModel = new TransactionCreateViewModel();
            createTransactionModel.AccountId = account.Id;
            var categories = db.Categories.ToList();
            createTransactionModel.CategoryList = new SelectList(categories,"Id","Name");
            var householdId = User.Identity.GetHouseholdId();
            var householdUsers = db.Users.Where(x => x.HouseholdId == (int)householdId).ToList();
            createTransactionModel.HouseholdUsersList = new SelectList(householdUsers,"Id","UserName");
            //pass a model to create a new transaction through the ViewBag
            ViewBag.CreateModel = createTransactionModel;

            //get all the transactions for this account
            var transactions = db.Transactions.Where(x => x.AccountId == id).ToList();
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

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AccountId,Description,DateSpent,Amount,Type,SelectedCategory,SelectedUser,ReconciledAmount")] TransactionCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var transaction = new Transaction();
                transaction.AccountId = model.AccountId;
                transaction.Description = model.Description;
                transaction.DateEntered = DateTime.Now;
                transaction.DateSpent = model.DateSpent;
                transaction.Amount = model.Amount;
                transaction.Type = model.Type;
                transaction.CategoryId = model.SelectedCategory;
                transaction.EnteredById = User.Identity.GetUserId();
                transaction.SpentById = model.SelectedUser;
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
            return RedirectToAction("Index", new { id = model.AccountId });
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,AccountId,Description,DateEntered,DateSpent,Amount,Type,CategoryId,EnteredById,SpentById,IsReconciled,ReconciledAmount")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            return RedirectToAction("Index");
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
