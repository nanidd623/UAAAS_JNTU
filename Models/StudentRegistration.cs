using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class StudentRegistration
    {
        public int SpecializationId { get; set; }
        public int DepartmentId { get; set; }
        public string SpecializationName { get; set; }
        public string DegreeName { get; set; }
        [Required(ErrorMessage = "*")]
        [Remote("CheckHallticketNumber", "CollegeStudentRegistration", HttpMethod = "POST", ErrorMessage = "HallTicketNumber already exists.")]
        public string HallTicketNumber { get; set; }
        [Required(ErrorMessage = "*")]
        [Remote("EditHallticketNumber", "CollegeStudentRegistration", AdditionalFields = "AadhaarNumber", HttpMethod = "POST", ErrorMessage = "HallTicketNumber already exists.")]
        public string UpdateHallTicketNo { get; set; }
        [Required(ErrorMessage = "*")]
        public string Name { get; set; }
        public int ShiftId { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Gender")]
        public int GenderId { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Must be 3-100 characters long.")]
        [Display(Name = "Father Name")]
        public string FatherOrhusbandName { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Date of Birth")]
        public string studentDateOfBirth { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        [Display(Name = "Mobile No")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(16, ErrorMessage = "Must be 12 characters")]
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "Aadhaar Number")]
        [Remote("CheckAadharNumber", "CollegeStudentRegistration", HttpMethod = "POST", ErrorMessage = "Aadhaar Number already exists.")]
        public string AadhaarNumber { get; set; }

        public HttpPostedFileBase AadhaarDocument { get; set; }
        public int Id { get; set; }
        public int CollegeId { get; set; }
        public int DegreeId { get; set; }
        public string department { get; set; }
        public string AadhaarDocumentview { get; set; }
        public string PresentAcademicYear { get; set; }
        public bool Isedit { get; set; }
    }


    public class Jntuh_college_student_list
    {
        public int sid { get; set; }
        public string studentname { get; set; }
        public string HallticketNum { get; set; }
        public int pid { get; set; }
        public int MyProperty { get; set; }
    }
    public class jntuh_pgstudent_projectdetatails_class
    {
        public int ID { get; set; }
        public Nullable<int> AcademicYearId { get; set; }
        public int CollegeId { get; set; }
        public Nullable<bool> ProjectEnroll { get; set; }
        public string StudentsName { get; set; }
        public string HallticketNumber { get; set; }
        public string DeptName { get; set; }
        public string ProjectTitle { get; set; }
        public string ProjectDescription { get; set; }
        public Nullable<int> ProjectType { get; set; }
        public Nullable<int> NumOfReviews { get; set; }
        public string ReviewedWeeks { get; set; }
        public string InternalGuideRegNumber { get; set; }
        public string InternalGuideName { get; set; }
        public Nullable<int> InternalGuideDesignation { get; set; }
        public int InternalguideDepartment { get; set; }
        public string InternalguideDepartmentNAME { get; set; }
        public string InternalguideEmail { get; set; }
        public string ExternalGuideName { get; set; }
        public Nullable<int> ExternalGuideDesignation { get; set; }
        public string ExternalEmail { get; set; }
        public string ExternalQualification { get; set; }
        public Nullable<int> ExternalExperience { get; set; }
        public string ExternalOrgName { get; set; }
        public string ExternalOrgAddress { get; set; }

        public string Reason { get; set; }
    }
    public class ugstudents
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "*")]
        public string studentName { get; set; }
        public string DepartmentName { get; set; }
        [Required(ErrorMessage = "*")]
        public string HallTicketNumber { get; set; }
        public string Gender { get; set; }
        [Required(ErrorMessage = "*")]
        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        [Display(Name = "Mobile No")]
        public string MobileNumber { get; set; }
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Display(Name = "Name")]
        [Required(ErrorMessage = "*")]

        public string NameAsAadhaar { get; set; }


        [Required(ErrorMessage = "*")]
        [Display(Name = "DateOfBrith")]
        public string DOBasAadhaar { get; set; }


        [StringLength(16, ErrorMessage = "Must be 12 characters")]
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Remote("CheckAadharNumber", "Student", AdditionalFields = "HallTicketNumber", HttpMethod = "POST", ErrorMessage = "Aadhaar Number already exists.")]
        [Display(Name = "Aadhaar Number")]
        public string AadhaarNumber { get; set; }
       
        public HttpPostedFileBase aadhardocument { get; set; }
        public int? departmentid { get; set; }


        public string aadhardocumentview { get; set; }

    }
}