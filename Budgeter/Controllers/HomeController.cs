using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Budgeter.Models;
using Newtonsoft.Json;

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
        [AuthorizeHouseholdRequired]
        public ActionResult Dashboard()
        {
            //DateTime currentDate = DateTime.Now;
            //if (User.Identity.IsInHousehold())
            //{
            //    //get the household for this user
            //    var householdId = User.Identity.GetHouseholdId();
            //    var household = db.Households.Find(householdId);
            //    //find the current budget for this user
            //    if (household.Budgets.Where(x => x.Month == currentDate.Month).Where(x => x.Year == currentDate.Year).Any())
            //    {
            //        var currentBudget = household.Budgets.Where(x => x.Month == currentDate.Month).Where(x => x.Year == currentDate.Year).First();
            //        //get all the expense categories for the current budget
            //        var budgetCategories = currentBudget.BudgetItems.Where(x => x.Type == "Expense").GroupBy(x => x.CategoryId)
            //                               .Select(grp => grp.First()).Select(x => x.CategoryId).ToList();
            //        //get the accounts for this household
            //        var householdAccounts = household.Accounts.ToList();
            //        //get the transactions for this month
            //        var monthTransactions = new List<Transaction>();
            //        foreach(var a in householdAccounts)
            //        {
            //            var transList = a.Transactions.Where(x => x.DateSpent.Year == currentDate.Year).Where(x => x.DateSpent.Month == currentDate.Month);
            //            monthTransactions.AddRange(transList);
            //        }
            //        //get the expense transaction categories
            //        var transactionCategories = monthTransactions.Where(x => x.Type == "Expense").GroupBy(x => x.CategoryId)
            //                                    .Select(grp => grp.First()).Select(x => x.CategoryId).ToList();
            //        //now we have a list of categories for budget and transactions, merge them into one master list
            //        var masterCategoriesList = budgetCategories;
            //        masterCategoriesList.AddRange(transactionCategories);
            //        masterCategoriesList = masterCategoriesList.Distinct().ToList();
            //        ViewBag.CategoriesList = masterCategoriesList;

            //        var spendingCategoriesList = new List<SpendingCategory>();
            //        foreach(var c in masterCategoriesList)
            //        {
            //            var category = db.Categories.Find(c);
            //            var categoryName = category.Name; 
            //            var budgetCategoryTotal = currentBudget.BudgetItems.Where(x => x.CategoryId == c).Select(x => x.Amount).Sum();
            //            var transactionCategoryTotal = monthTransactions.Where(x => x.CategoryId == c).Select(x => x.Amount).Sum();
            //            var spendItem = new SpendingCategory { CategoryName = categoryName, BudgetTotal = budgetCategoryTotal, TransactionTotal = transactionCategoryTotal };
            //            spendingCategoriesList.Add(spendItem);
            //        }

            //    }
            //    else
            //    {
            //        ViewBag.Message = "No budget exists for this month";
            //    }

            //}
            

            return View();
        }

        public ActionResult GetSpendingCategoryChart()
        {
            //get an array of SpendingCategory objects with category name, budget total and transaction total
            var spendingData = GetSpendingCategoryData();
            if(spendingData != null)
            {
                var s = new Object[spendingData.Length];
                for(int i = 0; i < spendingData.Length; i++)
                {
                    s[i] = new { y = spendingData[i].CategoryName, a = spendingData[i].BudgetTotal, b = spendingData[i].TransactionTotal };
                }
                return Content(JsonConvert.SerializeObject(s), "application/json");
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "No data found for this month" });
        }

        public SpendingCategory[] GetSpendingCategoryData()
        {
            DateTime currentDate = DateTime.Now;
            SpendingCategory[] spendingCategoriesList = null;
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
                    foreach (var a in householdAccounts)
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

                    spendingCategoriesList = new SpendingCategory[masterCategoriesList.Count];
                    int i = 0;
                    foreach (var c in masterCategoriesList)
                    {
                        var category = db.Categories.Find(c);
                        spendingCategoriesList[i] = new SpendingCategory();
                        spendingCategoriesList[i].CategoryName = category.Name;
                        spendingCategoriesList[i].BudgetTotal = currentBudget.BudgetItems.Where(x => x.CategoryId == c).Select(x => x.Amount).Sum();
                        spendingCategoriesList[i].TransactionTotal = monthTransactions.Where(x => x.CategoryId == c).Select(x => x.Amount).Sum();
                        i++;
                    }
                    return spendingCategoriesList;
                }
                else
                {
                    return spendingCategoriesList;
                }
            }
            else
            {
                return spendingCategoriesList;
            }
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