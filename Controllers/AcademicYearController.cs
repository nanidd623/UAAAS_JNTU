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
    public class AcademicYearController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /AcademicYear/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_academic_year = db.jntuh_academic_year.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1).OrderByDescending(j => j.actualYear);
            return View(jntuh_academic_year.ToList());
        }

        //
        // GET: /AcademicYear/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_academic_year jntuh_academic_year = db.jntuh_academic_year.Find(id);
            return View(jntuh_academic_year);
        }

        //
        // GET: /AcademicYear/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        }

        //
        // POST: /AcademicYear/Create

        [HttpPost]
        public ActionResult Create(jntuh_academic_year jntuh_academic_year)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_academic_year
                                where s.academicYear == jntuh_academic_year.academicYear
                                select s.academicYear);
                if (rowExist.Count() == 0)
                {
                    db.jntuh_academic_year.Add(jntuh_academic_year);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Academic Year is already exists. Please enter a different Academic Year.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_academic_year.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_academic_year.updatedBy);
            return View(jntuh_academic_year);
        }

        //
        // GET: /AcademicYear/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_academic_year jntuh_academic_year = db.jntuh_academic_year.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_academic_year.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_academic_year.updatedBy);
            return View(jntuh_academic_year);
        }

        //
        // POST: /AcademicYear/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_academic_year jntuh_academic_year)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_academic_year
                                where s.academicYear == jntuh_academic_year.academicYear && s.id != jntuh_academic_year.id
                                select s.academicYear);
                if (rowExist.Count() == 0)
                {
                    db.Entry(jntuh_academic_year).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Academic Year is already exists. Please enter a different Academic Year.";
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_academic_year.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_academic_year.updatedBy);
            return View(jntuh_academic_year);
        }

        //
        // GET: /AcademicYear/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_academic_year jntuh_academic_year = db.jntuh_academic_year.Find(id);
            return View(jntuh_academic_year);
        }

        //
        // POST: /AcademicYear/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            jntuh_academic_year jntuh_academic_year = db.jntuh_academic_year.Find(id);
            db.jntuh_academic_year.Remove(jntuh_academic_year);
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