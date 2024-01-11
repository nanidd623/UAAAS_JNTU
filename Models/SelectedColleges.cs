using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class SelectedColleges
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public bool Selected { get; set; }

    }
}