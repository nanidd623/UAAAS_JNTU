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
    public class QualificationController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /Qualification/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_qualification = db.jntuh_qualification.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_qualification.ToList());
        }

        //
        // GET: /Qualification/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_qualification jntuh_qualification = db.jntuh_qualification.Find(id);
            return View(jntuh_qualification);
        }

        //
        // GET: /Qualification/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        }

        //
        // POST: /Qualification/Create

        [HttpPost]
        public ActionResult Create(jntuh_qualification jntuh_qualification)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_qualification
                                 where s.qualification == jntuh_qualification.qualification
                                 select s.qualification);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_qualification.Add(jntuh_qualification);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Qualification is already exists. Please enter a different Qualification.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_qualification.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_qualification.updatedBy);
            return View(jntuh_qualification);
        }

        //
        // GET: /Qualification/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_qualification jntuh_qualification = db.jntuh_qualification.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_qualification.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_qualification.updatedBy);
            return View(jntuh_qualification);
        }

        //
        // POST: /Qualification/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_qualification jntuh_qualification)
        {
            if (ModelState.IsValid)
            {

                var rowExists = (from s in db.jntuh_qualification
                                 where s.qualification == jntuh_qualification.qualification && s.id != jntuh_qualification.id
                                 select s.qualification);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_qualification).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_qualification);
                }
                else
                {
                    TempData["Error"] = "Qualification is already exists. Please enter a different Qualification.";
                    return View(jntuh_qualification);
                }

            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_qualification.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_qualification.updatedBy);
            return View(jntuh_qualification);
        }

        //
        // GET: /Qualification/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_qualification jntuh_qualification = db.jntuh_qualification.Find(id);
            return View(jntuh_qualification);
        }

        //
        // POST: /Qualification/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            jntuh_qualification jntuh_qualification = db.jntuh_qualification.Find(id);
            db.jntuh_qualification.Remove(jntuh_qualification);
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