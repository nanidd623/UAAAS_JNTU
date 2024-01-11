using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class MissedProposedCourses
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public string collegeCode { get; set; }
        public int degreeId { get; set; }
        public int departmentId { get; set; }
        public int specializationId { get; set; }        
        public int shiftId { get; set; }
        public string degree { get; set; }
        public string department { get; set; }
        public string specilization { get; set; }
        public string shift { get; set; }
        public int existingIntake { get; set; }
        public int proposedIntake { get; set; }
        public bool status { get; set; }
    }
}