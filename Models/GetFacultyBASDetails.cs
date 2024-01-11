using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class GetFacultyBASDetails
    {
        public string RegistarationNumber { get; set; }
        public string BasJoiningDate { get; set; }
        public string Month { get; set; }
        public int? JulyTotalDays { get; set; }
        public int? AugustTotalDays { get; set; }
        public int? SeptemberTotalDays { get; set; }
        public int? OctoberTotalDays { get; set; }
        public int? NovemberTotalDays { get; set; }
        public int? DecemberTotalDays { get; set; }
        public int? JanuaryTotalDays { get; set; }
        public int? FebruaryTotalDays { get; set; }
        public int? MarchTotalDays { get; set; }
        public int? JulyPresentDays { get; set; }
        public int? AugustPresentDays { get; set; }
        public int? SeptemberPresentDays { get; set; }
        public int? OctoberPresentDays { get; set; }
        public int? NovemberPresentDays { get; set; }
        public int? DecemberPresentDays { get; set; }
        public int? JanuaryPresentDays { get; set; }
        public int? FebruaryPresentDays { get; set; }
        public int? MarchPresentDays { get; set; }
        public int? TotalWorkingDays { get; set; }
        public int? TotalPresentDays { get; set; }
        public string FacultyName { get; set; }
        public DateTime? Createdon { get; set; }
        public int? MonthHolidays { get; set; }
        public int? MonthPresentDays { get; set; }
        public int? MonthWorkingDays { get; set; }
        public List<GetFacultyBASDetails> FacultyBASDetails { get; set; }
    }
}