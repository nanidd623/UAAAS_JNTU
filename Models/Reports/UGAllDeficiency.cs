using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class UGAllDeficiency
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string CollegeAddress { get; set; }
        public string Labs { get; set; }
        public int Computers { get; set; }
        public int Strength { get; set; }
        public int Faculty { get; set; }
        public string FacultyNumber { get; set; }
        public decimal FacultyPercentage { get; set; }
        public string Library { get; set; }
        public string BoysHostel { get; set; }
        public string GirlsHostel { get; set; }
        public string PlacementAndTraining { get; set; }
        public IEnumerable<UGWithDeficiencyCollegeSpecializations> CollegeSpecializations { get; set; }
    }
}