using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class PublicColleges
    {
        public int DistrictId { get; set; }
        public int DegreeId { get; set; }
        public List<jntuh_district> DistrictList { get; set; }
        public List<jntuh_degree> DegreeList { get; set; }
        public List<PublicCollegeList> PublicCollegeList { get; set; }
    }

    public class PublicCollegeList
    {
        public int id { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
    }
}