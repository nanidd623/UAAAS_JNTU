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
    public class InspectionPhaseController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /InspectionPhase/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_inspection_phase = db.jntuh_inspection_phase.Include(j => j.jntuh_academic_year).Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            ViewBag.AcademicYear = (from s in db.jntuh_academic_year
                                    where s.isActive == true
                                    select s).ToList();
            return View(jntuh_inspection_phase.ToList());
        }

        //
        // GET: /InspectionPhase/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_inspection_phase jntuh_inspection_phase = db.jntuh_inspection_phase.Find(id);
            ViewBag.AcademicYear = (from s in db.jntuh_academic_year
                                    where s.isActive == true
                                    select s).ToList().OrderByDescending(s => s.actualYear);
            return View(jntuh_inspection_phase);
        }

        //
        // GET: /InspectionPhase/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            //ViewBag.academicYearId = new SelectList(db.jntuh_academic_year, "id", "academicYear");
            ViewBag.AcademicYear = (from s in db.jntuh_academic_year
                                    where s.isActive == true
                                    select s).ToList().OrderByDescending(s => s.actualYear);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        }

        //
        // POST: /InspectionPhase/Create

        [HttpPost]
        public ActionResult Create(jntuh_inspection_phase jntuh_inspection_phase)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_inspection_phase
                                 where s.inspectionPhase == jntuh_inspection_phase.inspectionPhase
                                 select s.inspectionPhase);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_inspection_phase.Add(jntuh_inspection_phase);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Inspection Phase is already exists. Please enter a different Inspection Phase.";
                }
            }

            //ViewBag.academicYearId = new SelectList(db.jntuh_academic_year, "id", "academicYear", jntuh_inspection_phase.academicYearId);
            ViewBag.AcademicYear = (from s in db.jntuh_academic_year
                                    where s.isActive == true
                                    select s).ToList().OrderByDescending(s => s.actualYear);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_inspection_phase.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_inspection_phase.updatedBy);
            return View(jntuh_inspection_phase);
        }

        //
        // GET: /InspectionPhase/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_inspection_phase jntuh_inspection_phase = db.jntuh_inspection_phase.Find(id);
            //ViewBag.academicYearId = new SelectList(db.jntuh_academic_year, "id", "academicYear", jntuh_inspection_phase.academicYearId);
            ViewBag.AcademicYear = (from s in db.jntuh_academic_year
                                    where s.isActive == true
                                    select s).ToList().OrderByDescending(s => s.actualYear);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_inspection_phase.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_inspection_phase.updatedBy);
            return View(jntuh_inspection_phase);
        }

        //
        // POST: /InspectionPhase/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_inspection_phase jntuh_inspection_phase)
        {
            ViewBag.AcademicYear = (from s in db.jntuh_academic_year
                                    where s.isActive == true
                                    select s).ToList().OrderByDescending(s => s.actualYear);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_inspection_phase.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_inspection_phase.updatedBy);
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_inspection_phase
                                 where s.inspectionPhase == jntuh_inspection_phase.inspectionPhase && s.id != jntuh_inspection_phase.id
                                 select s.inspectionPhase);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_inspection_phase).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_inspection_phase);
                }
                else
                {
                    TempData["Error"] = "Inspection Phase is already exists. Please enter a different Inspection Phase.";
                    return View(jntuh_inspection_phase);
                }
            }
            //ViewBag.academicYearId = new SelectList(db.jntuh_academic_year, "id", "academicYear", jntuh_inspection_phase.academicYearId);

            return View(jntuh_inspection_phase);
        }

        //
        // GET: /InspectionPhase/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_inspection_phase jntuh_inspection_phase = db.jntuh_inspection_phase.Find(id);
            return View(jntuh_inspection_phase);
        }

        //
        // POST: /InspectionPhase/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            jntuh_inspection_phase jntuh_inspection_phase = db.jntuh_inspection_phase.Find(id);
            db.jntuh_inspection_phase.Remove(jntuh_inspection_phase);
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