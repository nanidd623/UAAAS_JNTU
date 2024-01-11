using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class FacultyReport
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string Gender { get; set; }
        public decimal? Experience { get; set; }
        public System.DateTime DateOfAppointment { get; set; }
        public string PayScale { get; set; }
        public string UniversityRatified { get; set; }
        public bool IsCollegeEditable { get; set; }
        public string SelectedCollegeType { get; set; }
        public string SelectedGender { get; set; }
        public int DistrictId { get; set; }
        public int CollegeTypeId { get; set; }
        public int GengerId { get; set; }
    }
}