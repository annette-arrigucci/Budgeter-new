using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Models
{
    public class TransactionEditViewModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Account")]
        public int AccountId { get; set; }
        [Required]
        [StringLength(300, MinimumLength = 3)]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Transaction date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateSpent { get; set; }
        [Required]
        public Decimal Amount { get; set; }
        [Required]
        public string Type { get; set; }
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [Display(Name = "Category")]
        public SelectList CategoryList { get; set; }
        [Required(ErrorMessage = "No type selected")]
        [Display(Name = "Category")]
        public int SelectedCategory { get; set; }
        [Display(Name = "Transaction by")]
        public SelectList HouseholdUsersList { get; set; }
        [Required(ErrorMessage = "No user selected")]
        [Display(Name = "Transaction by")]
        public string SelectedUser { get; set; }
        [Display(Name = "Reconciled amount")]
        public Decimal ReconciledAmount { get; set; }
        [Display(Name = "Entered by")]
        public string EnteredById { get; set; }
        [Display(Name = "Entered by")]
        public string EnteredByName { get; set; }
        [Display(Name = "Date entered")]
        public DateTime DateEntered { get; set; }
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
    }
}