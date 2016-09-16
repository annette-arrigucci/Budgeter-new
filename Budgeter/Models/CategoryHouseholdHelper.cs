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
            if(!type.Equals("Income") || !type.Equals("Expense"))
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
    }
}