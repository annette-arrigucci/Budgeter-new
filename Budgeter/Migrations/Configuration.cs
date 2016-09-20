namespace Budgeter.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Budgeter.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Budgeter.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            var userManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(context));
            if (!context.Users.Any(u => u.Email == "annette.arrigucci@outlook.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "annette.arrigucci@outlook.com",
                    Email = "annette.arrigucci@outlook.com",
                    FirstName = "Annette",
                    LastName = "Arrigucci",
                }, "Abc&123!");
            }
            context.Categories.AddOrUpdate(
                    c => new { c.Name, c.Type, c.IsDefault }, 
                    new Category { Name = "Paycheck", Type = "Income", IsDefault= true },
                    new Category { Name = "Other income", Type = "Income", IsDefault = true },
                    new Category { Name = "House", Type = "Expense", IsDefault = true },
                    new Category { Name = "Utilities", Type = "Expense", IsDefault = true },
                    new Category { Name = "Food", Type = "Expense", IsDefault = true },
                    new Category { Name = "Transportation", Type = "Expense", IsDefault = true },
                    new Category { Name = "Health", Type = "Expense", IsDefault = true },
                    new Category { Name = "Child care", Type = "Expense", IsDefault = true },
                    new Category { Name = "Personal care", Type = "Expense", IsDefault = true },
                    new Category { Name = "Clothing", Type = "Expense", IsDefault = true },
                    new Category { Name = "Entertainment", Type = "Expense", IsDefault = true },
                    new Category { Name = "Financing", Type = "Expense", IsDefault = true },
                    new Category { Name = "Gifts/donations", Type = "Expense", IsDefault = true },
                    new Category { Name = "Other expense", Type = "Expense", IsDefault = true }
                  );
        }
    }
}
