using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OrgDocs.Models
{
    public class Document
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Last Update")]
        public DateTime LastUpdate { get; set; }
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        [Display(Name = "Dept")]
        public int DeptId { get; set;}
        [ForeignKey("DeptId")]
        public Dept Dept { get; set; }

        public string PdfUrl { get; set; }
        

    }
}
