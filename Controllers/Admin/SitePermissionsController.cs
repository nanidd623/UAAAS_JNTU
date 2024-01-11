using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class SitePermissionsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //GET: College Link Screens
        [Authorize(Roles = "Admin")]
        public ActionResult CollegeLinkScreens()
        {
            List<linkscreenpermissions> linkscreenpermissions= new List<linkscreenpermissions>();
            List<jntuh_college_links_assigned> linkscreenassigned = db.jntuh_college_links_assigned.Select(s => s).ToList();
            foreach (var item in linkscreenassigned)
            {
                linkscreenpermissions newitem= new linkscreenpermissions();
                newitem.LinkName =
                    db.jntuh_link_screens.Where(l => l.id == item.id).Select(s => s.linkName).FirstOrDefault();
                newitem.ControllerName = newitem.LinkName;
                //newitem.ActionName = newitem.LinkName;
                newitem.fromdate = item.fromdate.ToString().Split(' ')[0];
                newitem.Todate = newitem.Todate.ToString().Split(' ')[0];
                newitem.isactive =true;
                linkscreenpermissions.Add(newitem);
            }
            return View(linkscreenpermissions);
        }

        // GET: /SitePermissions/Menu
        [Authorize(Roles = "Admin")]
        public ActionResult Menu()
        {
            SiteMenu SiteMenu = new SiteMenu();

            //get parent menu details
            SiteMenu = GetSiteMenuDetails("Parent");

            //show only parent menu details and hide submenu fields
            ViewBag.ParentMenu = true;

            return View("~/Views/Admin/SitePermissions.cshtml", SiteMenu);
        }

        // POST: /SitePermissions/Menu
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Menu(SiteMenu SiteMenu)
        {
            //save parent menu details
            SaveMenuDetails(SiteMenu);
            TempData["Success"] = "Parent Menu details are added Successfully";
            return RedirectToAction("Menu", "SitePermissions");
        }

        // GET: /SitePermissions/EditParentMenu
        [Authorize(Roles = "Admin")]
        public ActionResult EditParentMenu(int id)
        {
            SiteMenu SiteMenu = new SiteMenu();

            //get all parent menu details and bind to table
            SiteMenu = GetSiteMenuDetails("Parent");

            //get parent menu details based on id and bind to view
            jntuh_menu MenuDetails = db.jntuh_menu.Where(m => m.id == id).FirstOrDefault();
            if (MenuDetails != null)
            {
                SiteMenu.Id = MenuDetails.id;
                SiteMenu.MenuName = MenuDetails.menuName;
                SiteMenu.MenuControllerName = MenuDetails.menuControllerName;
                SiteMenu.MenuActionName = MenuDetails.menuActionName;
                SiteMenu.IsActive = MenuDetails.isActive;
                SiteMenu.MenuOrder = db.jntuh_menu.Where(m => m.menuParentID == 0 &&
                                                         m.menuOrder > MenuDetails.menuOrder &&
                                                         m.Roles == MenuDetails.Roles)
                                        .OrderBy(m => m.menuOrder)
                                        .Select(m => m.menuOrder)
                                        .FirstOrDefault();
                SiteMenu.MenuOrder = SiteMenu.MenuOrder == null ? -1 : SiteMenu.MenuOrder;
                SiteMenu.Roles = MenuDetails.Roles;
            }
            ViewBag.ParentMenu = true;
            return View("~/Views/Admin/SitePermissions.cshtml", SiteMenu);
        }

        // POST: /SitePermissions/EditParentMenu
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditParentMenu(SiteMenu SiteMenu)
        {
            //save edited parent menu details 
            SaveMenuDetails(SiteMenu);
            TempData["Success"] = "Parent Menu details are updated Successfully";
            return RedirectToAction("Menu", "SitePermissions");
        }

        //when we change dropdownlist of roles, parent menu, level1 menu, level2 menu, level3 menu this action will be execute
        public ActionResult SubMenuDetails(SiteMenu SiteMenu)
        {
            //bind sitemenu model values from view page to tempdata
            TempData["Model"] = SiteMenu;
            TempData["Data"] = "true";
            return RedirectToAction("SubMenu", "SitePermissions");
        }

        // GET: /SitePermissions/SubMenu
        [Authorize(Roles = "Admin")]
        public ActionResult SubMenu()
        {
            SiteMenu SiteMenu = new SiteMenu();

            //check null values
            if (TempData["Data"] != null)
            {
                //if temp["data"] is true then bind submenu details based on model values
                if (TempData["Data"] == "true")
                {
                    //converting tempdata to model object
                    SiteMenu = TempData["Model"] as SiteMenu;

                    //get submenu details
                    SiteMenu = GetSubMenuDetails(SiteMenu);
                }
                else
                {
                    //get parent menu details during page load
                    SiteMenu = GetSiteMenuDetails("SubMenu");
                }
            }
            else
            {
                //get parent menu details during page load
                SiteMenu = GetSiteMenuDetails("SubMenu");
            }

            //set submenu true to show all fields of submenu 
            ViewBag.SubMenu = true;

            //by using this we can show or hides the parent menu dropdown
            ViewBag.Edit = false;

            return View("~/Views/Admin/SitePermissions.cshtml", SiteMenu);
        }

        // POST: /SitePermissions/SubMenu
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SubMenu(SiteMenu SiteMenu)
        {
            //save submenu details
            SaveSubMenuDetails(SiteMenu);

            //by using this we can show or hides the parent menu dropdown
            ViewBag.Edit = false;

            TempData["Success"] = "Sub Menu details are added Successfully";

            return RedirectToAction("SubMenu", "SitePermissions");
        }

        // GET: /SitePermissions/EditSubMenu
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditSubMenu(int id, string Type)
        {
            //show or hide submenu fields
            ViewBag.SubMenu = true;
            SiteMenu SiteMenu = new SiteMenu();

            //if type is parent then bind data to form up parent menu details
            if (Type == "Parent")
            {
                //get submenu details based on id
                jntuh_menu MenuDetails = db.jntuh_menu.Where(m => m.id == id).FirstOrDefault();
                if (MenuDetails != null)
                {
                    SiteMenu.Id = MenuDetails.id;
                    SiteMenu.MenuName = MenuDetails.menuName;
                    SiteMenu.MenuControllerName = MenuDetails.menuControllerName;
                    SiteMenu.MenuActionName = MenuDetails.menuActionName;
                    SiteMenu.IsActive = MenuDetails.isActive;
                    SiteMenu.MenuOrder = db.jntuh_menu.Where(m => m.menuParentID == MenuDetails.menuParentID &&
                                                             m.menuOrder > MenuDetails.menuOrder &&
                                                             m.Roles == MenuDetails.Roles)
                                            .OrderBy(m => m.menuOrder)
                                            .Select(m => m.menuOrder)
                                            .FirstOrDefault();
                    SiteMenu.MenuOrder = SiteMenu.MenuOrder == null ? -1 : SiteMenu.MenuOrder;
                    SiteMenu.Roles = MenuDetails.Roles;
                    SiteMenu.MenuParentID = MenuDetails.menuParentID;
                }

                ViewBag.Edit = false;

                SiteMenu.Type = "Parent";

                //get data of all parent menu details and bind to table
                GetSubMenuDetails(SiteMenu);
            }

            //if type is level1 then bind data to form up level1 menu details
            else if (Type == "Level1")
            {
                //bind data to form based on id 
                jntuh_menu MenuDetails = db.jntuh_menu.Where(m => m.id == id).FirstOrDefault();
                if (MenuDetails != null)
                {
                    SiteMenu.Id = MenuDetails.id;
                    SiteMenu.MenuName = MenuDetails.menuName;
                    SiteMenu.MenuControllerName = MenuDetails.menuControllerName;
                    SiteMenu.MenuActionName = MenuDetails.menuActionName;
                    SiteMenu.IsActive = MenuDetails.isActive;
                    SiteMenu.subMenuID1 = MenuDetails.menuParentID;
                    SiteMenu.MenuParentID = db.jntuh_menu.Where(m => m.id == MenuDetails.menuParentID)
                                                       .Select(m => m.menuParentID)
                                                       .FirstOrDefault();
                    SiteMenu.MenuOrder = db.jntuh_menu.Where(m => m.menuParentID == MenuDetails.menuParentID &&
                                                             m.menuOrder > MenuDetails.menuOrder &&
                                                             m.Roles == MenuDetails.Roles)
                                            .OrderBy(m => m.menuOrder)
                                            .Select(m => m.menuOrder)
                                            .FirstOrDefault();
                    SiteMenu.MenuOrder = SiteMenu.MenuOrder == null ? -1 : SiteMenu.MenuOrder;
                    SiteMenu.Roles = MenuDetails.Roles;
                }

                ViewBag.Edit = false;

                SiteMenu.Type = "Level1";

                //get data of all submenu details upto level1
                GetSubMenuDetails(SiteMenu);
            }

            //if type is level2 then bind data to form up level2 menu details
            else if (Type == "Level2")
            {
                jntuh_menu MenuDetails = db.jntuh_menu.Where(m => m.id == id).FirstOrDefault();
                if (MenuDetails != null)
                {
                    SiteMenu.Id = MenuDetails.id;
                    SiteMenu.MenuName = MenuDetails.menuName;
                    SiteMenu.MenuControllerName = MenuDetails.menuControllerName;
                    SiteMenu.MenuActionName = MenuDetails.menuActionName;
                    SiteMenu.IsActive = MenuDetails.isActive;
                    SiteMenu.subMenuID2 = MenuDetails.menuParentID;
                    SiteMenu.subMenuID1 = db.jntuh_menu.Where(m => m.id == MenuDetails.menuParentID)
                                                       .Select(m => m.menuParentID)
                                                       .FirstOrDefault();
                    SiteMenu.MenuParentID = db.jntuh_menu.Where(m => m.id == SiteMenu.subMenuID1)
                                                       .Select(m => m.menuParentID)
                                                       .FirstOrDefault();
                    SiteMenu.MenuOrder = db.jntuh_menu.Where(m => m.menuParentID == MenuDetails.menuParentID &&
                                                             m.menuOrder > MenuDetails.menuOrder &&
                                                             m.Roles == MenuDetails.Roles)
                                            .OrderBy(m => m.menuOrder)
                                            .Select(m => m.menuOrder)
                                            .FirstOrDefault();
                    SiteMenu.MenuOrder = SiteMenu.MenuOrder == null ? -1 : SiteMenu.MenuOrder;
                    SiteMenu.Roles = MenuDetails.Roles;
                }
                ViewBag.Edit = false;
                SiteMenu.Type = "Level2";
                //get data of all submenu details upto level2
                GetSubMenuDetails(SiteMenu);
            }
            //if type is level3 then bind data to form up level3 menu details
            else if (Type == "Level3")
            {
                jntuh_menu MenuDetails = db.jntuh_menu.Where(m => m.id == id).FirstOrDefault();
                if (MenuDetails != null)
                {
                    SiteMenu.Id = MenuDetails.id;
                    SiteMenu.MenuName = MenuDetails.menuName;
                    SiteMenu.MenuControllerName = MenuDetails.menuControllerName;
                    SiteMenu.MenuActionName = MenuDetails.menuActionName;
                    SiteMenu.IsActive = MenuDetails.isActive;
                    SiteMenu.subMenuID3 = MenuDetails.menuParentID;
                    SiteMenu.subMenuID2 = db.jntuh_menu.Where(m => m.id == MenuDetails.menuParentID)
                                                       .Select(m => m.menuParentID)
                                                       .FirstOrDefault();
                    SiteMenu.subMenuID1 = db.jntuh_menu.Where(m => m.id == SiteMenu.subMenuID2)
                                                       .Select(m => m.menuParentID)
                                                       .FirstOrDefault();
                    SiteMenu.MenuParentID = db.jntuh_menu.Where(m => m.id == SiteMenu.subMenuID1)
                                                       .Select(m => m.menuParentID)
                                                       .FirstOrDefault();
                    SiteMenu.MenuOrder = db.jntuh_menu.Where(m => m.menuParentID == MenuDetails.menuParentID &&
                                                             m.menuOrder > MenuDetails.menuOrder &&
                                                             m.Roles == MenuDetails.Roles)
                                            .OrderBy(m => m.menuOrder)
                                            .Select(m => m.menuOrder)
                                            .FirstOrDefault();
                    SiteMenu.MenuOrder = SiteMenu.MenuOrder == null ? -1 : SiteMenu.MenuOrder;
                    SiteMenu.Roles = MenuDetails.Roles;
                }
                ViewBag.Edit = false;
                SiteMenu.Type = "Level3";
                //get data of all submenu details upto level3
                GetSubMenuDetails(SiteMenu);
            }
            else//if user edit during pageload then bind data based on id 
            {
                jntuh_menu MenuDetails = db.jntuh_menu.Where(m => m.id == id).FirstOrDefault();
                if (MenuDetails != null)
                {
                    SiteMenu.Id = MenuDetails.id;
                    SiteMenu.MenuName = MenuDetails.menuName;
                    SiteMenu.MenuControllerName = MenuDetails.menuControllerName;
                    SiteMenu.MenuActionName = MenuDetails.menuActionName;
                    SiteMenu.IsActive = MenuDetails.isActive;
                    SiteMenu.MenuOrder = db.jntuh_menu.Where(m => m.menuParentID == 0 &&
                                                             m.menuOrder > MenuDetails.menuOrder &&
                                                             m.Roles == MenuDetails.Roles)
                                            .OrderBy(m => m.menuOrder)
                                            .Select(m => m.menuOrder)
                                            .FirstOrDefault();
                    SiteMenu.MenuOrder = SiteMenu.MenuOrder == null ? -1 : SiteMenu.MenuOrder;
                    SiteMenu.Roles = MenuDetails.Roles;
                }
                SiteMenu.Type = "Roles";
                GetSubMenuDetails(SiteMenu);
                ViewBag.Edit = true;
            }
            return View("~/Views/Admin/SitePermissions.cshtml", SiteMenu);
        }

        // POST: /SitePermissions/EditSubMenu
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditSubMenu(SiteMenu SiteMenu)
        {
            //save edited submenu details
            SaveSubMenuDetails(SiteMenu);
            TempData["Success"] = "Sub Menu details are updated Successfully";
            return RedirectToAction("SubMenu", "SitePermissions");
        }

        //Get all parentmenu details
        private SiteMenu GetSiteMenuDetails(string MenuType)
        {
            SiteMenu SiteMenu = new SiteMenu();

            //Get all parent menu details
            List<jntuh_menu> MenuItems = db.jntuh_menu.Where(m => m.menuParentID == 0).OrderBy(m => m.menuName).ToList();

            //set radio button property to check based on menutype
            if (MenuType == "SubMenu")
            {
                SiteMenu.CheckedMenu = "SubMenu";
            }
            else
            {
                SiteMenu.CheckedMenu = "ParentMenu";
            }


            List<SitePermissions> SitePermissions = new List<SitePermissions>();
            int RoleId = 0;
            foreach (var item in MenuItems)
            {
                RoleId = 0;
                SitePermissions Permissions = new SitePermissions();
                Permissions.Id = item.id;
                Permissions.MenuName = item.menuName;
                Permissions.MenuParentID = item.menuParentID;
                if (item.menuParentID != 0)
                {
                    Permissions.menuParentName = db.jntuh_menu.Where(m => m.id == item.menuParentID).Select(m => m.menuName).FirstOrDefault();
                }
                else
                {
                    Permissions.menuParentName = string.Empty;
                }
                Permissions.MenuActionName = item.menuActionName;
                Permissions.MenuControllerName = item.menuControllerName;
                Permissions.IsActive = item.isActive;
                RoleId = Convert.ToInt32(item.Roles);
                Permissions.Roles = db.my_aspnet_roles.Where(r => r.id == RoleId).Select(r => r.name).FirstOrDefault();
                SitePermissions.Add(Permissions);
            }

            SiteMenu.MenuItems = SitePermissions.OrderBy(m => m.MenuName).ToList();

            //Get all Controller Names
            List<string> Controllers = GetControllerNames();
            Controllers = Controllers.Select(c => c.Remove(c.Length - 10))
                                     .OrderBy(c => c)
                                     .ToList();

            //Assign Controller Names to Viewbag
            ViewBag.Controllers = Controllers;

            //get all roles and bind to viewbag
            ViewBag.Roles = db.my_aspnet_roles
                            .Select(r => new
                            {
                                ID = r.id,
                                Name = r.name
                            }).ToList();
            return SiteMenu;
        }

        //get all submenu details
        private SiteMenu GetSubMenuDetails(SiteMenu SiteMenu)
        {
            List<jntuh_menu> MenuItems = new List<jntuh_menu>();

            List<SitePermissions> SitePermissions = new List<SitePermissions>();

            //set checked property to submenu radio button
            SiteMenu.CheckedMenu = "SubMenu";

            //if user selets roles dropdown then bindmenudetails based on roles
            if (SiteMenu.Type == "Roles")
            {
                MenuItems = db.jntuh_menu.Where(m => m.menuParentID == 0 && m.Roles == SiteMenu.Roles).OrderBy(m => m.menuName).ToList();
                int RoleId = 0;
                foreach (var item in MenuItems)
                {
                    RoleId = 0;
                    SitePermissions Permissions = new SitePermissions();
                    Permissions.Id = item.id;
                    Permissions.MenuName = item.menuName;
                    Permissions.MenuActionName = item.menuActionName;
                    Permissions.MenuControllerName = item.menuControllerName;
                    Permissions.IsActive = item.isActive;
                    RoleId = Convert.ToInt32(item.Roles);
                    Permissions.Roles = db.my_aspnet_roles.Where(r => r.id == RoleId).Select(r => r.name).FirstOrDefault();
                    SitePermissions.Add(Permissions);
                }
            }
            //if user selets parent dropdown then bind menudetails based on parent
            else if (SiteMenu.Type == "Parent")
            {
                //show parentmenu dropdown
                ViewBag.Parent = true;
                MenuItems = db.jntuh_menu.Where(m => m.menuParentID == SiteMenu.MenuParentID && m.Roles == SiteMenu.Roles).OrderBy(m => m.menuName).ToList();
                int RoleId = 0;
                foreach (var item in MenuItems)
                {
                    RoleId = 0;
                    SitePermissions Permissions = new SitePermissions();
                    Permissions.Id = item.id;
                    Permissions.MenuName = item.menuName;
                    Permissions.MenuActionName = item.menuActionName;
                    Permissions.MenuControllerName = item.menuControllerName;
                    Permissions.IsActive = item.isActive;
                    RoleId = Convert.ToInt32(item.Roles);
                    Permissions.Roles = db.my_aspnet_roles.Where(r => r.id == RoleId).Select(r => r.name).FirstOrDefault();
                    Permissions.menuParentName = db.jntuh_menu
                                                   .Where(m => m.id == item.menuParentID)
                                                   .Select(m => m.menuName)
                                                   .FirstOrDefault();
                    SitePermissions.Add(Permissions);
                }
            }
            //if user selets Level1 dropdown then bind menudetails based on Level1
            else if (SiteMenu.Type == "Level1")
            {
                //shows parentmenu dropdown
                ViewBag.Parent = true;

                //shows level1 dropdown
                ViewBag.Level1 = true;
                MenuItems = db.jntuh_menu.Where(m => m.menuParentID == SiteMenu.subMenuID1 && m.Roles == SiteMenu.Roles).OrderBy(m => m.menuName).ToList();
                int RoleId = 0;
                foreach (var item in MenuItems)
                {
                    RoleId = 0;
                    SitePermissions Permissions = new SitePermissions();
                    Permissions.Id = item.id;
                    Permissions.MenuName = item.menuName;
                    Permissions.MenuActionName = item.menuActionName;
                    Permissions.MenuControllerName = item.menuControllerName;
                    Permissions.IsActive = item.isActive;
                    RoleId = Convert.ToInt32(item.Roles);
                    Permissions.Roles = db.my_aspnet_roles.Where(r => r.id == RoleId).Select(r => r.name).FirstOrDefault();
                    Permissions.menuParentName = db.jntuh_menu
                                                   .Where(m => m.id == SiteMenu.MenuParentID)
                                                   .Select(m => m.menuName)
                                                   .FirstOrDefault();
                    Permissions.SubMenuLevel1 = db.jntuh_menu
                                                  .Where(m => m.id == SiteMenu.subMenuID1)
                                                  .Select(m => m.menuName)
                                                  .FirstOrDefault();
                    SitePermissions.Add(Permissions);
                }
            }
            //if user selets Level2 dropdown then bind menudetails based on Level2
            else if (SiteMenu.Type == "Level2")
            {
                //shows parentmenu dropdown
                ViewBag.Parent = true;

                //shows Level1 dropdown
                ViewBag.Level1 = true;

                //shows Level2 dropdown
                ViewBag.Level2 = true;

                MenuItems = db.jntuh_menu.Where(m => m.menuParentID == SiteMenu.subMenuID2 && m.Roles == SiteMenu.Roles).OrderBy(m => m.menuName).ToList();
                int RoleId = 0;
                foreach (var item in MenuItems)
                {
                    RoleId = 0;
                    SitePermissions Permissions = new SitePermissions();
                    Permissions.Id = item.id;
                    Permissions.MenuName = item.menuName;
                    Permissions.MenuActionName = item.menuActionName;
                    Permissions.MenuControllerName = item.menuControllerName;
                    Permissions.IsActive = item.isActive;
                    RoleId = Convert.ToInt32(item.Roles);
                    Permissions.Roles = db.my_aspnet_roles.Where(r => r.id == RoleId).Select(r => r.name).FirstOrDefault();
                    Permissions.menuParentName = db.jntuh_menu
                                                   .Where(m => m.id == SiteMenu.MenuParentID)
                                                   .Select(m => m.menuName)
                                                   .FirstOrDefault();
                    Permissions.SubMenuLevel1 = db.jntuh_menu
                                                  .Where(m => m.id == SiteMenu.subMenuID1)
                                                  .Select(m => m.menuName)
                                                  .FirstOrDefault();
                    Permissions.SubMenuLevel2 = db.jntuh_menu
                                                  .Where(m => m.id == SiteMenu.subMenuID2)
                                                  .Select(m => m.menuName)
                                                  .FirstOrDefault();
                    SitePermissions.Add(Permissions);
                }
            }
            //if user selets Level3 dropdown then bind menudetails based on Level3
            else if (SiteMenu.Type == "Level3")
            {
                //shows parentmenu dropdown
                ViewBag.Parent = true;

                //shows Level1 dropdown
                ViewBag.Level1 = true;

                //shows Level2 dropdown
                ViewBag.Level2 = true;

                //shows Level3 dropdown
                ViewBag.Level3 = true;

                MenuItems = db.jntuh_menu.Where(m => m.menuParentID == SiteMenu.subMenuID3 && m.Roles == SiteMenu.Roles).OrderBy(m => m.menuName).ToList();
                int RoleId = 0;
                foreach (var item in MenuItems)
                {
                    RoleId = 0;
                    SitePermissions Permissions = new SitePermissions();
                    Permissions.Id = item.id;
                    Permissions.MenuName = item.menuName;
                    Permissions.MenuActionName = item.menuActionName;
                    Permissions.MenuControllerName = item.menuControllerName;
                    Permissions.IsActive = item.isActive;
                    RoleId = Convert.ToInt32(item.Roles);
                    Permissions.Roles = db.my_aspnet_roles.Where(r => r.id == RoleId).Select(r => r.name).FirstOrDefault();
                    Permissions.menuParentName = db.jntuh_menu
                                                   .Where(m => m.id == SiteMenu.MenuParentID)
                                                   .Select(m => m.menuName)
                                                   .FirstOrDefault();
                    Permissions.SubMenuLevel1 = db.jntuh_menu
                                                  .Where(m => m.id == SiteMenu.subMenuID1)
                                                  .Select(m => m.menuName)
                                                  .FirstOrDefault();
                    Permissions.SubMenuLevel2 = db.jntuh_menu
                                                  .Where(m => m.id == SiteMenu.subMenuID2)
                                                  .Select(m => m.menuName)
                                                  .FirstOrDefault();
                    Permissions.SubMenuLevel3 = db.jntuh_menu
                                                  .Where(m => m.id == SiteMenu.subMenuID3)
                                                  .Select(m => m.menuName)
                                                  .FirstOrDefault();
                    SitePermissions.Add(Permissions);
                }
            }

            SiteMenu.MenuItems = SitePermissions.OrderBy(m => m.MenuName).ToList();
            //Get all Controller Names
            List<string> Controllers = GetControllerNames();
            Controllers = Controllers.Select(c => c.Remove(c.Length - 10))
                                     .OrderBy(c => c)
                                     .ToList();
            //Assign Controller Names to Viewbag
            ViewBag.Controllers = Controllers;


            ViewBag.Roles = db.my_aspnet_roles
                            .Select(r => new
                            {
                                ID = r.id,
                                Name = r.name
                            }).ToList();
            return SiteMenu;
        }

        //Save parent menu details
        private void SaveMenuDetails(SiteMenu SiteMenu)
        {
            if (ModelState.IsValid)
            {
                int UserID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                jntuh_menu MenuDetails = new jntuh_menu();
                MenuDetails.menuName = SiteMenu.MenuName;
                MenuDetails.menuActionName = SiteMenu.MenuActionName == null ? string.Empty : SiteMenu.MenuActionName;
                MenuDetails.menuControllerName = SiteMenu.MenuControllerName == null ? string.Empty : SiteMenu.MenuControllerName;
                MenuDetails.menuParentID = SiteMenu.MenuParentID == null ? 0 : SiteMenu.MenuParentID;
                MenuDetails.Roles = SiteMenu.Roles;

                //if user selects menuorder last then get max id and add one to it save as menuorder id
                if (SiteMenu.MenuOrder == -1)
                {

                    MenuDetails.menuOrder = (db.jntuh_menu.Where(m => m.menuParentID == 0 &&
                                                                          m.Roles == SiteMenu.Roles)
                                                              .Select(m => m.menuOrder)
                                                              .ToArray()
                                                              .Max()) + 1;
                    MenuDetails.menuOrder = MenuDetails.menuOrder == null ? 1 : MenuDetails.menuOrder;
                }
                else
                {
                    MenuDetails.menuOrder = SiteMenu.MenuOrder;
                }
                MenuDetails.isActive = SiteMenu.IsActive;
                MenuDetails.subMenuID1 = 0;
                MenuDetails.subMenuID2 = 0;
                MenuDetails.subMenuID3 = 0;
                MenuDetails.SelectedRoles = new string[] { "", "" };
                MenuDetails.menuParentName = string.Empty;
                if (SiteMenu.MenuOrder != -1)
                {
                    int[] MenuListId = db.jntuh_menu
                                         .Where(m => m.menuOrder >= SiteMenu.MenuOrder &&
                                                     m.menuParentID == 0 &&
                                                     m.Roles == SiteMenu.Roles)
                                         .OrderBy(m => m.menuOrder)
                                         .Select(m => m.id)
                                         .ToArray();
                    UpdateMenuTable(MenuListId, SiteMenu.MenuOrder);
                }
                if (SiteMenu.Id == 0)
                {
                    MenuDetails.createdBy = UserID;
                    MenuDetails.createdOn = DateTime.Now;
                    db.jntuh_menu.Add(MenuDetails);
                    db.SaveChanges();
                }
                else
                {
                    MenuDetails.id = SiteMenu.Id;
                    MenuDetails.createdBy = db.jntuh_menu.Where(m => m.id == SiteMenu.Id).Select(m => m.createdBy).FirstOrDefault();
                    MenuDetails.createdOn = db.jntuh_menu.Where(m => m.id == SiteMenu.Id).Select(m => m.createdOn).FirstOrDefault(); ;
                    MenuDetails.updatedBy = UserID;
                    MenuDetails.updatedOn = DateTime.Now;
                    jntuh_menu existing = db.jntuh_menu.Find(SiteMenu.Id);
                    ((IObjectContextAdapter)db).ObjectContext.Detach(existing);
                    db.Entry(MenuDetails).State = EntityState.Modified;
                    db.SaveChanges();
                    UpdateStatus(SiteMenu.Id, SiteMenu.IsActive);
                }
            }
        }

        //update status of submenu details
        private void UpdateStatus(int Id, bool Status)
        {
            bool ActualStatus = Status;
            bool UpdateStatus = Status == true ? false : true;

            List<jntuh_menu> MenuDetails = db.jntuh_menu.Where(m => m.menuParentID == Id &&
                                                                        m.isActive == UpdateStatus)
                                                             .ToList();
            if (MenuDetails.Count() > 0)
            {
                foreach (var item in MenuDetails)
                {
                    item.isActive = ActualStatus;
                    item.subMenuID1 = 0;
                    item.subMenuID2 = 0;
                    item.subMenuID3 = 0;
                    item.SelectedRoles = new string[] { "", "" };
                    item.menuParentName = string.Empty;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    List<jntuh_menu> MenuDetails1 = db.jntuh_menu.Where(m => m.menuParentID == item.id &&
                                                                    m.isActive == UpdateStatus)
                                                         .ToList();
                    if (MenuDetails1.Count() > 0)
                    {
                        foreach (var item1 in MenuDetails1)
                        {
                            item1.isActive = ActualStatus;
                            item1.subMenuID1 = 0;
                            item1.subMenuID2 = 0;
                            item1.subMenuID3 = 0;
                            item1.SelectedRoles = new string[] { "", "" };
                            item1.menuParentName = string.Empty;
                            db.Entry(item1).State = EntityState.Modified;
                            db.SaveChanges();
                            List<jntuh_menu> MenuDetails2 = db.jntuh_menu.Where(m => m.menuParentID == item1.id &&
                                                                            m.isActive == UpdateStatus)
                                                                 .ToList();
                            if (MenuDetails2.Count() > 0)
                            {
                                foreach (var item2 in MenuDetails2)
                                {
                                    item2.isActive = ActualStatus;
                                    item2.subMenuID1 = 0;
                                    item2.subMenuID2 = 0;
                                    item2.subMenuID3 = 0;
                                    item2.SelectedRoles = new string[] { "", "" };
                                    item2.menuParentName = string.Empty;
                                    db.Entry(item2).State = EntityState.Modified;
                                    db.SaveChanges();
                                    List<jntuh_menu> MenuDetails3 = db.jntuh_menu.Where(m => m.menuParentID == item2.id &&
                                                                                    m.isActive == UpdateStatus)
                                                                         .ToList();
                                    if (MenuDetails3.Count() > 0)
                                    {
                                        foreach (var item3 in MenuDetails3)
                                        {
                                            item3.isActive = ActualStatus;
                                            item3.subMenuID1 = 0;
                                            item3.subMenuID2 = 0;
                                            item3.subMenuID3 = 0;
                                            item3.SelectedRoles = new string[] { "", "" };
                                            item3.menuParentName = string.Empty;
                                            db.Entry(item3).State = EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //save submenu details
        private void SaveSubMenuDetails(SiteMenu SiteMenu)
        {
            int UserID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            jntuh_menu MenuDetails = new jntuh_menu();
            MenuDetails.menuName = SiteMenu.MenuName;
            MenuDetails.menuActionName = SiteMenu.MenuActionName == null ? string.Empty : SiteMenu.MenuActionName;
            MenuDetails.menuControllerName = SiteMenu.MenuControllerName == null ? string.Empty : SiteMenu.MenuControllerName;
            if (SiteMenu.subMenuID3 != null)
            {
                MenuDetails.menuParentID = SiteMenu.subMenuID3;
            }
            else if (SiteMenu.subMenuID2 != null)
            {
                MenuDetails.menuParentID = SiteMenu.subMenuID2;
            }
            else if (SiteMenu.subMenuID1 != null)
            {
                MenuDetails.menuParentID = SiteMenu.subMenuID1;
            }
            else
            {
                MenuDetails.menuParentID = SiteMenu.MenuParentID == null ? 0 : SiteMenu.MenuParentID;
            }
            MenuDetails.Roles = SiteMenu.Roles;
            if (SiteMenu.MenuOrder == -1)
            {
                if (SiteMenu.MenuParentID == null)
                {
                    MenuDetails.menuOrder = (db.jntuh_menu.Where(m => m.menuParentID == 0 &&
                                                                          m.Roles == SiteMenu.Roles)
                                                              .Select(m => m.menuOrder)
                                                              .ToArray()
                                                              .Max()) + 1;
                }
                else
                {
                    MenuDetails.menuOrder = (db.jntuh_menu.Where(m => m.menuParentID == MenuDetails.menuParentID &&
                                                                          m.Roles == SiteMenu.Roles)
                                                              .Select(m => m.menuOrder)
                                                              .ToArray()
                                                              .Max()) + 1;
                }
                MenuDetails.menuOrder = MenuDetails.menuOrder == null ? 1 : MenuDetails.menuOrder;
            }
            else
            {
                MenuDetails.menuOrder = SiteMenu.MenuOrder;
            }
            MenuDetails.isActive = SiteMenu.IsActive;
            MenuDetails.subMenuID1 = 0;
            MenuDetails.subMenuID2 = 0;
            MenuDetails.subMenuID3 = 0;
            MenuDetails.SelectedRoles = new string[] { "", "" };
            MenuDetails.menuParentName = string.Empty;
            if (SiteMenu.MenuOrder != -1)
            {
                int[] MenuListId = db.jntuh_menu
                                     .Where(m => m.menuOrder >= SiteMenu.MenuOrder &&
                                                 m.menuParentID == MenuDetails.menuParentID &&
                                                 m.Roles == SiteMenu.Roles)
                                     .OrderBy(m => m.menuOrder)
                                     .Select(m => m.id)
                                     .ToArray();
                UpdateMenuTable(MenuListId, SiteMenu.MenuOrder);
            }
            if (SiteMenu.Id == 0)
            {
                MenuDetails.createdBy = UserID;
                MenuDetails.createdOn = DateTime.Now;
                db.jntuh_menu.Add(MenuDetails);
                db.SaveChanges();
            }
            else
            {
                MenuDetails.id = SiteMenu.Id;
                MenuDetails.createdBy = db.jntuh_menu.Where(m => m.id == SiteMenu.Id).Select(m => m.createdBy).FirstOrDefault();
                MenuDetails.createdOn = db.jntuh_menu.Where(m => m.id == SiteMenu.Id).Select(m => m.createdOn).FirstOrDefault(); ;
                MenuDetails.updatedBy = UserID;
                MenuDetails.updatedOn = DateTime.Now;
                jntuh_menu existing = db.jntuh_menu.Find(SiteMenu.Id);
                ((IObjectContextAdapter)db).ObjectContext.Detach(existing);
                db.Entry(MenuDetails).State = EntityState.Modified;
                db.SaveChanges();
                UpdateStatus(SiteMenu.Id, SiteMenu.IsActive);
            }
        }

        //updates menu order
        private void UpdateMenuTable(int[] MenuOrderIds, int? PresentMenuOrderId)
        {
            int? MenuOrderId = PresentMenuOrderId;
            foreach (var item in MenuOrderIds)
            {
                MenuOrderId++;
                jntuh_menu EachMenuDetails = db.jntuh_menu
                                               .Where(m => m.id == item)
                                               .OrderBy(m => m.menuOrder)
                                               .FirstOrDefault();
                EachMenuDetails.subMenuID1 = 0;
                EachMenuDetails.subMenuID2 = 0;
                EachMenuDetails.subMenuID3 = 0;
                EachMenuDetails.SelectedRoles = new string[] { "", "" };
                EachMenuDetails.menuParentName = string.Empty;
                EachMenuDetails.menuOrder = MenuOrderId;
                db.Entry(EachMenuDetails).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        //Bind all controllers
        public static List<string> GetControllerNames()
        {
            List<string> controllerNames = new List<string>();
            GetSubClasses<Controller>().ForEach(
                type => controllerNames.Add(type.Name));
            return controllerNames;
        }

        //get all action names based on controller name
        public List<string> ActionNames(string controllerName)
        {
            List<string> ActionMethods = new List<string>();
            Assembly asm = Assembly.GetExecutingAssembly();
            var types = (from t in asm.GetTypes()
                         where typeof(IController).IsAssignableFrom(t) &&
                                string.Equals(controllerName + "Controller", t.Name, StringComparison.OrdinalIgnoreCase)
                         select t);
            var controllerType = types.FirstOrDefault();
            MethodInfo[] mi = controllerType.GetMethods();
            foreach (MethodInfo m in mi)
            {
                if (m.IsPublic)
                {
                    if (m.ReturnType == typeof(ActionResult) || m.ReturnType == typeof(ViewResult))
                    {
                        ActionMethods.Add(m.Name);
                    }
                }

            }
            if (controllerType == null)
            {
                return Enumerable.Empty<string>().ToList();
            }
            return ActionMethods;

        }

        //binding controller names
        private static List<Type> GetSubClasses<T>()
        {
            return Assembly.GetCallingAssembly().GetTypes().Where(
                type => type.IsSubclassOf(typeof(T))).ToList();
        }

        //binding action names
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetActions(string ControllerName)
        {
            if (ControllerName == string.Empty)
            {
                ControllerName = "0";
            }
            var ControllerNamesList = this.ActionNames(ControllerName).Distinct();

            var myData = ControllerNamesList.Select(a => new SelectListItem()
            {
                Text = a.ToString(),
                Value = a.ToString(),
            });

            return Json(myData, JsonRequestBehavior.AllowGet);
        }

        //binding menu order details
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetMenuSortOrder(string RoleName, int ExistId, int SubMenuParentId)
        {
            List<SelectListItem> RolesList = new List<SelectListItem>();
            if (SubMenuParentId == 0)
            {
                if (ExistId != 0)
                {
                    string MenuName = db.jntuh_menu.Where(m => m.id == ExistId).Select(m => m.menuName).FirstOrDefault();
                    var MenuOrderList = db.jntuh_menu
                                      .Where(m => m.Roles == RoleName &&
                                                  m.menuParentID == 0 &&
                                                  m.menuName != MenuName)
                                      .OrderBy(m => m.menuOrder)
                                      .Select(m => new
                                      {
                                          ID = m.menuOrder,
                                          Name = "Before " + m.menuName
                                      }).ToList();
                    foreach (var item in MenuOrderList)
                    {
                        SelectListItem NewValue = new SelectListItem()
                        {
                            Value = item.ID.ToString(),
                            Text = item.Name
                        };
                        RolesList.Add(NewValue);
                    }
                    SelectListItem Lastvalue = new SelectListItem()
                    {
                        Value = "-1",
                        Text = "Last"
                    };
                    RolesList.Add(Lastvalue);
                }
                else
                {
                    var MenuOrderList = db.jntuh_menu
                                      .Where(m => m.Roles == RoleName &&
                                                  m.menuParentID == 0)
                                      .OrderBy(m => m.menuOrder)
                                      .Select(m => new
                                      {
                                          ID = m.menuOrder,
                                          Name = "Before " + m.menuName
                                      }).ToList();
                    foreach (var item in MenuOrderList)
                    {
                        SelectListItem NewValue = new SelectListItem()
                        {
                            Value = item.ID.ToString(),
                            Text = item.Name
                        };
                        RolesList.Add(NewValue);
                    }
                    SelectListItem Lastvalue = new SelectListItem()
                    {
                        Value = "-1",
                        Text = "Last"
                    };
                    RolesList.Add(Lastvalue);
                }
            }
            else
            {
                if (ExistId != 0)
                {
                    string MenuName = db.jntuh_menu.Where(m => m.id == ExistId).Select(m => m.menuName).FirstOrDefault();
                    var MenuOrderList = db.jntuh_menu
                                      .Where(m => m.Roles == RoleName &&
                                                  m.menuParentID == SubMenuParentId &&
                                                  m.menuName != MenuName)
                                      .OrderBy(m => m.menuOrder)
                                      .Select(m => new
                                      {
                                          ID = m.menuOrder,
                                          Name = "Before " + m.menuName
                                      }).ToList();
                    foreach (var item in MenuOrderList)
                    {
                        SelectListItem NewValue = new SelectListItem()
                        {
                            Value = item.ID.ToString(),
                            Text = item.Name
                        };
                        RolesList.Add(NewValue);
                    }
                    SelectListItem Lastvalue = new SelectListItem()
                    {
                        Value = "-1",
                        Text = "Last"
                    };
                    RolesList.Add(Lastvalue);
                }
                else
                {
                    var MenuOrderList = db.jntuh_menu
                                      .Where(m => m.Roles == RoleName &&
                                                  m.menuParentID == SubMenuParentId)
                                      .OrderBy(m => m.menuOrder)
                                      .Select(m => new
                                      {
                                          ID = m.menuOrder,
                                          Name = "Before " + m.menuName
                                      }).ToList();
                    foreach (var item in MenuOrderList)
                    {
                        SelectListItem NewValue = new SelectListItem()
                        {
                            Value = item.ID.ToString(),
                            Text = item.Name
                        };
                        RolesList.Add(NewValue);
                    }
                    SelectListItem Lastvalue = new SelectListItem()
                    {
                        Value = "-1",
                        Text = "Last"
                    };
                    RolesList.Add(Lastvalue);
                }
            }
            return Json(RolesList, JsonRequestBehavior.AllowGet);
        }

        //binding level1, level2, level3 menus
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetLevelMenu(int ParentId)
        {
            List<SelectListItem> SubMenuList = new List<SelectListItem>();
            var SubMenuOrderList = db.jntuh_menu
                                .Where(m => m.menuParentID == ParentId)
                                      .OrderBy(m => m.menuOrder)
                                      .Select(m => new
                                      {
                                          ID = m.id,
                                          Name = m.menuName
                                      }).ToList();
            foreach (var item in SubMenuOrderList)
            {
                SelectListItem NewValue = new SelectListItem()
                {
                    Value = item.ID.ToString(),
                    Text = item.Name
                };
                SubMenuList.Add(NewValue);
            }
            return Json(SubMenuList, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteMenu(int id)
        {
            int? ParentId = 0;
            jntuh_menu DeletedMenuDetails = db.jntuh_menu.Where(m => m.id == id).FirstOrDefault();
            if (DeletedMenuDetails != null)
            {
                ParentId = DeletedMenuDetails.menuParentID;
                db.jntuh_menu.Remove(DeletedMenuDetails);
                db.SaveChanges();
                List<jntuh_menu> MenuDetails = db.jntuh_menu.Where(m => m.menuParentID == DeletedMenuDetails.id)
                                                             .ToList();
                if (MenuDetails.Count() > 0)
                {
                    foreach (var item in MenuDetails)
                    {
                        db.jntuh_menu.Remove(item);
                        db.SaveChanges();
                        List<jntuh_menu> MenuDetails1 = db.jntuh_menu.Where(m => m.menuParentID == item.id)
                                                             .ToList();
                        if (MenuDetails1.Count() > 0)
                        {
                            foreach (var item1 in MenuDetails1)
                            {
                                db.jntuh_menu.Remove(item1);
                                db.SaveChanges();
                                List<jntuh_menu> MenuDetails2 = db.jntuh_menu.Where(m => m.menuParentID == item1.id)
                                                                     .ToList();
                                if (MenuDetails2.Count() > 0)
                                {
                                    foreach (var item2 in MenuDetails2)
                                    {
                                        db.jntuh_menu.Remove(item2);
                                        db.SaveChanges();
                                        List<jntuh_menu> MenuDetails3 = db.jntuh_menu.Where(m => m.menuParentID == item2.id)
                                                                             .ToList();
                                        if (MenuDetails3.Count() > 0)
                                        {
                                            foreach (var item3 in MenuDetails3)
                                            {
                                                db.jntuh_menu.Remove(item3);
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (ParentId == 0)
            {
                TempData["Success"] = "Parent Menu details are deleted Successfully";
                return RedirectToAction("Menu", "SitePermissions");
            }
            else
            {
                TempData["Success"] = "Sub Menu details are deleted Successfully";
                return RedirectToAction("SubMenu", "SitePermissions");
            }
        }

        //bind partent menu names to drop down
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetParentMenuId(int ParentRoleId)
        {
            List<SelectListItem> MenuList = new List<SelectListItem>();
            string StrParentRoleId = ParentRoleId.ToString();
            var MenuOrderList = db.jntuh_menu
                                  .Where(m => m.Roles == StrParentRoleId &&
                                              m.menuParentID == 0)
                                  .OrderBy(m => m.menuOrder)
                                  .Select(m => new
                                  {
                                      ID = m.id,
                                      Name = m.menuName
                                  }).ToList();

            var myData = MenuOrderList.Select(a => new SelectListItem()
            {
                Text = a.Name.ToString(),
                Value = a.ID.ToString(),
            });

            return Json(myData, JsonRequestBehavior.AllowGet);
        }
    }
}
