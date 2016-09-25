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
            DateTime currentDate = DateTime.Now;
            var householdId = User.Identity.GetHouseholdId();
            var household = db.Households.Find(householdId);
            //find the current budget for this user
            if (household.Budgets.Where(x => x.Month == currentDate.Month).Where(x => x.Year == currentDate.Year).Any())
            {
                var currentBudget = household.Budgets.Where(x => x.Month == currentDate.Month).Where(x => x.Year == currentDate.Year).First();
                ViewBag.BudgetName = currentBudget.Name;
            }
            else
            {
                ViewBag.HasBudget = false;
            }
            ViewBag.RecentTransactions = GetRecentTransactions((int)householdId);
            var householdAccounts = household.Accounts.ToList();
            foreach(var h in householdAccounts)
            {
                h.UpdateAccountBalance();
                h.UpdateReconciledAccountBalance();
            }
            ViewBag.Accounts = householdAccounts;
            return View();
        }

        public List<TransactionsIndexViewModel> GetRecentTransactions(int householdId)
        {
            DateTime currentDate = DateTime.Now;
            var household = db.Households.Find(householdId);
            var householdAccounts = household.Accounts.ToList();
            //get the transactions for this month
            var monthTransactions = new List<Transaction>();
            foreach (var a in householdAccounts)
            {
                var transList = a.Transactions.Where(x => x.DateSpent.Year == currentDate.Year).Where(x => x.DateSpent.Month == currentDate.Month);
                monthTransactions.AddRange(transList);
            }
            var recentTransactions = monthTransactions.OrderByDescending(x => x.DateSpent).ToList();

            if (recentTransactions.Count > 4)
            {
                recentTransactions = recentTransactions.GetRange(0, 3);
            }
            var recentList = new List<TransactionsIndexViewModel>();
            foreach (var r in recentTransactions)
            {
                var recent = new TransactionsIndexViewModel(r);
                recentList.Add(recent);
            }
            return recentList;
        }

        public ActionResult GetCategoriesChart()
        {
            //get an array of SpendingCategory objects with category name, budget total and transaction total
            var spendingData = GetCategoryTotals();
            if(spendingData != null)
            {
                var s = new Object[spendingData.Length];
                for(int i = 0; i < spendingData.Length; i++)
                {
                    s[i] = new { y = spendingData[i].CategoryName, a = spendingData[i].TransactionTotal, b = spendingData[i].BudgetTotal };
                }
                return Content(JsonConvert.SerializeObject(s), "application/json");
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "No data found for this month" });
        }

        public TotalsByCategory[] GetCategoryTotals()
        {
            DateTime currentDate = DateTime.Now;
            TotalsByCategory[] spendingCategoriesList = null;
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

                    spendingCategoriesList = new TotalsByCategory[masterCategoriesList.Count];
                    int i = 0;
                    foreach (var c in masterCategoriesList)
                    {
                        var category = db.Categories.Find(c);
                        spendingCategoriesList[i] = new TotalsByCategory();
                        spendingCategoriesList[i].CategoryName = category.Name;
                        spendingCategoriesList[i].TransactionTotal = monthTransactions.Where(x => x.CategoryId == c).Select(x => x.Amount).Sum();
                        spendingCategoriesList[i].BudgetTotal = currentBudget.BudgetItems.Where(x => x.CategoryId == c).Select(x => x.Amount).Sum();
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

        public ActionResult GetIncomeExpenseChart()
        {
            var data = GetIncomeExpenseData();
            if (data != null)
            {
                var s = new[]
                {
                    new { y = data[0].Type, a = data[0].Total },
                    new { y = data[1].Type, a = data[1].Total }
                };
                return Content(JsonConvert.SerializeObject(s), "application/json");
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "Error in retrieving income/expense data" });
        }

        public TotalByType[] GetIncomeExpenseData()
        {
            DateTime currentDate = DateTime.Now;
            var incomeExpenseDataList = new TotalByType[2];
            if (User.Identity.IsInHousehold())
            {
                //get the household for this user
                var householdId = User.Identity.GetHouseholdId();
                var household = db.Households.Find(householdId);

                //get the accounts for this household
                var householdAccounts = household.Accounts.ToList();

                //TO DO: what do we do if user has no accounts set up?

                //get the transactions for this month
                var monthTransactions = new List<Transaction>();
                foreach (var a in householdAccounts)
                {
                    var transList = a.Transactions.Where(x => x.DateSpent.Year == currentDate.Year).Where(x => x.DateSpent.Month == currentDate.Month);
                    monthTransactions.AddRange(transList);
                }

                var incomeTotal = monthTransactions.Where(x => x.Type == "Income").Select(x => x.Amount).Sum();
                var expenseTotal = monthTransactions.Where(x => x.Type == "Expense").Select(x => x.Amount).Sum();

                incomeExpenseDataList[0] = new TotalByType { Type = "Income", Total = incomeTotal };
                incomeExpenseDataList[1] = new TotalByType { Type = "Expense", Total = expenseTotal };
                return incomeExpenseDataList;
            }
            return incomeExpenseDataList;
        }

        public ActionResult GetExpenseChart()
        {
            //get an array of TotalByCategory objects with category name and transaction total
            var data = GetCategoryData("Expense");
            if (data != null)
            {
                var s = new Object[data.Length];
                for (int i = 0; i < data.Length; i++)
                {
                    s[i] = new { label = data[i].CategoryName, value = data[i].Total };
                }
                return Content(JsonConvert.SerializeObject(s), "application/json");
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "No data found for this month" });
        }

        public ActionResult GetIncomeChart()
        {
            //get an array of TotalByCategory objects with category name and transaction total
            var data = GetCategoryData("Income");
            if (data != null)
            {
                var s = new Object[data.Length];
                for (int i = 0; i < data.Length; i++)
                {
                    s[i] = new { label = data[i].CategoryName, value = data[i].Total };
                }
                return Content(JsonConvert.SerializeObject(s), "application/json");
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "No data found for this month" });
        }

        public TotalByCategory[] GetCategoryData(string type)
        {
            DateTime currentDate = DateTime.Now;
            TotalByCategory[] categoryTotalsList = null;
            if (User.Identity.IsInHousehold())
            {
                //get the household for this user
                var householdId = User.Identity.GetHouseholdId();
                var household = db.Households.Find(householdId);
                //find the current budget for this user
                if (household.Budgets.Where(x => x.Month == currentDate.Month).Where(x => x.Year == currentDate.Year).Any())
                {
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
                    var transactionCategories = monthTransactions.Where(x => x.Type == type).GroupBy(x => x.CategoryId)
                                                .Select(grp => grp.First()).Select(x => x.CategoryId).ToList();
                    categoryTotalsList = new TotalByCategory[transactionCategories.Count];
                    int i = 0;
                    foreach (var c in transactionCategories)
                    {
                        var category = db.Categories.Find(c);
                        categoryTotalsList[i] = new TotalByCategory();
                        categoryTotalsList[i].CategoryName = category.Name;
                        categoryTotalsList[i].Total = monthTransactions.Where(x => x.CategoryId == c).Select(x => x.Amount).Sum();
                        i++;
                    }
                    return categoryTotalsList;
                }
                return categoryTotalsList;
            }
            return categoryTotalsList;
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