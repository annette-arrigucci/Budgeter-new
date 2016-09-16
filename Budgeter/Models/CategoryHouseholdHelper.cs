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
    }
}