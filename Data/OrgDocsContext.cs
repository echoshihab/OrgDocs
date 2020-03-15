using OrgDocs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OrgDocs.Data
{
    public class OrgDocsContext : IdentityDbContext
    {
        public OrgDocsContext(DbContextOptions<OrgDocsContext> options)
            : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dept> Depts { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    
    }


 }
