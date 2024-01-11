using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class AllCollegesStaffReport
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public int Total { get; set; }
        public int Teaching { get; set; }
        public int NonTeaching { get; set; }
        public decimal Ratified { get; set; }
        public int Professors { get; set; }
        public int AssociateProfessors { get; set; }
        public int AssistantProfessors { get; set; }
        public int Others { get; set; }
        public int BTech { get; set; }
        public int BPharmacy { get; set; }
        public int MTech { get; set; }
        public int MPharmacy { get; set; }
        public int MCA { get; set; }
        public int MBA { get; set; }
        public int MAM { get; set; }
        public int MTM { get; set; }
        public int Pharmd { get; set; }
        public int PharmdPB { get; set; }
    }
}