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
    public class FacultyCategoryController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /FacultyCategory/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_faculty_category = db.jntuh_faculty_category.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_faculty_category.ToList());
        }

        //
        // GET: /FacultyCategory/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_faculty_category jntuh_faculty_category = db.jntuh_faculty_category.Find(id);
            return View(jntuh_faculty_category);
        }

        //
        // GET: /FacultyCategory/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /FacultyCategory/Create

        [HttpPost]
        public ActionResult Create(jntuh_faculty_category jntuh_faculty_category)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_faculty_category
                                 where s.facultyCategory == jntuh_faculty_category.facultyCategory
                                 select s.facultyCategory);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_faculty_category.Add(jntuh_faculty_category);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Faculty Category is already exists. Please enter a different Faculty Category.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_category.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_category.updatedBy);
            return View(jntuh_faculty_category);
        }
        
        //
        // GET: /FacultyCategory/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_faculty_category jntuh_faculty_category = db.jntuh_faculty_category.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_category.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_category.updatedBy);
            return View(jntuh_faculty_category);
        }

        //
        // POST: /FacultyCategory/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_faculty_category jntuh_faculty_category)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_faculty_category
                                 where s.facultyCategory == jntuh_faculty_category.facultyCategory && s.id !=jntuh_faculty_category.id
                                 select s.facultyCategory);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_faculty_category).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_faculty_category);
                }
                else
                {
                    TempData["Error"] = "Faculty Category is already exists. Please enter a different Faculty Category.";
                    return View(jntuh_faculty_category);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_category.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_faculty_category.updatedBy);
            return View(jntuh_faculty_category);
        }

        //
        // GET: /FacultyCategory/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_faculty_category jntuh_faculty_category = db.jntuh_faculty_category.Find(id);
            return View(jntuh_faculty_category);
        }

        //
        // POST: /FacultyCategory/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_faculty_category jntuh_faculty_category = db.jntuh_faculty_category.Find(id);
            db.jntuh_faculty_category.Remove(jntuh_faculty_category);
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