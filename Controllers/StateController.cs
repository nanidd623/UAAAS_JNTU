using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class StateController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /State/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_state = db.jntuh_state.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_state.OrderBy(i => i.stateName).ToList());
        }

        //
        // GET: /State/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_state jntuh_state = db.jntuh_state.Find(id);
            return View(jntuh_state);
        }

        //
        // GET: /State/Create

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        }

        //
        // POST: /State/Create

        [HttpPost]
        public ActionResult Create(jntuh_state jntuh_state)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_state
                                 where s.stateName == jntuh_state.stateName
                                 select s.stateName);
                if (rowExists.Count() == 0)
                {
                    //jntuh_state.createdBy = (from user in db.my_aspnet_users
                    //                         where user.name == User.Identity.Name
                    //                         select user.id).FirstOrDefault();
                    //jntuh_state.createdBy = jntuh_state.createdBy.Equals(0) ? null : jntuh_state.createdBy;
                    //jntuh_state.createdBy = db.my_aspnet_users.First(u => u.name == User.Identity.Name).id;

                    db.jntuh_state.Add(jntuh_state);
                    db.SaveChanges();

                    TempData["Success"] = "Added successfully.";
                    //return RedirectToAction("Index");
                }
                else
                {
                    //ModelState.AddModelError("", "State Name is already exists. Please enter a different State Name.");
                    TempData["Error"] = "State Name is already exists. Please enter a different State Name.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_state.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_state.updatedBy);
            return View(jntuh_state);
        }

        //
        // GET: /State/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_state jntuh_state = db.jntuh_state.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_state.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_state.updatedBy);
            return View(jntuh_state);
        }

        //
        // POST: /State/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_state jntuh_state)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_state
                                 where s.stateName == jntuh_state.stateName && s.id != jntuh_state.id
                                 select s.stateName);
                if (rowExists.Count() == 0)
                {
                    //jntuh_state.updatedBy = (from user in db.my_aspnet_users
                    //                         where user.name == User.Identity.Name
                    //                         select user.id).FirstOrDefault();
                    //jntuh_state.updatedBy = jntuh_state.updatedBy.Equals(0) ? null : jntuh_state.updatedBy;

                    db.Entry(jntuh_state).State = EntityState.Modified;
                    db.SaveChanges();

                    //ModelState.AddModelError("", "Updated successfully.");
                    //return RedirectToAction("Index");

                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_state);
                }
                else
                {
                    //ModelState.AddModelError("", "State Name is already exists. Please enter a different State Name.");
                    TempData["Error"] = "State Name is already exists. Please enter a different State Name.";
                    return View(jntuh_state);
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_state.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_state.updatedBy);
            return View(jntuh_state);
        }

        //
        // GET: /State/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_state jntuh_state = db.jntuh_state.Find(id);
            return View(jntuh_state);
        }

        //
        // POST: /State/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            jntuh_state jntuh_state = db.jntuh_state.Find(id);
            db.jntuh_state.Remove(jntuh_state);
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