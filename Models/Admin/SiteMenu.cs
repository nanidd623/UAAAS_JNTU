using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.WebPages.Html;

namespace UAAAS.Models
{
    public class SiteMenu
    {
        public int Id { get; set; }
        public string MenuName { get; set; }
        public string MenuActionName { get; set; }
        public string MenuControllerName { get; set; }
        public Nullable<int> MenuParentID { get; set; }
        public string Roles { get; set; }
        public string MenuParentName { get; set; }
        public Nullable<int> MenuOrder { get; set; }
        public bool IsActive { get; set; }
        public string CheckedMenu { get; set; }
        public List<SitePermissions> MenuItems { get; set; }
        public string[] SelectedRoles { get; set; }
        public int? subMenuID1 { get; set; }
        public int? subMenuID2 { get; set; }
        public int? subMenuID3 { get; set; }
        public string Type { get; set; }
        public string MenuType { get; set; }
        public IEnumerable<SelectListItem> SelectRoles { get; set; }
    }
    public class SitePermissions
    {
        public int Id { get; set; }
        public string MenuName { get; set; }
        public string MenuActionName { get; set; }
        public string MenuControllerName { get; set; }
        public Nullable<int> MenuParentID { get; set; }
        public string Roles { get; set; }
        public string menuParentName { get; set; }
        public Nullable<int> menuOrder { get; set; }
        public bool IsActive { get; set; }
        public int? subMenuID1 { get; set; }
        public int? subMenuID2 { get; set; }
        public int? subMenuID3 { get; set; }
        public string SubMenuLevel1 { get; set; }
        public string SubMenuLevel2 { get; set; }
        public string SubMenuLevel3 { get; set; }
    }
    //Link Classes
    public class linkscreenpermissions
    {
        public int id { get; set; }
        public string LinkName { get; set; }
        public string Linkcode { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string fromdate { get; set; }
        public string Todate { get; set; }
        public bool isactive { get; set; }
    }
}
