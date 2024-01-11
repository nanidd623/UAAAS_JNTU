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
    public class CollegeIncomeTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /CollegeIncomeType/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_college_income_type = db.jntuh_college_income_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_college_income_type.ToList());
        }

        //
        // GET: /CollegeIncomeType/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_college_income_type jntuh_college_income_type = db.jntuh_college_income_type.Find(id);
            return View(jntuh_college_income_type);
        }

        //
        // GET: /CollegeIncomeType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /CollegeIncomeType/Create

        [HttpPost]
        public ActionResult Create(jntuh_college_income_type jntuh_college_income_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_college_income_type
                                 where s.sourceOfIncome == jntuh_college_income_type.sourceOfIncome
                                 select s.sourceOfIncome);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_college_income_type.Add(jntuh_college_income_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Source Of Income is already exists. Please enter a different Source Of Income.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_income_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_income_type.updatedBy);
            return View(jntuh_college_income_type);
        }
        
        //
        // GET: /CollegeIncomeType/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_college_income_type jntuh_college_income_type = db.jntuh_college_income_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_income_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_income_type.updatedBy);
            return View(jntuh_college_income_type);
        }

        //
        // POST: /CollegeIncomeType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_college_income_type jntuh_college_income_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_college_income_type
                                 where s.sourceOfIncome == jntuh_college_income_type.sourceOfIncome && s.id != jntuh_college_income_type.id
                                 select s.sourceOfIncome);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_college_income_type).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_college_income_type);
                }
                else
                {
                    TempData["Error"] = "Source Of Income is already exists. Please enter a different Source Of Income.";
                    return View(jntuh_college_income_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_income_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_college_income_type.updatedBy);
            return View(jntuh_college_income_type);
        }

        //
        // GET: /CollegeIncomeType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_college_income_type jntuh_college_income_type = db.jntuh_college_income_type.Find(id);
            return View(jntuh_college_income_type);
        }

        //
        // POST: /CollegeIncomeType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_college_income_type jntuh_college_income_type = db.jntuh_college_income_type.Find(id);
            db.jntuh_college_income_type.Remove(jntuh_college_income_type);
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