using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class NAACCollegesList
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public Nullable<System.DateTime> NAACFromDate { get; set; }
        public Nullable<System.DateTime> NAACToDate { get; set; }
        public string NAACApprovalLetter { get; set; }
    }

    public class NAACCollegesListExport
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string NAACFromDate { get; set; }
        public string NAACToDate { get; set; }
        public string NAACApprovalLetter { get; set; }
    }
}