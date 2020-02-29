using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrgDocs.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrgDocs.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new OrgDocsContext(
                serviceProvider.GetRequiredService<DbContextOptions<OrgDocsContext>>()))
                {
                if (context.Categories.Any())
                {
                    return;
                }
                context.Categories.AddRange(
                    new Category
                    {
                        Name = "Procedure"
                    },
                    new Category
                    {
                        Name = "Policy"
                    },
                    new Category
                    {
                        Name = "Downtime"
                    },
                    new Category
                    {
                        Name = "Contact"
                    }


                    );
                context.SaveChanges();
            }
        }
    }
}
