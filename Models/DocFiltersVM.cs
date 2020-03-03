using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;


namespace OrgDocs.Models
{
    public class DocFiltersVM
    {
        public List<Document> Documents { get; set; }
        public SelectList Categories { get; set; }
        public SelectList Depts { get; set; }


    }
}
