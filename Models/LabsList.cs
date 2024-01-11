using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class LabsList
    {
        public int Id { get; set; }
        public int? CollegeTypeId { get; set; }
        public int? CollegeId { get; set; }
        [Required]
        public string Degree { get; set; }
        [Required]
        public string Department { get; set; }

        public int SpecializationId { get; set; }
        public string Specialization { get; set; }

        [Required]
        public int Year { get; set; }
        [Required]
        public int? Semester { get; set; }
        public string Labcode { get; set; }
        public string LabName { get; set; }

        public string EquipmentName { get; set; }
        public string Remarks { get; set; }
        public string noofUnits { get; set; }
        public string ExperimentsIds { get; set; }

       
        public string FileName { get; set; }
        public bool IsChecked { get; set; }
       
        public List<LabsList> lablists { get; set; }

    }
}