using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class MinorityCollegesList
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public Nullable<System.DateTime> StatusFromDate { get; set; }
        public Nullable<System.DateTime> StatusToDate { get; set; }
        public string StatusDocument { get; set; }
    }

    public class MinorityCollegesListExport
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string StatusFromDate { get; set; }
        public string StatusToDate { get; set; }
        public string StatusDocument { get; set; }
    }
}