using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    [MetadataType(typeof(MenuAttribs))]
    public partial class jntuh_menu
    {
        //leave it empty
    }

    public class MenuAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Menu Name")]
        public string menuName { get; set; }

        //[Required(ErrorMessage = "Required")]
        //[StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Page Type")]
        public string menuActionName { get; set; }

        //[Required(ErrorMessage = "Required")]
        //[StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Navigate Url")]
        public string menuControllerName { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Parent Menu")]
        public Nullable<int> menuParentID { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Sub Menu")]
        public int subMenuID1 { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Sub Menu")]
        public int subMenuID2 { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Sub Menu")]
        public int subMenuID3 { get; set; }

        //[Required(ErrorMessage = "Required")]
        //[StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Menu Roles")]
        public List<string> Roles { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Menu Roles")]
        public string[] SelectedRoles { get; set; }

        public string menuParentName { get; set; }

        [Required(ErrorMessage = "Required")]
        //[StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Order")]
        public Nullable<int> menuOrder { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }

    public partial class jntuh_menu
    {
        public int subMenuID1 { get; set; }
        public int subMenuID2 { get; set; }
        public int subMenuID3 { get; set; }
       
        public string[] SelectedRoles { get; set; }
        public string menuParentName { get; set; }
    }
}