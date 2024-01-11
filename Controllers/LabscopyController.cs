using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using System.IO;
using DotNetOpenAuth.Messaging;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Threading;

namespace UAAAS.Controllers
{
      [ErrorHandling]
    public class LabscopyController : Controller
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return RedirectToAction("Index", "UnderConstruction");
            //}        
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "Labs");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LAB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "Labs");
            }

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int AY1 = jntuh_academic_year.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();

            var jntuh_departments = db.jntuh_department.Where(d => d.isActive == true).ToList();
            var jntuh_degrees = db.jntuh_degree.Where(de => de.isActive == true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();
            int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && (e.academicYearId == AY1 || e.academicYearId == AY2 || e.academicYearId == AY3)).Select(e => e.specializationId).ToArray();
            var DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();
            var DegreeIds = jntuh_departments.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToArray();

            ViewBag.Degree = jntuh_degrees.Where(e => e.isActive == true && DegreeIds.Contains(e.id)).Select(e => new { DegreeId = e.id, DegreeName = e.degree }).ToList();
            Lab laboratories = new Lab();
          
            return View(laboratories);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult LabsJson(int? DegreeId, int? DepartmentId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            List<jntuh_lab_master_copy> collegeLabMaster = null;
            //List<jntuh_college_laboratories_copy> jntuhcollegelaboratories = null;
            List<Lab> lstlaboratories = new List<Lab>();

            var jntuh_college_laboratories1 = db.jntuh_college_laboratories_copy.AsNoTracking().Where(e => e.CollegeID == userCollegeID).ToList();
            if (DegreeId != 0 && DepartmentId != 0)
            {
                List<jntuh_academic_year> jntuhAcademicyear =
                db.jntuh_academic_year.Where(a => a.isActive == true).Select(s => s).ToList();
                int actualYear = jntuhAcademicyear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

                int AY0 = jntuhAcademicyear.Where(s => s.actualYear == actualYear + 1).Select(s => s.id).FirstOrDefault();
                int AY1 = jntuhAcademicyear.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
                int AY2 = jntuhAcademicyear.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
                int AY3 = jntuhAcademicyear.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();
                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                var jntuh_college_laboratories = jntuh_college_laboratories1.Where(l => l.CollegeID == userCollegeID).ToList();


                if (CollegeAffiliationStatus == "Yes")//autonomous College 
                {
                    //return RedirectToAction("ComingSoon");
                    //collegeLabMaster = "ComingSoon";
                    collegeLabMaster = db.jntuh_lab_master_copy.AsNoTracking().Where(l => l.CollegeId == userCollegeID && l.DegreeID == DegreeId && l.SpecializationID == DepartmentId).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();

                }
                else//non autonomous College
                {

                    //if college Mechanical &Civil Engineering s have specializations shows Engineering Physics Lab(PH105BS) other wise not show.
                    //int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && e.academicYearId == AY3 && e.courseStatus != "Closure").Select(e => e.specializationId).ToArray();
                    int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && (e.academicYearId == AY1 || e.academicYearId == AY2 || e.academicYearId == AY3)).Select(e => e.specializationId).ToArray();
                    if (specializationIDs.Contains(33) || specializationIDs.Contains(43))
                    {
                        collegeLabMaster = db.jntuh_lab_master_copy.AsNoTracking().Where(l => l.DegreeID == DegreeId && l.SpecializationID == DepartmentId && l.CollegeId == null).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();
                    }
                    else
                    {
                        collegeLabMaster = db.jntuh_lab_master_copy.AsNoTracking().Where(l => l.DegreeID == DegreeId && l.SpecializationID == DepartmentId && l.CollegeId == null && l.Labcode != "PH105BS").OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();
                    }
                }
                int[] Labids=collegeLabMaster.Select(l=>l.id).ToArray();

                var degree = db.jntuh_degree.ToList();
                var Departments = db.jntuh_department.ToList();
                var Specealizations = db.jntuh_specialization.ToList();
               // jntuhcollegelaboratories = db.jntuh_college_laboratories_copy.AsNoTracking().Where(cl => Labids.Contains(cl.EquipmentID)).ToList();
                foreach (var item in collegeLabMaster)
                {
                    if (CollegeAffiliationStatus == "Yes")//item.Labcode == "TMP-CL" &&
                    {

                        Lab lstlabs = new Lab();
                        lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
                        lstlabs.EquipmentID = item.id;
                        lstlabs.degree = degree.Where(d=>d.id==item.DegreeID).Select(a=>a.degree).FirstOrDefault();
                        lstlabs.department = Departments.Where(d => d.id == item.DepartmentID).Select(a => a.departmentName).FirstOrDefault(); ;
                        lstlabs.specializationName = Specealizations.Where(d => d.id == item.SpecializationID).Select(a => a.specializationName).FirstOrDefault(); 
                        lstlabs.AffiliationStatus = CollegeAffiliationStatus;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.Year;
                        lstlabs.Labcode = item.Labcode;
                        lstlabs.LabName = item.LabName;
                        lstlabs.EquipmentName = item.EquipmentName;
                        lstlabs.LabEquipmentName = item.EquipmentName;
                        lstlabs.collegeId = userCollegeID;
                        lstlabs.EquipmentNo = 1;
                        //lstlabs.AvailableArea =jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.AvailableArea).FirstOrDefault();
                        //lstlabs.RoomNumber = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.RoomNumber).FirstOrDefault();
                        //lstlabs.Make = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.Make).FirstOrDefault();
                        //lstlabs.Model = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.Model).FirstOrDefault();
                        //lstlabs.EquipmentUniqueID = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentUniqueID).FirstOrDefault();
                        //lstlabs.AvailableUnits = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.AvailableUnits).FirstOrDefault();
                        //lstlabs.EquipmentDateOfPurchasing = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentDateOfPurchasing).FirstOrDefault();
                        //lstlabs.DelivaryChalanaDate = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.DelivaryChalanaDate).FirstOrDefault();
                        lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
                        lstlaboratories.Add(lstlabs);

                    }
                    else
                    {
                        if (item.SpecializationID != 39)//item.Labcode != "TMP-CL" && item.Labcode != "MPCLAB" &&
                        {
                            Lab lstlabs = new Lab();
                            lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
                            lstlabs.EquipmentID = item.id;
                            lstlabs.degree = degree.Where(d => d.id == item.DegreeID).Select(a => a.degree).FirstOrDefault();
                            lstlabs.department = Departments.Where(d => d.id == item.DepartmentID).Select(a => a.departmentName).FirstOrDefault(); ;
                            lstlabs.specializationName = Specealizations.Where(d => d.id == item.SpecializationID).Select(a => a.specializationName).FirstOrDefault(); 
                            lstlabs.Semester = item.Semester;
                            lstlabs.year = item.Year;
                            lstlabs.Labcode = item.Labcode;
                            lstlabs.LabName = item.LabName;
                            lstlabs.EquipmentName = item.EquipmentName;
                            lstlabs.LabEquipmentName = item.EquipmentName;
                            lstlabs.collegeId = userCollegeID;
                            lstlabs.EquipmentNo = 1;
                            //lstlabs.AvailableArea = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.AvailableArea).FirstOrDefault();
                            //lstlabs.RoomNumber = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.RoomNumber).FirstOrDefault();
                            //lstlabs.Make = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.Make).FirstOrDefault();
                            //lstlabs.Model = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.Model).FirstOrDefault();
                            //lstlabs.EquipmentUniqueID = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentUniqueID).FirstOrDefault();
                            //lstlabs.AvailableUnits = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.AvailableUnits).FirstOrDefault();
                            //lstlabs.EquipmentDateOfPurchasing = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentDateOfPurchasing).FirstOrDefault();
                            //lstlabs.DelivaryChalanaDate = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.DelivaryChalanaDate).FirstOrDefault();
                            lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
                            lstlaboratories.Add(lstlabs);
                        }
                        else if (item.DegreeID == 4 && item.SpecializationID == 39)
                        {

                            Lab lstlabs = new Lab();
                            lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
                            lstlabs.EquipmentID = item.id;
                            lstlabs.degree = degree.Where(d => d.id == item.DegreeID).Select(a => a.degree).FirstOrDefault();
                            lstlabs.department = Departments.Where(d => d.id == item.DepartmentID).Select(a => a.departmentName).FirstOrDefault(); ;
                            lstlabs.specializationName = Specealizations.Where(d => d.id == item.SpecializationID).Select(a => a.specializationName).FirstOrDefault();
                            lstlabs.Semester = item.Semester;
                            lstlabs.year = item.Year;
                            lstlabs.Labcode = item.Labcode;
                            lstlabs.LabName = item.LabName;
                            lstlabs.EquipmentName = item.EquipmentName;
                            lstlabs.LabEquipmentName = item.EquipmentName;
                            lstlabs.collegeId = userCollegeID;
                            lstlabs.EquipmentNo = 1;
                            //lstlabs.AvailableArea = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.AvailableArea).FirstOrDefault();
                            //lstlabs.RoomNumber = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.RoomNumber).FirstOrDefault();
                            //lstlabs.Make = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.Make).FirstOrDefault();
                            //lstlabs.Model = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.Model).FirstOrDefault();
                            //lstlabs.EquipmentUniqueID = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentUniqueID).FirstOrDefault();
                            //lstlabs.AvailableUnits = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.AvailableUnits).FirstOrDefault();
                            //lstlabs.EquipmentDateOfPurchasing = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentDateOfPurchasing).FirstOrDefault();
                            //lstlabs.DelivaryChalanaDate = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.DelivaryChalanaDate).FirstOrDefault();
                            lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
                            lstlaboratories.Add(lstlabs);
                        }
                    }
                }
            }
            foreach (var it in lstlaboratories)
            {
                int count = jntuh_college_laboratories1.Where(e => e.id == it.id && e.EquipmentNo == it.EquipmentNo && e.CollegeID == it.collegeId).Select(e => e.id).Count();
                it.ExpCount = count;
            }
            return Json(lstlaboratories.OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ThenBy(l => l.LabName).ToList(), "application/json", JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public JsonResult AddEditRecord(int? id, string collegeId, int? eqpid, int? eqpno)
        {
            string imagepath = "Content/Upload/EquipmentsPhotos";
            string deliverychallana = "Content/Upload/Labs/DeliverychallanDocuments";
            string bankstatement = "Content/Upload/Labs/BankstatementDocuments";
            string stockregisterentry = "Content/Upload/Labs/StockregisterentryDocuments";
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //userCollegeID = 77;
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                    else if (id != null)
                    {
                        userCollegeID = db.jntuh_college_laboratories_copy.Where(i => i.id == id).Select(i => i.CollegeID).FirstOrDefault();
                    }
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }



            Lab laboratories = new Lab();
            laboratories.collegeId = userCollegeID;

            if (Request.IsAjaxRequest())
            {
                if (id != null && id != 0)
                {
                    ViewBag.IsUpdate = true;
                    laboratories = (from m in db.jntuh_lab_master_copy
                                    join labs in db.jntuh_college_laboratories_copy on m.id equals labs.EquipmentID
                                    join degree in db.jntuh_degree on m.DegreeID equals degree.id
                                    join dept in db.jntuh_department on m.DepartmentID equals dept.id
                                    join spe in db.jntuh_specialization on m.SpecializationID equals spe.id
                                    where (labs.CollegeID == userCollegeID && labs.id == id)
                                    select new Lab
                                    {
                                        id = labs.id,
                                        collegeId = userCollegeID,
                                        EquipmentID = labs.EquipmentID,
                                        EquipmentName = m.EquipmentName,
                                        LabEquipmentName = labs.EquipmentName,
                                        EquipmentNo = labs.EquipmentNo,
                                        Make = labs.Make,
                                        Model = labs.Model,
                                        EquipmentUniqueID = labs.EquipmentUniqueID,
                                        AvailableUnits = labs.AvailableUnits,
                                        AvailableArea = labs.AvailableArea,
                                        RoomNumber = labs.RoomNumber,
                                        createdBy = labs.createdBy,
                                        createdOn = labs.createdOn,
                                        IsActive = true,
                                        degreeId = m.DegreeID,
                                        departmentId = m.DepartmentID,
                                        specializationId = m.SpecializationID,
                                        degree = degree.degree,
                                        department =dept.departmentName,
                                        specializationName = spe.specializationName,
                                        year = m.Year,
                                        Semester = m.Semester,
                                        Labcode = m.Labcode,
                                        LabName = m.LabName,
                                        EquipmentDateOfPurchasing = labs.EquipmentDateOfPurchasing,
                                        // EquipmentDateOfPurchasing1 = labs.EquipmentDateOfPurchasing != null ? string.Format("{0:yyyy-MM-dd}", labs.EquipmentDateOfPurchasing.Value) : null

                                        DelivaryChalanaDate = labs.DelivaryChalanaDate,
                                        ViewEquipmentPhoto = labs.EquipmentPhoto
                                        //ViewDelivaryChalanaImage = labs.DelivaryChalanaImage,
                                        //ViewBankStatementImage = labs.BankStatementImage,
                                        //ViewStockRegisterEntryImage =  labs.StockRegisterEntryImage,
                                        //ViewPurchaseOrderImage = labs.PurchaseOrder


                                    }).FirstOrDefault();
                    if (laboratories != null)
                    {
                        laboratories.EquipmentDateOfPurchasing1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing.ToString()) : null;
                        laboratories.DelivaryChalanaDate1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate.ToString()) : null;
                        laboratories.ViewCreatedOn = laboratories.createdOn.ToString();
                    }

                    //  return PartialView("_LaboratoriesData", laboratories);
                    return Json(laboratories, "application/json", JsonRequestBehavior.AllowGet);

                    var pp = db.jntuh_degree.ToList();
                }
                else
                {
                    if (eqpid != null && eqpid != 0)
                    {
                        ViewBag.IsUpdate = false;
                        jntuh_lab_master_copy master = db.jntuh_lab_master_copy.Find(eqpid);
                        if (master != null)
                        {
                            var degree = db.jntuh_degree.ToList();
                            var Departments = db.jntuh_department.ToList();
                            var Specealizations = db.jntuh_specialization.ToList();
                            laboratories.collegeId = userCollegeID;
                            laboratories.degreeId = master.DegreeID;
                            laboratories.degree = degree.Where(d => d.id == master.DegreeID).Select(a => a.degree).FirstOrDefault();
                            laboratories.departmentId = master.DepartmentID;
                            laboratories.department = Departments.Where(d => d.id == master.DepartmentID).Select(a => a.departmentName).FirstOrDefault();
                            laboratories.specializationId = master.SpecializationID;
                            laboratories.specializationName = Specealizations.Where(d => d.id == master.SpecializationID).Select(a => a.specializationName).FirstOrDefault(); 
                            laboratories.year = master.Year;
                            laboratories.LabName = master.LabName;
                            laboratories.EquipmentName = master.EquipmentName;
                            laboratories.EquipmentNo = eqpno;
                            string EIds = master.ExperimentsIds;
                            if (EIds != null && EIds != "")
                                laboratories.EquipmentIds = EIds.Remove(EIds.Length - 1, 1);
                            laboratories.NoofUnits = master.noofUnits;
                            //int? eqno = db.jntuh_college_laboratories.Where(l => l.EquipmentID == eqpid && l.CollegeID == userCollegeID).OrderByDescending(l => l.EquipmentNo).Select(l => l.EquipmentNo).FirstOrDefault();
                            //if (eqno != null)
                            //{
                            //    laboratories.EquipmentNo = eqno + 1;
                            //}
                            //else
                            //{
                            //    laboratories.EquipmentNo = 1;
                            //}
                            laboratories.EquipmentID = master.id;
                            laboratories.Semester = master.Semester;
                            laboratories.Labcode = master.Labcode;
                        }
                        return Json(laboratories, "application/json", JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(laboratories, "application/json", JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    laboratories = (from m in db.jntuh_lab_master_copy
                                    join labs in db.jntuh_college_laboratories_copy on m.id equals labs.EquipmentID
                                    join degree in db.jntuh_degree on m.DegreeID equals degree.id
                                    join dept in db.jntuh_department on m.DepartmentID equals dept.id
                                    join spe in db.jntuh_specialization on m.SpecializationID equals spe.id
                                    where (labs.CollegeID == userCollegeID && labs.id == id)
                                    select new Lab
                                    {
                                        id = labs.id,
                                        collegeId = userCollegeID,
                                        EquipmentID = labs.EquipmentID,
                                        EquipmentName = m.EquipmentName,
                                        LabEquipmentName = labs.EquipmentName,
                                        EquipmentNo = labs.EquipmentNo,
                                        Make = labs.Make,
                                        Model = labs.Model,
                                        EquipmentUniqueID = labs.EquipmentUniqueID,
                                        AvailableUnits = labs.AvailableUnits,
                                        AvailableArea = labs.AvailableArea,
                                        RoomNumber = labs.RoomNumber,
                                        createdBy = labs.createdBy,
                                        createdOn = labs.createdOn,
                                        IsActive = true,

                                        degreeId = m.DegreeID,
                                        departmentId = m.DepartmentID,
                                        specializationId = m.SpecializationID,
                                        degree = degree.degree,
                                        department = dept.departmentName,
                                        specializationName = spe.specializationName,
                                        year = m.Year,
                                        Semester = m.Semester,
                                        Labcode = m.Labcode,
                                        LabName = m.LabName,
                                        EquipmentDateOfPurchasing = labs.EquipmentDateOfPurchasing,
                                        DelivaryChalanaDate = labs.DelivaryChalanaDate,
                                        ViewEquipmentPhoto = labs.EquipmentPhoto,

                                        //ViewDelivaryChalanaImage = labs.DelivaryChalanaImage,
                                        //ViewBankStatementImage = labs.BankStatementImage,
                                        //ViewStockRegisterEntryImage =  labs.StockRegisterEntryImage,
                                        //ViewPurchaseOrderImage =labs.PurchaseOrder

                                    }).FirstOrDefault();
                    laboratories.EquipmentDateOfPurchasing1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing.ToString()) : null;
                    laboratories.DelivaryChalanaDate1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate.ToString()) : null;
                    laboratories.ViewCreatedOn = laboratories.createdOn.ToString();
                }
                else
                {
                    var degree = db.jntuh_degree.ToList();
                    var Departments = db.jntuh_department.ToList();
                    var Specealizations = db.jntuh_specialization.ToList();
                    ViewBag.IsUpdate = false;
                    jntuh_lab_master_copy master = db.jntuh_lab_master_copy.Find(eqpid);
                    laboratories.collegeId = userCollegeID;
                    laboratories.degreeId = master.DegreeID;
                    laboratories.degree = degree.Where(d => d.id == master.DegreeID).Select(a => a.degree).FirstOrDefault();
                    laboratories.departmentId = master.DepartmentID;
                    laboratories.department = Departments.Where(d => d.id == master.DepartmentID).Select(a => a.departmentName).FirstOrDefault();
                    laboratories.specializationId = master.SpecializationID;
                    laboratories.specializationName = Specealizations.Where(d => d.id == master.SpecializationID).Select(a => a.specializationName).FirstOrDefault(); 
                    laboratories.year = master.Year;
                    laboratories.LabName = master.LabName;
                    laboratories.EquipmentName = master.EquipmentName;
                    //int? eqno = db.jntuh_college_laboratories.Where(l => l.EquipmentID == eqpid && l.CollegeID == userCollegeID).OrderByDescending(l => l.EquipmentNo).Select(l => l.EquipmentNo).FirstOrDefault();
                    //if (eqno != null)
                    //{
                    //    laboratories.EquipmentNo = eqno + 1;
                    //}
                    //else
                    //{
                    //    laboratories.EquipmentNo = 1;
                    //}
                    laboratories.EquipmentID = master.id;
                    laboratories.Semester = master.Semester;
                    laboratories.Labcode = master.Labcode;
                    string EIds = master.ExperimentsIds;
                    if (EIds != null && EIds != "")
                        laboratories.EquipmentIds = EIds.Remove(EIds.Length - 1, 1);
                }

                // return View("LaboratoriesData", laboratories);
                return Json(laboratories, "application/json", JsonRequestBehavior.AllowGet);
            }


        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public JsonResult AddEditRecord(Lab laboratories)
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            object jsondata = null;
            if (userdata == null)
            {
                jsondata = "please try after login";
            }
            else
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();


                if (userCollegeID == 0)
                {
                    userCollegeID = laboratories.collegeId;
                }
                if (laboratories.EquipmentUniqueID == null)
                {
                    laboratories.EquipmentUniqueID = string.Empty;
                }
                if (userCollegeID == 375)
                {
                    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                if (ModelState.IsValid)
                {

                    jntuh_college_laboratories_copy jntuh_college_laboratories = new jntuh_college_laboratories_copy();
                    jntuh_college_laboratories.CollegeID = userCollegeID;
                    jntuh_college_laboratories.EquipmentID = laboratories.EquipmentID;
                    jntuh_college_laboratories.LabName = laboratories.LabName;
                    jntuh_college_laboratories.Make = laboratories.Make.Trim();
                    jntuh_college_laboratories.Model = laboratories.Model.Trim();
                    jntuh_college_laboratories.EquipmentUniqueID = laboratories.EquipmentUniqueID;
                    jntuh_college_laboratories.EquipmentName = laboratories.EquipmentName.Length > 250
                        ? laboratories.EquipmentName.Substring(0, 240)
                        : laboratories.EquipmentName;
                    jntuh_college_laboratories.AvailableUnits = laboratories.AvailableUnits;
                    jntuh_college_laboratories.AvailableArea = laboratories.AvailableArea;
                    jntuh_college_laboratories.RoomNumber = laboratories.RoomNumber.Trim();
                    jntuh_college_laboratories.EquipmentNo = laboratories.EquipmentNo;
                    jntuh_college_laboratories.isActive = true;
                    if (laboratories.EquipmentDateOfPurchasing1 != null)
                    {
                        laboratories.EquipmentDateOfPurchasing1 =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing1);
                        jntuh_college_laboratories.EquipmentDateOfPurchasing =
                            Convert.ToDateTime(laboratories.EquipmentDateOfPurchasing1);

                    }

                    if (laboratories.DelivaryChalanaDate1 != null)
                    {
                        laboratories.DelivaryChalanaDate1 =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate1);
                        jntuh_college_laboratories.DelivaryChalanaDate =
                            Convert.ToDateTime(laboratories.DelivaryChalanaDate1);
                    }

                    var fileName = "";
                    string randID = string.Empty;
                    if (laboratories.EquipmentPhoto != null)
                    {
                        // int Id = db.jntuh_college_news.Count() > 0 ? db.jntuh_newsevents.Select(d => d.id).Max() : 0;
                        // Id = Id + 1;
                        // string RamdomCode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeNews.collegeId).Select(r => r.RandamCode).FirstOrDefault();                
                        // Random rnd = new Random();
                        //int RandomNumber = rnd.Next(1000, 9999);
                        //string randID = string.Empty;
                        //// RamdomCode; + Id;
                        const int DelayOnRetry = 3000;
                        try
                        {
                            if (!Directory.Exists(Server.MapPath("~/Content/Upload/EquipmentsPhotos")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/Content/Upload/EquipmentsPhotos"));
                            }

                            var ext = Path.GetExtension(laboratories.EquipmentPhoto.FileName);
                            if (ext.ToUpper().Equals(".GIF") || ext.ToUpper().Equals(".BMP") || ext.ToUpper().Equals(".JPEG") ||
                                ext.ToUpper().Equals(".JPG") || ext.ToUpper().Equals(".PNG"))
                            {
                                string fileName1 = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                   laboratories.EquipmentPhoto.FileName.Substring(0, 1);
                                fileName = userCollegeID + "-" + laboratories.EquipmentID + "-" + fileName1;
                                //string path = Server.MapPath("~/Content/Upload/EquipmentsPhotos/" + DateTime.Now.ToString()+fileName);
                                var PicName = userCollegeID + "-" + laboratories.EquipmentID + "-" + DateTime.Now.ToString() +
                                              "-";
                                laboratories.EquipmentPhoto.SaveAs(string.Format("{0}/{1}{2}",
                                    Server.MapPath("~/Content/Upload/EquipmentsPhotos"), fileName, ext));

                                //jntuh_college_laboratories.EquipmentPhoto = fileName;
                                jntuh_college_laboratories.EquipmentPhoto = string.Format("{0}{1}", fileName, ext);
                            }
                        }
                        catch (IOException e)
                        {
                            Thread.Sleep(DelayOnRetry);
                        }
                    }
                    else
                    {
                        jntuh_college_laboratories.EquipmentPhoto = laboratories.ViewEquipmentPhoto;
                    }

                    






                    if (laboratories.id == 0 || laboratories.id == null)
                    {
                        var existingID =
                            db.jntuh_college_laboratories_copy.Where(
                                c =>
                                    c.CollegeID == userCollegeID && c.EquipmentID == laboratories.EquipmentID &&
                                    c.EquipmentNo == laboratories.EquipmentNo).Select(c => c).FirstOrDefault();

                        if (existingID == null)
                        {
                            jntuh_college_laboratories.createdBy = userID;
                            jntuh_college_laboratories.createdOn = DateTime.Now;
                            db.jntuh_college_laboratories_copy.Add(jntuh_college_laboratories);
                            db.SaveChanges();
                            //try
                            //{
                            //    db.SaveChanges();
                            //}
                            //catch (DbEntityValidationException dbEx)
                            //{
                            //    foreach (var validationErrors in dbEx.EntityValidationErrors)
                            //    {
                            //        foreach (var validationError in validationErrors.ValidationErrors)
                            //        {
                            //            Trace.TraceInformation("Property: {0} Error: {1}",validationError.PropertyName,validationError.ErrorMessage);
                            //        }
                            //    }
                            //}
                            // TempData["Success"] = "Lab Added Successfully.";
                            jsondata = laboratories.EquipmentName + " Added Successfully.";
                        }
                        else
                        {
                            // TempData["Success"] = "Lab already exists.";
                            jsondata = "Lab already exists.";
                        }
                    }
                    else
                    {
                        jntuh_college_laboratories.id = (int)laboratories.id;
                        jntuh_college_laboratories.createdBy = laboratories.createdBy;
                        jntuh_college_laboratories.createdOn = laboratories.createdOn;
                        jntuh_college_laboratories.updatedBy = userID;
                        jntuh_college_laboratories.updatedOn = DateTime.Now;
                        jntuh_college_laboratories.isActive = true;
                        db.Entry(jntuh_college_laboratories).State = EntityState.Modified;
                        db.SaveChanges();
                        //  TempData["Success"] = "Lab Updated Successfully.";
                        jsondata = laboratories.EquipmentName + " Lab Updated Successfully.";
                    }
                }
                else
                {
                    jsondata = "Please Enter All Mandatory Fileds.";
                }
            }

            // return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(), pageNumber = pageNo });
            return Json(new { jsondata }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}
