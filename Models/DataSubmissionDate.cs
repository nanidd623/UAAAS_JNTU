using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class DataSubmissionDate
    {

        #region jntuh_college_edit_ststus#region 
        public int Id { get; set; }
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }        
        public Nullable<System.DateTime> FromDate { get; set; }        
        public Nullable<System.DateTime> ToDate { get; set; }
        [Required]
        public string strFromDate { get; set; }
        [Required]
        public string strToDate { get; set; }
        public bool EditStatus { get; set; }
        public bool IsCollegeRemarks { get; set; }
        public string CollegeEditRemarks { get; set; }

        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; } 

        #endregion

    }
}