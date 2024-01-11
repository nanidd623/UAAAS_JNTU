using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using System.IO;
using System.Net;

namespace UAAAS.Controllers
{
    public class ExperimentsController : BaseController
    {
        //
        // GET: /Experiments/

        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId, int? pageNumber)
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

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
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
            //List<jntuh_lab_master> collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == 34 || specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();
            List<jntuh_lab_master_experments> collegeLabMaster = db.jntuh_lab_master_experments.AsNoTracking().Where(l => specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();

          //  var query = objEntities.Employee.Join(objEntities.Department, r => r.EmpId, p => p.EmpId, (r, p) => new { r.FirstName, r.LastName, p.DepartmentName });


            List<Lab> collegeLabMaster1 = (from E in db.jntuh_lab_master_experments.AsNoTracking()
                                                                  join EC in db.jntuh_college_experiments.AsNoTracking() on E.id equals EC.ExperimentId
                                                                   orderby E.DegreeID,E.DepartmentID,E.SpecializationID
                                                                   where (specializationIDs.Contains(E.SpecializationID))
                                                                   select new Lab
                                                                   { 
                                                                       id=E.id,
                                                                       degreeId = E.DegreeID, 
                                                                       departmentId = E.DepartmentID,
                                                                       specializationId=E.SpecializationID,
                                                                       year=E.Year,
                                                                       Semester=E.Semester,
                                                                       Labcode=E.Labcode,
                                                                       LabName=E.LabName,
                                                                       EquipmentName = E.ExperimentName != "" && E.ExperimentName!=null?E.ExperimentName:EC.ExperimentName,
                                                                   }).ToList();


            List<Lab> lstlaboratories = new List<Lab>();
            //if (Session["CollegeLabs"] == null)
            //{
            int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);

