using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class AutonomousCollegesList
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public Nullable<System.DateTime> AffiliationFromDate { get; set; }
        public Nullable<System.DateTime> AffiliationToDate { get; set; }
        public string AffiliationDocument { get; set; }
    }

    public class AutonomousCollegesListExport
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string AffiliationFromDate { get; set; }
        public string AffiliationToDate { get; set; }
        public string AffiliationDocument { get; set; }
    }
}