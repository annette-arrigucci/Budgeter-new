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
        public ActionResult EditCategories(int householdId)
        {
            //check that user is in the household and can view this page
            if(!(User.Identity.GetHouseholdId() == householdId)){
                return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
            }
            var categoriesToEdit = new EditCategoriesViewModel();
            categoriesToEdit.HouseholdId = householdId;
            var hholdCategories = db.CategoryHouseholds.Where(x => x.HouseholdId == householdId).ToList();
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

            for(int i=0; i < model.IncomeCategoriesToSelect.Length; i++)
            {
                if(model.IncomeCategoriesToSelect[i].Checked == true)
                {
                    selectedIncomeCategories.Add(model.IncomeCategoriesToSelect[i].CategoryId);
                }
            }

            for (int i = 0; i < model.ExpenseCategoriesToSelect.Length; i++)
            {
                if (model.ExpenseCategoriesToSelect[i].Checked == true)
                {
                    selectedIncomeCategories.Add(model.IncomeCategoriesToSelect[i].CategoryId);
                }
            }

            if(selectedIncomeCategories == null)
            {
                foreach(var i in incomeCategories)
                {
                    CategoryHousehold assignment = db.CategoryHouseholds.FirstOrDefault(x => x.CategoryId == i);
                    db.CategoryHouseholds.Remove(assignment);
                    db.SaveChanges();
                }
            }
            else
            {
                foreach(var i in selectedIncomeCategories)
                {
                    //if the household isn't already assigned to this category, assign them
                    if(!db.CategoryHouseholds.Where(y => y.HouseholdId == model.HouseholdId).Any(x => x.CategoryId == i))
                    {
                        var catHold = new CategoryHousehold { CategoryId = i, HouseholdId = model.HouseholdId };
                    }
                }
            }

            if (selectedExpenseCategories == null)
            {
                foreach (var e in expenseCategories)
                {
                    CategoryHousehold assignment = db.CategoryHouseholds.FirstOrDefault(x => x.CategoryId == e);
                    db.CategoryHouseholds.Remove(assignment);
                    db.SaveChanges();
                }
            }
            else
            {
                foreach (var e in selectedExpenseCategories)
                {
                    //if the household isn't already assigned to this category, assign them
                    if (!db.CategoryHouseholds.Where(y => y.HouseholdId == model.HouseholdId).Any(x => x.CategoryId == e))
                    {
                        var catHold = new CategoryHousehold { CategoryId = e, HouseholdId = model.HouseholdId };
                    }
                }
            }

            //now look for income and expense categories to remove the user from - delete these entries from the CategoryHouseholds table
            var incomeCategoriesToRemove = incomeCategories.Except(selectedExpenseCategories);
            foreach (var m in incomeCategoriesToRemove)
            {
                var assignment = db.CategoryHouseholds.Where(y => y.HouseholdId == model.HouseholdId).First(x => x.CategoryId == m);
                if(assignment != null)
                {
                    db.CategoryHouseholds.Remove(assignment);
                    db.SaveChanges();
                }                            
            }

            var expenseCategoriesToRemove = expenseCategories.Except(selectedExpenseCategories);
            foreach(var m in expenseCategoriesToRemove)
            {
                var assignment = db.CategoryHouseholds.Where(y => y.HouseholdId == model.HouseholdId).First(x => x.CategoryId == m);
                if (assignment != null)
                {
                    db.CategoryHouseholds.Remove(assignment);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Dashboard","Home",null);       
        }
    }
}