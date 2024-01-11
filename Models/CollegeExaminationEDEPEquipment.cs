using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegeExaminationEDEPEquipment
    {
    public IEnumerable<CollegeEDEPDetails> CollegeEDEP { get; set; }
    }
    
    public class CollegeEDEPDetails
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Equipment")]
        public int EDEPEquipmentId { get; set; }

        [Display(Name = "Equipment")]
        [StringLength(150,ErrorMessage="Must be 150 charecters")]
        public string EDEPEquipment { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Actual Equipment")]
        public string ActualValue { get; set; }

        public virtual jntuh_edep_equipment jntuh_edep_equipment { get; set; }
    }
}