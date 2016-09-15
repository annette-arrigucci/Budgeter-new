using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class UserInfoViewModel
    {
        [Display(Name = "Name")]
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public string HouseholdName { get; set; }

        public UserInfoViewModel()
        {

        }

        public UserInfoViewModel(string uId)
        {
            var db = new ApplicationDbContext();
            var user = db.Users.Find(uId);
            Email = user.Email;
            DisplayName = user.DisplayName;
            UserId = uId;
            if(user.HouseholdId != null)
            {
                var household = db.Households.Find(user.HouseholdId);
                HouseholdName = household.Name;
            }         
        }
    }
}