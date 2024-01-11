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
    public class EducationCategoryController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /EducationCategory/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_education_category = db.jntuh_education_category.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_education_category.ToList());
        }

        //
        // GET: /EducationCategory/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_education_category jntuh_education_category = db.jntuh_education_category.Find(id);
            return View(jntuh_education_category);
        }

        //
        // GET: /EducationCategory/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /EducationCategory/Create

        [HttpPost]
        public ActionResult Create(jntuh_education_category jntuh_education_category)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_education_category
                                 where s.educationCategoryName == jntuh_education_category.educationCategoryName
                                 select s.educationCategoryName);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_education_category.Add(jntuh_education_category);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Education category Name is already exists. Please enter a different Education category Name.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_education_category.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_education_category.updatedBy);
            return View(jntuh_education_category);
        }
        
        //
        // GET: /EducationCategory/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_education_category jntuh_education_category = db.jntuh_education_category.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_education_category.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_education_category.updatedBy);
            return View(jntuh_education_category);
        }

        //
        // POST: /EducationCategory/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_education_category jntuh_education_category)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_education_category
                                 where s.educationCategoryName == jntuh_education_category.educationCategoryName && s.id != jntuh_education_category.id
                                 select s.educationCategoryName);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_education_category).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_education_category);
                }
                else
                {
                    TempData["Error"] = "Education category Name is already exists. Please enter a different Education category Name.";
                    return View(jntuh_education_category);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_education_category.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_education_category.updatedBy);
            return View(jntuh_education_category);
        }

        //
        // GET: /EducationCategory/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_education_category jntuh_education_category = db.jntuh_education_category.Find(id);
            return View(jntuh_education_category);
        }

        //
        // POST: /EducationCategory/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_education_category jntuh_education_category = db.jntuh_education_category.Find(id);
            db.jntuh_education_category.Remove(jntuh_education_category);
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