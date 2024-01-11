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
    public class FacultyTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /FacultyType/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_faculty_type = db.jntuh_faculty_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_faculty_type.ToList());
        }

        //
        // GET: /FacultyType/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_faculty_type jntuh_faculty_type = db.jntuh_faculty_type.Find(id);
            return View(jntuh_faculty_type);
        }

        //
        // GET: /FacultyType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /FacultyType/Create

        [HttpPost]
        public ActionResult Create(jntuh_faculty_type jntuh_faculty_type)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_faculty_type
                                where s.facultyType == jntuh_faculty_type.facultyType
                                select s.facultyType);
                if (rowExist.Count() == 0)
                {
                    db.jntuh_faculty_type.Add(jntuh_faculty_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Faculty Type is already exists. Please enter a different Faculty Type.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_type.updatedBy);
            return View(jntuh_faculty_type);
        }
        
        //
        // GET: /FacultyType/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_faculty_type jntuh_faculty_type = db.jntuh_faculty_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_type.updatedBy);
            return View(jntuh_faculty_type);
        }

        //
        // POST: /FacultyType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_faculty_type jntuh_faculty_type)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_faculty_type
                                where s.facultyType == jntuh_faculty_type.facultyType && s.id != jntuh_faculty_type.id
                                select s.facultyType);
                if (rowExist.Count() == 0)
                {
                    db.Entry(jntuh_faculty_type).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_faculty_type);
                }
                else
                {
                    TempData["Error"] = "Faculty Type is already exists. Please enter a different Faculty Type.";
                    return View(jntuh_faculty_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_type.updatedBy);
            return View(jntuh_faculty_type);
        }

        //
        // GET: /FacultyType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_faculty_type jntuh_faculty_type = db.jntuh_faculty_type.Find(id);
            return View(jntuh_faculty_type);
        }

        //
        // POST: /FacultyType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_faculty_type jntuh_faculty_type = db.jntuh_faculty_type.Find(id);
            db.jntuh_faculty_type.Remove(jntuh_faculty_type);
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