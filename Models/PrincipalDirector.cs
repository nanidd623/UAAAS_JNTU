using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    //[MetadataType(typeof(PrincipalDirectorRequestAttribs))]
    //public partial class jntuh_college_principal_director
    //{
    //}

    public class PrincipalDirector
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public string type { get; set; }

        [Display(Name = "First Name")]
        public string firstName { get; set; }

        [Display(Name = "Last Name")]
        public string lastName { get; set; }

        [Display(Name = "Surname")]
        public string surname { get; set; }

        [Display(Name = "SCM Document")]
        public string SCM { get; set; }

        [Display(Name = "Date Of Appointment")]
        public string dateOfAppointment { get; set; }

        [Display(Name = "Date Of Resignation")]
        public Nullable<System.DateTime> dateOfResignation { get; set; }

        [Display(Name = "Date Of Birth")]
        public string dateOfBirth { get; set; }

        [StringLength(11, ErrorMessage = "Must be 11 digits, ex: 04012345678")]
        [Display(Name = "Fax")]
        [RegularExpression(@"\d{11}", ErrorMessage = "Please enter correct fax number, ex: 04012345678")]
        public string fax { get; set; }

        [StringLength(11, ErrorMessage = "Must be 11 digits, ex: 04012345678")]
        [Display(Name = "Landline")]
        [RegularExpression(@"\d{11}", ErrorMessage = "Please enter correct landline number, ex: 04012345678")]
        public string landline { get; set; }

        [StringLength(10, ErrorMessage = "Must be 10 digits, ex: 9493929190")]
        [Display(Name = "Mobile")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Please enter correct mobile number, ex: 9493929190")]
        public string mobile { get; set; }

        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]
        [Display(Name = "Email")]
        public string email { get; set; }

        [Display(Name = "Department")]
        public Nullable<int> departmentId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(12, ErrorMessage = "Must be 12 characters")]
        //[RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [RegularExpression(@"^(\d{12})$", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "Principal Aadhaar Number")]
        [Remote("CheckAadharNumber", "PrincipalDirector", AdditionalFields = "RegistrationNumber", HttpMethod = "POST", ErrorMessage = "Aadhar Number is Not Correct.")]
        public string PrincipalAadhaarNumber { get; set; }

        [Display(Name = "PrincipalAadharPhotoDocument")]
        public HttpPostedFileBase PrincipalAadharPhotoDocument { get; set; }
        public string PrincipalAadharDocument { get; set; }

        public bool IsPrincipleChecked { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "Photo")]
        public HttpPostedFileBase PrincipalPhotoDocument { get; set; }
        public string PrincipalPhoto { get; set; }

        [Required(ErrorMessage = "Required")]
        //[Remote("CheckRegistrationNumber", "PrincipalDirector", HttpMethod = "POST", ErrorMessage = "*")]
        public string RegistrationNumber { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_qualification jntuh_qualification { get; set; }
        public virtual jntuh_department jntuh_department { get; set; }
        public virtual jntuh_phd_subject jntuh_phd_subject { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }

        public string departmentName { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }

        // Added on 14-02-2020
        public string principalfresherexperiance { get; set; }

        public int ExperienceId { get; set; }
        public int PrincipalId { get; set; }
        public int Previouscollegeid { get; set; }
        public string Otherscollegename { get; set; }
        //public int? facultyDesignationId { get; set; }
        //public string facultyOtherDesignation { get; set; }
        public string PrincipaldateOfResignation { get; set; }
        public string PrincipaldateOfreliving { get; set; }
        public string PrincipalSalary { get; set; }
        public HttpPostedFileBase PrincipalAppointmentDocument { get; set; }
        public HttpPostedFileBase PrincipalRelivingDocument { get; set; }
        public HttpPostedFileBase PrincipalSelectionCommitteeDocument { get; set; }
        public string AppointmentDocumentView { get; set; }
        public string RelivingDocumentView { get; set; }
        public string SelectionCommitteeDocumentView { get; set; }
        public string AadharDocumentView { get; set; }
        [Required(ErrorMessage = "Required")]
        public int AcademicYearId { get; set; }
        public string AcademicYear { get; set; }
        //public string Principalfresherexperiance { get; set; }
    }
}