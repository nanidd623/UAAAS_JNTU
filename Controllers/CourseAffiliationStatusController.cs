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
    public class CourseAffiliationStatusController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /CourseAffiliationStatus/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_course_affiliation_status = db.jntuh_course_affiliation_status.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_course_affiliation_status.ToList());
        }

        //
        // GET: /CourseAffiliationStatus/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_course_affiliation_status jntuh_course_affiliation_status = db.jntuh_course_affiliation_status.Find(id);
            return View(jntuh_course_affiliation_status);
        }

        //
        // GET: /CourseAffiliationStatus/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /CourseAffiliationStatus/Create

        [HttpPost]
        public ActionResult Create(jntuh_course_affiliation_status jntuh_course_affiliation_status)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_course_affiliation_status
                                 where s.courseAffiliationStatusCode == jntuh_course_affiliation_status.courseAffiliationStatusCode
                                 select s.courseAffiliationStatusCode);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_course_affiliation_status.Add(jntuh_course_affiliation_status);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Course Affiliation Status Code is already exists. Please enter a different Course Affiliation Status Code.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_course_affiliation_status.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_course_affiliation_status.updatedBy);
            return View(jntuh_course_affiliation_status);
        }
        
        //
        // GET: /CourseAffiliationStatus/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_course_affiliation_status jntuh_course_affiliation_status = db.jntuh_course_affiliation_status.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_course_affiliation_status.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_course_affiliation_status.updatedBy);
            return View(jntuh_course_affiliation_status);
        }

        //
        // POST: /CourseAffiliationStatus/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_course_affiliation_status jntuh_course_affiliation_status)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_course_affiliation_status
                                 where s.courseAffiliationStatusCode == jntuh_course_affiliation_status.courseAffiliationStatusCode && s.id != jntuh_course_affiliation_status.id
                                 select s.courseAffiliationStatusCode);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_course_affiliation_status).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_course_affiliation_status);
                }
                else
                {
                    TempData["Error"] = "Course Affiliation Status Code is already exists. Please enter a different Course Affiliation Status Code.";
                    return View(jntuh_course_affiliation_status);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_course_affiliation_status.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_course_affiliation_status.updatedBy);
            return View(jntuh_course_affiliation_status);
        }

        //
        // GET: /CourseAffiliationStatus/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_course_affiliation_status jntuh_course_affiliation_status = db.jntuh_course_affiliation_status.Find(id);
            return View(jntuh_course_affiliation_status);
        }

        //
        // POST: /CourseAffiliationStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_course_affiliation_status jntuh_course_affiliation_status = db.jntuh_course_affiliation_status.Find(id);
            db.jntuh_course_affiliation_status.Remove(jntuh_course_affiliation_status);
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