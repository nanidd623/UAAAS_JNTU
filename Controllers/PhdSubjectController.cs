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
    public class PhdSubjectController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /PhdSubject/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_phd_subject = db.jntuh_phd_subject.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_phd_subject.ToList());
        }

        //
        // GET: /PhdSubject/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_phd_subject jntuh_phd_subject = db.jntuh_phd_subject.Find(id);
            return View(jntuh_phd_subject);
        }

        //
        // GET: /PhdSubject/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /PhdSubject/Create

        [HttpPost]
        public ActionResult Create(jntuh_phd_subject jntuh_phd_subject)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_phd_subject
                                where s.phdSubjectName == jntuh_phd_subject.phdSubjectName
                                select s.phdSubjectName);
                if (rowExist.Count() == 0)
                {
                    db.jntuh_phd_subject.Add(jntuh_phd_subject);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Phd Subject Name is already exists. Please enter a different Phd Subject Name.";
                } 
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_phd_subject.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_phd_subject.updatedBy);
            return View(jntuh_phd_subject);
        }
        
        //
        // GET: /PhdSubject/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_phd_subject jntuh_phd_subject = db.jntuh_phd_subject.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_phd_subject.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_phd_subject.updatedBy);
            return View(jntuh_phd_subject);
        }

        //
        // POST: /PhdSubject/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_phd_subject jntuh_phd_subject)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_phd_subject
                                where s.phdSubjectName == jntuh_phd_subject.phdSubjectName && s.id != jntuh_phd_subject.id
                                select s.phdSubjectName);
                if (rowExist.Count() == 0)
                {
                    db.Entry(jntuh_phd_subject).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_phd_subject);
                }
                else
                {
                    TempData["Error"] = "Phd Subject Name is already exists. Please enter a different Phd Subject Name.";
                    return View(jntuh_phd_subject);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_phd_subject.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_phd_subject.updatedBy);
            return View(jntuh_phd_subject);
        }

        //
        // GET: /PhdSubject/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_phd_subject jntuh_phd_subject = db.jntuh_phd_subject.Find(id);
            return View(jntuh_phd_subject);
        }

        //
        // POST: /PhdSubject/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_phd_subject jntuh_phd_subject = db.jntuh_phd_subject.Find(id);
            db.jntuh_phd_subject.Remove(jntuh_phd_subject);
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