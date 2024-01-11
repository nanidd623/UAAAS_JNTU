using System;
using System.Collections.Generic;
using System.Web.Providers.Entities;
using System.Web.Security;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace UAAAS.Models
{
    public class SiteMenuManager
    {
        private uaaasDBContext db = new uaaasDBContext();

        public List<ISiteLink> GetSitemMenuItems()
        {
            var items = new List<ISiteLink>();

            string roles = string.Empty;
            List<int> userRoles = null;

            if (HttpContext.Current.User.Identity.Name != string.Empty)
            {
                int userID = 0;
                if (HttpContext.Current.Request.Params["rstu"] != null)
                {
                    userID = Convert.ToInt32(Membership.GetUser(QueryStringEncryption.Encryption64.Decrypt(HttpContext.Current.Request.Params["rstu"].ToString(), WebConfigurationManager.AppSettings["CryptoKey"])).ProviderUserKey);
                }
                else
                {
                    userID = Convert.ToInt32(Membership.GetUser(HttpContext.Current.User.Identity.Name).ProviderUserKey);
                }

                int loggedInUserId = userID;

                userRoles = (from login in db.my_aspnet_usersinroles
                             where login.userId == loggedInUserId
                             select login.roleId).ToList();
            }

            if (userRoles != null)
            {
                foreach (var role in userRoles)
                {
                    roles += role.ToString() + ",";
                }
            }

            if (roles.Equals(string.Empty))
                roles = db.my_aspnet_roles.Where(r => r.name.Equals("User")).Select(r => r.id).FirstOrDefault().ToString();
            string[] userAssignedRoles = roles.TrimEnd(',').Split(',');
            var jntuh_menus = db.jntuh_menu.Where(m => m.isActive==true).ToList();
            db.Database.Connection.Close();
            foreach (var menu in jntuh_menus)
            {
                string[] menuRoles = menu.Roles.Split(',');
                string firstMatch = string.Empty;
                if (!string.IsNullOrWhiteSpace(roles))
                    firstMatch = menuRoles.FirstOrDefault(userAssignedRoles.Contains);
                else
                    firstMatch = "0";

                if (!string.IsNullOrWhiteSpace(firstMatch))
                {
                    if (menu.Roles.Contains(firstMatch))
                    {
                        int parent = 0;
                        if (menu.menuParentID == null)
                            parent = 0;
                        else
                            parent = Convert.ToInt32(menu.menuParentID);

                        items.Add(new SiteMenuItem
                        {
                            Id = menu.id,
                            ParentId = parent,
                            Text = menu.menuName,
                            Url = menu.menuControllerName.Equals(string.Empty) ? "#" : string.Format("/{0}/{1}", menu.menuControllerName, menu.menuActionName),
                            OpenInNewWindow = false,
                            SortOrder = (int)menu.menuOrder
                        });
                    }
                }
            }

            //if (userAssignedRoles.Contains("5"))
            //{
            //    int menuid = db.jntuh_menu.OrderByDescending(m => m.id).Select(m => m.id).FirstOrDefault() + 1;
            //    int parentid = menuid;
            //    items.Add(new SiteMenuItem
            //    {
            //        Id = menuid,
            //        ParentId = 0,
            //        Text = "Public Reports",
            //        Url = "#",
            //        OpenInNewWindow = true,
            //        SortOrder = (int)db.jntuh_menu.Where(m => m.menuParentID == 0).OrderByDescending(m => m.menuOrder).Select(m => m.menuOrder).FirstOrDefault() + 1
            //    });

            //    items.Add(new SiteMenuItem
            //    {
            //        Id = menuid + 1,
            //        ParentId = parentid,
            //        Text = "Inspection Reports A.Y.2013-14",
            //        Url = "http://www.google.com",
            //        OpenInNewWindow = true,
            //        SortOrder = 1
            //    });
            //}
            return items;
        }
    }
}