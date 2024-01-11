using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class AICTEFacultyClass
    {
       // [Required(ErrorMessage = "Select College")]
        [Display(Name = "College")]
        public int CollegeId { get; set; }

        [Display(Name = "Id")]
        public int Id { get; set; }

        public int AcademicyearId { get; set; }

        [Required(ErrorMessage = "Enter Your AICTE FacultyId")]
        [Display(Name="AICTE FacultyId")]
        public string FacultyId { get; set; }

       // [Required(ErrorMessage = "Enter Your Jntuh RegistrationNumber")]
        [Display(Name = "JNTUH Registration Number")]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Enter PANNumber")]
        [Display(Name = "PAN Number")]
        [RegularExpression(@"[a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}", ErrorMessage = "Invalid PAN number")]
        public string PanNumber { get; set; }

        [Required(ErrorMessage = "Enter AadhaarNumber")]
        [Display(Name="Aadhaar Number")]
        [StringLength(16, ErrorMessage = "Must be 12 characters")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Remote("CheckAadharNumber", "AICTEFaculty", HttpMethod = "POST", ErrorMessage = "Aadhar Number is Not Correct.")]
        public string AadhaarNumber { get; set; }

        //[Required(ErrorMessage = "Enter Mobile NUmber")]
        //[Display(Name = "Mobile")]
        //[RegularExpression(@"\d{10}", ErrorMessage = "Invalid Mobile")]
        //[StringLength(10, ErrorMessage = "Maximum 10 Characters")]
        //public string Mobile { get; set; }
        
        //[Required(ErrorMessage="Enter Email Id")]
        //[Display(Name="EmailID")]
        //[StringLength(100,MinimumLength=6,ErrorMessage="Email Id is 6-100 Characters Required")]
        //[RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}",ErrorMessage="Invalid EmailId")]
        //public string Email { get; set; }

        [Required(ErrorMessage = "Select Programme")]
        [Display(Name = "Programme")]
        public string Programme { get; set; }

       //  [Required(ErrorMessage = "Enter FacultyType")]
       // [Display(Name = "Faculty Type")]
       // public string FacultyType { get; set; }

       //[Required(ErrorMessage = "Enter Jobtype")]
       // [Display(Name = "Job Type(FT/PT)")]
       // public string JobType { get; set; }

        [Required(ErrorMessage = "Select Course")]
        [Display(Name = "Course")]
        public string Course { get; set; }

        [Required(ErrorMessage = "Enter Firstname")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

         [Required(ErrorMessage = "Enter Surname")]
        [Display(Name = "SurName")]
        public string SurName { get; set; }

        //[Required(ErrorMessage = "Enter ExactDesignation")]
        //[Display(Name = "Exact Designation")]
        //public string ExactDesignation { get; set; }

        ////[Required(ErrorMessage="Enter Your Designation Name")]
        // public string  OtherDesignation { get; set; }


        //[Required(ErrorMessage = "Select DateOfJoiningTheInstitute")]
        //[Display(Name = "DateOfJoiningTheInstitute")]
        //public string DateOfJoiningTheInstitute { get; set; }

        //[Required(ErrorMessage = "Enter AppointmentType")]
        //[Display(Name = "Appointment Type")]
        //public string AppionmentType { get; set; }

        //[Required(ErrorMessage = "Enter Docotorate")]
        //[Display(Name = "Doctorate")]
        //public string Doctorate { get; set; }

        // [Required(ErrorMessage = "Enter MastersDegree")]
        //[Display(Name = "Masters Degree")]
        //public string MastersDegree { get; set; }

        //[Required(ErrorMessage = "Enter BachelorsDegree")]
        //[Display(Name = "Bachelors Degree")]
        //public string BachelorsDegree { get; set; }

        //[Required(ErrorMessage = "Enter OtherQualification")]
        //[Display(Name = "Other Qualification")]
        //public string OtherQualification { get; set; }
    }
}