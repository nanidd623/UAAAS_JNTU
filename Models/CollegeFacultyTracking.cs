using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegeFacultyTracking
    {
        public string Department { get; set; }
        public string Specialization { get; set; }
        public string AadhaarNumber { get; set; }
        public string AadhaarDoc { get; set; }
        public string FacultyJoinDate { get; set; }
        public string FacultyJoinDoc { get; set; }
        public string ReleavingDate { get; set; }
        public string ReleavingDoc { get; set; }
        public string ScmDoc { get; set; }
        public string Payscale { get; set; }
        public string Designation { get; set; }
        public string FacultyType { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}