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
    public class EDEPEquipmentController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /EDEPEquipment/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_edep_equipment = db.jntuh_edep_equipment.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_edep_equipment.ToList());
        }

        //
        // GET: /EDEPEquipment/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_edep_equipment jntuh_edep_equipment = db.jntuh_edep_equipment.Find(id);
            return View(jntuh_edep_equipment);
        }

        //
        // GET: /EDEPEquipment/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /EDEPEquipment/Create

        [HttpPost]
        public ActionResult Create(jntuh_edep_equipment jntuh_edep_equipment)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_edep_equipment
                                 where s.equipmentName == jntuh_edep_equipment.equipmentName
                                 select s);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_edep_equipment.Add(jntuh_edep_equipment);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Equipment Name is already exists. Please enter a different Equipment Name.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_edep_equipment.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_edep_equipment.updatedBy);
            return View(jntuh_edep_equipment);
        }
        
        //
        // GET: /EDEPEquipment/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_edep_equipment jntuh_edep_equipment = db.jntuh_edep_equipment.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_edep_equipment.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_edep_equipment.updatedBy);
            return View(jntuh_edep_equipment);
        }

        //
        // POST: /EDEPEquipment/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_edep_equipment jntuh_edep_equipment)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_edep_equipment
                                 where s.equipmentName == jntuh_edep_equipment.equipmentName && s.id != jntuh_edep_equipment.id
                                 select s);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_edep_equipment).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_edep_equipment);
                }
                else
                {
                    TempData["Error"] = "Equipment Name is already exists. Please enter a different Equipment Name.";
                    return View(jntuh_edep_equipment);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_edep_equipment.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_edep_equipment.updatedBy);
            return View(jntuh_edep_equipment);
        }

        //
        // GET: /EDEPEquipment/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_edep_equipment jntuh_edep_equipment = db.jntuh_edep_equipment.Find(id);
            return View(jntuh_edep_equipment);
        }

        //
        // POST: /EDEPEquipment/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_edep_equipment jntuh_edep_equipment = db.jntuh_edep_equipment.Find(id);
            db.jntuh_edep_equipment.Remove(jntuh_edep_equipment);
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