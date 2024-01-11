using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class SocietyInformation
    {
        public SocietyInformation()
        {
            
        }

        #region jntuh_college_establishment and jntuh_address Table Parameters

        public int id { get; set; }

        public int collegeId { get; set; }

        #endregion

        #region jntuh_college_establishment Table Parameters

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Year of Establishment of the Institution	")]
        public int instituteEstablishedYear { get; set; }

        //Added Two New Fields on 07-02-2020
        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Copy of Registration Certificate")]
        public HttpPostedFileBase RegistrationDocument { get; set; }
        public string RegistrationDocumentfile { get; set; }

        [Display(Name = "Proceedings of the Appointment of Members")]
        public HttpPostedFileBase MembersDetailsDocument { get; set; }
        public string MembersDetailsDocumentfile { get; set; }


        [Required(ErrorMessage = "Required")]
        [Display(Name = "Year of Establishment")]
        public int societyEstablishmentYear { get; set; }

        [Display(Name = "Date on which first approval was accorded by the AICTE")]
        public string firstApprovalDateByAICTE { get; set; }

        [Display(Name = "Date on which first affiliation was accorded by the JNTU/JNTUH")]
        public string firstAffiliationDateByJNTU { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Year of commencement of First Batch")]
        public int firstBatchCommencementYear { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500,ErrorMessage = "Must be 500 characters")]
        [Display(Name = "Society / Trust / Company Name")]
        public string societyName { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(50, ErrorMessage = "Must be 50 characters")]
        [Display(Name = "Society / Trust / Company Registration Number")]
        public string societyRegisterNumber { get; set; }

        [StringLength(500, ErrorMessage = "Must be 500 characters")]
        [Display(Name = "Old Society / Trust / Company Name in the past (if any)")]
        public string OldSocietyName { get; set; }

        [Display(Name = "Society / Trust / Company MoU")]
        public HttpPostedFileBase Societymoudoc { get; set; }
        public string Societymoufile { get; set; }

        [Display(Name = "Society / Trust / Company bye-laws")]
        public HttpPostedFileBase Societybyelawsdoc { get; set; }
        public string Societybylawsfile { get; set; }

        #endregion       

        #region jntuh_address Table Parameters
        
        [Required(ErrorMessage = "Required")]
        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [Display(Name = "Address Type")]
        public string addressTye { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(250, ErrorMessage = "Maximum 250 characters")]
        [Display(Name = "Address")]
        public string address { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(50, ErrorMessage = "Maximum 50 characters")]
        [Display(Name = "Town (or) City")]
        public string townOrCity { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(50, ErrorMessage = "Maximum 50 characters")]
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
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [Display(Name = "Email")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]
        public string email { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [Display(Name = "Website Address")]
        [RegularExpression(@"^((ftp|http|https):\/\/)?([a-zA-Z0-9]+(\.[a-zA-Z0-9]+)+.*)$", ErrorMessage = "Please enter correct website address")]
        public string website { get; set; }


        public string stateName  { get; set; }

        public string districtName { get; set; }

        #endregion

        #region jntuh_college Table Parameters

        [Required(ErrorMessage = "Required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Must be 2 characters")]
        [Display(Name = "College Code")]
        public string collegeCode { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters")]
        [Display(Name = "College Name")]
        public string collegeName { get; set; }

        #endregion

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_district jntuh_district { get; set; }
        public virtual jntuh_state jntuh_state { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}