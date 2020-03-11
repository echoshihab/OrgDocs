using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace OrgDocs.Models
{
    public class ApplicationUser : IdentityUser
    {

        [NotMapped]
        public string Role { get; set; }
    }
}
