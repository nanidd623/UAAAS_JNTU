using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class TempFacultyAnalysis_2014_15
    {
        public string Code { get; set; }
        public string CollegeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Surname { get; set; }
        public string Father { get; set; }
        public DateTime DateOfAppointment { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string photo { get; set; }
        public int count { get; set; }
        public string facultyinothercollege { get; set; }    
    }
}