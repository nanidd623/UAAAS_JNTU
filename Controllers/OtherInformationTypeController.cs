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
    public class OtherInformationTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        //// GET: /OtherInformationType/
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ViewResult Index()
        //{
        //    var jntuh_other_information_type = db.jntuh_other_information_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
        //    return View(jntuh_other_information_type.ToList());
        //}

        ////
        //// GET: /OtherInformationType/Details/5
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ViewResult Details(int id)
        //{
        //    jntuh_other_information_type jntuh_other_information_type = db.jntuh_other_information_type.Find(id);
        //    return View(jntuh_other_information_type);
        //}

        ////
        //// GET: /OtherInformationType/Create
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ActionResult Create()
        //{
        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
        //    return View();
        //} 

        ////
        //// POST: /OtherInformationType/Create

        //[HttpPost]
        //public ActionResult Create(jntuh_other_information_type jntuh_other_information_type)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var rowExists = (from s in db.jntuh_other_information_type
        //                         where s.scannedDocumentType == jntuh_other_information_type.scannedDocumentType
        //                         select s.scannedDocumentType);
        //        if (rowExists.Count() == 0)
        //        {
        //            db.jntuh_other_information_type.Add(jntuh_other_information_type);
        //            db.SaveChanges();
        //            TempData["Success"] = "Added successfully.";
        //        }
        //        else
        //        {
        //            TempData["Error"] = "Scanned Document Type is already exists. Please enter a different Scanned Document Type.";
        //        }
        //    }

        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_other_information_type.createdBy);
        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_other_information_type.updatedBy);
        //    return View(jntuh_other_information_type);
        //}
        
        ////
        //// GET: /OtherInformationType/Edit/5
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ActionResult Edit(int id)
        //{
        //    jntuh_other_information_type jntuh_other_information_type = db.jntuh_other_information_type.Find(id);
        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_other_information_type.createdBy);
        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_other_information_type.updatedBy);
        //    return View(jntuh_other_information_type);
        //}

        ////
        //// POST: /OtherInformationType/Edit/5

        //[HttpPost]
        //public ActionResult Edit(jntuh_other_information_type jntuh_other_information_type)
        //{
        //    if (ModelState.IsValid)
        //    {
        //         var rowExists = (from s in db.jntuh_other_information_type
        //                         where s.scannedDocumentType == jntuh_other_information_type.scannedDocumentType && s.id != jntuh_other_information_type.id
        //                         select s.scannedDocumentType);
        //         if (rowExists.Count() == 0)
        //         {
        //             db.Entry(jntuh_other_information_type).State = EntityState.Modified;
        //             db.SaveChanges();
        //             TempData["Success"] = "Updated successfully.";
        //             return View(jntuh_other_information_type);
        //         }
        //         else
        //         {
        //             TempData["Error"] = "Scanned Document Type is already exists. Please enter a different Scanned Document Type.";
        //             return View(jntuh_other_information_type);
        //         }
        //    }
        //    ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_other_information_type.createdBy);
        //    ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_other_information_type.updatedBy);
        //    return View(jntuh_other_information_type);
        //}

        ////
        //// GET: /OtherInformationType/Delete/5
        //[Authorize(Roles = "Admin, SuperAdmin")]
        //public ActionResult Delete(int id)
        //{
        //    jntuh_other_information_type jntuh_other_information_type = db.jntuh_other_information_type.Find(id);
        //    return View(jntuh_other_information_type);
        //}

        ////
        //// POST: /OtherInformationType/Delete/5

        //[HttpPost, ActionName("Delete")]
        //public ActionResult DeleteConfirmed(int id)
        //{            
        //    jntuh_other_information_type jntuh_other_information_type = db.jntuh_other_information_type.Find(id);
        //    db.jntuh_other_information_type.Remove(jntuh_other_information_type);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}
    }
}