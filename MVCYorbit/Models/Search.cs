using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCYorbit.Models
{
    public class Search
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? DOB { get; set; }

        public string ApplicationID { get; set; }

        public string Status { get; set; }
    }
}