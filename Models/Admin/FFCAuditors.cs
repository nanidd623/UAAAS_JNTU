using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class FFCAuditors
    {
        #region jntuh_ffc_auditor
        public int id { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Auditor Name")]
        public string auditorName { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department")]
        public int auditorDepartmentID { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Designation")]
        public int auditorDesignationID { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Email1")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]
        public string auditorEmail1 { get; set; }
        [Display(Name = "Email2")]
        public string auditorEmail2 { get; set; }
        [Display(Name = "Preferred Designation")]
        public string auditorPreferredDesignation { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Mobile1")]
        public string auditorMobile1 { get; set; }
        [Display(Name = "Mobile2")]
        public string auditorMobile2 { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Campus")]
        public string auditorPlace { get; set; }
        public bool isActive { get; set; }
        public int createdBy { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }

        public virtual jntuh_department jntuh_department { get; set; }
        public virtual jntuh_designation jntuh_designation { get; set; }

        #endregion
    }

}