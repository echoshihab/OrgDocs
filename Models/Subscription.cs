using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrgDocs.Models
{
    public class Subscription
    {
        public int SubscriptionID { get; set;}
        public int DocumentID { get; set; }
        public string ApplicationUserID { get; set; }

        public Document Document { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}
