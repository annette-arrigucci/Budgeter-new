﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Controllers
{
    public class BudgetItemController : Controller
    {
        // GET: BudgetItem
        public ActionResult Index()
        {
            return View();
        }

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
            return RedirectToAction("Index", new { id = model.AccountId });
        }
    }
}