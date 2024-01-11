using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    [MetadataType(typeof(RoleAttribs))]
    public partial class my_aspnet_roles
    {
        //leave it empty
    }

    public class RoleAttribs
    {
        public int id { get; set; }
        public int applicationId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(25, ErrorMessage = "Must be under 25 characters")]
        [Display(Name = "Role Name")]
        public string name { get; set; }

        public virtual ICollection<my_aspnet_usersinroles> my_aspnet_usersinroles { get; set; }

    }
}