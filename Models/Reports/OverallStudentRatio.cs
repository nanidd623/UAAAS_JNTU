using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class OverallStudentRatio
    {
        public int id { get; set; }
        public int academicYearId { get; set; }
        public int collegeId { get; set; }       
        public int degreeID { get; set; }     
        public int DepartmentID { get; set; }    
        public int specializationId { get; set; }      
        public int shiftId { get; set; }
        public int approvedIntake1 { get; set; }        
        public int approvedIntake2 { get; set; }     
        public int approvedIntake3 { get; set; }      
        public int approvedIntake4 { get; set; }       
        public int approvedIntake5 { get; set; }    
        public int approvedIntake6 { get; set; }      
        public int totalFaculty { get; set; }      
        public int ratifiedFaculty { get; set; }            
        public string Department { get; set; }     
        public string Specialization { get; set; }  
        public int? degreeDisplayOrder { get; set; }
        public string degree { get; set; }
    }
}