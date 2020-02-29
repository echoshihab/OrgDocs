using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrgDocs.Models
{
    public class Dept
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(30)]
        public string Department { get; set; }

    }
}
