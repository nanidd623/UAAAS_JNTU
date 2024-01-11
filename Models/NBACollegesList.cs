using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class NBACollegesList
    {
        public long sno { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public string Specialization { get; set; }
        public Nullable<System.DateTime> NBAFrom { get; set; }
        public Nullable<System.DateTime> NBATo { get; set; }
        public string NBAApprovalLetter { get; set; }
    }

    public class NBACollegesListExport
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public string Specialization { get; set; }
        public string NBAFrom { get; set; }
        public string NBATo { get; set; }
        public string NBAApprovalLetter { get; set; }
    }
}