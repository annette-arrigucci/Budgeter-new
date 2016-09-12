using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class UserInfoViewModel
    {
        [Display(Name ="Name")]
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }     
    }
}