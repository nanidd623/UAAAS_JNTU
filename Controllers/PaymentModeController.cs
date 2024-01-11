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
    public class PaymentModeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /PaymentMode/
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ViewResult Index()
        //{
        //    var jntuh_payment_mode = db.jntuh_payment_mode.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
        //    return View(jntuh_payment_mode.ToList());
        //}

        //
        // GET: /PaymentMode/Details/5
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ViewResult Details(int id)
        //{
        //    jntuh_payment_mode jntuh_payment_mode = db.jntuh_payment_mode.Find(id);
        //    return View(jntuh_payment_mode);
        //}

        //
        // GET: /PaymentMode/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /PaymentMode/Create

        //[HttpPost]
        //public ActionResult Create(jntuh_payment_mode jntuh_payment_mode)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var rowExists = (from s in db.jntuh_payment_mode
        //                         where s.paymentMode == jntuh_payment_mode.paymentMode
        //                         select s.paymentMode);
        //        if (rowExists.Count() == 0)
        //        {
        //            db.jntuh_payment_mode.Add(jntuh_payment_mode);
        //            db.SaveChanges();
        //            TempData["Success"] = "Added successfully.";
        //        }
        //        else
        //        {
        //            TempData["Error"] = "Payment Mode is already exists. Please enter a different Payment Mode.";
        //        }
        //    }

        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_payment_mode.updatedBy);
        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_payment_mode.createdBy);
        //    return View(jntuh_payment_mode);
        //}
        
        //
        // GET: /PaymentMode/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        //public ActionResult Edit(int id)
        //{
        //    jntuh_payment_mode jntuh_payment_mode = db.jntuh_payment_mode.Find(id);
        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_payment_mode.updatedBy);
        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_payment_mode.createdBy);
        //    return View(jntuh_payment_mode);
        //}

        //
        // POST: /PaymentMode/Edit/5

        //[HttpPost]
        //public ActionResult Edit(jntuh_payment_mode jntuh_payment_mode)
        //{
        //    if (ModelState.IsValid)
        //    {
        //         var rowExists = (from s in db.jntuh_payment_mode
        //                         where s.paymentMode == jntuh_payment_mode.paymentMode && s.id != jntuh_payment_mode.id
        //                         select s.paymentMode);
        //         if (rowExists.Count() == 0)
        //         {
        //             db.Entry(jntuh_payment_mode).State = EntityState.Modified;
        //             db.SaveChanges();
        //             TempData["Success"] = "Updated successfully.";
        //             return View(jntuh_payment_mode);
        //         }
        //         else
        //         {
        //             TempData["Error"] = "Payment Mode is already exists. Please enter a different Payment Mode.";
        //             return View(jntuh_payment_mode);
        //         }
        //    }
        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_payment_mode.updatedBy);
        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_payment_mode.createdBy);
        //    return View(jntuh_payment_mode);
        //}

        //
        // GET: /PaymentMode/Delete/5
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ActionResult Delete(int id)
        //{
        //    jntuh_payment_mode jntuh_payment_mode = db.jntuh_payment_mode.Find(id);
        //    return View(jntuh_payment_mode);
        //}

        //
        // POST: /PaymentMode/Delete/5

        //[HttpPost, ActionName("Delete")]
        //public ActionResult DeleteConfirmed(int id)
        //{            
        //    jntuh_payment_mode jntuh_payment_mode = db.jntuh_payment_mode.Find(id);
        //    db.jntuh_payment_mode.Remove(jntuh_payment_mode);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}