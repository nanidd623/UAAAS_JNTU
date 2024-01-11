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
    public class AffiliationTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /AffiliationType/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_affiliation_type = db.jntuh_affiliation_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_affiliation_type.ToList());
        }

        //
        // GET: /AffiliationType/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_affiliation_type jntuh_affiliation_type = db.jntuh_affiliation_type.Find(id);
            return View(jntuh_affiliation_type);
        }

        //
        // GET: /AffiliationType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /AffiliationType/Create

        [HttpPost]
        public ActionResult Create(jntuh_affiliation_type jntuh_affiliation_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_affiliation_type
                                 where s.affiliationType == jntuh_affiliation_type.affiliationType
                                 select s.affiliationType);
                if (rowExists.Count() == 0)
                {
                    //jntuh_state.createdBy = (from user in db.my_aspnet_users
                    //                         where user.name == User.Identity.Name
                    //                         select user.id).FirstOrDefault();
                    //jntuh_state.createdBy = jntuh_state.createdBy.Equals(0) ? null : jntuh_state.createdBy;
                    //jntuh_state.createdBy = db.my_aspnet_users.First(u => u.name == User.Identity.Name).id;

                    db.jntuh_affiliation_type.Add(jntuh_affiliation_type);
                    db.SaveChanges();

                    //ModelState.AddModelError("", "Added successfully.");
                    TempData["Success"] = "Added successfully.";
                    //return RedirectToAction("Index");
                }
                else
                {
                    //ModelState.AddModelError("", "State Name is already exists. Please enter a different State Name.");
                    TempData["Error"] = "Affiliation Type is already exists. Please enter a different Affiliation Type.";
                }
                
               
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_affiliation_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_affiliation_type.updatedBy);
            return View(jntuh_affiliation_type);
        }
        
        //
        // GET: /AffiliationType/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_affiliation_type jntuh_affiliation_type = db.jntuh_affiliation_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_affiliation_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_affiliation_type.updatedBy);
            return View(jntuh_affiliation_type);
        }

        //
        // POST: /AffiliationType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_affiliation_type jntuh_affiliation_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_affiliation_type
                                 where s.affiliationType == jntuh_affiliation_type.affiliationType && s.id != jntuh_affiliation_type.id
                                 select s.affiliationType);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_affiliation_type).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_affiliation_type);
                }
                else
                {
                    TempData["Error"] = "Affiliation Type is already exists. Please enter a different Affiliation Type.";
                    return View(jntuh_affiliation_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_affiliation_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_affiliation_type.updatedBy);
            return View(jntuh_affiliation_type);
        }

        //
        // GET: /AffiliationType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_affiliation_type jntuh_affiliation_type = db.jntuh_affiliation_type.Find(id);
            return View(jntuh_affiliation_type);
        }

        //
        // POST: /AffiliationType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_affiliation_type jntuh_affiliation_type = db.jntuh_affiliation_type.Find(id);
            db.jntuh_affiliation_type.Remove(jntuh_affiliation_type);
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