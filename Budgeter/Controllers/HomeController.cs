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
                //get the household for this user
                var householdId = User.Identity.GetHouseholdId();
                var household = db.Households.Find(householdId);
                //find the current budget for this user
                if (household.Budgets.Where(x => x.Month == currentDate.Month).Where(x => x.Year == currentDate.Year).Any())
                {
                    var currentBudget = household.Budgets.Where(x => x.Month == currentDate.Month).Where(x => x.Year == currentDate.Year).First();
                    //get all the expense categories for the current budget
                    var budgetCategories = currentBudget.BudgetItems.Where(x => x.Type == "Expense").GroupBy(x => x.CategoryId)
                                           .Select(grp => grp.First()).Select(x => x.CategoryId).ToList();
                    //get the accounts for this household
                    var householdAccounts = household.Accounts.ToList();
                    //get the transactions for this month
                    var monthTransactions = new List<Transaction>();
                    foreach(var a in householdAccounts)
                    {
                        var transList = a.Transactions.Where(x => x.DateSpent.Year == currentDate.Year).Where(x => x.DateSpent.Month == currentDate.Month);
                        monthTransactions.AddRange(transList);
                    }
                    //get the expense transaction categories
                    var transactionCategories = monthTransactions.Where(x => x.Type == "Expense").GroupBy(x => x.CategoryId)
                                                .Select(grp => grp.First()).Select(x => x.CategoryId).ToList();
                    //now we have a list of categories for budget and transactions, merge them into one master list
                    var masterCategoriesList = budgetCategories;
                    masterCategoriesList.AddRange(transactionCategories);
                    masterCategoriesList = masterCategoriesList.Distinct().ToList();
                    ViewBag.CategoriesList = masterCategoriesList;
                    foreach(var c in masterCategoriesList)
                    {
                        var category = db.Categories.Find(c);
                        var categoryName = category.Name;
                        var budgetCategoryTotal = currentBudget.BudgetItems.Where(x => x.CategoryId == c);
                        var transactionCategoryTotal = monthTransactions.Where(x => x.CategoryId == c);
                    }

                }
                else
                {
                    ViewBag.Message = "No budget exists for this month";
                }

            }
            
            //ViewBag.Message = "This is the dashboard page.";

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