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
    public class WaterTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /WaterType/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_water_type = db.jntuh_water_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_water_type.ToList());
        }

        //
        // GET: /WaterType/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_water_type jntuh_water_type = db.jntuh_water_type.Find(id);
            return View(jntuh_water_type);
        }

        //
        // GET: /WaterType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /WaterType/Create

        [HttpPost]
        public ActionResult Create(jntuh_water_type jntuh_water_type)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_water_type
                                where s.waterType == jntuh_water_type.waterType
                                select s.waterType);
                if (rowExist.Count() == 0)
                {
                    db.jntuh_water_type.Add(jntuh_water_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Water Type is already exists. Please enter a different Water Type.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_water_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_water_type.updatedBy);
            return View(jntuh_water_type);
        }
        
        //
        // GET: /WaterType/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_water_type jntuh_water_type = db.jntuh_water_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_water_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_water_type.updatedBy);
            return View(jntuh_water_type);
        }

        //
        // POST: /WaterType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_water_type jntuh_water_type)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_water_type
                                where s.waterType == jntuh_water_type.waterType && s.id != jntuh_water_type.id
                                select s.waterType);
                if (rowExist.Count() == 0)
                {
                    db.Entry(jntuh_water_type).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_water_type);
                }
                else
                {
                    TempData["Error"] = "Water Type is already exists. Please enter a different Water Type.";
                    return View(jntuh_water_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_water_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_water_type.updatedBy);
            return View(jntuh_water_type);
        }

        //
        // GET: /WaterType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_water_type jntuh_water_type = db.jntuh_water_type.Find(id);
            return View(jntuh_water_type);
        }

        //
        // POST: /WaterType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_water_type jntuh_water_type = db.jntuh_water_type.Find(id);
            db.jntuh_water_type.Remove(jntuh_water_type);
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