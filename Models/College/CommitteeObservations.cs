using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CommitteeObservations
    {
        public int id { get; set; }

        public int collegeId { get; set; }

        public int observationId { get; set; }

        public string type { get; set; }

        public int committeeObservationId1 { get; set; }

        public int committeeObservationId2 { get; set; }

        public int committeeObservationId3 { get; set; }

        public int committeeObservationId4 { get; set; }

        public int committeeObservationId5 { get; set; }

        public int committeeObservationId6 { get; set; }

        public int committeeObservationId7 { get; set; }

        public int committeeObservationId8 { get; set; }

        public int committeeObservationId9 { get; set; }

        public int committeeObservationId10 { get; set; }

        public int committeeObservationId11 { get; set; }

        public int committeeObservationId12 { get; set; }

        public int committeeObservationId13 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations1 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations2 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations3 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations4 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations5 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations6 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations7 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations8 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations9 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations10 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations11 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations12 { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(4000, ErrorMessage = "Maximum 4000 characters")]
        public string committeeObservations13 { get; set; }
    }
}