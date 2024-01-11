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
    public class LandRegistrationTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /LandRegistrationType/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_land_registration_type = db.jntuh_land_registration_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_land_registration_type.ToList());
        }

        //
        // GET: /LandRegistrationType/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_land_registration_type jntuh_land_registration_type = db.jntuh_land_registration_type.Find(id);
            return View(jntuh_land_registration_type);
        }

        //
        // GET: /LandRegistrationType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /LandRegistrationType/Create

        [HttpPost]
        public ActionResult Create(jntuh_land_registration_type jntuh_land_registration_type)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_land_registration_type
                                where s.landRegistrationType == jntuh_land_registration_type.landRegistrationType
                                select s.landRegistrationType);
                if (rowExist.Count() == 0)
                {
                    db.jntuh_land_registration_type.Add(jntuh_land_registration_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Land Registration Type is already exists. Please enter a different Land Registration Type.";
                } 
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_registration_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_registration_type.updatedBy);
            return View(jntuh_land_registration_type);
        }
        
        //
        // GET: /LandRegistrationType/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_land_registration_type jntuh_land_registration_type = db.jntuh_land_registration_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_registration_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_registration_type.updatedBy);
            return View(jntuh_land_registration_type);
        }

        //
        // POST: /LandRegistrationType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_land_registration_type jntuh_land_registration_type)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_land_registration_type
                                where s.landRegistrationType == jntuh_land_registration_type.landRegistrationType && s.id != jntuh_land_registration_type.id
                                select s.landRegistrationType);
                if (rowExist.Count() == 0)
                {
                    db.Entry(jntuh_land_registration_type).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_land_registration_type);
                }
                else
                {
                    TempData["Error"] = "Land Registration Type is already exists. Please enter a different Land Registration Type.";
                    return View(jntuh_land_registration_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_registration_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_registration_type.updatedBy);
            return View(jntuh_land_registration_type);
        }

        //
        // GET: /LandRegistrationType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_land_registration_type jntuh_land_registration_type = db.jntuh_land_registration_type.Find(id);
            return View(jntuh_land_registration_type);
        }

        //
        // POST: /LandRegistrationType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_land_registration_type jntuh_land_registration_type = db.jntuh_land_registration_type.Find(id);
            db.jntuh_land_registration_type.Remove(jntuh_land_registration_type);
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