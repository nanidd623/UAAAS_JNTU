using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class AdministrativeLand
    {
        public IEnumerable<AdminLand> adminLand { get; set; }
    }

    public class AdminLand
    {
        public int id { get; set; }
        public string requirementType { get; set; }
        public int? programId { get; set; }
        public int specializationID { get; set; }
        public string specializationName { get; set; }
        public decimal? requiredRooms { get; set; }
        public string requiredRoomsCalculation { get; set; }
        public decimal? requiredArea { get; set; }
        public string requiredAreaCalculation { get; set; }
        public string areaTypeDescription { get; set; }
        public int? areaTypeDisplayOrder { get; set; }
        public decimal? availableRooms { get; set; }
        public decimal? availableArea { get; set; }
        public int? collegeId { get; set; }
        public HttpPostedFileBase supportingdoc { get; set; }
        public string supportingdocpath { get; set; }
        public virtual jntuh_program_type jntuh_program_type { get; set; }
        public decimal? cfRooms { get; set; }
        public decimal? cfArea { get; set; }
    }
}