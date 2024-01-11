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
    public class CollegeStatusController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /CollegeStatus/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_college_status = db.jntuh_college_status.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_college_status.ToList());
        }

        //
        // GET: /CollegeStatus/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_college_status jntuh_college_status = db.jntuh_college_status.Find(id);
            return View(jntuh_college_status);
        }

        //
        // GET: /CollegeStatus/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /CollegeStatus/Create

        [HttpPost]
        public ActionResult Create(jntuh_college_status jntuh_college_status)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_college_status
                                 where s.collegeStatus == jntuh_college_status.collegeStatus
                                 select s.collegeStatus);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_college_status.Add(jntuh_college_status);
                    db.SaveChanges();

                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "College Status is already exists. Please enter a different College Status.";
                }
                //db.jntuh_college_status.Add(jntuh_college_status);
                //db.SaveChanges();
                //return RedirectToAction("Index");  
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_status.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_status.updatedBy);
            return View(jntuh_college_status);

           
        }
        
        //
        // GET: /CollegeStatus/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")] 
        public ActionResult Edit(int id)
        {
            jntuh_college_status jntuh_college_status = db.jntuh_college_status.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_status.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_status.updatedBy);
            return View(jntuh_college_status);
        }

        //
        // POST: /CollegeStatus/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_college_status jntuh_college_status)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_college_status
                                 where s.collegeStatus == jntuh_college_status.collegeStatus && s.id != jntuh_college_status.id
                                 select s.collegeStatus);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_college_status).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_college_status);
                }
                else
                {
                    //ModelState.AddModelError("", "State Name is already exists. Please enter a different State Name.");
                    TempData["Error"] = "College Status is already exists. Please enter a different College Status.";
                    return View(jntuh_college_status);
                }
                
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_status.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_status.updatedBy);
            return View(jntuh_college_status);
        }

        //
        // GET: /CollegeStatus/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_college_status jntuh_college_status = db.jntuh_college_status.Find(id);
            return View(jntuh_college_status);
        }

        //
        // POST: /CollegeStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_college_status jntuh_college_status = db.jntuh_college_status.Find(id);
            db.jntuh_college_status.Remove(jntuh_college_status);
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