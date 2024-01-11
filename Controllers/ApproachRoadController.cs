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
    public class ApproachRoadController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /ApproachRoad/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_approach_road = db.jntuh_approach_road.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_approach_road.ToList());
        }

        //
        // GET: /ApproachRoad/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_approach_road jntuh_approach_road = db.jntuh_approach_road.Find(id);
            return View(jntuh_approach_road);
        }

        //
        // GET: /ApproachRoad/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /ApproachRoad/Create

        [HttpPost]
        public ActionResult Create(jntuh_approach_road jntuh_approach_road)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_approach_road
                                 where s.approachRoadType == jntuh_approach_road.approachRoadType
                                 select s.approachRoadType);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_approach_road.Add(jntuh_approach_road);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Approach Road Type is already exists. Please enter a different Approach Road Type.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_approach_road.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_approach_road.updatedBy);
            return View(jntuh_approach_road);
        }
        
        //
        // GET: /ApproachRoad/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_approach_road jntuh_approach_road = db.jntuh_approach_road.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_approach_road.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_approach_road.updatedBy);
            return View(jntuh_approach_road);
        }

        //
        // POST: /ApproachRoad/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_approach_road jntuh_approach_road)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_approach_road
                                 where s.approachRoadType == jntuh_approach_road.approachRoadType && s.id != jntuh_approach_road.id
                                 select s.approachRoadType);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_approach_road).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_approach_road);
                }
                else
                {
                    TempData["Error"] = "Approach Road Type is already exists. Please enter a different Approach Road Type.";
                    return View(jntuh_approach_road);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_approach_road.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_approach_road.updatedBy);
            return View(jntuh_approach_road);
        }

        //
        // GET: /ApproachRoad/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_approach_road jntuh_approach_road = db.jntuh_approach_road.Find(id);
            return View(jntuh_approach_road);
        }

        //
        // POST: /ApproachRoad/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_approach_road jntuh_approach_road = db.jntuh_approach_road.Find(id);
            db.jntuh_approach_road.Remove(jntuh_approach_road);
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