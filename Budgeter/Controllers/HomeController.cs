using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Budgeter.Models;

namespace Budgeter.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return RedirectToAction("Dashboard", "Home");
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            DateTime currentDate = DateTime.Now;
            if (User.Identity.IsInHousehold())
            {
                var householdId = User.Identity.GetHouseholdId();
                var household = db.Households.Find(householdId);
                if (household.Budgets.Where(x => x.Month == currentDate.Month).Where(x => x.Year == currentDate.Year).Any())
                {
                    var currentBudget = household.Budgets.Where(x => x.Month == currentDate.Month).Where(x => x.Year == currentDate.Year).First();
                    var budgetCategories = currentBudget.BudgetItems.GroupBy(x => x.CategoryId)
                                           .Select(grp => grp.First()).ToList();
                    var householdAccounts = household.Accounts.ToList();
                    var monthTransactions = new List<Transaction>();
                    foreach(var a in householdAccounts)
                    {
                        var transList = a.Transactions.Where(x => x.DateSpent.Year == currentDate.Year).Where(x => x.DateSpent.Month == currentDate.Month));
                        monthTransactions.AddRange(transList);
                    }
                    var transactionCategories = monthTransactions.GroupBy(x => x.CategoryId)
                                                .Select(grp => grp.First()).ToList();
                    //now we have a list of categories for budget and transactions, merge them into one master list


                }
                else
                {
                    ViewBag.Message = "No budget exists for this month";
                }

            }
            ViewBag.Message = "This is the dashboard page.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}