            var jntuh_college_Experiments = db.jntuh_college_experiments.AsNoTracking().Where(l => l.CollegeId == userCollegeID).ToList();
            string Experment = "";
            foreach (var item in collegeLabMaster)
            {
                if (item.Labcode == "TMP-EL" && CollegeAffiliationStatus == "Yes")
                {
                    //for (int i = 1; i <= PGEquipmentCount; i++)
                    //{
                    Lab lstlabs = new Lab();
                    lstlabs.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.Id).FirstOrDefault();
                    lstlabs.ExperimentAutoincrementID = item.id;
                    lstlabs.degree = item.jntuh_degree.degree;
                    lstlabs.department = item.jntuh_department.departmentName;
                    lstlabs.specializationName = item.jntuh_specialization.specializationName;
                    lstlabs.Semester = item.Semester;
                    lstlabs.year = item.Year;
                    lstlabs.Labcode = item.Labcode;
                    lstlabs.LabName = item.LabName;
                   // Experment=jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                    lstlabs.ExperimentName = item.ExperimentName != null && item.ExperimentName != "" ? item.ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                    lstlabs.ExperimentID = item.ExperimentNO;
                    lstlabs.collegeId = userCollegeID;
                    lstlaboratories.Add(lstlabs);
                    //}
                }
                //}
                else
                {
                    if (item.Labcode != "TMP-EL")
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.Id).FirstOrDefault();
                        lstlabs.ExperimentAutoincrementID = item.id;
                        lstlabs.degree = item.jntuh_degree.degree;
                        lstlabs.department = item.jntuh_department.departmentName;
                        lstlabs.specializationName = item.jntuh_specialization.specializationName;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.Year;
                        lstlabs.Labcode = item.Labcode;
                        lstlabs.LabName = item.LabName;
                        lstlabs.ExperimentName = item.ExperimentName != null && item.ExperimentName != "" ? item.ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                        lstlabs.ExperimentID = item.ExperimentNO;
                        lstlabs.collegeId = userCollegeID;
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
                return RedirectToAction("View", "Experiments");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LAB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "Experiments");
            }

            return View(lstlaboratories.OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ToList());
        }

        [HttpPost]
        public ActionResult AddEditRecord(string status, int? id, string ExpermtntName, string LabName)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (ModelState.IsValid)
            {
                if(status=="True")
                {
                    jntuh_lab_master_experments jntuh_lab_master_experments = db.jntuh_lab_master_experments.Find(id);
                    jntuh_college_experiments collegeexperiments = new jntuh_college_experiments();
                    collegeexperiments.CollegeId = userCollegeID;
                    collegeexperiments.ExperimentId = id;
                    collegeexperiments.ExperimentName = jntuh_lab_master_experments.ExperimentName != "" && jntuh_lab_master_experments.ExperimentName != null ? jntuh_lab_master_experments.ExperimentName : ExpermtntName;
                    collegeexperiments.LabName = jntuh_lab_master_experments.LabName != "" && jntuh_lab_master_experments.LabName != null ? jntuh_lab_master_experments.LabName : LabName;
                    collegeexperiments.ExperimentStatus = 1;
                    collegeexperiments.ExperimentNO = jntuh_lab_master_experments.ExperimentNO;
                    collegeexperiments.CreatedDate = DateTime.Now;
                    collegeexperiments.CreatedBY = userID;
                    db.jntuh_college_experiments.Add(collegeexperiments);
                    db.SaveChanges();
                    TempData["Success"] = "Lab Related details Added Successfully.";
                }
                else if (status == "False")
                {
                    var lab = db.jntuh_college_experiments.Where(l => l.ExperimentId == id && l.CollegeId == userCollegeID).Select(l => l).FirstOrDefault();
                    if (lab != null)
                    {
                        try
                        {
                            db.jntuh_college_experiments.Remove(lab);
                            db.SaveChanges();
                            TempData["Success"] = "Lab Related details Deleted Successfully.";
                        }
                        catch { }
                    }
                }
               
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK); 
            //return View();
           // return RedirectToAction("Index", "Experiments");
           // return Content("<script language='javascript' type='text/javascript'>alert('Thanks for Feedback!');</script>");
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && id != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            
            int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID).Select(e => e.specializationId).ToArray();
            List<jntuh_lab_master_experments> collegeLabMaster = db.jntuh_lab_master_experments.AsNoTracking().Where(l => specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();

            var query = db.jntuh_lab_master_experments.Select(i => i.Labcode);



            List<Lab> collegeLabMaster1 = (from E in db.jntuh_lab_master_experments.AsNoTracking()
                                           join EC in db.jntuh_college_experiments.AsNoTracking() on E.id equals EC.ExperimentId
                                           where (specializationIDs.Contains(E.SpecializationID) && EC.CollegeId == userCollegeID)
                                           select new Lab
                                           {
                                               id = E.id,
                                               degree = E.jntuh_degree.degree,
                                               department = E.jntuh_department.departmentName,
                                               degreeId = E.DegreeID,
                                               departmentId = E.DepartmentID,
                                               specializationId = E.SpecializationID,
                                               specializationName = E.jntuh_specialization.specializationName,
                                               year = E.Year,
                                               Semester = E.Semester,
                                               Labcode = E.Labcode,
                                               LabName = E.LabName,
                                               ExperimentName = E.ExperimentName != "" && E.ExperimentName != null ? E.ExperimentName : EC.ExperimentName,
                                           }).ToList();


            List<Lab> lstlaboratories = new List<Lab>();
            int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);

            var jntuh_college_Experiments = db.jntuh_college_experiments.AsNoTracking().Where(l => l.CollegeId == userCollegeID).ToList();
            var records = collegeLabMaster1.GroupBy(i => i.Labcode).ToList();

            var spclids = collegeLabMaster1.Select(it => it.specializationId).ToList();
            var colmaster = db.jntuh_lab_master_experments.ToList();
            var groupAll = collegeLabMaster.GroupBy(i => new { Labcode = i.Labcode, Year = i.Year, Semester = i.Semester }).ToList();
            foreach (var item in collegeLabMaster.GroupBy(i => new { Labcode = i.Labcode, year = i.Year, Semester = i.Semester, spclId = i.SpecializationID }))
            {
                var groupofrecords = collegeLabMaster1.Where(it => it.Labcode == item.Key.Labcode).ToList();

                if (item.FirstOrDefault().Labcode == "TMP-EL" && CollegeAffiliationStatus == "Yes")
                {
                    Lab lstlabs = new Lab();
                    lstlabs.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.FirstOrDefault().id && l.CollegeId == userCollegeID).Select(l => l.Id).FirstOrDefault();
                    lstlabs.ExperimentAutoincrementID = item.FirstOrDefault().id;
                    lstlabs.degree = item.FirstOrDefault().jntuh_degree.degree;
                    lstlabs.department = item.FirstOrDefault().jntuh_department.departmentName;
                    lstlabs.specializationName = item.FirstOrDefault().jntuh_specialization.specializationName;
                    lstlabs.specializationId = item.FirstOrDefault().jntuh_specialization.id;
                    lstlabs.Semester = item.FirstOrDefault().Semester;
                    lstlabs.year = item.FirstOrDefault().Year;
                    lstlabs.Labcode = ViewBag.labcode = item.FirstOrDefault().Labcode;
                    lstlabs.LabName = item.FirstOrDefault().LabName;
                    lstlabs.ExperimentName = item.FirstOrDefault().ExperimentName != null && item.FirstOrDefault().ExperimentName != "" ? item.FirstOrDefault().ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.FirstOrDefault().id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                    lstlabs.ExpCount = collegeLabMaster1.Where(i => i.Labcode == lstlabs.Labcode && i.year == lstlabs.year && i.Semester == lstlabs.Semester && i.specializationId == lstlabs.specializationId).ToList().Count;
                    lstlabs.MasterExpCount = colmaster.Where(it => it.Labcode == item.Key.Labcode && it.SpecializationID == lstlabs.specializationId && it.Semester == lstlabs.Semester && it.Year == lstlabs.year).Count();
                    lstlabs.ExperimentID = item.FirstOrDefault().ExperimentNO;
                    lstlabs.collegeId = userCollegeID;
                    lstlaboratories.Add(lstlabs);
                }
                else
                {
                    if (item.FirstOrDefault().Labcode != "TMP-EL")
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.FirstOrDefault().id && l.CollegeId == userCollegeID).Select(l => l.Id).FirstOrDefault();
                        lstlabs.ExperimentAutoincrementID = item.FirstOrDefault().id;
                        lstlabs.degree = item.FirstOrDefault().jntuh_degree.degree;
                        lstlabs.department = item.FirstOrDefault().jntuh_department.departmentName;
                        lstlabs.specializationName = item.FirstOrDefault().jntuh_specialization.specializationName;
                        lstlabs.specializationId = item.FirstOrDefault().jntuh_specialization.id;
                        lstlabs.Semester = item.FirstOrDefault().Semester;
                        lstlabs.year = item.FirstOrDefault().Year;
                        lstlabs.Labcode = ViewBag.labcode = item.FirstOrDefault().Labcode;
                        lstlabs.LabName = item.FirstOrDefault().LabName;
                        lstlabs.ExperimentName = item.FirstOrDefault().ExperimentName != null && item.FirstOrDefault().ExperimentName != "" ? item.FirstOrDefault().ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.FirstOrDefault().id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                        lstlabs.ExpCount = collegeLabMaster1.Where(i => i.Labcode == lstlabs.Labcode && i.year == lstlabs.year && i.Semester == lstlabs.Semester).ToList().Count;
                        lstlabs.MasterExpCount = colmaster.Where(it => it.Labcode == item.Key.Labcode && it.SpecializationID == lstlabs.specializationId && it.Semester == lstlabs.Semester && it.Year == lstlabs.year).Count();
                        lstlabs.ExperimentID = item.FirstOrDefault().ExperimentNO;
                        lstlabs.collegeId = userCollegeID;
                        lstlaboratories.Add(lstlabs);
                    }
                }
            }

            ViewBag.Laboratories = lstlaboratories;
            ViewBag.Count = lstlaboratories.Count();
            return View(lstlaboratories.OrderByDescending(l => l.ExpCount).ToList());
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Summary(string collegeId)
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

            

            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            

            int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID).Select(e => e.specializationId).ToArray();
            List<jntuh_lab_master_experments> collegeLabMaster = db.jntuh_lab_master_experments.AsNoTracking().Where(l => specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();

            var query = db.jntuh_lab_master_experments.Select(i => i.Labcode);



            List<Lab> collegeLabMaster1 = (from E in db.jntuh_lab_master_experments.AsNoTracking()
                                           join EC in db.jntuh_college_experiments.AsNoTracking() on E.id equals EC.ExperimentId
                                           where (specializationIDs.Contains(E.SpecializationID) && EC.CollegeId == userCollegeID)
                                           select new Lab
                                           {
                                               id = E.id,
                                               degree=E.jntuh_degree.degree,
                                               department = E.jntuh_department.departmentName,
                                               degreeId = E.DegreeID,
                                               departmentId = E.DepartmentID,
                                               specializationId = E.SpecializationID,
                                               specializationName=E.jntuh_specialization.specializationName,
                                               year = E.Year,
                                               Semester = E.Semester,
                                               Labcode = E.Labcode,
                                               LabName = E.LabName,
                                               ExperimentName = E.ExperimentName != "" && E.ExperimentName != null ? E.ExperimentName : EC.ExperimentName,
                                           }).ToList();

            
            List<Lab> lstlaboratories = new List<Lab>();
            int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);

            var jntuh_college_Experiments = db.jntuh_college_experiments.AsNoTracking().Where(l => l.CollegeId == userCollegeID).ToList();
            var records = collegeLabMaster1.GroupBy(i => i.Labcode).ToList();

            var spclids = collegeLabMaster1.Select(it => it.specializationId).ToList();
            var colmaster = db.jntuh_lab_master_experments.ToList();
            var groupAll = collegeLabMaster.GroupBy(i => new { Labcode = i.Labcode, Year = i.Year, Semester = i.Semester }).ToList();
            foreach (var item in collegeLabMaster.GroupBy(i => new { Labcode = i.Labcode, year = i.Year, Semester = i.Semester,spclId=i.SpecializationID }))
            {
                var groupofrecords = collegeLabMaster1.Where(it => it.Labcode == item.Key.Labcode).ToList();

                if (item.FirstOrDefault().Labcode == "TMP-EL" && CollegeAffiliationStatus == "Yes")
                {
                    Lab lstlabs = new Lab();
                    lstlabs.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.FirstOrDefault().id && l.CollegeId == userCollegeID).Select(l => l.Id).FirstOrDefault();
                    lstlabs.ExperimentAutoincrementID = item.FirstOrDefault().id;
                    lstlabs.degree = item.FirstOrDefault().jntuh_degree.degree;
                    lstlabs.department = item.FirstOrDefault().jntuh_department.departmentName;
                    lstlabs.specializationName = item.FirstOrDefault().jntuh_specialization.specializationName;
                    lstlabs.specializationId = item.FirstOrDefault().jntuh_specialization.id;
                    lstlabs.Semester = item.FirstOrDefault().Semester;
                    lstlabs.year = item.FirstOrDefault().Year;
                    lstlabs.Labcode = ViewBag.labcode = item.FirstOrDefault().Labcode;
                    lstlabs.LabName = item.FirstOrDefault().LabName;
                    lstlabs.ExperimentName = item.FirstOrDefault().ExperimentName != null && item.FirstOrDefault().ExperimentName != "" ? item.FirstOrDefault().ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.FirstOrDefault().id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                    lstlabs.ExpCount = collegeLabMaster1.Where(i=>i.Labcode == lstlabs.Labcode && i.year == lstlabs.year && i.Semester == lstlabs.Semester && i.specializationId==lstlabs.specializationId).ToList().Count;
                    lstlabs.MasterExpCount = colmaster.Where(it => it.Labcode == item.Key.Labcode && it.SpecializationID == lstlabs.specializationId && it.Semester == lstlabs.Semester && it.Year == lstlabs.year).Count();
                    lstlabs.ExperimentID = item.FirstOrDefault().ExperimentNO;
                    lstlabs.collegeId = userCollegeID;
                    lstlaboratories.Add(lstlabs);
                }
                else
                {
                    if (item.FirstOrDefault().Labcode != "TMP-EL")
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.FirstOrDefault().id && l.CollegeId == userCollegeID).Select(l => l.Id).FirstOrDefault();
                        lstlabs.ExperimentAutoincrementID = item.FirstOrDefault().id;
                        lstlabs.degree = item.FirstOrDefault().jntuh_degree.degree;
                        lstlabs.department = item.FirstOrDefault().jntuh_department.departmentName;
                        lstlabs.specializationName = item.FirstOrDefault().jntuh_specialization.specializationName;
                        lstlabs.specializationId = item.FirstOrDefault().jntuh_specialization.id;
                        lstlabs.Semester = item.FirstOrDefault().Semester;
                        lstlabs.year = item.FirstOrDefault().Year;
                        lstlabs.Labcode = ViewBag.labcode = item.FirstOrDefault().Labcode;
                        lstlabs.LabName = item.FirstOrDefault().LabName;
                        lstlabs.ExperimentName = item.FirstOrDefault().ExperimentName != null && item.FirstOrDefault().ExperimentName != "" ? item.FirstOrDefault().ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.FirstOrDefault().id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                        lstlabs.ExpCount = collegeLabMaster1.Where(i => i.Labcode == lstlabs.Labcode && i.year == lstlabs.year && i.Semester == lstlabs.Semester).ToList().Count;
                        lstlabs.MasterExpCount = colmaster.Where(it => it.Labcode == item.Key.Labcode && it.SpecializationID == lstlabs.specializationId && it.Semester == lstlabs.Semester && it.Year == lstlabs.year).Count();
                        lstlabs.ExperimentID = item.FirstOrDefault().ExperimentNO;
                        lstlabs.collegeId = userCollegeID;
                        lstlaboratories.Add(lstlabs);
                    }
                }
            }

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.iseditable = false;
                return View(lstlaboratories.OrderByDescending(l => l.ExpCount).ToList());
            }
            else
                ViewBag.iseditable = true;
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LAB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                ViewBag.iseditable = false;
                return View(lstlaboratories.OrderByDescending(l => l.ExpCount).ToList());
            }   

            return View(lstlaboratories.OrderByDescending(l => l.ExpCount).ToList());
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(string labcode, int? spclid, int year, int? semester)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && e.specializationId == spclid).Select(e => e.specializationId).ToArray();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            List<jntuh_lab_master_experments> collegeLabMaster = db.jntuh_lab_master_experments.AsNoTracking().Where(l => l.Labcode == labcode && specializationIDs.Contains(l.SpecializationID) && l.Year == year && l.Semester == semester).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();
            var query = db.jntuh_lab_master_experments.Select(i => i.Labcode);
            List<Lab> collegeLabMaster1 = (from E in db.jntuh_lab_master_experments.AsNoTracking()
                                           join EC in db.jntuh_college_experiments.AsNoTracking() on E.id equals EC.ExperimentId
                                           where (query.Contains(E.Labcode) && specializationIDs.Contains(E.SpecializationID) && EC.CollegeId == userCollegeID)
                                           select new Lab
                                           {
                                               Labcode = E.Labcode,
                                               LabName = E.LabName,
                                               ExperimentName = E.ExperimentName
                                           }).ToList();
            var jntuh_college_Experiments = db.jntuh_college_experiments.AsNoTracking().Where(l => l.CollegeId == userCollegeID).ToList();
            var labcodenames = collegeLabMaster1.Where(i => i.Labcode == labcode).ToList();
            List<Lab> lstlaboratories = new List<Lab>();
            //foreach (var item in labcodenames)
            //{
            //    var lst = new Lab()
            //    {
            //        LabName = item.LabName,
            //        ExperimentName = item.ExperimentName
            //    };
            //    lstlaboratories.Add(lst);
            //}

            foreach (var item in collegeLabMaster)
            {
                if (item.Labcode == "TMP-EL" && CollegeAffiliationStatus == "Yes")
                {
                    //for (int i = 1; i <= PGEquipmentCount; i++)
                    //{
                    Lab lstlabs = new Lab();
                    lstlabs.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.Id).FirstOrDefault();
                    lstlabs.ExperimentAutoincrementID = item.id;
                    lstlabs.degree = item.jntuh_degree.degree;
                    lstlabs.department = item.jntuh_department.departmentName;
                    lstlabs.specializationName = item.jntuh_specialization.specializationName;
                    lstlabs.specializationId = item.jntuh_specialization.id;
                    lstlabs.Semester = item.Semester;
                    lstlabs.year = item.Year;
                    lstlabs.Labcode = item.Labcode;
                    lstlabs.LabName = item.LabName != null && item.LabName != "" ? item.LabName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.LabName).FirstOrDefault();
                    lstlabs.ExperimentName = item.ExperimentName != null && item.ExperimentName != "" ? item.ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                    lstlabs.ExperimentID = item.ExperimentNO;
                    lstlabs.collegeId = userCollegeID;
                    lstlaboratories.Add(lstlabs);
                }
                else
                {
                    if (item.Labcode != "TMP-EL")
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.Id).FirstOrDefault();
                        lstlabs.ExperimentAutoincrementID = item.id;
                        lstlabs.degree = item.jntuh_degree.degree;
                        lstlabs.department = item.jntuh_department.departmentName;
                        lstlabs.specializationName = item.jntuh_specialization.specializationName;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.Year;
                        lstlabs.Labcode = item.Labcode;
                        lstlabs.LabName = item.LabName != null && item.LabName != "" ? item.LabName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.LabName).FirstOrDefault();
                        lstlabs.ExperimentName = item.ExperimentName != null && item.ExperimentName != "" ? item.ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                        lstlabs.ExperimentID = item.ExperimentNO;
                        lstlabs.collegeId = userCollegeID;
                        lstlaboratories.Add(lstlabs);
                    }

                }
            }
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.iseditable = false;
                return RedirectToAction("Summary");
            }
            else
                ViewBag.iseditable = true;
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LAB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                ViewBag.iseditable = false;
                return RedirectToAction("Summary");
            }   

            return View("SummaryOfLabExperiments", lstlaboratories);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DetailsView(string labcode, int? spclid, int year, int? semester, string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            ViewBag.collegeId = collegeId;
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && e.specializationId == spclid).Select(e => e.specializationId).ToArray();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == userCollegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            List<jntuh_lab_master_experments> collegeLabMaster = db.jntuh_lab_master_experments.AsNoTracking().Where(l => l.Labcode == labcode && specializationIDs.Contains(l.SpecializationID) && l.Year == year && l.Semester == semester).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID }).ToList();
            var query = db.jntuh_lab_master_experments.Select(i => i.Labcode);
            List<Lab> collegeLabMaster1 = (from E in db.jntuh_lab_master_experments.AsNoTracking()
                                           join EC in db.jntuh_college_experiments.AsNoTracking() on E.id equals EC.ExperimentId
                                           where (query.Contains(E.Labcode) && specializationIDs.Contains(E.SpecializationID) && EC.CollegeId == userCollegeID)
                                           select new Lab
                                           {
                                               Labcode = E.Labcode,
                                               LabName = E.LabName,
                                               ExperimentName = E.ExperimentName
                                           }).ToList();
            var jntuh_college_Experiments = db.jntuh_college_experiments.AsNoTracking().Where(l => l.CollegeId == userCollegeID).ToList();
            var labcodenames = collegeLabMaster1.Where(i => i.Labcode == labcode).ToList();
            List<Lab> lstlaboratories = new List<Lab>();
            
            foreach (var item in collegeLabMaster)
            {
                if (item.Labcode == "TMP-EL" && CollegeAffiliationStatus == "Yes")
                {
                    Lab lstlabs = new Lab();
                    lstlabs.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.Id).FirstOrDefault();
                    lstlabs.ExperimentAutoincrementID = item.id;
                    lstlabs.degree = item.jntuh_degree.degree;
                    lstlabs.department = item.jntuh_department.departmentName;
                    lstlabs.specializationName = item.jntuh_specialization.specializationName;
                    lstlabs.specializationId = item.jntuh_specialization.id;
                    lstlabs.Semester = item.Semester;
                    lstlabs.year = item.Year;
                    lstlabs.Labcode = item.Labcode;
                    lstlabs.LabName = item.LabName != null && item.LabName != "" ? item.LabName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.LabName).FirstOrDefault();
                    lstlabs.ExperimentName = item.ExperimentName != null && item.ExperimentName != "" ? item.ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                    lstlabs.ExperimentID = item.ExperimentNO;
                    lstlabs.collegeId = userCollegeID;
                    lstlaboratories.Add(lstlabs);
                }
                else
                {
                    if (item.Labcode != "TMP-EL")
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.Id).FirstOrDefault();
                        lstlabs.ExperimentAutoincrementID = item.id;
                        lstlabs.degree = item.jntuh_degree.degree;
                        lstlabs.department = item.jntuh_department.departmentName;
                        lstlabs.specializationName = item.jntuh_specialization.specializationName;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.Year;
                        lstlabs.Labcode = item.Labcode;
                        lstlabs.LabName = item.LabName != null && item.LabName != "" ? item.LabName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.LabName).FirstOrDefault();
                        lstlabs.ExperimentName = item.ExperimentName != null && item.ExperimentName != "" ? item.ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.ExperimentName).FirstOrDefault();
                        lstlabs.ExperimentID = item.ExperimentNO;
                        lstlabs.collegeId = userCollegeID;
                        lstlaboratories.Add(lstlabs);
                    }

                }
            }
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();

            return View("DetailsView", lstlaboratories);
        }
    }
}
