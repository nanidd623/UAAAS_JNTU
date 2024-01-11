using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class UserController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /User/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            int roleID = db.my_aspnet_roles.Where(r => r.name == "Faculty").Select(r => r.id).FirstOrDefault();
            var list = db.my_aspnet_users.Join(db.my_aspnet_usersinroles, u => u.id, r => r.userId, (u, r) => new { u, r })
                .Where(a => a.r.roleId != roleID).Select(a => a.u).OrderBy(a => a.name).ToList();
            return View(list);
        }

        //
        // GET: /User/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            my_aspnet_users my_aspnet_users = db.my_aspnet_users.Find(id);
            my_aspnet_users.email = db.my_aspnet_membership.Where(m => m.userId == id).Select(m => m.Email).FirstOrDefault();
            my_aspnet_users.SelectedRole = db.my_aspnet_roles
                                           .Where(r => r.id == db.my_aspnet_usersinroles
                                                               .Where(ur => ur.userId == id)
                                                               .Select(ur => ur.roleId).FirstOrDefault())
                                           .Select(r => r.name).FirstOrDefault();
            my_aspnet_users.isApproved = db.my_aspnet_membership.Where(m => m.userId == id).Select(m => (m.IsApproved.HasValue ? m.IsApproved.Value : false)).FirstOrDefault();
            return View(my_aspnet_users);
        }

        //
        // GET: /User/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /User/Create

        [HttpPost]
        public ActionResult Create(my_aspnet_users my_aspnet_users)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from u in db.my_aspnet_users
                                 where u.name == my_aspnet_users.name
                                 select u.name);
                if (rowExists.Count() == 0)
                {
                    db.my_aspnet_users.Add(my_aspnet_users);
                    db.SaveChanges();

                    //return RedirectToAction("Index");
                    TempData["Success"] = "Username added successfully.";
                }
                else
                {
                    //ModelState.AddModelError("", "State Name is already exists. Please enter a different State Name.");
                    TempData["Error"] = "Userame is already exists.";
                }
            }

            return View(my_aspnet_users);
        }

        //
        // GET: /User/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            my_aspnet_users my_aspnet_users = db.my_aspnet_users.Find(id);
            GetAllRoles();
            my_aspnet_users.email = db.my_aspnet_membership.Where(m => m.userId == id).Select(m => m.Email).FirstOrDefault();
            my_aspnet_users.SelectedRole = db.my_aspnet_usersinroles.Where(ur => ur.userId == id).Select(ur => ur.roleId).FirstOrDefault().ToString();
            my_aspnet_users.isApproved = db.my_aspnet_membership.Where(m => m.userId == id).Select(m => (m.IsApproved.HasValue ? m.IsApproved.Value : false)).FirstOrDefault();
            return View(my_aspnet_users);
        }

        public void GetAllRoles()
        {
            var values = db.my_aspnet_roles.ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.name.ToString()
                });

            var menus = new SelectList(values, "Value", "Text");
            ViewData["Roles"] = menus;
        }
        //
        // POST: /User/Edit/5

        [HttpPost]
        public ActionResult Edit(my_aspnet_users my_aspnet_users)
        {
            if (my_aspnet_users.name == null)
            {
                my_aspnet_users.name = db.my_aspnet_users.Where(u => u.id == my_aspnet_users.id).Select(u => u.name).FirstOrDefault();
            }

            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                //bool changePasswordSucceeded;
                //try
                //{
                //    MembershipUser currentUser = Membership.GetUser(db.my_aspnet_users.Where(u => u.id == my_aspnet_users.id).Select(u => u.name).FirstOrDefault(), false /* userIsOnline */);
                //    changePasswordSucceeded = currentUser.ChangePassword(my_aspnet_users.OldPassword, my_aspnet_users.Password);
                //}
                //catch (Exception)
                //{
                //    changePasswordSucceeded = false;
                //}

                //if (changePasswordSucceeded)
                //{
                //return RedirectToAction("ChangePasswordSuccess");

                //db.Entry(my_aspnet_users).State = EntityState.Modified;
                //db.SaveChanges();

                my_aspnet_membership my_aspnet_membership = db.my_aspnet_membership.Find(my_aspnet_users.id);
                my_aspnet_membership.Email = my_aspnet_users.email;
                my_aspnet_membership.IsApproved = my_aspnet_users.isApproved;
                db.Entry(my_aspnet_membership).State = EntityState.Modified;
                db.SaveChanges();

                my_aspnet_usersinroles my_aspnet_usersinroles = db.my_aspnet_usersinroles
                                                                  .SingleOrDefault(x => x.userId == my_aspnet_users.id);
                my_aspnet_usersinroles.roleId = Convert.ToInt32(my_aspnet_users.SelectedRole);
                db.Entry(my_aspnet_usersinroles).State = EntityState.Modified;
                db.SaveChanges();

                //my_aspnet_users.email = db.my_aspnet_membership.Where(m => m.userId == id).Select(m => m.Email).FirstOrDefault();
                //my_aspnet_users.SelectedRole = db.my_aspnet_usersinroles.Where(ur => ur.userId == id).Select(ur => ur.roleId).FirstOrDefault().ToString();

                //send email
                IUserMailer mailer = new UserMailer();
                mailer.Welcome(my_aspnet_users.email, "PasswordReset", "JNTUH Account password changed", my_aspnet_users.name).SendAsync();

                TempData["Success"] = "User credentials updated successfully";
                //return RedirectToAction("Index");
                //}
                //else
                //{
                //    //ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                //    TempData["Error"] = "The current password is incorrect or the new password is invalid.";
                //}
            }

            GetAllRoles();
            return View(my_aspnet_users);
        }

        //
        // GET: /User/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            my_aspnet_users my_aspnet_users = db.my_aspnet_users.Find(id);
            return View(my_aspnet_users);
        }

        //
        // POST: /User/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            my_aspnet_users my_aspnet_users = db.my_aspnet_users.Find(id);
            db.my_aspnet_users.Remove(my_aspnet_users);
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