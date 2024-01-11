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
    public class TransportTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /TransportType/
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ViewResult Index()
        //{
        //    var jntuh_transport_type = db.jntuh_transport_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
        //    return View(jntuh_transport_type.ToList());
        //}

        //
        // GET: /TransportType/Details/5
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ViewResult Details(int id)
        //{
        //    jntuh_transport_type jntuh_transport_type = db.jntuh_transport_type.Find(id);
        //    return View(jntuh_transport_type);
        //}

        //
        // GET: /TransportType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /TransportType/Create

        //[HttpPost]
        //public ActionResult Create(jntuh_transport_type jntuh_transport_type)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var rowExists = (from s in db.jntuh_transport_type
        //                         where s.transportType == jntuh_transport_type.transportType
        //                         select s.transportType);
        //        if(rowExists.Count()==0)
        //        {
        //        db.jntuh_transport_type.Add(jntuh_transport_type);
        //        db.SaveChanges();
        //        TempData["Success"] = "Added successfully.";
        //    }
        //    else
        //    {
        //        TempData["Error"] = "Transport Type is already exists. Please enter a different Transport Type.";
        //    }
        //    }

        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_transport_type.createdBy);
        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_transport_type.updatedBy);
        //    return View(jntuh_transport_type);
        //}
        
        //
        // GET: /TransportType/Edit/5
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ActionResult Edit(int id)
        //{
        //    jntuh_transport_type jntuh_transport_type = db.jntuh_transport_type.Find(id);
        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_transport_type.createdBy);
        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_transport_type.updatedBy);
        //    return View(jntuh_transport_type);
        //}

        //
        // POST: /TransportType/Edit/5

        //[HttpPost]
        //public ActionResult Edit(jntuh_transport_type jntuh_transport_type)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var rowExists = (from s in db.jntuh_transport_type
        //                         where s.transportType == jntuh_transport_type.transportType && s.id != jntuh_transport_type.id
        //                         select s.transportType);
        //        if (rowExists.Count() == 0)
        //        {
        //            db.Entry(jntuh_transport_type).State = EntityState.Modified;
        //            db.SaveChanges();
        //            TempData["Success"] = "Updated successfully.";
        //            return View(jntuh_transport_type);
        //        }
        //        else
        //        {
        //            TempData["Error"] = "Transport Type is already exists. Please enter a different Transport Type.";
        //            return View(jntuh_transport_type);
        //        }
        //    }
        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_transport_type.createdBy);
        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_transport_type.updatedBy);
        //    return View(jntuh_transport_type);
        //}

        //
        // GET: /TransportType/Delete/5
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ActionResult Delete(int id)
        //{
        //    jntuh_transport_type jntuh_transport_type = db.jntuh_transport_type.Find(id);
        //    return View(jntuh_transport_type);
        //}

        //
        // POST: /TransportType/Delete/5

        //[HttpPost, ActionName("Delete")]
        //public ActionResult DeleteConfirmed(int id)
        //{            
        //    jntuh_transport_type jntuh_transport_type = db.jntuh_transport_type.Find(id);
        //    db.jntuh_transport_type.Remove(jntuh_transport_type);
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