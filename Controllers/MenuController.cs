using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class MenuController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /Menu/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_menu = db.jntuh_menu.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);

            List<jntuh_menu> jntuh_menu2 = jntuh_menu.ToList();
            List<my_aspnet_roles> my_aspnet_roles2 = db.my_aspnet_roles.ToList();

            foreach (var row in jntuh_menu)
            {
                foreach (var x in jntuh_menu2)
                {
                    if (x.id == row.menuParentID)
                    {
                        row.menuParentName = x.menuName;
                    }
                }

                string[] roles = row.Roles.Split(',');
                string assignedRoles = string.Empty;
                foreach (string r in roles)
                {
                    foreach (var role in my_aspnet_roles2)
                    {
                        if (r == role.id.ToString())
                        {
                            assignedRoles += role.name + ", ";
                        }
                    }
                }
                assignedRoles = assignedRoles.Remove(assignedRoles.LastIndexOf(','));
                row.Roles = assignedRoles;
            }

            return View(jntuh_menu.ToList());
        }

        //
        // GET: /Menu/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_menu jntuh_menu = db.jntuh_menu.Find(id);
            List<jntuh_menu> jntuh_menu2 = db.jntuh_menu.ToList();
            List<my_aspnet_roles> my_aspnet_roles2 = db.my_aspnet_roles.ToList();

            foreach (var x in jntuh_menu2)
            {
                if (jntuh_menu.menuParentID == 0)
                {
                    jntuh_menu.menuParentName = "";
                }
                if (x.id == jntuh_menu.menuParentID)
                {
                    jntuh_menu.menuParentName = x.menuName;
                }
            }

            string[] roles = jntuh_menu.Roles.Split(',');
            string assignedRoles = string.Empty;
            foreach (string r in roles)
            {
                foreach (var role in my_aspnet_roles2)
                {
                    if (r == role.id.ToString())
                    {
                        assignedRoles += role.name + ", ";
                    }
                }
            }
            assignedRoles = assignedRoles.Remove(assignedRoles.LastIndexOf(','));
            jntuh_menu.Roles = assignedRoles;

            return View(jntuh_menu);
        }

        //
        // GET: /Menu/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");

            GetPageActions();
            GetMenuOrder();
            GetParentMenuItems();
            GetAllRoles();

            var values = GetControllerNames()
                .Select(x => new SelectListItem
                {
                    Value = x.Replace("Controller", string.Empty).ToString(),
                    Text = x.Replace("Controller", string.Empty).ToString()
                });
            var Controllers = new SelectList(values, "Value", "Text");

            ViewData["Controllers"] = Controllers;

            return View();
        }

        public void GetPageActions()
        {
            var pageActions = new SelectList(new[]{ new {Value="1", Text="List"},
                                                    new {Value="2", Text="Create"},
                                                    new {Value="3", Text="Edit"},
                                                    new {Value="4", Text="Create"},
                                                    new {Value="5", Text="Details"},
                                          }, "Text", "Text", 0);

            ViewData["Actions"] = pageActions;
        }

        public void GetMenuOrder()
        {
            var menuOrder = new SelectList(new[]{   new {Value="1", Text="1"},
                                                    new {Value="2", Text="2"},
                                                    new {Value="3", Text="3"},
                                                    new {Value="4", Text="4"},
                                                    new {Value="5", Text="5"},
                                                    new {Value="6", Text="6"},
                                                    new {Value="7", Text="7"},
                                                    new {Value="8", Text="8"},
                                                    new {Value="9", Text="9"},
                                                    new {Value="10", Text="10"},
                                                    new {Value="11", Text="11"},
                                                    new {Value="12", Text="12"},
                                                    new {Value="13", Text="13"},
                                                    new {Value="14", Text="14"},
                                                    new {Value="15", Text="15"},
                                                    new {Value="16", Text="16"},
                                                    new {Value="17", Text="17"},
                                                    new {Value="18", Text="18"},
                                                    new {Value="19", Text="19"},
                                                    new {Value="20", Text="20"},
                                                    new {Value="21", Text="21"},
                                                    new {Value="22", Text="22"},
                                                    new {Value="23", Text="23"},
                                                    new {Value="24", Text="24"},
                                                    new {Value="25", Text="25"},
                                                    new {Value="26", Text="26"},
                                                    new {Value="27", Text="27"},
                                                    new {Value="28", Text="28"},
                                                    new {Value="29", Text="29"},
                                                    new {Value="30", Text="30"},
                                          }, "Value", "Text", 0);

            ViewData["MenuOrder"] = menuOrder;
        }

        public void GetAllRoles()
        {
            var values = Roles.GetAllRoles().ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.ToString(),
                    Text = x.ToString()
                });

            var menus = new SelectList(values, "Value", "Text");
            ViewData["Roles"] = menus;
        }

        public void GetAllRolesWithSelectItems(int id)
        {
            jntuh_menu jntuh_menu = db.jntuh_menu.Find(id);
            string[] strRoles = jntuh_menu.Roles.Split(',');
            int[] roles = Array.ConvertAll(strRoles, s => int.Parse(s));

            var values = db.my_aspnet_roles.ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.name.ToString()
                });

            var sel = values.Where(s => strRoles.Contains(s.Value)).Select(s => s.Value);

            var menus = new MultiSelectList(values, "Value", "Text", sel);

            ViewData["Roles"] = menus;
        }

        public void GetParentMenuItems()
        {
            var list1 = db.jntuh_menu.Where(m => m.menuParentID == 0).ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.menuName.ToString()
                });

            var menu1 = new SelectList(list1, "Value", "Text");

            ViewData["ParentMenu"] = menu1;
        }

        public void GetMenuItems(int id, jntuh_menu jntuh_menu)
        {
            var parentMenu = 0;
            var subMenu1 = 0;
            var subMenu2 = 0;
            var subMenu3 = 0;

            parentMenu = db.jntuh_menu.Find(id).menuParentID.HasValue ? db.jntuh_menu.Find(id).menuParentID.Value : 0;

            if (!db.jntuh_menu.Where(m => m.id == parentMenu).Select(m => (m.menuParentID.HasValue ? m.menuParentID.Value : 0)).Equals("0") && parentMenu != 0)
            {
                subMenu1 = parentMenu;
                parentMenu = db.jntuh_menu.Find(subMenu1).menuParentID.HasValue ? db.jntuh_menu.Find(subMenu1).menuParentID.Value : 0;

                if (parentMenu > 0)
                {
                    jntuh_menu.subMenuID1 = subMenu1;
                    jntuh_menu.menuParentID = parentMenu;
                }

                if (!db.jntuh_menu.Where(m => m.id == parentMenu).Select(m => (m.menuParentID.HasValue ? m.menuParentID.Value : 0)).Equals("0") && parentMenu != 0)
                {
                    subMenu2 = subMenu1;
                    subMenu1 = parentMenu;
                    parentMenu = db.jntuh_menu.Find(subMenu1).menuParentID.HasValue ? db.jntuh_menu.Find(subMenu1).menuParentID.Value : 0;

                    if (parentMenu > 0)
                    {
                        jntuh_menu.subMenuID2 = subMenu2;
                        jntuh_menu.subMenuID1 = subMenu1;
                        jntuh_menu.menuParentID = parentMenu;
                    }

                    if (!db.jntuh_menu.Where(m => m.id == parentMenu).Select(m => (m.menuParentID.HasValue ? m.menuParentID.Value : 0)).Equals("0") && parentMenu != 0)
                    {
                        subMenu3 = subMenu2;
                        subMenu2 = subMenu1;
                        subMenu1 = parentMenu;
                        parentMenu = db.jntuh_menu.Find(subMenu1).menuParentID.HasValue ? db.jntuh_menu.Find(subMenu1).menuParentID.Value : 0;

                        if (parentMenu > 0)
                        {
                            jntuh_menu.subMenuID3 = subMenu3;
                            jntuh_menu.subMenuID2 = subMenu2;
                            jntuh_menu.subMenuID1 = subMenu1;
                            jntuh_menu.menuParentID = parentMenu;

                        }
                    }
                }
            }

            var emptyList = db.jntuh_menu.Where(m => m.menuParentID == -1).ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.menuName.ToString()
                });

            var emptyMenu = new SelectList(emptyList, "Value", "Text");

            var list1 = db.jntuh_menu.Where(m => m.menuParentID == parentMenu).ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.menuName.ToString()
                });

            var menu1 = new SelectList(list1, "Value", "Text");

            ViewData["ParentMenu"] = menu1;


            if (parentMenu != 0)
            {
                var list2 = db.jntuh_menu.Where(m => m.menuParentID == subMenu1).ToList()
                    .Select(x => new SelectListItem
                    {
                        Value = x.id.ToString(),
                        Text = x.menuName.ToString()
                    });

                var menu2 = new SelectList(list2, "Value", "Text");

                ViewData["SubMenu1"] = menu2;
            }
            else
            { ViewData["SubMenu1"] = emptyMenu; }

            if (subMenu1 != 0)
            {
                var list3 = db.jntuh_menu.Where(m => m.menuParentID == subMenu2).ToList()
                    .Select(x => new SelectListItem
                    {
                        Value = x.id.ToString(),
                        Text = x.menuName.ToString()
                    });

                var menu3 = new SelectList(list3, "Value", "Text");

                ViewData["SubMenu2"] = menu3;
            }
            else
            { ViewData["SubMenu2"] = emptyMenu; }

            if (subMenu2 != 0)
            {
                var list4 = db.jntuh_menu.Where(m => m.menuParentID == subMenu3).ToList()
                    .Select(x => new SelectListItem
                    {
                        Value = x.id.ToString(),
                        Text = x.menuName.ToString()
                    });

                var menu4 = new SelectList(list4, "Value", "Text");

                ViewData["SubMenu3"] = subMenu3 > 0 ? menu4 : emptyMenu;
            }
            else
            { ViewData["SubMenu3"] = emptyMenu; }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetMenu(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                int menuID = Convert.ToInt32(id);
                var myData = db.jntuh_menu.Where(m => m.menuParentID == menuID).ToList()
                    .Select(x => new SelectListItem
                    {
                        Value = x.id.ToString(),
                        Text = x.menuName.ToString()
                    });
                return Json(myData, JsonRequestBehavior.AllowGet);
            }
            else
            { return Json(null, JsonRequestBehavior.AllowGet); }

        }

        private static List<Type> GetSubClasses<T>()
        {
            return Assembly.GetCallingAssembly().GetTypes().Where(
                type => type.IsSubclassOf(typeof(T))).ToList();
        }

        public List<string> GetControllerNames()
        {
            List<string> controllerNames = new List<string>();
            GetSubClasses<Controller>().ForEach(
                type => controllerNames.Add(type.Name));
            return controllerNames;
        }

        //
        // POST: /Menu/Create

        [HttpPost]
        public ActionResult Create(jntuh_menu jntuh_menu)
        {
            if (ModelState.IsValid)
            {
                int? parentMenuID = 0;
                if (jntuh_menu.subMenuID3 != null)
                {
                    parentMenuID = jntuh_menu.subMenuID3;
                }
                else if (jntuh_menu.subMenuID2 != null)
                {
                    parentMenuID = jntuh_menu.subMenuID2;
                }
                else if (jntuh_menu.subMenuID1 != null)
                {
                    parentMenuID = jntuh_menu.subMenuID1;
                }
                else if (jntuh_menu.menuParentID != null)
                {
                    parentMenuID = jntuh_menu.menuParentID;
                }

                jntuh_menu.menuParentID = parentMenuID;

                string selectedRoles = string.Empty;
                foreach (string r in jntuh_menu.SelectedRoles)
                {
                    foreach (var dr in db.my_aspnet_roles)
                    {
                        if (dr.name == r)
                        {
                            selectedRoles += dr.id.ToString() + ",";
                        }
                    }

                }
                selectedRoles = selectedRoles.TrimEnd(',');
                jntuh_menu.Roles = selectedRoles;
                db.jntuh_menu.Add(jntuh_menu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_menu.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_menu.updatedBy);
            return View(jntuh_menu);
        }

        //
        // GET: /Menu/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_menu jntuh_menu = db.jntuh_menu.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_menu.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_menu.updatedBy);

            GetPageActions();
            GetMenuOrder();
            GetMenuItems(id, jntuh_menu);
            GetAllRolesWithSelectItems(id);

            var values = GetControllerNames()
                .Select(x => new SelectListItem
                {
                    Value = x.Replace("Controller", string.Empty).ToString(),
                    Text = x.Replace("Controller", string.Empty).ToString()
                });
            var Controllers = new SelectList(values, "Value", "Text");

            ViewData["Controllers"] = Controllers;

            return View(jntuh_menu);
        }

        //
        // POST: /Menu/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_menu jntuh_menu)
        {
            jntuh_menu.subMenuID3 = jntuh_menu.subMenuID3 == 0 ? -1 : jntuh_menu.subMenuID3;
            jntuh_menu.subMenuID2 = jntuh_menu.subMenuID2 == 0 ? -1 : jntuh_menu.subMenuID2;
            jntuh_menu.subMenuID1 = jntuh_menu.subMenuID1 == 0 ? -1 : jntuh_menu.subMenuID1;

            if (ModelState.IsValid)
            {
                jntuh_menu.Roles = String.Join(",", from s in jntuh_menu.SelectedRoles select s);
                db.Entry(jntuh_menu).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_menu.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_menu.updatedBy);

            GetPageActions();
            GetMenuOrder();
            GetMenuItems(jntuh_menu.id, jntuh_menu);
            GetAllRolesWithSelectItems(jntuh_menu.id);

            var values = GetControllerNames()
                .Select(x => new SelectListItem
                {
                    Value = x.Replace("Controller", string.Empty).ToString(),
                    Text = x.Replace("Controller", string.Empty).ToString()
                });
            var Controllers = new SelectList(values, "Value", "Text");

            ViewData["Controllers"] = Controllers;

            return View(jntuh_menu);
        }

        //
        // GET: /Menu/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_menu jntuh_menu = db.jntuh_menu.Find(id);
            List<jntuh_menu> jntuh_menu2 = db.jntuh_menu.ToList();
            List<my_aspnet_roles> my_aspnet_roles2 = db.my_aspnet_roles.ToList();

            foreach (var x in jntuh_menu2)
            {
                if (jntuh_menu.menuParentID == 0)
                {
                    jntuh_menu.menuParentName = "";
                }
                if (x.id == jntuh_menu.menuParentID)
                {
                    jntuh_menu.menuParentName = x.menuName;
                }
            }

            string[] roles = jntuh_menu.Roles.Split(',');
            string assignedRoles = string.Empty;
            foreach (string r in roles)
            {
                foreach (var role in my_aspnet_roles2)
                {
                    if (r == role.id.ToString())
                    {
                        assignedRoles += role.name + ", ";
                    }
                }
            }
            assignedRoles = assignedRoles.Remove(assignedRoles.LastIndexOf(','));
            jntuh_menu.Roles = assignedRoles;

            return View(jntuh_menu);
        }

        //
        // POST: /Menu/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            jntuh_menu jntuh_menu = db.jntuh_menu.Find(id);
            db.jntuh_menu.Remove(jntuh_menu);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}