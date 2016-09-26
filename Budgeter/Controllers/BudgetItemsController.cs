using Budgeter.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        public ActionResult Create([Bind(Include = "BudgetId,Type,SelectedIncomeCategory,SelectedExpenseCategory,Description,Amount,IsRepeating")] BudgetItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                var bItem = new BudgetItem();
                var householdId = User.Identity.GetHouseholdId();
                bItem.BudgetId = model.BudgetId;
                //If income was selected, make sure the selected income property is not null
                if (model.Type == "Income")
                {
                    if (model.SelectedIncomeCategory == null)
                    {
                        return RedirectToAction("Index", "Errors", new { errorMessage = "Error in transaction category" });
                    }
                    bItem.CategoryId = (int)model.SelectedIncomeCategory;
                }
                //If expense was selected, make sure the selected income property is not null
                if (model.Type == "Expense")
                {
                    if (model.SelectedExpenseCategory == null)
                    {
                        return RedirectToAction("Index", "Errors", new { errorMessage = "Error in transaction category" });
                    }
                    bItem.CategoryId = (int)model.SelectedExpenseCategory;
                }
              
                bItem.Description = model.Description;
                bItem.Type = model.Type;
                bItem.Amount = model.Amount;
                bItem.IsRepeating = model.IsRepeating;
                bItem.IsOriginal = true;
                if(bItem.IsRepeating == true)
                {
                    bItem.RepeatActive = true;

                }
                else if (bItem.IsRepeating == false)
                {
                    bItem.RepeatActive = false;
                }
                db.BudgetItems.Add(bItem);
                db.SaveChanges();

                if (bItem.IsRepeating == true)
                {
                    AddItemToFutureBudgets(bItem);
                }
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
                    var newItem = new BudgetItem { CategoryId = budgetItem.CategoryId, BudgetId = b.Id, Amount = budgetItem.Amount, IsRepeating = budgetItem.IsRepeating, Type = budgetItem.Type, Description = budgetItem.Description, IsOriginal = false, RepeatActive = false, IsCopyOf = budgetItem.Id };
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
                    var newItem = new BudgetItem { CategoryId = budgetItem.CategoryId, BudgetId = c.Id, Amount = budgetItem.Amount, IsRepeating = budgetItem.IsRepeating, Type = budgetItem.Type, Description = budgetItem.Description, IsOriginal = false, RepeatActive = false, IsCopyOf = budgetItem.Id };
                    db.BudgetItems.Add(newItem);
                    db.SaveChanges();
                }
            }
        }

        public void RemoveItemFromFutureBudgets(BudgetItem budgetItem)
        {
            //remove an item from future budgets - starting with the current month - we will not remove from any budgets for past months
            var budget = db.Budgets.Find(budgetItem.BudgetId);

            //find the ID of the original item - if this is the original, this item's ID
            //if this is a copy - the item number in IsCopyOf
            int originalId = 0;
            if(budgetItem.IsOriginal == true)
            {
                originalId = budgetItem.Id;
            }
            else if(budgetItem.IsCopyOf != null)
            {
                originalId = (int)budgetItem.IsCopyOf;
            }
            else
            {
                RedirectToAction("Index", "Errors", new { errorMessage = "Can't find original item" });
            }

            var householdId = User.Identity.GetHouseholdId();


            int monthToStartDelete = 0;

            //get the budget's month and year
            var budgetMonth = budget.Month;
            var budgetYear = budget.Year;

            //get the current month and year
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            if (budgetYear == currentYear)
            {
                if (budgetMonth > currentMonth)
                {
                    monthToStartDelete = budgetMonth;
                }
                else
                {
                    monthToStartDelete = currentMonth;
                }
                //delete copies of this item from all my future household budgets for the current year
                var futureBudgetsThisYear = db.Budgets.Where(x => x.HouseholdId == householdId).Where(x => x.Year == budgetYear).Where(x => x.Month > monthToStartDelete).ToList();
                if (futureBudgetsThisYear.Count > 0)
                {
                    foreach (var b in futureBudgetsThisYear)
                    {
                        //find any copies of this budget item in future budgets this year
                        var toRemove = db.BudgetItems.Where(x => x.BudgetId == b.Id).Where(x => x.IsOriginal == false).Where(x => x.IsCopyOf == originalId).ToList();
                        if (toRemove.Count > 0)
                        {
                            foreach (var t in toRemove)
                            {
                                db.BudgetItems.Remove(t);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                //remove any copies of this item from my future household budgets for the following year
                var futureBudgetsNextYear = db.Budgets.Where(x => x.HouseholdId == householdId).Where(x => x.Year > currentYear).ToList();
                if (futureBudgetsNextYear.Count > 0)
                {
                    foreach (var c in futureBudgetsNextYear)
                    {
                        //find any copies of this budget item in future budgets this year
                        var toRemoveNextYear = db.BudgetItems.Where(x => x.BudgetId == c.Id).Where(x => x.IsOriginal == false).Where(x => x.IsCopyOf == originalId).ToList();
                        if (toRemoveNextYear.Count > 0)
                        {
                            foreach (var t in toRemoveNextYear)
                            {
                                db.BudgetItems.Remove(t);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            else if(budgetYear > currentYear)
            {
                monthToStartDelete = budgetMonth;
                //remove any copies of this item from my household budgets after the selected month in the following year
                var futureBudgetsNextYear = db.Budgets.Where(x => x.HouseholdId == householdId).Where(x => x.Year == budgetYear).Where(x => x.Month > monthToStartDelete).ToList();
                if (futureBudgetsNextYear.Count > 0)
                {
                    foreach (var c in futureBudgetsNextYear)
                    {
                        //find any copies of this budget item in future budgets for next year
                        var toRemoveNextYear = db.BudgetItems.Where(x => x.BudgetId == c.Id).Where(x => x.IsOriginal == false).Where(x => x.IsCopyOf == originalId).ToList();
                        if (toRemoveNextYear.Count > 0)
                        {
                            foreach (var t in toRemoveNextYear)
                            {
                                db.BudgetItems.Remove(t);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }          
        }

        //not using this anymore, instead refreshing page to get Create form to appear
        //public ActionResult GetCreateView(int budgetId)
        //{
        //    if (budgetId == null)
        //    {
        //        RedirectToAction("Index", "Errors", new { errorMessage = "Budget not found" });
        //    }
            //var createModel = new BudgetItemViewModel();
            //createModel.BudgetId = budgetId;
            //createModel.IsRepeating = false;

            //return PartialView("_CreateBudgetItem", createModel);
        //    return View("Details", "Budgets", new { id = budgetId });
        //}

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
                var bModel = new BudgetItemDeleteViewModel();
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
        public ActionResult DeleteConfirmed(int id, string repeatingDeleteOption)
        {
            //TODO: Add delete confirmed page
            if (ModelState.IsValid)
            {
                BudgetItem budgetItem = db.BudgetItems.Find(id);
                if (budgetItem == null)
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Budget item not found" });
                }
                //budget item doesn't repeat - delete this one item from the database
                if (budgetItem.IsRepeating == false)
                {
                    db.BudgetItems.Remove(budgetItem);
                    db.SaveChanges();
                    return RedirectToAction("DeleteItemConfirmed", "Budgets", new { id = budgetItem.BudgetId, message = "Item deleted from budget" });
                }
                else if (budgetItem.IsRepeating == true)
                {
                    if (repeatingDeleteOption == "This month only")
                    {
                        db.BudgetItems.Remove(budgetItem);
                        db.SaveChanges();
                        return RedirectToAction("DeleteItemConfirmed", "Budgets", new { id = budgetItem.BudgetId, message = "Item deleted from budget" });
                    }
                    else if (repeatingDeleteOption == "This month and all future budgets")
                    {
                        if (budgetItem.IsOriginal == true)
                        {
                            RemoveItemFromFutureBudgets(budgetItem);
                            db.BudgetItems.Remove(budgetItem);
                            db.SaveChanges();

                            return RedirectToAction("DeleteItemConfirmed", "Budgets", new { id = budgetItem.BudgetId, message = "Item removed from this budget and from budgets after current month" });
                        }
                        //if the item is not the original one, find the original repeating item and set it to not active
                        else if (budgetItem.IsOriginal == false)
                        {
                            //if the original item exists, set repeat to not active - it may not exist because the user already deleted it
                            var toDeactivate = db.BudgetItems.Find(budgetItem.IsCopyOf);
                            if (toDeactivate != null)
                            {
                                if (toDeactivate.RepeatActive == true)
                                {
                                    //deactivate the original item
                                    toDeactivate.RepeatActive = false;
                                    db.Entry(toDeactivate).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            //now delete the item that was a copy of this original from future budgets
                            RemoveItemFromFutureBudgets(budgetItem);

                            //finally remove the item from the current month
                             db.BudgetItems.Remove(budgetItem);
                             db.SaveChanges();

                             return RedirectToAction("DeleteItemConfirmed", "Budgets", new { id = budgetItem.BudgetId, message = "Item removed from this budget and budgets after current month. Item still exists in past budgets but will not be added to future budgets." });
                        }
                     }
                   }
                }
                return RedirectToAction("Index", "Errors", new { errorMessage = "Error in deleting budget item" });
             }
    }
}