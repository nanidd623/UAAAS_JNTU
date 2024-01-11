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
    public class LandTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /LandType/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_land_type = db.jntuh_land_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_land_type.ToList());
        }

        //
        // GET: /LandType/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_land_type jntuh_land_type = db.jntuh_land_type.Find(id);
            return View(jntuh_land_type);
        }

        //
        // GET: /LandType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /LandType/Create

        [HttpPost]
        public ActionResult Create(jntuh_land_type jntuh_land_type)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_land_type
                                where s.landType == jntuh_land_type.landType
                                select s.landType);
                if (rowExist.Count() == 0)
                {
                    db.jntuh_land_type.Add(jntuh_land_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Land Type is already exists. Please enter a different Land Type.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_type.updatedBy);
            return View(jntuh_land_type);
        }
        
        //
        // GET: /LandType/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_land_type jntuh_land_type = db.jntuh_land_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_type.updatedBy);
            return View(jntuh_land_type);
        }

        //
        // POST: /LandType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_land_type jntuh_land_type)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_land_type
                                where s.landType == jntuh_land_type.landType && s.id != jntuh_land_type.id 
                                select s.landType);
                if (rowExist.Count() == 0)
                {
                    db.Entry(jntuh_land_type).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_land_type);
                }
                else
                {
                    TempData["Error"] = "Land Type is already exists. Please enter a different Land Type.";
                    return View(jntuh_land_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_land_type.updatedBy);
            return View(jntuh_land_type);
        }

        //
        // GET: /LandType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_land_type jntuh_land_type = db.jntuh_land_type.Find(id);
            return View(jntuh_land_type);
        }

        //
        // POST: /LandType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_land_type jntuh_land_type = db.jntuh_land_type.Find(id);
            db.jntuh_land_type.Remove(jntuh_land_type);
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