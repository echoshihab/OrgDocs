using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrgDocs.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Display(Name="Category")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
