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
    public class FacilityStatusController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /FacilityStatus/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_facility_status = db.jntuh_facility_status.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_facility_status.ToList());
        }

        //
        // GET: /FacilityStatus/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_facility_status jntuh_facility_status = db.jntuh_facility_status.Find(id);
            return View(jntuh_facility_status);
        }

        //
        // GET: /FacilityStatus/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /FacilityStatus/Create

        [HttpPost]
        public ActionResult Create(jntuh_facility_status jntuh_facility_status)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_facility_status
                                 where s.facilityStatus == jntuh_facility_status.facilityStatus
                                 select s.facilityStatus);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_facility_status.Add(jntuh_facility_status);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Facility Status is already exists. Please enter a different Facility Status.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_facility_status.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_facility_status.updatedBy);
            return View(jntuh_facility_status);
        }
        
        //
        // GET: /FacilityStatus/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_facility_status jntuh_facility_status = db.jntuh_facility_status.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_facility_status.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_facility_status.updatedBy);
            return View(jntuh_facility_status);
        }

        //
        // POST: /FacilityStatus/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_facility_status jntuh_facility_status)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_facility_status
                                 where s.facilityStatus == jntuh_facility_status.facilityStatus && s.id != jntuh_facility_status.id
                                 select s.facilityStatus);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_facility_status).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_facility_status);
                }
                else
                {
                    TempData["Error"] = "Facility Status is already exists. Please enter a different Facility Status.";
                    return View(jntuh_facility_status);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_facility_status.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_facility_status.updatedBy);
            return View(jntuh_facility_status);
        }

        //
        // GET: /FacilityStatus/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_facility_status jntuh_facility_status = db.jntuh_facility_status.Find(id);
            return View(jntuh_facility_status);
        }

        //
        // POST: /FacilityStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_facility_status jntuh_facility_status = db.jntuh_facility_status.Find(id);
            db.jntuh_facility_status.Remove(jntuh_facility_status);
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