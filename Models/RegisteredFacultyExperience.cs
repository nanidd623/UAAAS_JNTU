using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class jntuRegisteredFacultyExperience
    {

        public int Id { get; set; }
        public int facultyId { get; set; }
        public Nullable<int> collegeId { get; set; }
        public string OtherCollege { get; set; }
        public Nullable<int> createdBycollegeId { get; set; }
        public Nullable<int> facultyDesignationId { get; set; }
        public string OtherDesignation { get; set; }
        public Nullable<System.DateTime> facultyDateOfAppointment { get; set; }
        public Nullable<System.DateTime> facultyDateOfResignation { get; set; }
        public string facultyJoiningOrder { get; set; }
        public string facultyRelievingLetter { get; set; }
        public Nullable<decimal> facultySalary { get; set; }
        public string FacultySCMDocument { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
    }
}