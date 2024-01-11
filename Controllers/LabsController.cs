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
    public class LabsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)//int? pageNumber by suresh
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
            //int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY1 = jntuh_academic_year.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();

            var jntuh_departments = db.jntuh_department.Where(d=>d.isActive==true).ToList();
            var jntuh_degrees = db.jntuh_degree.Where(de=>de.isActive==true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(s=>s.isActive==true).ToList();



            int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && (e.academicYearId == AY1 || e.academicYearId == AY2 || e.academicYearId == AY3)).Select(e => e.specializationId).ToArray();
            //int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && e.academicYearId == AY3 && e.courseStatus != "Closure").Select(e => e.specializationId).ToArray();


            var DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            // var Dapartmentdetails =jntuh_departments.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => new {DeptId = e.id, DeptName = e.departmentName}).ToList();

            var DegreeIds = jntuh_departments.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToArray();

            ViewBag.Degree = jntuh_degrees.Where(e => e.isActive == true && DegreeIds.Contains(e.id)).Select(e => new { DegreeId = e.id, DegreeName = e.degree }).ToList();



            Lab laboratories = new Lab();




            #region Commented by suresh


            //// int count = specializationIDs.Count() + 1;
            //// specializationIDs[count] = 34;
            //List<jntuh_lab_master> collegeLabMaster = null;
            //int[] DegreeIDs = db.jntuh_lab_master.AsNoTracking().Where(l => l.DegreeID == 4 && specializationIDs.Contains(l.SpecializationID)).Select(l => l.DegreeID).ToArray();
            //if (DegreeIDs.Contains(4))
            //{
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == 39 || specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();

            //}
            //else
            //{
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();

            //}

            //List<Lab> lstlaboratories = new List<Lab>();

            //List<Lab> lstlaboratories1 = new List<Lab>();
            ////if (Session["CollegeLabs"] == null)
            ////{
            //int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);

            //var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == userCollegeID).ToList();
            //int totalPages = 1;
            //int totalLabs = collegeLabMaster.Count();
            //int First = 0;
            //int second = 0;
            ////if (totalLabs > 100)
            ////{
            ////    totalPages = totalLabs / 100;
            ////    if (totalLabs > 100 * totalPages)
            ////    {
            ////        totalPages = totalPages + 1;
            ////    }
            ////}
            ////if (pageNumber == null)
            ////{
            ////    First = 0;
            ////    second = 100;
            ////}
            ////else if (pageNumber > 1)
            ////{
            ////    First = ((pageNumber ?? default(int)) * 100) - 100;

            ////    //second = (pageNumber ?? default(int)) * 100;
            ////    second = 100;
            ////    int Total = collegeLabMaster.Count;
            ////    if (Total <= ((pageNumber ?? default(int)) * 100))
            ////    {

            ////        second = Total - First;
            ////    }
            ////}

            ////int[] total = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).Select(L=>L.id).ToArray(); ;
            ////var firstNumbersLessThan6 = total.TakeWhile(n => n >=First && n<=second);

            //ViewBag.Pages = totalPages;
            //collegeLabMaster = collegeLabMaster.GetRange(First, totalLabs > 100 ? second : totalLabs);
            //foreach (var item in collegeLabMaster)
            //{


            //    //if (CollegeAffiliationStatus == "Yes")
            //    //item.jntuh_degree.degree != "B.Tech" && item.jntuh_degree.degree != "B.Pharmacy"
            //    if (item.Labcode == "MPCLAB")
            //    {
            //        if (CollegeAffiliationStatus == "Yes")
            //        {
            //        }
            //        else
            //        {


            //        }
            //    }
            //    // Commented By Srinivas because  In DB Lab code TMP-CL Only one record is there based up on Specialization but in frent end shows 5 records so if update any one of lab data its reflecet to all labs and based up on specialization 
            //    //if (item.jntuh_degree.degree != "B.Tech" && item.jntuh_degree.degree != "B.Pharmacy")
            //    //{

            //    if (item.Labcode == "TMP-CL" && CollegeAffiliationStatus == "Yes")
            //    {
            //        //for (int i = 1; i <= PGEquipmentCount; i++)
            //        //{
            //        Lab lstlabs = new Lab();
            //        //lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
            //        lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
            //        lstlabs.EquipmentID = item.id;
            //        lstlabs.degree = item.jntuh_degree.degree;
            //        lstlabs.AffiliationStatus = CollegeAffiliationStatus;
            //        lstlabs.department = item.jntuh_department.departmentName;
            //        lstlabs.specializationName = item.jntuh_specialization.specializationName;
            //        lstlabs.Semester = item.Semester;
            //        lstlabs.year = item.Year;
            //        lstlabs.Labcode = item.Labcode;
            //        lstlabs.LabName = item.LabName;
            //        lstlabs.EquipmentName = item.EquipmentName;
            //        lstlabs.LabEquipmentName = item.EquipmentName;
            //        lstlabs.collegeId = userCollegeID;
            //        lstlabs.EquipmentNo = 1;
            //        lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
            //        lstlaboratories.Add(lstlabs);
            //        //}
            //    }
            //    //}
            //    else
            //    {
            //        if (item.Labcode != "TMP-CL" && item.Labcode != "MPCLAB" && item.SpecializationID != 39)
            //        {
            //            Lab lstlabs = new Lab();
            //            lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
            //            lstlabs.EquipmentID = item.id;
            //            lstlabs.degree = item.jntuh_degree.degree;
            //            lstlabs.department = item.jntuh_department.departmentName;
            //            lstlabs.specializationName = item.jntuh_specialization.specializationName;
            //            lstlabs.Semester = item.Semester;
            //            lstlabs.year = item.Year;
            //            lstlabs.Labcode = item.Labcode;
            //            lstlabs.LabName = item.LabName;
            //            lstlabs.EquipmentName = item.EquipmentName;
            //            lstlabs.LabEquipmentName = item.EquipmentName;
            //            lstlabs.collegeId = userCollegeID;
            //            lstlabs.EquipmentNo = 1;
            //            lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
            //            lstlaboratories.Add(lstlabs);
            //        }

            //        else if (item.DegreeID == 4 && item.SpecializationID == 39)
            //        {

            //            Lab lstlabs = new Lab();
            //            lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
            //            lstlabs.EquipmentID = item.id;
            //            lstlabs.degree = item.jntuh_degree.degree;
            //            lstlabs.department = item.jntuh_department.departmentName;
            //            lstlabs.specializationName = item.jntuh_specialization.specializationName;
            //            lstlabs.Semester = item.Semester;
            //            lstlabs.year = item.Year;
            //            lstlabs.Labcode = item.Labcode;
            //            lstlabs.LabName = item.LabName;
            //            lstlabs.EquipmentName = item.EquipmentName;
            //            lstlabs.LabEquipmentName = item.EquipmentName;
            //            lstlabs.collegeId = userCollegeID;
            //            lstlabs.EquipmentNo = 1;
            //            lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
            //            lstlaboratories.Add(lstlabs);
            //        }
            //        //else
            //        //{
            //        //    Lab lstlabs = new Lab();
            //        //    lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
            //        //    lstlabs.EquipmentID = item.id;
            //        //    lstlabs.degree = item.jntuh_degree.degree;
            //        //    lstlabs.department = item.jntuh_department.departmentName;
            //        //    lstlabs.specializationName = item.jntuh_specialization.specializationName;
            //        //    lstlabs.Semester = item.Semester;
            //        //    lstlabs.year = item.Year;
            //        //    lstlabs.Labcode = item.Labcode;
            //        //    lstlabs.LabName = item.LabName;
            //        //    lstlabs.EquipmentName = item.EquipmentName;
            //        //    lstlabs.LabEquipmentName = item.EquipmentName;
            //        //    lstlabs.collegeId = userCollegeID;
            //        //    lstlabs.EquipmentNo = 1;
            //        //    lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
            //        //    lstlaboratories1.Add(lstlabs);

            //        //}

            //    }
            //}


            ////if (pageNumber == null)
            ////{
            ////    lstlaboratories = lstlaboratories.Take(100).ToList();
            ////}
            ////else
            ////{               
            ////    lstlaboratories = lstlaboratories.Skip(100 * ((int)pageNumber - 1)).Take(100).ToList();
            ////}


            //return View(lstlaboratories.OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ToList());

            #endregion
            //return RedirectToAction("View", "Labs");
            return View(laboratories);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public JsonResult GetDepartments(int degreeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            object Dept = null;
            if (degreeId != 0)
            {
                int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                //userCollegeID = 9;
                if (userCollegeID == 375)
                {
                    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

                int AY1 = jntuh_academic_year.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();

                var jntuh_departments = db.jntuh_department.ToList();

                var jntuh_specialization = db.jntuh_specialization.ToList();

                int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && (e.academicYearId == AY1 || e.academicYearId == AY2 || e.academicYearId == AY3)).Select(e => e.specializationId).ToArray();
                //int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && e.academicYearId == AY3 && e.courseStatus != "Closure").Select(e => e.specializationId).ToArray();
                int[] DepartmentsData;
                if (degreeId == 4)
                {
                    DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id) || e.id == 39).Select(e => e.id).Distinct().ToArray();
                }
                else
                {
                    DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id)).Select(e => e.id).Distinct().ToArray();
                }



                Dept = jntuh_specialization.Join(jntuh_departments, Spec => Spec.departmentId, DEPT => DEPT.id,
                        (Spec, DEPT) => new { Spec = Spec, DEPT = DEPT })
                        .Where(e => e.Spec.isActive == true && DepartmentsData.Contains(e.Spec.id) && e.DEPT.degreeId == degreeId)
                        .Select(e => new
                        {
                            DeptId = e.Spec.id,
                            DeptName = e.DEPT.departmentName + "-" + e.Spec.specializationName
                        })
                        .ToList();
                //jntuh_departments.Where(e => e.isActive == true && DepartmentsData.Contains(e.id) && e.degreeId == degreeId).Select(e => new { DeptId = e.id, DeptName = e.departmentName }).ToList();
            }
            return Json(new { Data = Dept }, "application/json", JsonRequestBehavior.AllowGet);
        }

        public ActionResult ComingSoon()
        {
            return View();
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

            List<jntuh_lab_master> collegeLabMaster = null;
            List<Lab> lstlaboratories = new List<Lab>();

            var jntuh_college_laboratories1 = db.jntuh_college_laboratories.AsNoTracking().Where(e => e.CollegeID == userCollegeID).ToList();
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
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.CollegeId == userCollegeID && l.DegreeID == DegreeId && l.SpecializationID == DepartmentId).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();
                }
                else//non autonomous College
                {
                    
                    //if college Mechanical &Civil Engineering s have specializations shows Engineering Physics Lab(PH105BS) other wise not show.
                    //int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && e.academicYearId == AY3 && e.courseStatus != "Closure").Select(e => e.specializationId).ToArray();
                    int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && (e.academicYearId == AY1 || e.academicYearId == AY2 || e.academicYearId == AY3)).Select(e => e.specializationId).ToArray();
                    if (specializationIDs.Contains(33) || specializationIDs.Contains(43))
                    {
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.DegreeID == DegreeId && l.SpecializationID == DepartmentId && l.CollegeId == null).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();
                    }
                    else
                    {
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.DegreeID == DegreeId && l.SpecializationID == DepartmentId && l.CollegeId == null && l.Labcode != "PH105BS").OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();
                    }
                }
                foreach (var item in collegeLabMaster)
                {
                    if (CollegeAffiliationStatus == "Yes")//item.Labcode == "TMP-CL" &&
                    {

                        Lab lstlabs = new Lab();
                        lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
                        lstlabs.EquipmentID = item.id;
                        lstlabs.degree = item.jntuh_degree.degree;
                        lstlabs.AffiliationStatus = CollegeAffiliationStatus;
                        lstlabs.department = item.jntuh_department.departmentName;
                        lstlabs.specializationName = item.jntuh_specialization.specializationName;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.Year;
                        lstlabs.Labcode = item.Labcode;
                        lstlabs.LabName = item.LabName;
                        lstlabs.EquipmentName = item.EquipmentName;
                        lstlabs.LabEquipmentName = item.EquipmentName;
                        lstlabs.collegeId = userCollegeID;
                        lstlabs.EquipmentNo = 1;
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
                            lstlabs.degree = item.jntuh_degree.degree;
                            lstlabs.department = item.jntuh_department.departmentName;
                            lstlabs.specializationName = item.jntuh_specialization.specializationName;
                            lstlabs.Semester = item.Semester;
                            lstlabs.year = item.Year;
                            lstlabs.Labcode = item.Labcode;
                            lstlabs.LabName = item.LabName;
                            lstlabs.EquipmentName = item.EquipmentName;
                            lstlabs.LabEquipmentName = item.EquipmentName;
                            lstlabs.collegeId = userCollegeID;
                            lstlabs.EquipmentNo = 1;
                            lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
                            lstlaboratories.Add(lstlabs);
                        }
                        else if (item.DegreeID == 4 && item.SpecializationID == 39)
                        {

                            Lab lstlabs = new Lab();
                            lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
                            lstlabs.EquipmentID = item.id;
                            lstlabs.degree = item.jntuh_degree.degree;
                            lstlabs.department = item.jntuh_department.departmentName;
                            lstlabs.specializationName = item.jntuh_specialization.specializationName;
                            lstlabs.Semester = item.Semester;
                            lstlabs.year = item.Year;
                            lstlabs.Labcode = item.Labcode;
                            lstlabs.LabName = item.LabName;
                            lstlabs.EquipmentName = item.EquipmentName;
                            lstlabs.LabEquipmentName = item.EquipmentName;
                            lstlabs.collegeId = userCollegeID;
                            lstlabs.EquipmentNo = 1;
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
            return Json(lstlaboratories.OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ThenBy(l=>l.LabName).ToList(), "application/json", JsonRequestBehavior.AllowGet);
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
                        userCollegeID = db.jntuh_college_laboratories.Where(i => i.id == id).Select(i => i.CollegeID).FirstOrDefault();
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
                if (id != null&&id!=0)
                {
                    ViewBag.IsUpdate = true;
                    laboratories = (from m in db.jntuh_lab_master
                                    join labs in db.jntuh_college_laboratories on m.id equals labs.EquipmentID
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
                                        degree = m.jntuh_degree.degree,
                                        department = m.jntuh_department.departmentName,
                                        specializationName = m.jntuh_specialization.specializationName,
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
                    if (laboratories!=null)
                    {
                        laboratories.EquipmentDateOfPurchasing1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing.ToString()) : null;
                        laboratories.DelivaryChalanaDate1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate.ToString()) : null;
                        laboratories.ViewCreatedOn = laboratories.createdOn.ToString();
                    }
                  
                    //  return PartialView("_LaboratoriesData", laboratories);
                    return Json(laboratories, "application/json", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (eqpid != null && eqpid != 0)
                    {
                        ViewBag.IsUpdate = false;
                        jntuh_lab_master master = db.jntuh_lab_master.Find(eqpid);
                        if (master!=null)
                        {
                            laboratories.collegeId = userCollegeID;
                            laboratories.degreeId = master.DegreeID;
                            laboratories.degree = master.jntuh_degree.degree;
                            laboratories.departmentId = master.DepartmentID;
                            laboratories.department = master.jntuh_department.departmentName;
                            laboratories.specializationId = master.SpecializationID;
                            laboratories.specializationName = master.jntuh_specialization.specializationName;
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
                    laboratories = (from m in db.jntuh_lab_master
                                    join labs in db.jntuh_college_laboratories on m.id equals labs.EquipmentID
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
                                        degree = m.jntuh_degree.degree,
                                        department = m.jntuh_department.departmentName,
                                        specializationName = m.jntuh_specialization.specializationName,
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
                    ViewBag.IsUpdate = false;
                    jntuh_lab_master master = db.jntuh_lab_master.Find(eqpid);
                    laboratories.collegeId = userCollegeID;
                    laboratories.degreeId = master.DegreeID;
                    laboratories.degree = master.jntuh_degree.degree;
                    laboratories.departmentId = master.DepartmentID;
                    laboratories.department = master.jntuh_department.departmentName;
                    laboratories.specializationId = master.SpecializationID;
                    laboratories.specializationName = master.jntuh_specialization.specializationName;
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

                jntuh_college_laboratories jntuh_college_laboratories = new jntuh_college_laboratories();
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

                #region Documents

                ////Delivery challan pdf code
                //var deliverychallanpath = "~/Content/Upload/Labs/DeliverychallanDocuments";
                //if (laboratories.DelivaryChalanaImage != null)
                //{
                //    if (!Directory.Exists(Server.MapPath(deliverychallanpath)))
                //    {
                //        Directory.CreateDirectory(Server.MapPath(deliverychallanpath));
                //    }

                //    var ext = Path.GetExtension(laboratories.DelivaryChalanaImage.FileName);
                //    if (ext.ToUpper().Equals(".PDF"))
                //    {
                //        string labfileName = userCollegeID + "_" + laboratories.EquipmentID + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                //        laboratories.DelivaryChalanaImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(deliverychallanpath), labfileName, ext));
                //        //jntuh_college_laboratories.DelivaryChalanaImage = string.Format("{0}/{1}{2}", deliverychallanpath, labfileName, ext);
                //        jntuh_college_laboratories.DelivaryChalanaImage = string.Format("{0}{1}",  labfileName, ext);
                //    }
                //}
                //else if (laboratories.ViewDelivaryChalanaImage != null)
                //{
                //    jntuh_college_laboratories.DelivaryChalanaImage = laboratories.ViewDelivaryChalanaImage;
                //}

                ////Bank statement Pdf code
                //var bankstatmentpath = "~/Content/Upload/Labs/BankstatementDocuments";
                //if (laboratories.BankStatementImage != null)
                //{
                //    if (!Directory.Exists(Server.MapPath(bankstatmentpath)))
                //    {
                //        Directory.CreateDirectory(Server.MapPath(bankstatmentpath));
                //    }

                //    var ext = Path.GetExtension(laboratories.BankStatementImage.FileName);
                //    if (ext.ToUpper().Equals(".PDF"))
                //    {
                //        string labfileName1 = userCollegeID + "_" + laboratories.EquipmentID + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                //        laboratories.BankStatementImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(bankstatmentpath), labfileName1, ext));
                //      //  jntuh_college_laboratories.BankStatementImage = string.Format("{0}/{1}{2}", bankstatmentpath, labfileName1, ext);
                //        jntuh_college_laboratories.BankStatementImage = string.Format("{0}{1}",  labfileName1, ext);
                //    }
                //}
                //else if (laboratories.ViewBankStatementImage != null)
                //{
                //    jntuh_college_laboratories.BankStatementImage = laboratories.ViewBankStatementImage;
                //}

                ////Stock Register Pdf code
                //var stockregisterpath = "~/Content/Upload/Labs/StockregisterentryDocuments";
                //if (laboratories.StockRegisterEntryImage != null)
                //{
                //    if (!Directory.Exists(Server.MapPath(stockregisterpath)))
                //    {
                //        Directory.CreateDirectory(Server.MapPath(stockregisterpath));
                //    }

                //    var ext = Path.GetExtension(laboratories.StockRegisterEntryImage.FileName);
                //    if (ext.ToUpper().Equals(".PDF"))
                //    {
                //        string labfileName2 = userCollegeID + "_" + laboratories.EquipmentID + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                //        laboratories.StockRegisterEntryImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(stockregisterpath), labfileName2, ext));
                //        jntuh_college_laboratories.StockRegisterEntryImage = string.Format("{0}{1}",  labfileName2, ext);
                //    }
                //}
                //else if (laboratories.ViewStockRegisterEntryImage != null)
                //{
                //    jntuh_college_laboratories.StockRegisterEntryImage = laboratories.ViewStockRegisterEntryImage;
                //}

                ////Purchase Order pdf code
                //var purchaseorderpath = "~/Content/Upload/Labs/PurchaseOrderDocument";
                //if (laboratories.PurchaseOrderImage != null)
                //{
                //    if (!Directory.Exists(Server.MapPath(purchaseorderpath)))
                //    {
                //        Directory.CreateDirectory(Server.MapPath(purchaseorderpath));
                //    }

                //    var ext = Path.GetExtension(laboratories.PurchaseOrderImage.FileName);
                //    if (ext.ToUpper().Equals(".PDF"))
                //    {
                //        string labfileName = userCollegeID + "_" + laboratories.EquipmentID + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                //        laboratories.PurchaseOrderImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(purchaseorderpath), labfileName, ext));
                //        //jntuh_college_laboratories.DelivaryChalanaImage = string.Format("{0}/{1}{2}", deliverychallanpath, labfileName, ext);
                //        jntuh_college_laboratories.PurchaseOrder = string.Format("{0}{1}", labfileName, ext);
                //    }
                //}
                //else if (laboratories.ViewPurchaseOrderImage != null)
                //{
                //    jntuh_college_laboratories.PurchaseOrder = laboratories.ViewPurchaseOrderImage;
                //}


                #endregion EndDocuments






                if (laboratories.id == 0 || laboratories.id == null)
                {
                    var existingID =
                        db.jntuh_college_laboratories.Where(
                            c =>
                                c.CollegeID == userCollegeID && c.EquipmentID == laboratories.EquipmentID &&
                                c.EquipmentNo == laboratories.EquipmentNo).Select(c => c).FirstOrDefault();

                    if (existingID == null)
                    {
                        jntuh_college_laboratories.createdBy = userID;
                        jntuh_college_laboratories.createdOn = DateTime.Now;
                        db.jntuh_college_laboratories.Add(jntuh_college_laboratories);
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
                    jntuh_college_laboratories.id = (int) laboratories.id;
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

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public JsonResult DeleteRecord(int id)
        {
            object jsondata = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var lab = db.jntuh_college_laboratories.Where(l => l.id == id && l.CollegeID == userCollegeID).Select(l => l).FirstOrDefault();
            if (lab != null)
            {
                try
                {
                    db.jntuh_college_laboratories.Remove(lab);
                    db.SaveChanges();
                    // TempData["Success"] = "Lab Related details Deleted Successfully.";
                    jsondata = "Lab Related details Deleted Successfully.";
                }
                catch
                {

                }
            }
            //else
            //{
            //    jsondata = "Lab Related details Deletion Failed.";
            //}
            return Json(new { jsondata }, "application/json", JsonRequestBehavior.AllowGet);
            // return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        public ActionResult UserView(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && e.academicYearId == 8).Select(e => e.specializationId).ToArray();
            List<jntuh_lab_master> collegeLabMaster = db.jntuh_lab_master.Where(l => specializationIDs.Contains(l.SpecializationID)).ToList();
            List<Lab> lstlaboratories = new List<Lab>();

            int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            foreach (var item in collegeLabMaster)
            {
                // Commented By Srinivas because  In DB Lab code TMP-CL Only one record is there based up on Specialization but in frent end shows 5 records so if update any one of lab data its reflecet to all labs and based up on specialization 
                //if (item.jntuh_degree.degree != "B.Tech" && item.jntuh_degree.degree != "B.Pharmacy")
                //{
                //    for (int i = 1; i <= PGEquipmentCount; i++)
                //    {
                if (item.Labcode == "TMP-CL" && CollegeAffiliationStatus == "Yes")
                {
                    Lab lstlabs = new Lab();
                    lstlabs.id = db.jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
                    lstlabs.EquipmentID = item.id;
                    lstlabs.degree = item.jntuh_degree.degree;
                    lstlabs.department = item.jntuh_department.departmentName;
                    lstlabs.specializationName = item.jntuh_specialization.specializationName;
                    lstlabs.Semester = item.Semester;
                    lstlabs.year = item.Year;
                    lstlabs.Labcode = item.Labcode;
                    lstlabs.LabName = item.LabName;
                    lstlabs.EquipmentName = item.EquipmentName;
                    lstlabs.LabEquipmentName = item.EquipmentName;
                    lstlabs.collegeId = userCollegeID;
                    lstlabs.EquipmentNo = 1;
                    lstlaboratories.Add(lstlabs);
                }

        //    }
                //}
                else
                {
                    if (item.Labcode != "TMP-CL" && item.Labcode != "MPCLAB")
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.id = db.jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
                        lstlabs.EquipmentID = item.id;
                        lstlabs.degree = item.jntuh_degree.degree;
                        lstlabs.department = item.jntuh_department.departmentName;
                        lstlabs.specializationName = item.jntuh_specialization.specializationName;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.Year;
                        lstlabs.Labcode = item.Labcode;
                        lstlabs.LabName = item.LabName;
                        lstlabs.EquipmentName = item.EquipmentName;
                        lstlabs.LabEquipmentName = item.EquipmentName;
                        lstlabs.collegeId = userCollegeID;
                        lstlabs.EquipmentNo = 1;
                        lstlaboratories.Add(lstlabs);
                    }
                }
            }
            ViewBag.Count = lstlaboratories.Count();
            return View(lstlaboratories.OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ToList());

        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return RedirectToAction("Index", "UnderConstruction");
            //}        
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            //if (CollegeAffiliationStatus == "Yes") //autonomous College 
            //{
            //    return RedirectToAction("ComingSoon");
            //}
         //return RedirectToAction("College", "Dashboard");



            var jntuh_departments = db.jntuh_department.Where(d=>d.isActive==true).ToList();
            var jntuh_degrees = db.jntuh_degree.Where(de=>de.isActive==true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(s=>s.isActive==true).ToList();

            int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && (e.academicYearId == 8||e.academicYearId==9)).Select(e => e.specializationId).ToArray();


            var DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            // var Dapartmentdetails =jntuh_departments.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => new {DeptId = e.id, DeptName = e.departmentName}).ToList();

            var DegreeIds = jntuh_departments.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToArray();

            ViewBag.Degree = jntuh_degrees.Where(e => e.isActive == true && DegreeIds.Contains(e.id)).Select(e => new { DegreeId = e.id, DegreeName = e.degree }).ToList();


            #region Committed by Suresh
            //int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID).Select(e => e.specializationId).ToArray();
            //int[] DegreeIDs = db.jntuh_lab_master.AsNoTracking().Where(l => l.DegreeID == 4 && specializationIDs.Contains(l.SpecializationID)).Select(l => l.DegreeID).ToArray();
            //List<jntuh_lab_master> collegeLabMaster = null;

            //if (DegreeIDs.Contains(4))
            //{
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == 39 || specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();

            //}
            //else
            //{
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();

            //}
            //List<Lab> lstlaboratories = new List<Lab>();

            //int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);
            //var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == userCollegeID).ToList();
            //foreach (var item in collegeLabMaster)
            //{
            //    if (item.jntuh_degree.degree != "B.Tech" && item.jntuh_degree.degree != "B.Pharmacy")
            //    {
            //        for (int i = 1; i <= PGEquipmentCount; i++)
            //        {
            //            Lab lstlabs = new Lab();
            //            //lstlabs.id = db.jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i).Select(l => l.id).FirstOrDefault();
            //            //lstlabs.EquipmentID = item.id;
            //            lstlabs.id = db.jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
            //            lstlabs.EquipmentID = item.id;
            //            lstlabs.degree = item.jntuh_degree.degree;
            //            lstlabs.department = item.jntuh_department.departmentName;
            //            lstlabs.specializationName = item.jntuh_specialization.specializationName;
            //            lstlabs.Semester = item.Semester;
            //            lstlabs.year = item.Year;
            //            lstlabs.Labcode = item.Labcode;
            //            lstlabs.LabName = item.LabName;
            //            lstlabs.EquipmentName = item.EquipmentName;
            //            lstlabs.LabEquipmentName = item.EquipmentName;
            //            lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
            //            lstlabs.collegeId = userCollegeID;
            //            lstlabs.EquipmentNo = i;
            //            lstlaboratories.Add(lstlabs);
            //        }
            //    }
            //    else
            //    {
            //        Lab lstlabs = new Lab();
            //        //lstlabs.id = db.jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1).Select(l => l.id).FirstOrDefault();
            //        //lstlabs.EquipmentID = item.id;
            //        lstlabs.id = db.jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
            //        lstlabs.EquipmentID = item.id;
            //        lstlabs.degree = item.jntuh_degree.degree;
            //        lstlabs.department = item.jntuh_department.departmentName;
            //        lstlabs.specializationName = item.jntuh_specialization.specializationName;
            //        lstlabs.Semester = item.Semester;
            //        lstlabs.year = item.Year;
            //        lstlabs.Labcode = item.Labcode;
            //        lstlabs.LabName = item.LabName;
            //        lstlabs.EquipmentName = item.EquipmentName;
            //        lstlabs.LabEquipmentName = item.EquipmentName;
            //        lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
            //        lstlabs.collegeId = userCollegeID;
            //        lstlabs.EquipmentNo = 1;
            //        lstlaboratories.Add(lstlabs);
            //    }
            //}

            //ViewBag.Laboratories = lstlaboratories;
            //ViewBag.Count = lstlaboratories.Count();
            //return View(lstlaboratories.OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ToList());
            #endregion
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult VewExperiments(string collegeId, int? pageNumber)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
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

            int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID).Select(e => e.specializationId).ToArray();
            // int count = specializationIDs.Count() + 1;
            // specializationIDs[count] = 34;
            List<jntuh_lab_master> collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == 39 || specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();
            List<Lab> lstlaboratories = new List<Lab>();
            //if (Session["CollegeLabs"] == null)
            //{
            int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);

            var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == userCollegeID).ToList();
            foreach (var item in collegeLabMaster)
            {


                //if (CollegeAffiliationStatus == "Yes")
                //item.jntuh_degree.degree != "B.Tech" && item.jntuh_degree.degree != "B.Pharmacy"
                if (item.Labcode == "MPCLAB")
                {
                    if (CollegeAffiliationStatus == "Yes")
                    {
                    }
                    else
                    {


                    }
                }
                // Commented By Srinivas because  In DB Lab code TMP-CL Only one record is there based up on Specialization but in frent end shows 5 records so if update any one of lab data its reflecet to all labs and based up on specialization 
                //if (item.jntuh_degree.degree != "B.Tech" && item.jntuh_degree.degree != "B.Pharmacy")
                //{

                if (item.Labcode == "TMP-CL" && CollegeAffiliationStatus == "Yes")
                {
                    //for (int i = 1; i <= PGEquipmentCount; i++)
                    //{
                    Lab lstlabs = new Lab();
                    //lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
                    lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
                    lstlabs.EquipmentID = item.id;
                    lstlabs.degree = item.jntuh_degree.degree;
                    lstlabs.AffiliationStatus = CollegeAffiliationStatus;
                    lstlabs.department = item.jntuh_department.departmentName;
                    lstlabs.specializationName = item.jntuh_specialization.specializationName;
                    lstlabs.Semester = item.Semester;
                    lstlabs.year = item.Year;
                    lstlabs.Labcode = item.Labcode;
                    lstlabs.LabName = item.LabName;
                    lstlabs.EquipmentName = item.EquipmentName;
                    lstlabs.LabEquipmentName = item.EquipmentName;
                    lstlabs.collegeId = userCollegeID;
                    lstlabs.EquipmentNo = 1;
                    lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
                    lstlaboratories.Add(lstlabs);
                    //}
                }
                //}
                else
                {
                    if (item.Labcode != "TMP-CL" && item.Labcode != "MPCLAB")
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.id).FirstOrDefault();
                        lstlabs.EquipmentID = item.id;
                        lstlabs.degree = item.jntuh_degree.degree;
                        lstlabs.department = item.jntuh_department.departmentName;
                        lstlabs.specializationName = item.jntuh_specialization.specializationName;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.Year;
                        lstlabs.Labcode = item.Labcode;
                        lstlabs.LabName = item.LabName;
                        lstlabs.EquipmentName = item.EquipmentName;
                        lstlabs.LabEquipmentName = item.EquipmentName;
                        lstlabs.collegeId = userCollegeID;
                        lstlabs.EquipmentNo = 1;
                        lstlabs.uploadFile = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == userCollegeID).Select(l => l.EquipmentPhoto).FirstOrDefault();
                        lstlaboratories.Add(lstlabs);
                    }

                }
            }
            int totalPages = 1;
            int totalLabs = lstlaboratories.Count();
            if (totalLabs > 100)
            {
                totalPages = totalLabs / 100;
                if (totalLabs > 100 * totalPages)
                {
                    totalPages = totalPages + 1;
                }
            }
            ViewBag.Pages = totalPages;

            if (pageNumber == null)
            {
                lstlaboratories = lstlaboratories.Take(100).ToList();
            }
            else
            {
                lstlaboratories = lstlaboratories.Skip(100 * ((int)pageNumber - 1)).Take(100).ToList();
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

            return View(lstlaboratories.OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ToList());
        }


        #region Checking Colleges Uploaded Labs List
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult CollegeLabsChecking(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            List<Labscount> Labdetails = new List<Labscount>();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            List<jntuh_academic_year> jntuhAcademicyear =
                db.jntuh_academic_year.Where(a => a.isActive == true).Select(s => s).ToList();
            int actualYear = jntuhAcademicyear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY0 = jntuhAcademicyear.Where(s => s.actualYear == actualYear+1).Select(s => s.id).FirstOrDefault();
            int AY1 = jntuhAcademicyear.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuhAcademicyear.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuhAcademicyear.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            var jntuh_departments = db.jntuh_department.ToList();
            var jntuh_degrees = db.jntuh_degree.ToList();
            var jntuh_specialization = db.jntuh_specialization.ToList();

            List<jntuh_lab_master> LabMasterdata = new List<jntuh_lab_master>();
            List<int> specializationIDs1 = new List<int>();

            specializationIDs1.AddRange(db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && (e.academicYearId == AY1)).Select(e => e.specializationId).ToArray());
            //specializationIDs1.AddRange(db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && e.academicYearId == AY3 && e.courseStatus != "Closure").Select(e => e.specializationId).ToArray());
            specializationIDs1.Add(39);
            if (CollegeAffiliationStatus == "Yes")
            {

                LabMasterdata = db.jntuh_lab_master.Where(e => e.isActive == true && e.CollegeId == userCollegeID).Select(e => e).ToList();
            }
            else
            {
                //if college Mechanical &Civil Engineering s have specializations shows Engineering Physics Lab(PH105BS) other wise not show. written by Narayana Reddy
                if (specializationIDs1.Contains(33) || specializationIDs1.Contains(43))
                {
                    LabMasterdata = db.jntuh_lab_master.Where(e => e.isActive == true && e.CollegeId == null && (specializationIDs1.Contains(e.SpecializationID) || e.SpecializationID == 39)).Select(e => e).ToList();
                }
                else
                {
                    LabMasterdata = db.jntuh_lab_master.Where(e => e.isActive == true && e.CollegeId == null && (specializationIDs1.Contains(e.SpecializationID) || e.SpecializationID == 39) && e.Labcode != "PH105BS").Select(e => e).ToList();
                }
            }
            var CollegeLabsUploadeddata = db.jntuh_college_laboratories.Where(e => e.CollegeID == userCollegeID).Select(e => e).ToList();

            foreach (var SpecId in specializationIDs1)
            {
                var Degreedata = (from Deg in jntuh_degrees
                                  join Dept in jntuh_departments on Deg.id equals Dept.degreeId
                                  join SPEC in jntuh_specialization on Dept.id equals SPEC.departmentId
                                  where SPEC.id == SpecId
                                  select new
                                  {
                                      DegreeName = Deg.degree,
                                      DepartmentName = Dept.departmentName,
                                      SpecializationName = SPEC.specializationName
                                  }).FirstOrDefault();

                if (Degreedata != null)
                {
                    Labscount Labsdata = new Labscount();
                    Labsdata.DegreeName = Degreedata.DegreeName;
                    Labsdata.DepartmentName = Degreedata.DepartmentName;
                    Labsdata.SpecializationName = Degreedata.SpecializationName;
                    Labsdata.TotalLabCount = LabMasterdata.Where(e => e.SpecializationID == SpecId).Select(e => e.id).Count();
                    Labsdata.CollegeLabCount = (from LAB in LabMasterdata join CLGLAB in CollegeLabsUploadeddata on LAB.id equals CLGLAB.EquipmentID where LAB.SpecializationID == SpecId select CLGLAB.EquipmentID).Distinct().Count();
                    Labdetails.Add(Labsdata);
                }

            }
            return View(Labdetails.OrderBy(e => e.DegreeName).ToList());
        }
        #endregion

        #region Physical Labs Enrty

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult ViewPhysicalLabs(string collegeId)
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

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            string CollegeAffiliationStatus1 = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
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
                return RedirectToAction("ViewPhysicalLabsDetails", "Labs");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LAB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("ViewPhysicalLabsDetails", "Labs");
            }
            List<jntuh_academic_year> jntuhAcademicyear =
                db.jntuh_academic_year.Where(a => a.isActive == true).Select(s => s).ToList();
            //int actualYear = jntuhAcademicyear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY0 = jntuhAcademicyear.Where(s => s.actualYear == actualYear + 1).Select(s => s.id).FirstOrDefault();
            int AY1 = jntuhAcademicyear.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuhAcademicyear.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuhAcademicyear.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();

            List<physicalLab> ObjPhysicalLab = new List<physicalLab>();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            var jntuh_physical_lab = db.jntuh_physical_labmaster.Where(k => k.Collegeid == userCollegeID).Select(k => new { Id = k.Id, LabCode = k.Labcode.Trim().ToUpper(), deptId = k.DepartmentId }).ToList();
            if (CollegeAffiliationStatus == "Yes")
            {
                ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                  join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                  join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                  join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                  where lab.CollegeId == userCollegeID && deg.id == 4
                                  select new physicalLab
                                  {
                                      Labid = lab.id,
                                      collegeId = (int)lab.CollegeId,
                                      degreeid = lab.DegreeID,
                                      departmentid = lab.DepartmentID,
                                      specializationid = lab.SpecializationID,
                                      degree = deg.degree,
                                      specialization = spec.specializationName,
                                      department = dep.departmentName,
                                      year = lab.Year,
                                      semister = lab.Semester,
                                      Labname = lab.LabName,
                                      LabCode = lab.Labcode.Trim().ToUpper()
                                  }).ToList();
            }
            else
            {

                var jntuh_specialization = db.jntuh_specialization.ToList();

                int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && (e.academicYearId == AY1 || e.academicYearId == AY2 || e.academicYearId == AY3)).Select(e => e.specializationId).ToArray();
                //int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && e.academicYearId == AY3 && e.courseStatus != "Closure").Select(e => e.specializationId).ToArray();

                var DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

                var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();


                if (DegreeIds.Contains(4))
                {
                        if (specializationIDs.Contains(134))
                        {
                            if(specializationIDs.Contains(33) || specializationIDs.Contains(43))
                            {
                                 ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                where
                                    lab.CollegeId == null && deg.id == 4 &&
                                    (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34)
                                select new physicalLab
                                {
                                    Labid = lab.id,
                                    collegeId = userCollegeID,
                                    degreeid = lab.DegreeID,
                                    departmentid = lab.DepartmentID,
                                    specializationid = lab.SpecializationID,
                                    degree = deg.degree,
                                    specialization = spec.specializationName,
                                    department = dep.departmentName,
                                    year = lab.Year,
                                    semister = lab.Semester,
                                    Labname = lab.LabName,
                                    LabCode = lab.Labcode.Trim().ToUpper()
                                }).ToList();
                            }
                            else
                            {
                                ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                where
                                    lab.CollegeId == null && deg.id == 4 &&
                                    (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34) && lab.Labcode != "PH105BS"
                                select new physicalLab
                                {
                                    Labid = lab.id,
                                    collegeId = userCollegeID,
                                    degreeid = lab.DegreeID,
                                    departmentid = lab.DepartmentID,
                                    specializationid = lab.SpecializationID,
                                    degree = deg.degree,
                                    specialization = spec.specializationName,
                                    department = dep.departmentName,
                                    year = lab.Year,
                                    semister = lab.Semester,
                                    Labname = lab.LabName,
                                    LabCode = lab.Labcode.Trim().ToUpper()
                                }).ToList();
                            }
                        }
                        else
                        {
                            if (specializationIDs.Contains(33) || specializationIDs.Contains(43))
                            {
                                ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                                  join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                                  join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                                  join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                                  where
                                                      lab.CollegeId == null && deg.id == 4 &&
                                                      (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34) &&
                                                      lab.SpecializationID != 134
                                                  select new physicalLab
                                                  {
                                                      Labid = lab.id,
                                                      collegeId = userCollegeID,
                                                      degreeid = lab.DegreeID,
                                                      departmentid = lab.DepartmentID,
                                                      specializationid = lab.SpecializationID,
                                                      degree = deg.degree,
                                                      specialization = spec.specializationName,
                                                      department = dep.departmentName,
                                                      year = lab.Year,
                                                      semister = lab.Semester,
                                                      Labname = lab.LabName,
                                                      LabCode = lab.Labcode.Trim().ToUpper()
                                                  }).ToList();
                            }
                            else
                            {
                                ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                                  join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                                  join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                                  join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                                  where
                                                      lab.CollegeId == null && deg.id == 4 &&
                                                      (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34) &&
                                                      lab.SpecializationID != 134 && lab.Labcode != "PH105BS"
                                                  select new physicalLab
                                                  {
                                                      Labid = lab.id,
                                                      collegeId = userCollegeID,
                                                      degreeid = lab.DegreeID,
                                                      departmentid = lab.DepartmentID,
                                                      specializationid = lab.SpecializationID,
                                                      degree = deg.degree,
                                                      specialization = spec.specializationName,
                                                      department = dep.departmentName,
                                                      year = lab.Year,
                                                      semister = lab.Semester,
                                                      Labname = lab.LabName,
                                                      LabCode = lab.Labcode.Trim().ToUpper()
                                                  }).ToList();
                            }
                        }
                }
                else
                {
                    ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                      join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                      join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                      join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                      where lab.CollegeId == null && deg.id == 4 && DepartmentsData.Contains(lab.DepartmentID)
                                      select new physicalLab
                                      {
                                          Labid = lab.id,
                                          collegeId = userCollegeID,
                                          degreeid = lab.DegreeID,
                                          departmentid = lab.DepartmentID,
                                          specializationid = lab.SpecializationID,
                                          degree = deg.degree,
                                          specialization = spec.specializationName,
                                          department = dep.departmentName,
                                          year = lab.Year,
                                          semister = lab.Semester,
                                          Labname = lab.LabName,
                                          LabCode = lab.Labcode.Trim().ToUpper()
                                      }).ToList();
                }

            }

            ObjPhysicalLab = ObjPhysicalLab.GroupBy(e => new { e.LabCode, e.departmentid }).Select(e => new physicalLab
            {
                Labid = e.FirstOrDefault().Labid,
                collegeId = e.FirstOrDefault().collegeId,
                degreeid = e.FirstOrDefault().degreeid,
                departmentid = e.FirstOrDefault().departmentid,
                specializationid = e.FirstOrDefault().specializationid,
                degree = e.FirstOrDefault().degree,
                specialization = e.FirstOrDefault().specialization,
                department = e.FirstOrDefault().department,
                year = e.FirstOrDefault().year,
                semister = e.FirstOrDefault().semister,
                Labname = e.FirstOrDefault().Labname,
                LabCode = e.FirstOrDefault().LabCode.Trim(),

                physicalId = jntuh_physical_lab.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid).Select(d => d.Id).FirstOrDefault()

            }).ToList();



            return View(ObjPhysicalLab);

        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditPhysicalLab(int? mid, int? pid)
        {

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            if (pid != null)
            {
                physicalLab ObjPhysicalLab = (from phys in db.jntuh_physical_labmaster
                                              join deg in db.jntuh_degree on phys.DegreeId equals deg.id
                                              join dep in db.jntuh_department on phys.DepartmentId equals dep.id
                                              join spec in db.jntuh_specialization on phys.SpecializationId equals spec.id
                                              where phys.Id == pid && phys.Collegeid == userCollegeID
                                              select new physicalLab
                                              {
                                                  id = phys.Id,
                                                  collegeId = phys.Collegeid,
                                                  degreeid = phys.DegreeId,
                                                  departmentid = phys.DepartmentId,
                                                  specializationid = phys.SpecializationId,
                                                  degree = deg.degree,
                                                  specialization = spec.specializationName,
                                                  department = dep.departmentName,
                                                  year = phys.Year,
                                                  semister = phys.Semister,
                                                  Labname = phys.LabName,
                                                  LabCode = phys.Labcode,
                                                  NoOfRequiredLabs = phys.Numberofrequiredlabs,
                                                  NoOfAvailabeLabs = phys.Numberofavilablelabs,
                                                  Remarks = phys.Remarks
                                              }).FirstOrDefault();

                return PartialView("AddEditPhysicalLab", ObjPhysicalLab);

            }
            else
            {
                physicalLab ObjPhysicalLab = new physicalLab();
                if (mid != null)
                {
                    if (CollegeAffiliationStatus == "Yes")
                    {
                        ObjPhysicalLab = (from lab in db.jntuh_lab_master
                            join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                            join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                            join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                            where lab.id == mid && lab.CollegeId == userCollegeID
                            select new physicalLab
                            {
                                Labid = lab.id,
                                collegeId = (int) lab.CollegeId,
                                degreeid = lab.DegreeID,
                                departmentid = lab.DepartmentID,
                                specializationid = lab.SpecializationID,
                                degree = deg.degree,
                                specialization = spec.specializationName,
                                department = dep.departmentName,
                                year = lab.Year,
                                LabCode = lab.Labcode,
                                semister = lab.Semester,
                                Labname = lab.LabName
                            }).FirstOrDefault();
                    }
                    else
                    {

                        ObjPhysicalLab = (from lab in db.jntuh_lab_master
                            join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                            join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                            join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                            where lab.id == mid && lab.CollegeId == null
                            select new physicalLab
                            {
                                Labid = lab.id,
                                collegeId = userCollegeID,
                                degreeid = lab.DegreeID,
                                departmentid = lab.DepartmentID,
                                specializationid = lab.SpecializationID,
                                degree = deg.degree,
                                specialization = spec.specializationName,
                                department = dep.departmentName,
                                year = lab.Year,
                                LabCode = lab.Labcode,
                                semister = lab.Semester,
                                Labname = lab.LabName
                            }).FirstOrDefault();
                    }

                }
                return PartialView("AddEditPhysicalLab", ObjPhysicalLab);
            }

        }



        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditPhysicalLab(physicalLab physicallab)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            if (ModelState.IsValid)
            {
                if (physicallab.id == 0 || physicallab.id == null)
                {
                    jntuh_physical_labmaster toAdd = new jntuh_physical_labmaster();
                    toAdd.Collegeid = userCollegeID;
                    toAdd.Year = physicallab.year;
                    toAdd.Semister = physicallab.semister;
                    toAdd.DegreeId = physicallab.degreeid;
                    toAdd.DepartmentId = physicallab.departmentid;
                    toAdd.SpecializationId = physicallab.specializationid;
                    //string replacementcode = Regex.Replace(physicallab.LabCode, @"\t|\n|\r", "");
                    toAdd.Labcode = physicallab.LabCode;
                    toAdd.LabName = physicallab.Labname;
                    toAdd.Numberofrequiredlabs = physicallab.NoOfRequiredLabs;
                    toAdd.Numberofavilablelabs = physicallab.NoOfAvailabeLabs;
                    toAdd.Remarks = physicallab.Remarks;
                    toAdd.Createdby = userCollegeID;
                    toAdd.Createdon = DateTime.Now;
                    db.jntuh_physical_labmaster.Add(toAdd);
                    db.SaveChanges();
                    TempData["Success"] = "Added Successfully";
                    //  return RedirectToAction("ViewPhysicalLabs");
                }
                else
                {
                    jntuh_physical_labmaster toUpdate = db.jntuh_physical_labmaster.Find(physicallab.id);
                    // toUpdate.Collegeid = physicallab.collegeId;
                    toUpdate.DegreeId = physicallab.degreeid;
                    toUpdate.DepartmentId = physicallab.departmentid;
                    toUpdate.LabName = physicallab.Labname;
                    toUpdate.Numberofrequiredlabs = physicallab.NoOfRequiredLabs;
                    toUpdate.Numberofavilablelabs = physicallab.NoOfAvailabeLabs;
                    toUpdate.Remarks = physicallab.Remarks;
                    toUpdate.Updatedby = userCollegeID;
                    toUpdate.UpdatedOn = DateTime.Now;
                    db.Entry(toUpdate).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated Successfully";
                    // return RedirectToAction("ViewPhysicalLabs");
                }
            }
            return RedirectToAction("ViewPhysicalLabs", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }



        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeletefromPhysical(int pid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //userCollegeID = 32;
            //if (userCollegeID == 375)
            //{
            //    userCollegeID = 192;
            //}

            jntuh_physical_labmaster recordToDelete = db.jntuh_physical_labmaster.Where(f => f.Id == pid && f.Collegeid == userCollegeID).Select(f => f).FirstOrDefault();
            if (recordToDelete != null)
            {
                db.jntuh_physical_labmaster.Remove(recordToDelete);
                db.SaveChanges();
                TempData["Success"] = "Deleted Successfully.";

            }
            else
            {
                TempData["Error"] = "No Reocrd found.";
            }

            return RedirectToAction("ViewPhysicalLabs");
        }


        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult ViewPhysicalLabsDetails()
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

            List<jntuh_academic_year> jntuhAcademicyear =
                 db.jntuh_academic_year.Where(a => a.isActive == true).Select(s => s).ToList();
            int actualYear = jntuhAcademicyear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY0 = jntuhAcademicyear.Where(s => s.actualYear == actualYear + 1).Select(s => s.id).FirstOrDefault();
            int AY1 = jntuhAcademicyear.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuhAcademicyear.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuhAcademicyear.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();
            var jntuh_physical_lab = db.jntuh_physical_labmaster.Where(k => k.Collegeid == userCollegeID).Select(k => new { Id = k.Id, LabCode = k.Labcode, NoofLabs = k.Numberofavilablelabs, deptId = k.DepartmentId }).ToList();
            List<physicalLab> ObjPhysicalLab = new List<physicalLab>();
            if (CollegeAffiliationStatus == "Yes")
            {
                ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                  join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                  join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                  join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                  where lab.CollegeId == userCollegeID && deg.id == 4
                                  select new physicalLab
                                  {
                                      Labid = lab.id,
                                      collegeId = (int)lab.CollegeId,
                                      degreeid = lab.DegreeID,
                                      departmentid = lab.DepartmentID,
                                      specializationid = lab.SpecializationID,
                                      degree = deg.degree,
                                      specialization = spec.specializationName,
                                      department = dep.departmentName,
                                      year = lab.Year,
                                      semister = lab.Semester,
                                      Labname = lab.LabName,
                                      LabCode = lab.Labcode.Trim().ToUpper()
                                  }).ToList();
            }
            else
            {

                var jntuh_specialization = db.jntuh_specialization.ToList();

                int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && (e.academicYearId == AY1 || e.academicYearId == AY2 || e.academicYearId == AY3)).Select(e => e.specializationId).ToArray();
                var DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();
                var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();
                if (DegreeIds.Contains(4))
                {
                    if (specializationIDs.Contains(134))
                    {
                        ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                          join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                          join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                          join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                          where lab.CollegeId == null && deg.id == 4 && (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34)
                                          select new physicalLab
                                          {
                                              Labid = lab.id,
                                              collegeId = userCollegeID,
                                              degreeid = lab.DegreeID,
                                              departmentid = lab.DepartmentID,
                                              specializationid = lab.SpecializationID,
                                              degree = deg.degree,
                                              specialization = spec.specializationName,
                                              department = dep.departmentName,
                                              year = lab.Year,
                                              semister = lab.Semester,
                                              Labname = lab.LabName,
                                              LabCode = lab.Labcode.Trim().ToUpper()
                                          }).ToList();
                    }
                    else
                    {
                        ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                          join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                          join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                          join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                          where lab.CollegeId == null && deg.id == 4 && (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34) && lab.SpecializationID != 134
                                          select new physicalLab
                                          {
                                              Labid = lab.id,
                                              collegeId = userCollegeID,
                                              degreeid = lab.DegreeID,
                                              departmentid = lab.DepartmentID,
                                              specializationid = lab.SpecializationID,
                                              degree = deg.degree,
                                              specialization = spec.specializationName,
                                              department = dep.departmentName,
                                              year = lab.Year,
                                              semister = lab.Semester,
                                              Labname = lab.LabName,
                                              LabCode = lab.Labcode.Trim().ToUpper()
                                          }).ToList();
                    }
                }
                else
                {
                    ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                      join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                      join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                      join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                      where lab.CollegeId == null && deg.id == 4 && DepartmentsData.Contains(lab.DepartmentID)
                                      select new physicalLab
                                      {
                                          Labid = lab.id,
                                          collegeId = userCollegeID,
                                          degreeid = lab.DegreeID,
                                          departmentid = lab.DepartmentID,
                                          specializationid = lab.SpecializationID,
                                          degree = deg.degree,
                                          specialization = spec.specializationName,
                                          department = dep.departmentName,
                                          year = lab.Year,
                                          semister = lab.Semester,
                                          Labname = lab.LabName,
                                          LabCode = lab.Labcode.Trim().ToUpper()
                                      }).ToList();
                }
            }



            ObjPhysicalLab = ObjPhysicalLab.GroupBy(e => new { e.LabCode, e.departmentid }).Select(e => new physicalLab
            {
                Labid = e.FirstOrDefault().Labid,
                collegeId = e.FirstOrDefault().collegeId,
                degreeid = e.FirstOrDefault().degreeid,
                departmentid = e.FirstOrDefault().departmentid,
                specializationid = e.FirstOrDefault().specializationid,
                degree = e.FirstOrDefault().degree,
                specialization = e.FirstOrDefault().specialization,
                department = e.FirstOrDefault().department,
                year = e.FirstOrDefault().year,
                semister = e.FirstOrDefault().semister,
                Labname = e.FirstOrDefault().Labname,
                LabCode = e.FirstOrDefault().LabCode,

                physicalId = jntuh_physical_lab.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid).Select(d => d.Id).FirstOrDefault(),

                NoOfAvailabeLabs = jntuh_physical_lab.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid).Select(d => d.NoofLabs).FirstOrDefault()



            }).ToList();



            return View(ObjPhysicalLab);
        }

        #endregion



        public class physicalLab
        {
            public int? id { get; set; }
            public int Labid { get; set; }
            public int collegeId { get; set; }
            public int? degreeid { get; set; }
            public int departmentid { get; set; }
            public int? specializationid { get; set; }
            public string degree { get; set; }
            // public string degree { get; set; }
            public string department { get; set; }
            public string specialization { get; set; }
            public int? year { get; set; }
            public int? semister { get; set; }
            public string Labname { get; set; }
            public string LabCode { get; set; }

            public string Remarks { get; set; }
            //[Required(ErrorMessage = "*")]
            public int? NoOfRequiredLabs { get; set; }
            [Required(ErrorMessage = "*")]
            public int? NoOfAvailabeLabs { get; set; }
            public string PhysicalLab { get; set; }
            public int physicalId { get; set; }


        }

    }
}
