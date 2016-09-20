using Budgeter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Controllers
{
    public class BudgetItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BudgetItem
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BudgetId,Type,SelectedCategory,Description,Amount,IsRepeating")] BudgetItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                var bItem = new BudgetItem();
                var householdId = User.Identity.GetHouseholdId();
                bItem.BudgetId = model.BudgetId;
                bItem.CategoryId = model.SelectedCategory;
                //check if the category exists
                //if (db.Categories.Any(x => x.Name == model.CategoryName)){                   
                //    var category = db.Categories.First(x => x.Name == model.CategoryName);
                //    //check if this is one of the household's budget categories
                //    var assignment = db.CategoryHouseholds.Where(x => x.CategoryId == category.Id).Where(x => x.HouseholdId == householdId).ToList();
                //    if(assignment.Count > 0)
                //    {
                //        bItem.CategoryId = category.Id;
                //    }        
                //}
                //else
                //{
                //    return RedirectToAction("Index", "Errors", new { errorMessage = "Category not found" });
                //}
                bItem.Description = model.Description;
                bItem.Type = model.Type;
                bItem.Amount = model.Amount;
                bItem.IsRepeating = model.IsRepeating;
                bItem.IsOriginal = true;
                if(bItem.IsRepeating == true)
                {
                    AddItemToFutureBudgets(bItem);
                }
                db.BudgetItems.Add(bItem);
                db.SaveChanges();
                return RedirectToAction("Details", "Budgets", new { id = model.BudgetId });
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "Error in creating budget item" });
        }

        public void AddItemToFutureBudgets(BudgetItem budgetItem)
        {
            var budget = db.Budgets.Find(budgetItem.BudgetId);
            var month = budget.Month;
            var year = budget.Year;
            var householdId = User.Identity.GetHouseholdId();
            //add a copy of this item to all my future household budgets for the current year
            var futureBudgetsThisYear = db.Budgets.Where(x => x.HouseholdId == householdId).Where(x => x.Year == year).Where(x => x.Month > month).ToList();
            if(futureBudgetsThisYear.Count > 0)
            {
                foreach (var b in futureBudgetsThisYear)
                {
                    var newItem = new BudgetItem { CategoryId = budgetItem.CategoryId, BudgetId = b.Id, Amount = budgetItem.Amount, IsRepeating = budgetItem.IsRepeating, Type = budgetItem.Type, Description = budgetItem.Description, IsOriginal = false };
                    db.BudgetItems.Add(newItem);
                    db.SaveChanges();
                }
            }           
            //add a copy of this item to all my future household budgets for the following year
            var futureBudgetsNextYear = db.Budgets.Where(x => x.HouseholdId == householdId).Where(x => x.Year > year).ToList();
            if (futureBudgetsNextYear.Count > 0)
            {
                foreach (var c in futureBudgetsNextYear)
                {
                    var newItem = new BudgetItem { CategoryId = budgetItem.CategoryId, BudgetId = c.Id, Amount = budgetItem.Amount, IsRepeating = budgetItem.IsRepeating, Type = budgetItem.Type, Description = budgetItem.Description, IsOriginal = false };
                    db.BudgetItems.Add(newItem);
                    db.SaveChanges();
                }
            }
        }

        //not using this anymore, instead refreshing page to get Create form to appear
        public ActionResult GetCreateView(int budgetId)
        {
            if (budgetId == null)
            {
                RedirectToAction("Index", "Errors", new { errorMessage = "Budget not found" });
            }
            //var createModel = new BudgetItemViewModel();
            //createModel.BudgetId = budgetId;
            //createModel.IsRepeating = false;

            //return PartialView("_CreateBudgetItem", createModel);
            return View("Details", "Budgets", new { id = budgetId });
        }

        public ActionResult GetView(int id, string viewName)
        {
            object model = null;
            if (viewName == "_DeleteBudgetItem")
            {
                if (id == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Budget item not found" });
                }
                BudgetItem budgetItem = db.BudgetItems.Find(id);
                if (budgetItem == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Budget item not found" });
                }
                var bModel = new BudgetItemViewModel();
                bModel.Id = budgetItem.Id;
                bModel.BudgetId = budgetItem.BudgetId;
                bModel.Type = budgetItem.Type;
                var category = db.Categories.Find(budgetItem.CategoryId);
                bModel.CategoryName = category.Name;
                bModel.Description = budgetItem.Description;
                bModel.Amount = budgetItem.Amount;
                bModel.IsRepeating = budgetItem.IsRepeating;
                model = bModel;
            }
            return PartialView(viewName, model);
        }

        // POST: Form posted in partial view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (ModelState.IsValid)
            {
                BudgetItem budgetItem = db.BudgetItems.Find(id);
                if (budgetItem == null)
                {
                    RedirectToAction("Index", "Errors", new { errorMessage = "Budget item not found" });
                }
                db.BudgetItems.Remove(budgetItem);
                db.SaveChanges();

                return RedirectToAction("Details", "Budgets", new { id = budgetItem.BudgetId });
            }
            return RedirectToAction("Index", "Errors", new { errorMessage = "Error in deleting budget item" });
        }
    }
}