using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models.Permanent_Affiliation
{
    public class SelfAppraisalModel
    {
        public int Id { get; set; }
        public string Selfappraisaldescription { get; set; }
        public int Selfappraisaldescriptiontype { get; set; }
        public string Remarks { get; set; }
        public bool Isactive { get; set; }
        public DateTime Createdon { get; set; }
        public int CollegeSelfAppraisalsCount { get; set; }
        public List<CollegeSelfAppraisalModel> CollegeSelfAppraisals { get; set; }
    }

    public class CollegeSelfAppraisalModel
    {
        public int Id { get; set; }
        public int Collegeid { get; set; }

        [Required(ErrorMessage = "Required")]
        public int AcademicyearId { get; set; }

        public int Selfappraisalid { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Selfappraisaltype { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Registrationnumber { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Membername { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Required")]
        public HttpPostedFileBase Suportingdocument { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Grantamount { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Fundingagency { get; set; }
        public string AcademicYear { get; set; }
        public string SuportingdocumentPath { get; set; }
        public string Remarks { get; set; }
        public bool Isactive { get; set; }
        public int Createdby { get; set; }
        public DateTime Createdon { get; set; }
        public int Updatedby { get; set; }
        public DateTime Updatedon { get; set; }
    }

    public class OtherCollegeSelfAppraisalModel
    {
        public int Id { get; set; }
        public int Collegeid { get; set; }

        [Required(ErrorMessage = "Required")]
        public int AcademicyearId { get; set; }

        public int Selfappraisalid { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Selfappraisaltype { get; set; }

        [Display(Name = "Registration Number")]
        [Required(ErrorMessage = "Required")]
        [Remote("CheckRegistrationNumber", "PA_FacultyOppurtunities", HttpMethod = "POST", ErrorMessage = "*")]
        public string FacultyRegistrationNumber { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Membername { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Required")]
        public HttpPostedFileBase Suportingdocument { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Grantamount { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Fundingagency { get; set; }
        public string AcademicYear { get; set; }
        public string SuportingdocumentPath { get; set; }
        public string Remarks { get; set; }
        public bool Isactive { get; set; }
        public int Createdby { get; set; }
        public DateTime Createdon { get; set; }
        public int Updatedby { get; set; }
        public DateTime Updatedon { get; set; }
    }

    public class FacultyOppurtunityModel
    {
        public int Id { get; set; }
        public int Collegeid { get; set; }

        [Required(ErrorMessage = "Required")]
        public int AcademicyearId { get; set; }

        public int Selfappraisalid { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Selfappraisaltype { get; set; }

        [Display(Name = "Registration Number")]
        [Remote("CheckRegistrationNumber", "PA_FacultyOppurtunities", HttpMethod = "POST", ErrorMessage = "*")]
        [Required(ErrorMessage = "Required")]
        public string FacultyRegistrationNumber { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Membername { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Required")]
        public HttpPostedFileBase Suportingdocument { get; set; }

        public string AcademicYear { get; set; }
        public string SuportingdocumentPath { get; set; }
        public string Remarks { get; set; }
        public bool Isactive { get; set; }
        public int Createdby { get; set; }
        public DateTime Createdon { get; set; }
        public int Updatedby { get; set; }
        public DateTime Updatedon { get; set; }
    }
}