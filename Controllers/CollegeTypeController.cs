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
    public class CollegeTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /CollegeType/

        public ViewResult Index()
        {
            var jntuh_college_type = db.jntuh_college_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_college_type.ToList());
        }

        //
        // GET: /CollegeType/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_college_type jntuh_college_type = db.jntuh_college_type.Find(id);
            return View(jntuh_college_type);
        }

        //
        // GET: /CollegeType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /CollegeType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        public ActionResult Create(jntuh_college_type jntuh_college_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_college_type
                                 where s.collegeType == jntuh_college_type.collegeType
                                 select s.collegeType);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_college_type.Add(jntuh_college_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "College Type is already exists. Please enter a different College Type.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_type.updatedBy);
            return View(jntuh_college_type);
        }
        
        //
        // GET: /CollegeType/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_college_type jntuh_college_type = db.jntuh_college_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_type.updatedBy);
            return View(jntuh_college_type);
        }

        //
        // POST: /CollegeType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_college_type jntuh_college_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_college_type
                                 where s.collegeType == jntuh_college_type.collegeType && s.id != jntuh_college_type.id
                                 select s.collegeType);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_college_type).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_college_type);
                }
                else
                {
                    TempData["Error"] = "College Type is already exists. Please enter a different College Type.";
                    return View(jntuh_college_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_type.updatedBy);
            return View(jntuh_college_type);
        }

        //
        // GET: /CollegeType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_college_type jntuh_college_type = db.jntuh_college_type.Find(id);
            return View(jntuh_college_type);
        }

        //
        // POST: /CollegeType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_college_type jntuh_college_type = db.jntuh_college_type.Find(id);
            db.jntuh_college_type.Remove(jntuh_college_type);
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