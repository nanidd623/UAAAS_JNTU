using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegeIntakeExisting
    {
        public int id { get; set; }
        public int academicYearId { get; set; }
        public int collegeId { get; set; }
        public int nocid { get; set; }
        public string nbaid { get; set; }
        public string enccollegeid { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeID { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department ")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Specialization")]
        public int specializationId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Shift")]
        public int shiftId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "courseStatus")]
        public string courseStatus { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "AICTE Approved Intake for the A.Y. ")]
        public int AICTEapprovedIntake1 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Approved Intake for the A.Y. ")]
        public int approvedIntake1 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake for the A.Y. ")]
        public int admittedIntake1 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake Exam Branch for the A.Y. ")]
        public int ExambranchadmittedIntakeR1 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake Exam Branch for the A.Y. ")]
        public int ExambranchadmittedIntakeL1 { get; set; }

        public string ExambranchadmittedIntake1 { get; set; }
        public string ExambranchadmittedIntake2 { get; set; }
        public string ExambranchadmittedIntake3 { get; set; }
        public string ExambranchadmittedIntake4 { get; set; }
        public string ExambranchadmittedIntake5 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "AICTE Approved Intake for the A.Y. ")]
        public int AICTEapprovedIntake2 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Approved Intake for the A.Y. ")]
        public int approvedIntake2 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake for the A.Y. ")]
        public int admittedIntake2 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake Exam Branch for the A.Y. ")]
        public int ExambranchadmittedIntakeR2 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake Exam Branch for the A.Y. ")]
        public int ExambranchadmittedIntakeL2 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "AICTE Approved Intake for the A.Y. ")]
        public int AICTEapprovedIntake3 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Approved Intake for the A.Y. ")]
        public int approvedIntake3 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake Exam Branch for the A.Y. ")]
        public int ExambranchadmittedIntakeR3 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake Exam Branch for the A.Y. ")]
        public int ExambranchadmittedIntakeL3 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake for the A.Y. ")]
        public int admittedIntake3 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "AICTE Approved Intake for the A.Y. ")]
        public int AICTEapprovedIntake4 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Approved Intake for the A.Y. ")]
        public int approvedIntake4 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake for the A.Y. ")]
        public int admittedIntake4 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake Exam Branch for the A.Y. ")]
        public int ExambranchadmittedIntakeR4 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake Exam Branch for the A.Y. ")]
        public int ExambranchadmittedIntakeL4 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "AICTE Approved Intake for the A.Y. ")]
        public int AICTEapprovedIntake5 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Approved Intake for the A.Y. ")]
        public int approvedIntake5 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake for the A.Y. ")]
        public int admittedIntake5 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake Exam Branch for the A.Y. ")]
        public int ExambranchadmittedIntakeR5 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake Exam Branch for the A.Y. ")]
        public int ExambranchadmittedIntakeL5 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "AICTE Approved Intake for the A.Y. ")]
        public int AICTEapprovedIntake6 { get; set; }

        public bool ispercentage { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Approved Intake for the A.Y. ")]
        public int approvedIntake6 { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "courseAffiliatedStatus")]
        public bool ? courseAffiliatedStatus { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Admitted Intake for the A.Y. ")]
        public int admittedIntake6 { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Proposed Intake as submitted to AICTE/PCI ")]
        [Required(ErrorMessage = "Required")]
        public int? ProposedIntake { get; set; }
        public bool isintakeediable { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Approved Intake for the A.Y. ")]
        public int? ApprovedIntake { get; set; }

        //[Required(ErrorMessage = "Required")]
        //[Display(Name = "Approval letter ")]
        public HttpPostedFileBase ApprovalLetter { get; set; }

        public string letterPath { get; set; }

        [Display(Name = "NBA Accreditation Period")]
        public Nullable<System.DateTime> nbaFrom { get; set; }

        public Nullable<System.DateTime> nbaTo { get; set; }

        public string nbaFromDate { get; set; }

        public string nbaToDate { get; set; }

        [Display(Name = "Shift")]
        public bool isActive { get; set; }
        public bool isClosed { get; set; }

        public bool isAffiliated { get; set; }

        [Display(Name = "Degree")]
        public string Degree { get; set; }

        [Display(Name = "Department")]
        public string Department { get; set; }

        [Display(Name = "Specialization")]
        public string Specialization { get; set; }

        [Display(Name = "Shift")]
        public string Shift { get; set; }
        public string UploadPic { get; set; }
        public int? degreeDisplayOrder { get; set; }

        public string AICTEApprovalLettr { get; set; }
        public HttpPostedFileBase NBAApproveLetter { get; set; }
        public HttpPostedFileBase AppealApprovalLetter { get; set; }
        public HttpPostedFileBase SCMApprovalLetter { get; set; }
        public HttpPostedFileBase Form16ApprovalLetter { get; set; }
        public string UploadNBAApproveLetter{get;set;}

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_academic_year jntuh_academic_year { get; set; }
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_specialization jntuh_specialization { get; set; }
        public virtual jntuh_shift jntuh_shift { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }

        //view SCM and Form16 
        public string ViewSCMApprovalLetter { get; set; }
        public string ViewForm16ApprovalLetter { get; set; }


    }
}