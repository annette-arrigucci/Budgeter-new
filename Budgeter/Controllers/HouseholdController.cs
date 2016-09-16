using Budgeter.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Controllers
{
    public class HouseholdController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        

        // GET: Households
        [Authorize]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            var householdMembersInfo = new List<UserInfoViewModel>();
            var householdId = user.HouseholdId;
            string householdName = null;
            
            if (user.HouseholdId != null) {                      
                ViewBag.HouseholdId = householdId;
                var householdMembers = db.Users.Where(x => x.HouseholdId == householdId).ToList();
                foreach(var member in householdMembers)
                {
                    var uinfo = new UserInfoViewModel
                    {
                        DisplayName = member.FirstName + " " + member.LastName,
                        Email = member.Email,
                        UserId = member.Id
                    };
                    householdMembersInfo.Add(uinfo);
                }
                householdName = db.Households.Find(householdId).Name;
            }
            ViewBag.HouseholdName = householdName;
            ViewBag.HouseholdId = householdId;                      
            return View(householdMembersInfo);
        }

        //GET: Households/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize]
        public ActionResult InvitationSent()
        {
            return View();
        }

        //POST: Households/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name")]Household model)
        {
            if (ModelState.IsValid)
            {
                //create the household in the database
                model.Code = GetRandomString();
                db.Households.Add(model);
                db.SaveChanges();

                //find the current user in the database
                //get the HouseholdId from the household that was added
                var uId = User.Identity.GetUserId();
                
                var household = db.Households.First(x => x.Name.Equals(model.Name));
                var hId = household.Id;
                //assign household default categories - users can adjust these later
                //get default categories, create assignment in CategoriesHousehold table
                var defaultCategories = db.Categories.Where(x => x.IsDefault == true).ToList();
                foreach(var c in defaultCategories)
                {
                    var categoryAssign = new CategoryHousehold { CategoryId = c.Id, HouseholdId = hId };
                    db.CategoryHouseholds.Add(categoryAssign);
                    db.SaveChanges();
                }
                await AssignUserToHousehold(uId, hId);
                await RefreshCookie(uId);

                return RedirectToAction("Index", "Household");
            }
            return View(model);
        }

        //assign the user the HouseholdId - need to use UserManager for this
        //this will overwrite any other HouseholdId that the user has been assigned to
        public async Task AssignUserToHousehold(string userId, int? householdId)
        {       
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            ApplicationUser user = userManager.FindById(userId);
            if(user.HouseholdId!= null)
            {
                if(user.HouseholdId == householdId)
                {
                    //if the user is already assigned to this household, do nothing
                }
                else
                {
                    user.HouseholdId = householdId;
                    IdentityResult result = await userManager.UpdateAsync(user);
                }
            }
            else
            {
                user.HouseholdId = householdId;
                IdentityResult result = await userManager.UpdateAsync(user);
            }                     
        }

        public string GetRandomString()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 5)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //POST: Households/InviteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InviteUser(string email, int householdId)
        {
            if (ModelState.IsValid)
            {
                Household hhold = db.Households.Find(householdId);
                if (hhold == null)
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Not Found" });
                }
                if (string.IsNullOrEmpty(email))
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Email address empty" });
                }
                else
                {
                    //use utilities to see if email is valid
                    var emailChecker = new RegexUtilities();
                    if (!emailChecker.IsValidEmail(email))
                    {
                        return RedirectToAction("Index", "Errors", new { errorMessage = "Email address not valid" });
                    }
                    //if the email address is valid, send the email
                    SendEmailInvitation(email, householdId);
                    ViewBag.Email = email;
                    return View("InvitationSent");
                }
            }
            return View();
        }

        public async Task SendEmailInvitation(string email, int householdId)
        {
            Household hhold = db.Households.Find(householdId);
            if (hhold == null)
            {
                RedirectToAction("Index", "Errors", new { errorMessage = "Not Found" });
            }
            var callbackUrl = "http://aarrigucci-budgeter.azurewebsites.net/Household/Join/" + householdId;
            string subject = "Invitation to join Budgeter household";
            var emailCode = hhold.Code;
            string message = "You have been invited to join the " + hhold.Name + " household on the Budgeter application. Click <a href=\"" + callbackUrl + "\" target=\"_blank\">here</a> to join. If you don’t have an account, you will be prompted to create one. Enter the code " + emailCode + ".";
            
            var es = new EmailService();
            es.SendAsync(new IdentityMessage
            {
                Destination = email,
                Subject = subject,
                Body = message
            });
        }

        //GET
        [Authorize]
        public ActionResult Join(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "No household found. Check your email for the correct link to join the household." });
            }
            var household = db.Households.Find(Id);
            if (household == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "No household found. Check your email for the correct link to join the household." });
            }
            //pass a new Household model to the view with everything except the Code value, which they will enter in the form
            var model = new Household();
            model.Id = household.Id;
            model.Name = household.Name;
            return View(model);
        }

        //POST:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Join([Bind(Include = "Id,Name,Code")]Household model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Code))
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Code not valid" });
                }
                var householdToJoin = db.Households.Find(model.Id);
                if (householdToJoin == null)
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "No household found" });
                }              
                //check that what the user entered is the right code for the household
                if (model.Code.Equals(householdToJoin.Code))
                {
                    var userId = User.Identity.GetUserId();
                    //assign the user to the household
                    await AssignUserToHousehold(userId, householdToJoin.Id);
                    //refresh the cookie
                    await RefreshCookie(userId);
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Errors", new { errorMessage = "Code is incorrect. Please check your email or contact the administrator at annette.arrigucci@gmail.com" });
                }
            }
            return View(model);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Leave([Bind(Include = "UserId,HasAgreedToLeave")]LeaveHouseholdViewModel model)
        {
            //if checkbox is checked, showing user has agreed to leave, assign their HouseholdId to null
            if (model.HasAgreedToLeave == true)
            {
                await AssignUserToHousehold(model.UserId, null);
                //Refresh the cookie holding the user identity so the user can't access the household anymore
                await RefreshCookie(model.UserId);
                return RedirectToAction("Index", "Household");
            }
            else
            {
                return RedirectToAction("Index", "Household");
            }
        }

        public async Task RefreshCookie(string userId)
        {
            var user = db.Users.Find(userId);
            await ControllerContext.HttpContext.RefreshAuthentication(user);
        }
    }
}