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
    public class DistrictController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /District/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_district = db.jntuh_district.Include(j => j.jntuh_state).Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            ViewBag.StateName = (from s in db.jntuh_state
                                 where s.isActive == true
                                 select s).ToList();
            return View(jntuh_district.ToList());
        }

        //
        // GET: /District/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_district jntuh_district = db.jntuh_district.Find(id);
            ViewBag.StateName = (from s in db.jntuh_state
                                 where s.isActive == true
                                 select s).ToList();
            return View(jntuh_district);
        }

        //
        // GET: /District/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            //ViewBag.stateId = new SelectList(db.jntuh_state, "id", "stateName");
            ViewBag.StateName = (from s in db.jntuh_state
                                 where s.isActive == true
                                 select s).ToList();
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /District/Create

        //[HttpPost]
        //public ActionResult Create(jntuh_district jntuh_district)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.jntuh_district.Add(jntuh_district);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");  
        //    }

        //    ViewBag.stateId = new SelectList(db.jntuh_state, "id", "stateName", jntuh_district.stateId);
        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_district.createdBy);
        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_district.updatedBy);
        //    return View(jntuh_district);
        //}


        [HttpPost]
        public ActionResult Create(jntuh_district jntuh_district)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_district
                                 where s.districtName == jntuh_district.districtName
                                 select s.districtName);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_district.Add(jntuh_district);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "District Name is already exists. Please enter a different District Name.";
                }
            }

            //ViewBag.stateId = new SelectList(db.jntuh_state, "id", "stateName", jntuh_district.stateId);
            ViewBag.StateName = (from s in db.jntuh_state
                                 where s.isActive == true
                                 select s).ToList();
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_district.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_district.updatedBy);
            return View(jntuh_district);
        }

        //
        // GET: /District/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_district jntuh_district = db.jntuh_district.Find(id);
            //ViewBag.stateId = new SelectList(db.jntuh_state, "id", "stateName", jntuh_district.stateId);
            ViewBag.StateName = (from s in db.jntuh_state
                                 where s.isActive == true
                                 select s).ToList();
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_district.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_district.updatedBy);
            return View(jntuh_district);
        }

        //
        // POST: /District/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_district jntuh_district)
        {
            ViewBag.StateName = (from s in db.jntuh_state
                                 where s.isActive == true
                                 select s).ToList();
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_district.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_district.updatedBy);
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_district
                                 where s.districtName == jntuh_district.districtName && s.id !=jntuh_district.id
                                 select s.districtName);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_district).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_district);
                }
                else
                {
                    TempData["Error"] = "District Name is already exists. Please enter a different District Name.";
                    return View(jntuh_district);
                }
            }
            //ViewBag.stateId = new SelectList(db.jntuh_state, "id", "stateName", jntuh_district.stateId);
            
            return View(jntuh_district);
        }

        //
        // GET: /District/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_district jntuh_district = db.jntuh_district.Find(id);
            return View(jntuh_district);
        }

        //
        // POST: /District/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_district jntuh_district = db.jntuh_district.Find(id);
            db.jntuh_district.Remove(jntuh_district);
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