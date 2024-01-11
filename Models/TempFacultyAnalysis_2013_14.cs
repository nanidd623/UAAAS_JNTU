using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class TempFacultyAnalysis_2013_14
    {
        public int id { get; set; }
        public Nullable<double> SNO { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Qualification { get; set; }
        public Nullable<double> Exp { get; set; }
        public Nullable<double> DOA { get; set; }
        public string PayScale { get; set; }
        public string UR { get; set; }
        public string facultyinothercollege { get; set; }
        public int count { get; set; }
        public string FileName { get; set; }
    }
}