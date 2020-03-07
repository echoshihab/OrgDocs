using Microsoft.AspNetCore.Mvc.Rendering;
using OrgDocs.Utility;
using System;
using System.Collections.Generic;


namespace OrgDocs.Models
{
    public class DocFiltersVM
    {
        public PaginatedList<Document> Documents { get; set; }
        public SelectList Categories { get; set; }
        public SelectList Depts { get; set; }


    }
}
