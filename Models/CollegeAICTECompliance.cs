using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class CollegeAICTECompliance
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Academic Year")]
        public int academicYearId { get; set; }


        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeID { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Specialization")]
        public int specializationId { get; set; }

        [Display(Name = "AICTE College ID")]
        public string aictecollegeId { get; set; }

        [Display(Name = "AICTE Intake")]
        [Required(ErrorMessage = "Required")]
        public string aicteIntake { get; set; }

        public bool DeclarationStatus { get; set; }
    }

    public class CollegeAicteAdjustments
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public string academicYear { get; set; }
        public string degree { get; set; }
        public string Department { get; set; }
        public Int32 specializationId { get; set; }
        public string specialization { get; set; }
        public string aictecollegeId { get; set; }
        public int aicteIntake { get; set; }
        public string[] oldcourseStatus { get; set; }
        public string[] newcourseStatus { get; set; }
        public List<SelectListItem> newspecs { get; set; }
        public Int32 remaningIntake { get; set; }
        //[Required(ErrorMessage = "Adjustment Status Required")]
        public Int32 adjustmentId { get; set; }
        public short adjustmentstatus { get; set; }
        public short adjustedcoursestatus { get; set; }
    }

    public class AdjustmentData
    {
        public Int32 CollegeaicteId { get; set; }
        public string AcademidYear { get; set; }
        public Int32 CollegeId { get; set; }
        public Int32 ActualaicteIntake { get; set; }
        public Int32 OldSpecialization { get; set; }
        public string OldCourseStatus { get; set; }
        public Int32 OldAicteIntake { get; set; }
        public string Specialization { get; set; }
        public string NewCourseStatus { get; set; }
        public Int32 NewAdjustmentAicteIntake { get; set; }
        public Int32 RemainingIntake { get; set; }
        public Int32 AdjustmentId { get; set; }
    }

    public class Aictepci
    {
        public Int32 CollegeId { get; set; }
        public int AictepciStatus { get; set; }
    }
}