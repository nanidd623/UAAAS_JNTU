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
    public class CollegeExpenditureTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /CollegeExpenditureType/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_college_expenditure_type = db.jntuh_college_expenditure_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_college_expenditure_type.ToList());
        }

        //
        // GET: /CollegeExpenditureType/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_college_expenditure_type jntuh_college_expenditure_type = db.jntuh_college_expenditure_type.Find(id);
            return View(jntuh_college_expenditure_type);
        }

        //
        // GET: /CollegeExpenditureType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /CollegeExpenditureType/Create

        [HttpPost]
        public ActionResult Create(jntuh_college_expenditure_type jntuh_college_expenditure_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_college_expenditure_type
                                 where s.expenditure == jntuh_college_expenditure_type.expenditure
                                 select s.expenditure);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_college_expenditure_type.Add(jntuh_college_expenditure_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Expenditure is already exists. Please enter a different Expenditure.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_expenditure_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_expenditure_type.updatedBy);
            return View(jntuh_college_expenditure_type);
        }
        
        //
        // GET: /CollegeExpenditureType/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_college_expenditure_type jntuh_college_expenditure_type = db.jntuh_college_expenditure_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_expenditure_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_expenditure_type.updatedBy);
            return View(jntuh_college_expenditure_type);
        }

        //
        // POST: /CollegeExpenditureType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_college_expenditure_type jntuh_college_expenditure_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_college_expenditure_type
                                 where s.expenditure == jntuh_college_expenditure_type.expenditure && s.id != jntuh_college_expenditure_type.id
                                 select s.expenditure);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_college_expenditure_type).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_college_expenditure_type);
                }
                else
                {
                    TempData["Error"] = "Expenditure is already exists. Please enter a different Expenditure.";
                    return View(jntuh_college_expenditure_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_expenditure_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_expenditure_type.updatedBy);
            return View(jntuh_college_expenditure_type);
        }

        //
        // GET: /CollegeExpenditureType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_college_expenditure_type jntuh_college_expenditure_type = db.jntuh_college_expenditure_type.Find(id);
            return View(jntuh_college_expenditure_type);
        }

        //
        // POST: /CollegeExpenditureType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_college_expenditure_type jntuh_college_expenditure_type = db.jntuh_college_expenditure_type.Find(id);
            db.jntuh_college_expenditure_type.Remove(jntuh_college_expenditure_type);
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