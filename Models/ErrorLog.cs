using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(ErrorLogAttribs))]
    public partial class jntuh_error_log
    {
        //leave it empty
    }

    public class ErrorLogAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(1000, ErrorMessage = "Must be under 1000 characters")]
        [Display(Name = "Exception")]        
        public string exception { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [Display(Name = "Controller")]
        public string controller { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [Display(Name = "Action")]
        public string action { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(5000, ErrorMessage = "Must be under 5000 characters")]
        [Display(Name = "StackTrace")]
        public string StackTrace { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }        
    }
}