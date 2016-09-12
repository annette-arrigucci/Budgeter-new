using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Models
{
    public class CreateAccountViewModel
    {
        [Required]
        [StringLength(160, MinimumLength = 3)]
        public string Name { get; set; }
        [Required]
        [Range(typeof(decimal), "0.00", "10000000.00",ErrorMessage ="Starting balance must be between $0.00 and $10,000,000.00")]
        [Display(Name="Starting Balance")]
        public Decimal StartingBalance { get; set; }
        [Display(Name = "Type")]
        public SelectList TypeList { get; set; }
        [Required(ErrorMessage = "No type selected")]
        [Display(Name = "Type")]
        public string SelectedType { get; set; }
    }
}