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
                    c => c.Name,
                    new Category { Name = "Paycheck" },
                    new Category { Name = "Other income" },
                    new Category { Name = "House" },
                    new Category { Name = "Utilities" },
                    new Category { Name = "Food" },
                    new Category { Name = "Transportation" },
                    new Category { Name = "Health" },
                    new Category { Name = "Child care" },
                    new Category { Name = "Personal care" },
                    new Category { Name = "Clothing" },
                    new Category { Name = "Entertainment" },
                    new Category { Name = "Financing" },
                    new Category { Name = "Gifts/donations" },
                    new Category { Name = "Other expense" }
                  );
            context.SaveChanges();
        }
    }
}
