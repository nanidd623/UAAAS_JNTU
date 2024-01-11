using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class ChairPerson
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public int UserID { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Address Type")]
        public string addressTye { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "First Name")]
        public string firstName { get; set; }

        [Display(Name = "Last Name")]
        public string lastName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Surname")]
        public string surname { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Designation")]
        public int designationId { get; set; }

        [Display(Name = "")]
        public string otherDesignation { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Address")]
        public string address { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "City/Town")]
        public string townOrCity { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Mandal")]
        public string mandal { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "District")]
        public int districtId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "State")]
        public int stateId { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"\d{6}", ErrorMessage = "Please enter correct pincode, ex: 123456")]
        [Display(Name = "Pincode")]
        public Nullable<int> pincode { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(11, ErrorMessage = "Must be 11 digits, ex: 04012345678")]
        [RegularExpression(@"\d{11}", ErrorMessage = "Please enter correct fax number, ex: 04012345678")]
        [Display(Name = "Fax")]
        public string fax { get; set; }

        [StringLength(11, ErrorMessage = "Must be 11 digits, ex: 04012345678")]
        [RegularExpression(@"\d{11}", ErrorMessage = "Please enter correct landline number, ex: 04012345678")]
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Landline")]
        public string landline { get; set; }

        [StringLength(10, ErrorMessage = "Must be 10 digits, ex: 9493929190")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Please enter correct mobile number, ex: 9493929190")]
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Mobile")]
        public string mobile { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Email")]
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]
        public string email { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Photo")]
        public HttpPostedFileBase ChairpersionPhoto{ get; set; }
        public string ChairpersionPhotoview { get; set; }
     
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [Display(Name = "Website Address")]
        [RegularExpression(@"^((ftp|http|https):\/\/)?([a-zA-Z0-9]+(\.[a-zA-Z0-9]+)+.*)$", ErrorMessage = "Please enter correct website address")]
        public string website { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        #region jntuh_college Table Parameters

        [Required(ErrorMessage = "Required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Must be 2 characters")]
        [Display(Name = "College Code")]
        public string collegeCode { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters")]
        [Display(Name = "College Name")]
        public string collegeName { get; set; }


        public string stateName { get; set; }

        public string districtName { get; set; }

        public string designationName { get; set; }
        #endregion

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_district jntuh_district { get; set; }
        public virtual jntuh_state jntuh_state { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_chairperson_designation jntuh_chairperson_designation { get; set; }
    }
}