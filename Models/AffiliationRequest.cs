using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    //[MetadataType(typeof(AffiliationRequestAttribs))]
    //public partial class jntuh_affiliation_requests
    //{
    //    //leave it empty   
        
    //    public string Captcha { get; set; }
    //}

    public class AffiliationRequestAttribs
    {
        public int id { get; set; }
        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        [Display(Name = "College Name")]
        public string collegeName { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        [Display(Name = "College Address")]
        public string collegeAddress { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "City")]
        public string townOrCity { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Mandal")]
        public string mandal { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "District")]
        public int districtId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "State")]
        public int stateId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Pincode")]
        [RegularExpression(@"\d{6}", ErrorMessage = "Please enter correct pincode, ex: 123456")]
        public int pincode { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(11, ErrorMessage = "Must be 11 digits, ex: 04012345678")]
        [Display(Name = "Fax")]
        [RegularExpression(@"\d{11}", ErrorMessage = "Please enter correct fax number, ex: 04012345678")]
        public string fax { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(11, ErrorMessage = "Must be 11 digits, ex: 04012345678")]
        [Display(Name = "Landline")]
        [RegularExpression(@"\d{11}", ErrorMessage = "Please enter correct landline number, ex: 04012345678")]
        public string landline { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(10, ErrorMessage = "Must be 10 digits, ex: 9493929190")]
        [Display(Name = "Mobile")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Please enter correct mobile number, ex: 9493929190")]
        public string mobile { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Email")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]
        public string email { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Website Address")]
        [RegularExpression(@"^((ftp|http|https):\/\/)?([a-zA-Z0-9]+(\.[a-zA-Z0-9]+)+.*)$", ErrorMessage = "Please enter correct website address")]
        public string website { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "College Type")]
        public int collegeTypeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "College Status")]
        public int collegeStatusId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Accredited by NAAC")]
        public bool isAccreditedByNAAC { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(250, ErrorMessage = "Must be under 250 characters")]
        [Display(Name = "Present Affiliated University")]
        public string presentAffiliatedUniversity { get; set; }

        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        [Display(Name = "Comments (if any)")]
        public string comments { get; set; }

        [Display(Name = "Society Name")]
        public string societyName { get; set; }

        [Display(Name = "Approval Status")]
        public bool isApproved { get; set; }

      

        [Display(Name = "Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_state jntuh_state { get; set; }
        public virtual jntuh_district jntuh_district { get; set; }
        public virtual jntuh_college_type jntuh_college_type { get; set; }
        public virtual jntuh_college_status jntuh_college_status { get; set; }
    }
}