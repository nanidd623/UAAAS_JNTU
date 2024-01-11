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
    public class DesignationController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /Designation/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_designation = db.jntuh_designation.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_designation.ToList());
        }

        //
        // GET: /Designation/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_designation jntuh_designation = db.jntuh_designation.Find(id);
            return View(jntuh_designation);
        }

        //
        // GET: /Designation/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /Designation/Create

        [HttpPost]
        public ActionResult Create(jntuh_designation jntuh_designation)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_designation
                                 where s.designation == jntuh_designation.designation
                                 select s.designation);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_designation.Add(jntuh_designation);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Designation is already exists. Please enter a different Designation.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_designation.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_designation.updatedBy);
            return View(jntuh_designation);
        }
        
        //
        // GET: /Designation/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_designation jntuh_designation = db.jntuh_designation.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_designation.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_designation.updatedBy);
            return View(jntuh_designation);
        }

        //
        // POST: /Designation/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_designation jntuh_designation)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_designation
                                 where s.designation == jntuh_designation.designation && s.id != jntuh_designation.id
                                 select s.designation);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_designation).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_designation);
                }
                else
                {
                    TempData["Error"] = "Designation is already exists. Please enter a different Designation.";
                    return View(jntuh_designation);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_designation.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_designation.updatedBy);
            return View(jntuh_designation);
        }

        //
        // GET: /Designation/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_designation jntuh_designation = db.jntuh_designation.Find(id);
            return View(jntuh_designation);
        }

        //
        // POST: /Designation/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_designation jntuh_designation = db.jntuh_designation.Find(id);
            db.jntuh_designation.Remove(jntuh_designation);
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