using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class AuditSchedule
    {
        //[Required(ErrorMessage = "Required")]
        [Display(Name = "District")]
        public int districtid { get; set; }

       // [Required(ErrorMessage = "Required")]
        [Display(Name = "Pincode")]
        public int pincode { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "College")]
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "College")]
        public string collegeCode { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Audit Date")]
        public string auditDate { get; set; }

        [Display(Name = "Alternate Audit Date")]
        public string alternateAuditDate { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Order Date")]
        public string orderDate { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Order Type")]
        public bool isRevised { get; set; }

        [Display(Name = "Departments")]
        public List<CollegeDepartments> departments { get; set; }

        [Display(Name = "Available Auditors")]
        public List<AvailableAuditors> availableAuditors { get; set; }

        [Display(Name = "Assigned Auditors")]
        public List<AssignedAuditors> assignedAuditors { get; set; }

        public Nullable<int> InspectionPhaseId { get; set; }

    }

    public class CollegeDepartments
    {
        public int degreeId { get; set; }
        public int deptartmentId { get; set; }
        public string degreeName { get; set; }
        public string deptartmentName { get; set; }
    }

    public class AvailableAuditors
    {
        public int departmentId { get; set; }
        public string deptartmentName { get; set; }
        public int auditorId { get; set; }
        public string auditorName { get; set; }
        public string auditorDesignation { get; set; }
        public string preferredDesignation { get; set; }
        public string auditorLoad { get; set; }
        public bool auditorSelected { get; set; }
    }

    public class AssignedAuditors
    {
        public int departmentId { get; set; }
        public string deptartmentName { get; set; }
        public int auditorId { get; set; }
        public string auditorName { get; set; }
        public string auditorDesignation { get; set; }
        public string preferredDesignation { get; set; }
        public string auditorLoad { get; set; }
        public bool isDeleted { get; set; }
        public bool isConvenor { get; set; }
        public int memberOrder { get; set; }
    }

    public class AuditScheduleReport
    {
        public int scheduleId { get; set; }
        public int collegeId { get; set; }
        public string collegeName { get; set; }
        public string collegeCode { get; set; }
        public string auditDate { get; set; }
        public string alternateAuditDate { get; set; }
        public string orderDate { get; set; }
        public int isRevised { get; set; }
        public List<AssignedAuditors> assignedAuditors { get; set; }
        public int totalOrdersSent { get; set; }
        public bool isLastOrderSent { get; set; }
        public int OldInspectionPhaseId { get; set; }

    }

    public class Committee
    {
        public int auditorId { get; set; }
        public int isConvenor { get; set; }
    }

    public class ProposedSpecialization
    {
        public string degree { get; set; }
        public string department { get; set; }
        public string specialization { get; set; }
        public string shift { get; set; }
        public int existing { get; set; }
        public int proposed { get; set; }
    }
    public class ExistingIntakeSpecializations
    {
        public string degree { get; set; }
        public string department { get; set; }
        public string specialization { get; set; }
        public string shift { get; set; }
        public int existing { get; set; }
        public int proposed { get; set; }
    }

    public class Districts
    {
        public string name { get; set; }
        public int id { get; set; }
    }

    public class OldInspectionPhaseIds
    {
        public string name { get; set; }
        public int id { get; set; }
    }
}