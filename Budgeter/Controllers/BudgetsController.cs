using Budgeter.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace Budgeter.Controllers
{
    public class BudgetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Index()
        {
            var hId = User.Identity.GetHouseholdId();
            ViewBag.HouseholdId = hId;
            var budgets = db.Budgets.Where(x => x.HouseholdId == hId);
            return View(budgets);
        }

        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Budget not found" });
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Budget not found" });
            }
            var budgetHousehold = budget.HouseholdId;
            //check if the user is authorized to view this budget
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (user.HouseholdId != budgetHousehold)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
            }

            //send the model to the view - create lists of the income and expense items
            var indexModel = new BudgetItemsIndexViewModel();
            var budgetItems = budget.BudgetItems.ToList();
            var incomeItems = new List<BudgetItemViewModel>();
            var expenseItems = new List<BudgetItemViewModel>();
            Decimal incomeItemsTotal = 0;
            Decimal expenseItemsTotal = 0;

            foreach (var b in budgetItems)
            {
                var categoryId = b.CategoryId;
                var category = db.Categories.Find(categoryId);
                if(category.Type  == "Income")
                {
                    incomeItemsTotal += b.Amount;
                    var incItem = ConvertBudgetItemToDisplay(b);
                    incomeItems.Add(incItem);
                }
                else if(category.Type == "Expense")
                {
                    expenseItemsTotal += b.Amount;
                    var expItem = ConvertBudgetItemToDisplay(b);
                    expenseItems.Add(expItem);
                }
            }
            indexModel.IncomeItems = incomeItems;
            indexModel.ExpenseItems = expenseItems;

            //create the model for a new budget item here
            var createModel = new BudgetItemViewModel();
            createModel.BudgetId = (int)id;
            createModel.IsRepeating = false;
            var catHelper = new CategoryHouseholdHelper();
            var householdId = User.Identity.GetHouseholdId();

            var incomeCategories = catHelper.GetIncomeCategories((int)householdId);
            var expenseCategories = catHelper.GetExpenseCategories((int)householdId);
            createModel.IncomeCategoryList = new SelectList(incomeCategories, "Id", "Name");
            createModel.ExpenseCategoryList = new SelectList(expenseCategories, "Id", "Name");

            ViewBag.CreateModel = createModel;
            ViewBag.BudgetName = budget.Name;
            ViewBag.BudgetId = budget.Id;
            ViewBag.ExpensesTotal = expenseItemsTotal;
            ViewBag.IncomesTotal = incomeItemsTotal;

            return View(indexModel);
        }

        public BudgetItemViewModel ConvertBudgetItemToDisplay(BudgetItem budgetItem)
        {
            var bModel = new BudgetItemViewModel();
            bModel.Id = budgetItem.Id;
            bModel.BudgetId = budgetItem.BudgetId;
            bModel.Type = budgetItem.Type;          
            var category = db.Categories.Find(budgetItem.CategoryId);
            bModel.CategoryName = category.Name;
            bModel.Description = budgetItem.Description;
            bModel.Amount = budgetItem.Amount;
            bModel.IsRepeating = budgetItem.IsRepeating;
            return bModel;
        }

        //not using these methods anymore - these were used for AutoComplete JQuery UI widget

        //return a JSON objects with this household's expense categories
        //public ActionResult ExpenseSearch(string term)
        //{
        //    // Get tags from database
        //    var helper = new CategoryHouseholdHelper();

        //    var expCategories = helper.GetExpenseCategories((int)User.Identity.GetHouseholdId());
        //    var tags = new string[expCategories.Count];
        //    int i = 0;
        //    foreach (var exp in expCategories)
        //    {
        //        tags[i] = exp.Name;
        //        i++;
        //    }
        //    return this.Json(tags.Where(t => t.StartsWith(term)),
        //                   JsonRequestBehavior.AllowGet);
        //}

        //return a JSON objects with this household's income categories
        //public ActionResult IncomeSearch(string term)
        //{
        //    // Get tags from database
        //    var helper = new CategoryHouseholdHelper();

        //    var incCategories = helper.GetIncomeCategories((int)User.Identity.GetHouseholdId());
        //    var tags = new string[incCategories.Count];
        //    int i = 0;
        //    foreach (var inc in incCategories)
        //    {
        //        tags[i] = inc.Name;
        //        i++;
        //    }
            //string[] tags = { "ASP.NET", "WebForms",
            //    "MVC", "jQuery", "ActionResult",
            //    "MangoDB", "Java", "Windows" };
        //    return this.Json(tags.Where(t => t.StartsWith(term)),
        //                   JsonRequestBehavior.AllowGet);
        //}

        // POST: Budget
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "HouseholdId, Name")] Budget model)
        {
            if (model.HouseholdId == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            //check that user is in the household and can view this page
            if (!(User.Identity.GetHouseholdId() == model.HouseholdId))
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
            }
            if (string.IsNullOrEmpty(model.Name))
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "No name entered" });
            }
            var budget = new Budget();
            budget.HouseholdId = model.HouseholdId;
            budget.Name = model.Name;

            string[] words = model.Name.Split(' ');
            var month = words[0];
            var monthInt = 0;
            //convert month to an integer
            monthInt = ConvertMonthToInt(month);
            //if conversion didn't happen, 0 is returned
            if(monthInt == 0)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Error in entering budget month" });
            }
            else
            {
                budget.Month = monthInt;
            }
            var currentYear = DateTime.Now.Year;

            if (!string.IsNullOrEmpty(words[1]))
            {
                var yearInt = Int32.Parse(words[1]);
                //make sure a valid year was parsed
                if (!(yearInt >= currentYear && yearInt <= (currentYear + 1)))
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Error in entering budget year" });
                }
                else
                {
                    budget.Year = yearInt;
                }
            }
           else
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Error in entering budget year" });
            }
                  
            //if budget already exists for this month, don't create it again
            if(db.Budgets.Where(x => x.HouseholdId == budget.HouseholdId).Where(x => x.Month == budget.Month).Where(x => x.Year == budget.Year).Any())
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Budget already exists for month" });
            }
   
            db.Budgets.Add(budget);
            db.SaveChanges();

            //add any recurring budget items to this month's budget
            var recurringItems = new List<BudgetItem>();
            //get the list of all budgets for this household
            var householdBudgets = db.Budgets.Where(x => x.HouseholdId == budget.HouseholdId).ToList();
            //for each budget that is in the household, get all the budget items with that budget Id that 
            //should be repeated every month - are recurring and original
            foreach(var h in householdBudgets)
            {
                var rItems = db.BudgetItems.Where(x => x.BudgetId == h.Id).Where(x => x.IsRepeating == true).Where(x => x.IsOriginal == true).Where(x => x.RepeatActive == true).ToList();
                recurringItems.AddRange(rItems);
            }

            if (recurringItems.Count > 0)
            {
                foreach (var r in recurringItems)
                {
                    var rItem = new BudgetItem { CategoryId = r.CategoryId, BudgetId = budget.Id, Amount = r.Amount, IsRepeating = r.IsRepeating, Type = r.Type, Description = r.Description, IsOriginal = false, RepeatActive = false, IsCopyOf = r.Id };
                    db.BudgetItems.Add(rItem);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index","Budgets");
        }
        
        public int ConvertMonthToInt(string monthName)
        {
            switch (monthName)
            {
                case "January": return 1;
                case "February": return 2;
                case "March": return 3;
                case "April": return 4;
                case "May": return 5;
                case "June": return 6;
                case "July": return 7;
                case "August": return 8;
                case "September": return 9;
                case "October": return 10;
                case "November": return 11;
                case "December": return 12;
                default: return 0;
            }
        }

        [Authorize]
        [AuthorizeHouseholdRequired]
        //GET: Budget/EditCategories/5
        public ActionResult EditCategories(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            //check that user is in the household and can view this page
            if (!(User.Identity.GetHouseholdId() == id)){
                return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
            }
            var categoriesToEdit = new EditCategoriesViewModel();
            categoriesToEdit.HouseholdId = (int)id;
            var hholdCategories = db.CategoryHouseholds.Where(x => x.HouseholdId == id).ToList();
            var incomeCategories = new List<CategoryCheckBox>();
            var expenseCategories = new List<CategoryCheckBox>();
            foreach (var h in hholdCategories)
            {
                var cat = db.Categories.Find(h.CategoryId);

                var catCheck = new CategoryCheckBox
                {
                    CategoryId = h.CategoryId,
                    CategoryName = cat.Name,
                    Checked = true
                };
                if (cat.Type == "Income")
                {
                    incomeCategories.Add(catCheck);
                }
                if (cat.Type == "Expense")
                {
                    expenseCategories.Add(catCheck);
                }
            }
            categoriesToEdit.IncomeCategoriesToSelect = incomeCategories.ToArray();
            categoriesToEdit.ExpenseCategoriesToSelect = expenseCategories.ToArray();
            return View(categoriesToEdit);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategories([Bind(Include = "HouseholdId, IncomeCategoriesToSelect, ExpenseCategoriesToSelect")] EditCategoriesViewModel model)
        {
            var helper = new CategoryHouseholdHelper();
            
            //building lists of all income and expense categories for this household
            var hholdCategories = db.CategoryHouseholds.Where(x => x.HouseholdId == model.HouseholdId).ToList();
            var incomeCategories = new List<int>();
            var expenseCategories = new List<int>();
            foreach (var h in hholdCategories)
            {
                var cat = db.Categories.Find(h.CategoryId);
                if (cat.Type == "Income")
                {
                    incomeCategories.Add(cat.Id);
                }
                if (cat.Type == "Expense")
                {
                    expenseCategories.Add(cat.Id);
                }
            }

            //building lists of the income and expense categories the user selected in the view
            //these were returned in two arrays
            var selectedIncomeCategories = new List<int>();
            var selectedExpenseCategories = new List<int>();

            if (model.IncomeCategoriesToSelect != null)
            {
                for (int i = 0; i < model.IncomeCategoriesToSelect.Length; i++)
                {
                    if (model.IncomeCategoriesToSelect[i].Checked == true)
                    {
                        selectedIncomeCategories.Add(model.IncomeCategoriesToSelect[i].CategoryId);
                    }
                }
            }

            if (model.ExpenseCategoriesToSelect != null)
            {
                for (int i = 0; i < model.ExpenseCategoriesToSelect.Length; i++)
                {
                    if (model.ExpenseCategoriesToSelect[i].Checked == true)
                    {
                        selectedExpenseCategories.Add(model.ExpenseCategoriesToSelect[i].CategoryId);
                    }
                }
            }

            if(selectedIncomeCategories == null)
            {
                foreach(var i in incomeCategories)
                {
                    helper.RemoveAssignment(model.HouseholdId, i);
                }
            }
            else
            {
                foreach(var i in selectedIncomeCategories)
                {
                    //if the household isn't already assigned to this category, assign them
                    helper.AddAssignment(model.HouseholdId, i);
                }
            }

            if (selectedExpenseCategories == null)
            {
                foreach (var e in expenseCategories)
                {
                    helper.RemoveAssignment(model.HouseholdId, e);
                }
            }
            else
            {
                foreach (var e in selectedExpenseCategories)
                {
                    helper.AddAssignment(model.HouseholdId, e);
                }
            }

            //now look for income and expense categories to remove the user from - delete these entries from the CategoryHouseholds table
            var incomeCategoriesToRemove = incomeCategories.Except(selectedIncomeCategories);
            foreach (var m in incomeCategoriesToRemove)
            {
                helper.RemoveAssignment(model.HouseholdId, m);
            }

            var expenseCategoriesToRemove = expenseCategories.Except(selectedExpenseCategories);
            foreach(var m in expenseCategoriesToRemove)
            {
                helper.RemoveAssignment(model.HouseholdId, m);
            }
            return RedirectToAction("EditCategories", "Budgets", new { id = model.HouseholdId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCategory(AddCategoryViewModel model)
        {
            if (model.HouseholdId == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            Household household = db.Households.Find(model.HouseholdId);
            if (household == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            //if empty string, refresh the page
            if (string.IsNullOrEmpty(model.CategoryName))
            {
                return RedirectToAction("EditCategories", "Budgets", new { id = model.HouseholdId });
            }
            //call helper method to add the category
            var helper = new CategoryHouseholdHelper();
            var categorySuccess = helper.AddCategory(model.CategoryName, model.Type);
            if (categorySuccess == false)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Category not able to be added" });
            }

            //add entry to assign the new category to the household
            var success = helper.AddAssignment(model.HouseholdId, model.CategoryName);
            if (success == false)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Category entry not able to be added" });
            }
            return RedirectToAction("EditCategories", "Budgets", new { id = model.HouseholdId });
        }

        public ActionResult DeleteItemConfirmed(int id, string message)
        {
            ViewBag.Id = id;
            ViewBag.Message = message;
            return View();
        }
    }
}