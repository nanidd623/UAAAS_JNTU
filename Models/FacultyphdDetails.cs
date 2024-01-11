using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class FacultyphdDetails
    {

        [Required(ErrorMessage = "*")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Must be 1-50 characters long.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        //LastName-MiddleName
        [StringLength(50, ErrorMessage = "Maximum 50 characters")]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        //Surname-LastName
        [Required(ErrorMessage = "*")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Must be 1-50 characters long.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Faculty Registration ID")]
        public string RegistrationNumber { get; set; }

        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public int Id { get; set; }
        public int FacultyId { get; set; }
        public int PhdType { get; set; }
        public string PhdTypeid { get; set; }
        public int PhdawardYear { get; set; }
        public string University { get; set; }
        public string PlaceofUniversity { get; set; }
        public int UniversityType { get; set; }
        public string PhdTitle { get; set; }
        public string AdmissionLetterpath { get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase AdmissionLetter { get; set; }
        public string PrephdLetterpath { get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase PrephdLetter { get; set; }
        public bool HaveyouprephdLetter { get; set; }
        [Required(ErrorMessage = "*")]
        public string SupervisorName1 { get; set; }
        public string SupervisorName2 { get; set; }
        public string SupervisorName3 { get; set; }
        public string ThesiscoverPagepath { get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase ThesiscoverPage { get; set; }
        public string Phdodpath { get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase Phdod { get; set; }
        public string GenuinenessLetterpath { get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase GenuinenessLetter { get; set; }
        public string CollegeAuthenticationLetterpath{ get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase CollegeAuthenticationLetter { get; set; }
        public string OtherLetterpath { get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase OtherLetter { get; set; }

        public string Remarks { get; set; }
        public bool PhdStatus { get; set; }
        public List<string> Universitys { get; set; }
        public List<string> Places { get; set; }
        public List<string> PhdDepts { get; set; }
        public List<string> PhdSpecs { get; set; }
        public int PhdAdmissionYear { get; set; }

        [Required(ErrorMessage = "*")]
        [RegularExpression(@"^([0-9][0-9]{0,2}|1000)$", ErrorMessage = "Invalid InterNational Journals")]
        [Display(Name = "InterNational Journals")]
        public int? InterNationalJrnls { get; set; }

        [Required(ErrorMessage = "*")]
        [RegularExpression(@"^([0-9][0-9]{0,2}|1000)$", ErrorMessage = "Invalid National Journals")]
        [Display(Name = "National Journals")]
        public int? NationalJrnls { get; set; }

        [Required(ErrorMessage = "*")]
        [RegularExpression(@"^([0-9][0-9]{0,2}|1000)$", ErrorMessage = "Invalid InterNational Conferences")]
        [Display(Name = "InterNational Conferences")]
        public int? InterNationalCnfrs { get; set; }

        [Required(ErrorMessage = "*")]
        [RegularExpression(@"^([0-9][0-9]{0,2}|1000)$", ErrorMessage = "Invalid National Conferences")]
        [Display(Name = "National Conferences")]
        public int? NationalCnfrs { get; set; }

        public string InterNationalJrnlspath { get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase InterNationalJrnlsLetter { get; set; }

        public string NationalJrnlspath { get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase NationalJrnlsLetter { get; set; }

        public string InterNationalCnfrspath { get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase InterNationalCnfrsLetter { get; set; }

        public string NationalCnfrspath { get; set; }
        //[Required(ErrorMessage = "*")]
        public HttpPostedFileBase NationalCnfrsLetter { get; set; }
        //Added by Siva
        [Required(ErrorMessage = "*")]
        public string UniversityWebsite { get; set; }
        [Required(ErrorMessage = "*")]
        public string UniversityAddress { get; set; }
        public HttpPostedFileBase NoCformthecollege { get; set; }
        public string NoCformthecollegepath { get; set; }
        [Required(ErrorMessage = "*")]
        public int PhdnotificationIssued { get; set; }
        [Required(ErrorMessage = "*")]
        public string RegistrationNumberOrHallticketNo { get; set; }
        [Required(ErrorMessage = "*")]
        public int? HowmanyreviewsRRMattended { get; set; }
        [Required(ErrorMessage = "*")]
        public int? HowmanypapersPublished { get; set; }
        [Required(ErrorMessage = "*")]
        public int? HowmanypapersPublishedduringPhdWork { get; set; }
        //[Required(ErrorMessage = "*")]
        public string Externalexamineratthetimeofdefense { get; set; }
        //[Required(ErrorMessage = "*")]
        public DateTime? Exactdateoffinalviva { get; set; }
        public string Exactdateviva { get; set; }
        //[Required(ErrorMessage = "*")]
        public string BOSChairpersonatthetimeofThesisSubmission { get; set; }
        //[Required(ErrorMessage = "*")]
        public DateTime? PressnotificationofyourPhDDegree { get; set; }
        public string PressnotificationPhDDegree { get; set; }
        [Required(ErrorMessage = "*")]
        public bool HaveyouattendConvocation { get; set; }
        [Required(ErrorMessage = "*")]
        public string PhdofferingDepartment { get; set; }
        public string PhdotherDepartment { get; set; }
        [Required(ErrorMessage = "*")]
        public string PhdSpecialization { get; set; }
        public string PhdotherSpecialization { get; set; }
        public string HaveyouattendConvocationTxt { get; set; }
        public string PhdnotificationIssuedTxt { get; set; }
        public string UniversityTypeTxt { get; set; }
        public string DateofVivaTxt { get; set; }
        public string PressnotDateTxt { get; set; }
        public bool isSubmitted { get; set; }
    }
    public class AdminFacultyphdDetails
    {
        public string EncryptId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string RegistrationNumber { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public int Id { get; set; }
        public int FacultyId { get; set; }
        public int PhdType { get; set; }
        public string PhdTypeid { get; set; }
        public int PhdawardYear { get; set; }
        public string University { get; set; }
        public string PlaceofUniversity { get; set; }
        public int UniversityType { get; set; }
        public string PhdTitle { get; set; }
        public string AdmissionLetterpath { get; set; }
        public HttpPostedFileBase AdmissionLetter { get; set; }
        public string PrephdLetterpath { get; set; }
        public HttpPostedFileBase PrephdLetter { get; set; }
        public bool HaveyouprephdLetter { get; set; }
        [Required(ErrorMessage = "*")]
        public string SupervisorName1 { get; set; }
        public string SupervisorName2 { get; set; }
        public string SupervisorName3 { get; set; }
        public string ThesiscoverPagepath { get; set; }
        public HttpPostedFileBase ThesiscoverPage { get; set; }
        public string Phdodpath { get; set; }
        public HttpPostedFileBase Phdod { get; set; }
        public string GenuinenessLetterpath { get; set; }
        public HttpPostedFileBase GenuinenessLetter { get; set; }
        public string CollegeAuthenticationLetterpath { get; set; }
        public HttpPostedFileBase CollegeAuthenticationLetter { get; set; }
        public string OtherLetterpath { get; set; }
        public HttpPostedFileBase OtherLetter { get; set; }
        public string Remarks { get; set; }
        public bool PhdStatus { get; set; }
        public List<string> Universitys { get; set; }
        public List<string> Places { get; set; }
        public List<string> PhdDepts { get; set; }
        public List<string> PhdSpecs { get; set; }
        public int PhdAdmissionYear { get; set; }
        public int? InterNationalJrnls { get; set; }
        public int? NationalJrnls { get; set; }
        public int? InterNationalCnfrs { get; set; }
        public int? NationalCnfrs { get; set; }
        public string InterNationalJrnlspath { get; set; }
        public HttpPostedFileBase InterNationalJrnlsLetter { get; set; }
        public string NationalJrnlspath { get; set; }
        public HttpPostedFileBase NationalJrnlsLetter { get; set; }
        public string InterNationalCnfrspath { get; set; }
        public HttpPostedFileBase InterNationalCnfrsLetter { get; set; }
        public string NationalCnfrspath { get; set; }
        public HttpPostedFileBase NationalCnfrsLetter { get; set; }
        //Added by Siva
        [Required(ErrorMessage = "*")]
        public string UniversityWebsite { get; set; }
        [Required(ErrorMessage = "*")]
        public string UniversityAddress { get; set; }
        public HttpPostedFileBase NoCformthecollege { get; set; }
        public string NoCformthecollegepath { get; set; }
        [Required(ErrorMessage = "*")]
        public int PhdnotificationIssued { get; set; }
        [Required(ErrorMessage = "*")]
        public string RegistrationNumberOrHallticketNo { get; set; }
        [Required(ErrorMessage = "*")]
        public int? HowmanyreviewsRRMattended { get; set; }
        [Required(ErrorMessage = "*")]
        public int? HowmanypapersPublished { get; set; }
        [Required(ErrorMessage = "*")]
        public int? HowmanypapersPublishedduringPhdWork { get; set; }
        public string Externalexamineratthetimeofdefense { get; set; }
        public DateTime? Exactdateoffinalviva { get; set; }
        public string BOSChairpersonatthetimeofThesisSubmission { get; set; }
        public DateTime? PressnotificationofyourPhDDegree { get; set; }
        public bool HaveyouattendConvocation { get; set; }
        public string PhdofferingDepartment { get; set; }
        public string PhdotherDepartment { get; set; }
        public string PhdSpecialization { get; set; }
        public string PhdotherSpecialization { get; set; }
        public string HaveyouattendConvocationTxt { get; set; }
        public string PhdnotificationIssuedTxt { get; set; }
        public string UniversityTypeTxt { get; set; }
        public string DateofVivaTxt { get; set; }
        public string PressnotDateTxt { get; set; }
    }
}