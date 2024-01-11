using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class FFCAuditorsCampus
    {
        public int id { get; set; }
        [Required(ErrorMessage="Required")]
        [Display(Name="Campus Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }

        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
    }
}