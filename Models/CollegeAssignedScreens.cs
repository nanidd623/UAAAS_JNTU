using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegeAssignedScreens
    {
        public int Id { get; set; }
        public int CollegeId { get; set; }
        public int ScreenId { get; set; }
        public bool IsEditable { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public string Remarks { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_college_screens jntuh_college_screens { get; set; }

        public virtual IList<Colleges> colleges { get; set; }
        public virtual IList<CollegeScreens> collegeScreens { get; set; }

        //public System.DateTime FromDate { get; set; }
        //public System.DateTime ToDate { get; set; }

        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
    public class Colleges
    {
        public int Id { get; set; }      
        public string CollegeName { get; set; }
    }
    public class CollegeScreens
    {
        public int Id { get; set; }
        public string ScreenCode { get; set; }
        public string ScreenName { get; set; }
        public bool IsSelected { get; set; }
    }
}