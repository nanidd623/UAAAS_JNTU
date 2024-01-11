using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class FailureLabsList
    {
        public int Id { get; set; }
        public Nullable<int> CollegeId { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public string Specialization { get; set; }
        public int Year { get; set; }
        public int? Semester { get; set; }
        public string Labcode { get; set; }
        public string LabName { get; set; }
        public string EquipmentName { get; set; }
        public string Remarks { get; set; }
        public string noofUnits { get; set; }
        public string ExperimentsIds { get; set; }
        public DateTime? createdOn { get; set; }
    }
}