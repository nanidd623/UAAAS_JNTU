using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
namespace UAAAS.Models
{
    public class MissedCourses
    {
        [Required(ErrorMessage="Required")]
        public int collegeID { get; set; }
        public int collegeCode { get; set; }
    }
    public class AuditorWiseReport
    {
        public Nullable<int> collegeID { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public Nullable<int> auditorID { get; set; }
        public string auditorName { get; set; }
        public string auditorPreferredDesignation { get; set; }
        public string auditorPlace { get; set; }
        //[DisplayFormat(DataFormatString = "{0:dd/mm/yyyy}")]
        public Nullable<System.DateTime> inspectionDate { get; set; }
        public string strinspectionDate { get; set; }
        
    }
}