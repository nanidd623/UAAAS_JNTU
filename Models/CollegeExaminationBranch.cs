using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegeExaminationBranch
    {
        #region jntuh_college_examination_branch table Parameters

        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[0-9]\d{0,2}(\.\d{1,2})?%?$", ErrorMessage = "Please enter correct Area, ex: 999.99")]
        [Display(Name = "Area (In Square meters)")]
        public decimal examinationBranchArea { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Confidential room for question paper preparation")]
        public bool isConfidenatialRoomExists { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "The examination branch is located adjacent to the Principal’s room")]
        public bool isAdjacentPrincipalRoom { get; set; }

        #endregion        

        #region jntuh_college_examination_branch_security  table Parameters

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters")]
        [Display(Name = "1)")]
        public string securityMesearesTaken1 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters")]
        [Display(Name = "2)")]
        public string securityMesearesTaken2 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters")]
        [Display(Name = "3)")]
        public string securityMesearesTaken3 { get; set; }

        #endregion

        #region Comman Parameters for three tables
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }

        #endregion
    }
}