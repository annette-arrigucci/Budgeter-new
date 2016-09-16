using Budgeter.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Controllers
{
    public class BudgetController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        // GET: Budget
        public ActionResult Index()
        {
            return View();
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
            return RedirectToAction("Dashboard","Home",null);       
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
                return RedirectToAction("EditCategories", "Budget", new { id = model.HouseholdId });
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
            return RedirectToAction("EditCategories", "Budget", new { id = model.HouseholdId });
        }
    }
}