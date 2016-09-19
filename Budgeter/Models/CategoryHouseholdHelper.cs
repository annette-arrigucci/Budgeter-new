using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class CategoryHouseholdHelper
    {
        private ApplicationDbContext db;

        public CategoryHouseholdHelper()
        {
            db = new ApplicationDbContext();
        }

        public void RemoveAssignment(int? householdId, int categoryId)
        {
            var assignment = db.CategoryHouseholds.Where(h => h.HouseholdId == (int)householdId).Where(h => h.CategoryId == categoryId).Any();
            if (assignment == true)
            {
                var toRemove = db.CategoryHouseholds.Where(h => h.HouseholdId == (int)householdId).Where(h => h.CategoryId == categoryId).First();
                db.CategoryHouseholds.Remove(toRemove);
                db.SaveChanges();
            }
        }

        public void AddAssignment(int? householdId, int categoryId)
        {
            var assignment = db.CategoryHouseholds.Where(h => h.HouseholdId == (int)householdId).Where(h => h.CategoryId == categoryId).Any();
            if (assignment == false)
            {
                var catHold = new CategoryHousehold { CategoryId = categoryId, HouseholdId = (int)householdId };
                db.CategoryHouseholds.Add(catHold);
                db.SaveChanges();
            }
        }

        public bool AddAssignment(int? householdId, string categoryName)
        {
            //find the Category that has this name in the database
            var category = db.Categories.Where(x => x.Name == categoryName).First();
            if (category == null)
            {
                return false;
            }
            var catHold = new CategoryHousehold { CategoryId = category.Id, HouseholdId = (int)householdId };
            db.CategoryHouseholds.Add(catHold);
            db.SaveChanges();
            return true;
        }

        public bool AddCategory(string categoryName, string type)
        {
            //check that it is a valid type
            if(!((type=="Income") || (type == "Expense")))
            {
                return false;
            }
            //check that a category with the same name doesn't already exist
            var check = db.Categories.Where(c => c.Name == categoryName).Any();
            //if not, add the category
            if(check == false)
            {
                var category = new Category { Name = categoryName, IsDefault = false, Type = type };
                db.Categories.Add(category);
                db.SaveChanges();
            }
            return true;
        }
        public List<Category> GetIncomeCategories(int id)
        {
            var categoryEntries = db.CategoryHouseholds.Where(x => x.HouseholdId == id);
            //var categories = new List<Category>();
            //foreach(var c in categoryEntries)
            //{
            //    db.Categories.Find(c.CategoryId);
            //}
            //var incomeCategories = from h in db.CategoryHouseholds where h.HouseholdId == id
            //                       join c in db.Categories on h.CategoryId equals c.Id where c.Type == "Income"
            //                       select c;
            var incomeCategories = db.Categories.Where(x => x.CategoryHouseholds.Any(y => y.HouseholdId == id)).Where(x => x.Type == "Income").ToList();
            //db.Categories.Where(x => x.CategoryHouseholds.Any(y => y.HouseholdId == id)).SelectMany(x => x.Categories.Type == "Income").ToList();
            return incomeCategories;
        }
        public List<Category> GetExpenseCategories(int id)
        {
            var expenseCategories = db.Categories.Where(x => x.CategoryHouseholds.Any(y => y.HouseholdId == id)).Where(x => x.Type == "Expense").ToList();
            return expenseCategories;
        }
    }
}