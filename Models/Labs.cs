using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class Lab
    {
        public int? id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Name of the Laboratory")]
        public string LabName { get; set; }
        
        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Name of the Equipment")]
        public int EquipmentID { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Name of the Equipment")]
        public string EquipmentName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Make")]
        public string Make { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Model")]
        public string Model { get; set; }


        [Display(Name="S.No of the experiments in the syllabus which use the equipment")]
        public string EquipmentIds { get; set; }
        public string NoofUnits { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Unique ID of the Equipment")]
        public string EquipmentUniqueID { get; set; }


        public int ExperimentID { get; set; }
        public int ? ExperimentAutoincrementID { get; set; }
        public int ExperimentStatus { get; set; }
        public string ExperimentName { get; set; }


        [Required(ErrorMessage = "Required")]
        [Display(Name = "Total Units Available")]
        public int? AvailableUnits { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Available Floor Area (in Sqm)")]
        public decimal? AvailableArea { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Specialization")]
        public int specializationId { get; set; }

        public string specializationName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Shift")]
        public int shiftId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Year")]
        public int yearInDegreeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department")]
        public int departmentId { get; set; }

        public string department { get; set; }

        public bool IsActive { get; set; }
        public bool isAffiliated { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeId { get; set; }
        public string degree { get; set; }
        public string shiftName { get; set; }
        //public string year { get; set; }
        public int year { get; set; }
        public int? degreeDisplayOrder { get; set; }
        public string AffiliationStatus { get; set; }
        [Display(Name = "Semester")]
        public int? Semester { get; set; }
        [Display(Name = "Labcode")]
        public string Labcode { get; set; }
        public int? EquipmentNo { get; set; }
        public string LabEquipmentName { get; set; }
        public string CircuitType { get; set; }
        public int? DisplayOrder { get; set; }
        public int? tempEquipmentId { get; set; }
        public int? degreeTypeId { get; set; }
        public bool? isCircuit { get; set; }
        
        public string Remarks { get; set; }

        public string RandomCode { get; set; }

        public int RandomId { get; set; }
        public bool? deficiency { get; set; }
        public bool? deficiencyStatus { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        //   [Required(ErrorMessage = "Required")]
        [Display(Name = "Date Of Purchase")]
        public DateTime? EquipmentDateOfPurchasing { get; set; }
        // [Required(ErrorMessage = "Required")]
        public DateTime? DelivaryChalanaDate { get; set; }

        //   [Required(ErrorMessage = "Required")]
        [Display(Name = "Date Of Purchase")]
        public string EquipmentDateOfPurchasing1 { get; set; }
        // [Required(ErrorMessage = "Required")]
        public string DelivaryChalanaDate1 { get; set; }
         public HttpPostedFileBase EquipmentPhoto { get; set; }
         public string uploadFile { get; set; }
         public string ViewEquipmentPhoto { get; set; }
         public int ExpCount { get; set; }
         public int MasterExpCount { get; set; }
         public bool NoPIDoc { get; set; }
         public bool NoDCDoc { get; set; }
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_specialization jntuh_specialization { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_shift jntuh_shift { get; set; }
        public virtual jntuh_year_in_degree jntuh_year_in_degree { get; set; }
        public virtual jntuh_department jntuh_department { get; set; }
        public virtual jntuh_degree jntuh_degree { get; set; }
        public virtual jntuh_lab_master jntuh_lab_master { get; set; }

        public string ViewDelivaryChalanaImage { get; set; }
        public string ViewPurchaseOrderImage { get; set; }
        public string ViewBankStatementImage { get; set; }
        public string ViewStockRegisterEntryImage { get; set; }
        public string ViewReVerificationScreenImage { get; set; }
        public HttpPostedFileBase DelivaryChalanaImage { get; set; }
        public HttpPostedFileBase PurchaseOrderImage { get; set; }
        public HttpPostedFileBase BankStatementImage { get; set; }
        public HttpPostedFileBase StockRegisterEntryImage { get; set; }
        public HttpPostedFileBase ReVerificationScreenImage { get; set; }
        public string ViewCreatedOn { get; set; }
    }

    public class Labscount
    {
        public string DegreeName { get; set; }
        public string DepartmentName { get; set; }
        public string SpecializationName { get; set; }
        public int TotalLabCount { get; set; }
        public int CollegeLabCount { get; set; }
    }


}