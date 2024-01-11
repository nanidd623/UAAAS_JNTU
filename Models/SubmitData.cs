using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class SubmitData
    {
        #region jntuh_college_edit_ststus

        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage="Required")]
        [Display(Name = "I Agree that College data uploded in this portal is valid")]
        public bool IsCollegeEditable { get; set; }
        public Nullable<System.DateTime> editFromDate { get; set; }
        public Nullable<System.DateTime> editToDate { get; set; }
        public HttpPostedFileBase DeclarationPath { get; set; }
        public HttpPostedFileBase OtherSupportingDoc { get; set; }
        public string DeclarationPathdoc { get; set; }
        public string OtherSupportingDocpath { get; set; }
        public string Remarks { get; set; }
        #endregion

        #region jntuh_college_edit_remarks

        public bool isCollegeRemarks { get; set; }
        [Display(Name = "Comments")]
        public string collegeEditRemarks { get; set; }

        #endregion

        #region CommanRecords

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }

        #endregion        
    }


    public class LabswithDeficiency
    {
        public string CollegeCode { get; set; }
        public int CollegeId { get; set; }
        public string CollegeName { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public string Specilization { get; set; }
        public int DegreeId { get; set; }
        public int DepartmentId { get; set; }
        public int SpecilizationId { get; set; }
        public string LabName { get; set; }
    }
}