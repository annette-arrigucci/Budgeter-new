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
    public class AccountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Accounts
        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Index()
        {
            var hId = User.Identity.GetHouseholdId();
            //pass all the accounts that belong to my household and that are active (meaning not deleted)
            var accountsList = db.Accounts.Where(m => m.HouseholdId == hId).Where(x => x.IsActive == true).ToList();
            //for each active account, update the account balance in the database, then add it to the list
            var updatedAccountsList = new List<Account>();
            foreach(var a in accountsList)
            {
                a.UpdateAccountBalance();
                a.UpdateReconciledAccountBalance();
                updatedAccountsList.Add(a);
            }
            //send the list to the Index view
            return View(updatedAccountsList);
        }

        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult DeletedAccounts()
        {
            var hId = User.Identity.GetHouseholdId();
            //pass all the accounts that belong to my household and that are active (meaning not deleted)
            var accountsList = db.Accounts.Where(m => m.HouseholdId == hId).ToList();
            var inactiveAccountsList = accountsList.Where(x => x.IsActive == false).ToList();
            return View(inactiveAccountsList);
        }

        // GET: Accounts/Details/5
        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Details(int? id)
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
            account.UpdateAccountBalance();
            account.UpdateReconciledAccountBalance();
            return View(account);
        }

        // GET: Accounts/Create
        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Create()
        {
            var model = new CreateAccountViewModel();
            var typeList = new List<string>();
            typeList.Add("Checking");
            typeList.Add("Savings");
            model.TypeList = new SelectList(typeList);
            return View(model);
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,SelectedType,StartingBalance")] CreateAccountViewModel cavModel)
        {
            if (ModelState.IsValid)
            {
                var account = new Account();
                //we know the household Id is not null since the page requires a user to be in a household to view the page
                account.HouseholdId = (int)User.Identity.GetHouseholdId();
                account.Name = cavModel.Name;
                account.Type = cavModel.SelectedType;
                account.StartingBalance = cavModel.StartingBalance;              
                account.ReconciledBalance = 0.00M;
                account.IsActive = true;

                db.Accounts.Add(account);
                db.SaveChanges();

                
                return RedirectToAction("Index");
            }

            return View(cavModel);
        }

        // GET: Accounts/Edit/5
        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Edit(int? id)
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
            //check if the user is authorized to edit this account
            var helper = new AccountUserHelper();
            var user = User.Identity.GetUserId();
            if (helper.CanUserAccessAccount(user, (int)id) == false)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
            }
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Type")] Account model)
        {
            if (ModelState.IsValid)
            {
                var account = db.Accounts.Find(model.Id);
                account.Name = model.Name;

                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }          
            return View(model);
        }

        // GET: Accounts/Delete/5
        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Delete(int? id)
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
            //check if the user is authorized to delete this account
            var helper = new AccountUserHelper();
            var user = User.Identity.GetUserId();
            if (helper.CanUserAccessAccount(user, (int)id) == false)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
            }
            account.UpdateAccountBalance();
            account.UpdateReconciledAccountBalance();
            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (ModelState.IsValid)
            {
                Account account = db.Accounts.Find(id);
                if (account == null)
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
                }
                //check if the user is authorized to delete this account
                var helper = new AccountUserHelper();
                var user = User.Identity.GetUserId();
                if (helper.CanUserAccessAccount(user, id) == false)
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
                }
                account.IsActive = false;

                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(id);
        }

        // GET: Accounts/Restore/5
        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Restore(int? id)
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
            //check if the user is authorized to restore this account
            var helper = new AccountUserHelper();
            var user = User.Identity.GetUserId();
            if (helper.CanUserAccessAccount(user, (int)id) == false)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
            }
            account.UpdateAccountBalance();
            account.UpdateReconciledAccountBalance();
            return View(account);
        }

        // POST: Accounts/RestoreConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreConfirmed(int id)
        {
            if (ModelState.IsValid)
            {
                Account account = db.Accounts.Find(id);
                if (account == null)
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
                }
                //check if the user is authorized to delete this account
                var helper = new AccountUserHelper();
                var user = User.Identity.GetUserId();
                if (helper.CanUserAccessAccount(user, id) == false)
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
                }
                account.IsActive = true;

                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(id);
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
