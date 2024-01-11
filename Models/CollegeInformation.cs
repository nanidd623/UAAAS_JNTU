using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class Item
    {
        public int id { get; set; }
        public string name { get; set; }
        public int selected { get; set; }
    }

    public class CollegeInformation
    {
        public CollegeInformation()
        {
            this.jntuh_address = new HashSet<jntuh_address>();
            this.jntuh_college_degree = new HashSet<jntuh_college_degree>();
            this.jntuh_college_affiliation = new HashSet<jntuh_college_affiliation>();
           
        }
        public IList<CollegeInformation> entrymaster { get; set; }
        public IEnumerable<Item> CheckBoxItems { get; set; }
        public string[] SelectedOptions { get; set; }

        //jntuh_college
        public int id { get; set; }
        public int academicYearid { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Affiliation Type")]
        public string[] collegeAffiliationTypeID { get; set; }
        public IEnumerable<Item> collegeAffiliationType { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Must be 2 characters")]
        [Display(Name = "College Code")]
        public string collegeCode { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters")]
        [Display(Name = "College Name")]
        public string collegeName { get; set; }

        [StringLength(500, ErrorMessage = "Maximum 500 characters")]
        [Display(Name = "Former College Name")]
        public string formerCollegeName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "College Type")]
        public string[] collegeTypeID { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "College Type")]
        public IEnumerable<Item> collegeType { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "College Status")]
        public int collegeStatusID { get; set; }

        public string collegeStatus { get; set; }
        public string collegeminorityStatus { get; set; }

        public int minortyid { get; set; }
        [Required(ErrorMessage = "*")]
        public int collegesubstatusId { get; set; }
        [Required(ErrorMessage = "*")]
        public string collegestatusfromdate { get; set; }
        [Required(ErrorMessage = "*")]
        public string collegestatustodate { get; set; }
        public string collegestatusfilepath { get; set; }
        public HttpPostedFileBase collegestatusfile { get; set; }


        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        //[Required(ErrorMessage = "Required")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Maximum 10 characters")]
        [Display(Name = "TS EAMCET Code")]
        public string eamcetCode { get; set; }

        //[Required(ErrorMessage = "Required")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Maximum 10 characters")]
        [Display(Name = "ICET Code")]
        public string icetCode { get; set; }

        [StringLength(10, MinimumLength = 2, ErrorMessage = "Maximum 10 characters")]
        [Display(Name = "PGECET Code")]
        //[Required(ErrorMessage = "Required")]
        public string pgcetCode { get; set; }

        [StringLength(25, MinimumLength =5, ErrorMessage = "Maximum 25 characters")]
        [Display(Name = "Permanent AICTE-id.")]
        [Required(ErrorMessage = "Required")]
        public string aicteid { get; set; }

        public virtual ICollection<jntuh_address> jntuh_address { get; set; }
        public virtual jntuh_college_type jntuh_college_type { get; set; }
        public virtual jntuh_college_status jntuh_college_status { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual ICollection<jntuh_college_degree> jntuh_college_degree { get; set; }
        public virtual ICollection<jntuh_college_affiliation> jntuh_college_affiliation { get; set; }
        public virtual jntuh_college_affiliation_type jntuh_college_affiliation_type { get; set; }

        //jntuh_adress
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

        public virtual jntuh_district jntuh_district { get; set; }
        public virtual jntuh_state jntuh_state { get; set; }

        //jntuh_college_affiliation

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Affiliation Type")]
        public string[] affiliationTypeId { get; set; }
        public IEnumerable<Item> affiliationType { get; set; }

        [Required(ErrorMessage = "Required")]
        public string affiliationTypeId1 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationTypeId2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public string affiliationTypeId3 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationTypeId4 { get; set; }
       // [Required(ErrorMessage = "Required")]
        public string[] affiliationTypeId5 { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "From")]
        //public System.DateTime affiliationFromDate { get; set; }
        public string[] affiliationFromDate { get; set; }

        [Required(ErrorMessage = "Required")]
        public string affiliationFromDate1 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationFromDate2 { get; set; }
        //public string[] affiliationFromDate2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public string affiliationFromDate3 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationFromDate4 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationFromDate5 { get; set; }

        [Required(ErrorMessage = "Required")]
        public string pa_affiliationFromDate5 { get; set; }


        //[Required(ErrorMessage = "Required")]
        [Display(Name = "To")]
        //public System.DateTime affiliationToDate { get; set; }
        public string[] affiliationToDate { get; set; }
        [Required(ErrorMessage = "Required")]
        public string affiliationToDate1 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationToDate2 { get; set; }
        //public string[] affiliationToDate2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public string affiliationToDate3 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationToDate4 { get; set; }
       // [Required(ErrorMessage = "Required")]
        public string[] affiliationToDate5 { get; set; }

        [Required(ErrorMessage = "Required")]
        public string pa_affiliationToDate5 { get; set; }


        [Display(Name = "Duration")]
        public string[] affiliationDuration { get; set; }
        [Required(ErrorMessage = "Required")]
        public string affiliationDuration1 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationDuration2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public string affiliationDuration3 { get; set; }

        //[Required(ErrorMessage = "Required")]
        public HttpPostedFileBase affiliationfile1 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public HttpPostedFileBase affiliationfile2 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public HttpPostedFileBase affiliationfile3 { get; set; }

        public HttpPostedFileBase affiliationfile5{ get; set; }

        public HttpPostedFileBase pa_affiliationfile5 { get; set; }

        //[Required(ErrorMessage = "Required")]
        public string affiliationfilepath1 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationfilepath2 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string affiliationfilepath3 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationfilepath4 { get; set; }
        public string[] affiliationfilepath5 { get; set; }

        public string pa_affiliationfilepath5 { get; set; }

        //[Required(ErrorMessage = "Required")]
        public string[] affiliationDuration4 { get; set; }
        //[Required(ErrorMessage = "Required")]
        public string[] affiliationDuration5 { get; set; }

        [Required(ErrorMessage = "Required")]
        public string pa_affiliationDuration5 { get; set; }


        //[Required(ErrorMessage = " Required")]
        [Display(Name = "Grade")]
        public string affiliationGrade { get; set; }

        [Display(Name = "CGPA")]
        public string affiliationCGPA { get; set; }

        public string affiliationStatus { get; set; }

        public bool isSelectedAffiliation { get; set; }

        public string[] selectedAffiliation { get; set; }

        public int affiliationSelected1 { get; set; }
        public int affiliationSelected2 { get; set; }
        public int affiliationSelected3 { get; set; }
        public int affiliationSelected4 { get; set; }
        public int affiliationSelected5 { get; set; }

        [Display(Name = "Other Category")]
        [StringLength(250, ErrorMessage = "Must be under 250 characters")]
        public string otherCategory { get; set; }

        public virtual jntuh_affiliation_type jntuh_affiliation_type { get; set; }
        public virtual jntuh_college jntuh_college { get; set; }

        //jntuh_college_degree
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Courses")]
        public string[] degreeId { get; set; }
        public IEnumerable<Item> degree { get; set; }

        public virtual jntuh_degree jntuh_degree { get; set; }

        public string stateName { get; set; }

        public string districtName { get; set; }

        public string statusName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Year of Establishment of the Institution	")]
        public int instituteEstablishedYear { get; set; }

        [Display(Name = "Date on which first approval was accorded by the AICTE")]
        public string firstApprovalDateByAICTE { get; set; }

        [Display(Name = "Supporting Document")]
        public HttpPostedFileBase FirstApprovalDateByAICTEDoc { get; set; }

        public string FirstApprovalDateByAICTEDocPath { get; set; }

        [Display(Name = "Date on which first affiliation was accorded by the JNTU/JNTUH")]
        public string firstAffiliationDateByJNTU { get; set; }

        [Display(Name = "Supporting Document")]
        public HttpPostedFileBase FirstAffiliationDateByJNTUDoc { get; set; }

        public string FirstAffiliationDateByJNTUDocPath { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Year of commencement of First Batch")]
        public int firstBatchCommencementYear { get; set; }

        public string affiliationId1 { get; set; }
        public string affiliationId3 { get; set; }
        public string affiliationId5 { get; set; }
    }
}