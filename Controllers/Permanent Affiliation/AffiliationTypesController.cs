using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.Permanent_Affiliation
{
    [ErrorHandling]
    public class AffiliationTypesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private uaaasDBContext db1 = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;
            var status = GetPageEditableStatus(userCollegeID);
            //if (userCollegeID > 0 && Roles.IsUserInRole("College"))
            //if (false)
            if (status == 0)
            {
                return RedirectToAction("View", "AffiliationTypes");
            }
            //if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            else
            {
                return RedirectToAction("Edit", "AffiliationTypes", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            #region Unreachable Code

            //if (userCollegeID == 0 && status == 0 && Roles.IsUserInRole("College"))
            //{
            //    ViewBag.NotUpload = true;
            //}
            //else
            //{
            //    ViewBag.NotUpload = false;
            //}
            //CollegeInformation collegeInformation = new CollegeInformation();

            //string[] strSelected = new string[] { };

            //List<Item> lstType = new List<Item>();

            //foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder))
            //{
            //    string strType = t.id.ToString();
            //    lstType.Add(new Item { id = t.id, name = t.affiliationType, selected = strSelected.Contains(strType) ? 1 : 0 });
            //}

            //collegeInformation.affiliationType = lstType;

            ////collegeInformation.affiliationFromDate1 = new string[] { string.Empty };
            //collegeInformation.affiliationFromDate2 = new string[] { string.Empty };
            ////collegeInformation.affiliationFromDate3 = new string[] { string.Empty };
            //collegeInformation.affiliationFromDate4 = new string[] { string.Empty };
            //collegeInformation.affiliationFromDate5 = new string[] { string.Empty };

            ////collegeInformation.affiliationToDate1 = new string[] { string.Empty };
            //collegeInformation.affiliationToDate2 = new string[] { string.Empty };
            ////collegeInformation.affiliationToDate3 = new string[] { string.Empty };
            //collegeInformation.affiliationToDate4 = new string[] { string.Empty };
            //collegeInformation.affiliationToDate5 = new string[] { string.Empty };

            ////collegeInformation.affiliationDuration1 = new string[] { string.Empty };
            ////collegeInformation.affiliationDuration2 = new string[] { string.Empty };
            ////collegeInformation.affiliationDuration3 = new string[] { string.Empty };
            //collegeInformation.affiliationDuration4 = new string[] { string.Empty };
            //collegeInformation.affiliationDuration5 = new string[] { string.Empty };


            //return View(collegeInformation);

            #endregion Unreachable Code
        }

        // POST: /CollegeInformation/Create
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(CollegeInformation collegeInformation)
        {
            SaveCollegeInformation(collegeInformation);

            TempData["Success"] = "Added successfully";

            return View(collegeInformation);
        }

        private void SaveCollegeInformation(CollegeInformation collegeInformation)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeInformation.id;
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var collegeCode = db.jntuh_college.Where(c => c.collegeCode == collegeInformation.collegeCode).Select(c => c.collegeCode).FirstOrDefault();
            var collegeID = collegeInformation.id;
            collegeInformation.affiliationFromDate1 = collegeInformation.affiliationFromDate1;
            collegeInformation.affiliationFromDate3 = collegeInformation.affiliationFromDate3;
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();


            collegeInformation.affiliationToDate1 = collegeInformation.affiliationToDate1;
            collegeInformation.affiliationToDate3 = collegeInformation.affiliationToDate3;
            collegeInformation.affiliationDuration1 = collegeInformation.affiliationDuration1;
            collegeInformation.affiliationDuration3 = collegeInformation.affiliationDuration3;

            //if college code exists & college id is ZERO then do not insert college record
            if (collegeCode != null && collegeID == 0)
            {
                TempData["Error"] = "College Code already exists";
            }
            else
            {
                int createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                if (collegeID != 0)
                {
                    if (collegeInformation.affiliationSelected1 != 0)
                    {
                        //create college affiliation
                        jntuh_college_affiliation jntuh_college_affiliation = new jntuh_college_affiliation();
                        jntuh_college_affiliation.collegeId = collegeID;

                        //File Saving
                        if (collegeInformation.affiliationfile1 != null)
                        {
                            string affiliationfile = "~/Content/Upload/College/UGC";
                            if (!Directory.Exists(Server.MapPath(affiliationfile)))
                            {
                                Directory.CreateDirectory(Server.MapPath(affiliationfile));
                            }
                            var ext = Path.GetExtension(collegeInformation.affiliationfile1.FileName);
                            if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                            {
                                if (collegeInformation.affiliationfilepath1 == null)
                                {
                                    string fileName = collegeInformation.collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                    collegeInformation.affiliationfile1.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                                    collegeInformation.affiliationfilepath1 = string.Format("{0}{1}", fileName, ext);
                                    jntuh_college_affiliation.filePath = collegeInformation.affiliationfilepath1;
                                }
                                else
                                {
                                    collegeInformation.affiliationfile1.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), collegeInformation.affiliationfilepath1));
                                    jntuh_college_affiliation.filePath = collegeInformation.affiliationfilepath1;
                                }
                            }
                        }
                        else
                        {
                            jntuh_college_affiliation.filePath = collegeInformation.affiliationfilepath1;
                        }
                        jntuh_college_affiliation.affiliationTypeId = Convert.ToInt32(collegeInformation.affiliationTypeId1);
                        if (collegeInformation.affiliationFromDate1 != null)
                        {

                            jntuh_college_affiliation.affiliationFromDate = Utilities.DDMMYY2MMDDYY(collegeInformation.affiliationFromDate1.ToString());

                        }
                        else
                        {
                            jntuh_college_affiliation.affiliationFromDate = null;
                        }
                        if (collegeInformation.affiliationToDate1 != null)
                        {
                            jntuh_college_affiliation.affiliationToDate =
                                Utilities.DDMMYY2MMDDYY(collegeInformation.affiliationToDate1.ToString());

                        }
                        else
                        {
                            jntuh_college_affiliation.affiliationToDate = null;
                        }
                        int? affiliationDuration = Convert.ToInt32(collegeInformation.affiliationDuration1);
                        if (affiliationDuration != null && collegeInformation.affiliationSelected1 == 1)
                        {
                            jntuh_college_affiliation.affiliationDuration = affiliationDuration;
                        }
                        jntuh_college_affiliation.filePath = collegeInformation.affiliationfilepath1;

                        if (collegeInformation.affiliationSelected1 == 1)
                        {
                            jntuh_college_affiliation.affiliationStatus = "Yes";
                        }
                        else
                        {
                            jntuh_college_affiliation.affiliationStatus = "No";
                        }

                        jntuh_college_affiliation.affiliationGrade = string.Empty;
                        jntuh_college_affiliation.CGPA = string.Empty;

                        var affiliationID = db.jntuh_college_affiliation
                                              .Where(a => a.collegeId == collegeID && a.affiliationTypeId == jntuh_college_affiliation.affiliationTypeId)
                                              .Select(a => a.id).FirstOrDefault();
                        if (affiliationID == 0)      //if affiliation id = 0; then insert the college affiliation record
                        {
                            jntuh_college_affiliation.createdBy = createdBy;
                            jntuh_college_affiliation.createdOn = DateTime.Now;
                            db.jntuh_college_affiliation.Add(jntuh_college_affiliation);
                        }
                        else                            //if affiliation id exists then modify the existing college affiliation record
                        {
                            jntuh_college_affiliation.id = affiliationID;
                            jntuh_college_affiliation.createdBy = db.jntuh_college_affiliation.Where(a => a.id == affiliationID).Select(a => a.createdBy).FirstOrDefault();
                            jntuh_college_affiliation.createdOn = db.jntuh_college_affiliation.Where(a => a.id == affiliationID).Select(a => a.createdOn).FirstOrDefault();
                            jntuh_college_affiliation.updatedBy = createdBy;
                            jntuh_college_affiliation.updatedOn = DateTime.Now;
                            db.Entry(jntuh_college_affiliation).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                    }

                    if (collegeInformation.affiliationSelected3 != 0)
                    {
                        //create college affiliation
                        jntuh_college_affiliation jntuh_college_affiliation = new jntuh_college_affiliation();
                        jntuh_college_affiliation.collegeId = collegeID;

                        //File Saving
                        if (collegeInformation.affiliationfile3 != null)
                        {
                            string affiliationfile = "~/Content/Upload/College/NAAC";
                            if (!Directory.Exists(Server.MapPath(affiliationfile)))
                            {
                                Directory.CreateDirectory(Server.MapPath(affiliationfile));
                            }
                            var ext = Path.GetExtension(collegeInformation.affiliationfile3.FileName);
                            if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                            {
                                if (collegeInformation.affiliationfilepath3 == null)
                                {
                                    string fileName = collegeInformation.collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "NAAC";
                                    collegeInformation.affiliationfile3.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                                    collegeInformation.affiliationfilepath3 = string.Format("{0}{1}", fileName, ext);
                                    jntuh_college_affiliation.filePath = collegeInformation.affiliationfilepath3;
                                }
                                else
                                {
                                    collegeInformation.affiliationfile3.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), collegeInformation.affiliationfilepath3));
                                    jntuh_college_affiliation.filePath = collegeInformation.affiliationfilepath3;
                                }

                            }
                        }
                        else
                        {
                            jntuh_college_affiliation.filePath = collegeInformation.affiliationfilepath3;
                        }

                        jntuh_college_affiliation.affiliationTypeId = Convert.ToInt32(collegeInformation.affiliationTypeId3);
                        if (collegeInformation.affiliationFromDate3 != null)
                        {
                            jntuh_college_affiliation.affiliationFromDate = Utilities.DDMMYY2MMDDYY(collegeInformation.affiliationFromDate3);
                        }
                        else
                        {
                            jntuh_college_affiliation.affiliationFromDate = null;
                        }
                        if (collegeInformation.affiliationToDate3 != null)
                        {
                            jntuh_college_affiliation.affiliationToDate = Utilities.DDMMYY2MMDDYY(collegeInformation.affiliationToDate3);

                        }
                        else
                        {
                            jntuh_college_affiliation.affiliationToDate = null;
                        }
                        int? affiliationDuration = Convert.ToInt32(collegeInformation.affiliationDuration3
                                                                                     );
                        if (affiliationDuration != null && collegeInformation.affiliationSelected3 == 1)
                        {
                            jntuh_college_affiliation.affiliationDuration = affiliationDuration;
                        }

                        if (collegeInformation.affiliationSelected3 == 1)
                        {
                            jntuh_college_affiliation.affiliationStatus = "Yes";
                        }
                        else
                        {
                            jntuh_college_affiliation.affiliationStatus = "No";
                        }

                        string type = db.jntuh_affiliation_type
                                        .Where(t => t.id == jntuh_college_affiliation.affiliationTypeId).Select(t => t.affiliationType).FirstOrDefault();
                        if (type != "0" && collegeInformation.affiliationSelected3 == 1)
                        {
                            jntuh_college_affiliation.affiliationGrade = collegeInformation.affiliationGrade;
                            jntuh_college_affiliation.CGPA = collegeInformation.affiliationCGPA;
                        }
                        else
                        {
                            jntuh_college_affiliation.affiliationGrade = string.Empty;
                            jntuh_college_affiliation.CGPA = string.Empty;
                        }

                        var affiliationID = db.jntuh_college_affiliation
                                              .Where(a => a.collegeId == collegeID && a.affiliationTypeId == jntuh_college_affiliation.affiliationTypeId)
                                              .Select(a => a.id).FirstOrDefault();

                        if (affiliationID == 0)      //if affiliation id = 0; then insert the college affiliation record
                        {
                            jntuh_college_affiliation.createdBy = createdBy;
                            jntuh_college_affiliation.createdOn = DateTime.Now;
                            db.jntuh_college_affiliation.Add(jntuh_college_affiliation);
                        }
                        else                            //if affiliation id exists then modify the existing college affiliation record
                        {
                            jntuh_college_affiliation.id = affiliationID;
                            jntuh_college_affiliation.createdBy = db.jntuh_college_affiliation.Where(a => a.id == affiliationID).Select(a => a.createdBy).FirstOrDefault();
                            jntuh_college_affiliation.createdOn = db.jntuh_college_affiliation.Where(a => a.id == affiliationID).Select(a => a.createdOn).FirstOrDefault();
                            jntuh_college_affiliation.updatedBy = createdBy;
                            jntuh_college_affiliation.updatedOn = DateTime.Now;
                            db.Entry(jntuh_college_affiliation).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                    }
                }
            }

            string selectedAffiliationType1 = string.Empty;

            if (collegeInformation.affiliationFromDate1 != null)
                selectedAffiliationType1 = collegeInformation.affiliationTypeId1;

            string selectedAffiliationType3 = string.Empty;

            if (collegeInformation.affiliationFromDate3 != null)
                selectedAffiliationType3 = collegeInformation.affiliationTypeId3;


            List<Item> lstType = new List<Item>();
            foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder))
            {
                string strType = t.id.ToString();
                lstType.Add(new Item { id = t.id, name = t.affiliationType });
            }

            collegeInformation.affiliationType = lstType;


            int[] selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder)
                                                                      .Select(s => s.id).ToArray();
            int affiliationCount = 1;
            foreach (var item in selectedAffiliationId)
            {
                if (affiliationCount == 1)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeID && a.affiliationTypeId == item)
                                                                    .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Yes")
                    {
                        collegeInformation.affiliationSelected1 = 1;
                    }
                    else
                    {
                        collegeInformation.affiliationSelected1 = 2;
                    }
                }
                else if (affiliationCount == 2)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeID && a.affiliationTypeId == item)
                                                                    .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Yes")
                    {
                        collegeInformation.affiliationSelected2 = 1;
                    }
                    else
                    {
                        collegeInformation.affiliationSelected2 = 2;
                    }
                }

                else if (affiliationCount == 3)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeID && a.affiliationTypeId == item)
                                                                    .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Yes")
                    {
                        collegeInformation.affiliationSelected3 = 1;
                    }
                    else
                    {
                        collegeInformation.affiliationSelected3 = 2;
                    }
                }
                else if (affiliationCount == 4)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeID && a.affiliationTypeId == item)
                                                                    .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Conferred")
                    {
                        collegeInformation.affiliationSelected4 = 1;
                    }
                    else if (statusType == "Applied")
                    {
                        collegeInformation.affiliationSelected4 = 2;
                    }
                    else
                    {
                        collegeInformation.affiliationSelected4 = 3;
                    }
                }
                else if (affiliationCount == 5)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeID && a.affiliationTypeId == item)
                                                                     .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Conferred")
                    {
                        collegeInformation.affiliationSelected5 = 1;
                    }
                    else if (statusType == "Applied")
                    {
                        collegeInformation.affiliationSelected5 = 2;
                    }
                    else
                    {
                        collegeInformation.affiliationSelected5 = 3;
                    }
                }
                affiliationCount++;
            }
        }

        // GET: /CollegeInformation/Edit/5
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "AffiliationTypes");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;

            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            //if(true)
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "AffiliationTypes");
            }
            else
            {
                ViewBag.IsEditable = true;

                ////RAMESH:To-DisableEdit
                //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
                //{
                //    ViewBag.IsEditable = false;
                //    return RedirectToAction("View", "CollegeInformation");
                //}
                //else
                //{
                //    ViewBag.IsEditable = true;
                //}

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PAT") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "AffiliationTypes");
                }
            }

            CollegeInformation collegeInformation = new CollegeInformation();

            jntuh_college jntuh_college = db.jntuh_college.Find(userCollegeID);

            //collegeInformation.collegeStatusID = jntuh_college.collegeStatusID;

            //var collegestatus =
            //                 db.jntuh_college_status.Where(c => c.id == collegeInformation.collegeStatusID)
            //                     .Select(s => s.collegeStatus)
            //                     .FirstOrDefault();
            //if (collegestatus == "Minority")
            //{
            //    //jntuh_college_minoritystatus minoritystatus =
            //    //    db.jntuh_college_minoritystatus.Where(r => r.collegeId == userCollegeID && r.statusTodate > todayDate)
            //    //        .Select(s => s)
            //    //        .FirstOrDefault();
            //    jntuh_college_minoritystatus minoritystatus =
            //        db.jntuh_college_minoritystatus.Where(r => r.collegeId == userCollegeID).OrderByDescending(r => r.statusTodate).Take(1)
            //            .Select(s => s)
            //            .FirstOrDefault();
            //    if (minoritystatus != null)
            //    {
            //        collegeInformation.collegeminorityStatus = db.jntuh_college_status.Where(s => s.id == minoritystatus.collegeStatusid).Select(s => s.collegeStatus).FirstOrDefault();
            //        collegeInformation.minortyid = minoritystatus.id;
            //        collegeInformation.academicYearid = minoritystatus.academicYearid;
            //        collegeInformation.collegesubstatusId = minoritystatus.collegeStatusid;
            //        DateTime fromdate = Convert.ToDateTime(minoritystatus.statusFromdate);
            //        collegeInformation.collegestatusfromdate = fromdate.ToString("dd/MM/yyyy").Split(' ')[0];
            //        DateTime todate = Convert.ToDateTime(minoritystatus.statusTodate);
            //        collegeInformation.collegestatustodate = todate.ToString("dd/MM/yyyy").Split(' ')[0];
            //        collegeInformation.collegestatusfilepath = minoritystatus.statusFile;

            //    }
            //}
            //collegeInformation.collegeStatus = db.jntuh_college_status.Where(s => s.id == collegeInformation.collegeStatusID).Select(s => s.collegeStatus).FirstOrDefault();

            string[] selectedAffiliationType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id).OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationTypeId).ToArray().Select(s => s.ToString()).ToArray();
            List<Item> lstType = new List<Item>();
            foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder).ToList())
            {
                string strType = t.id.ToString();
                lstType.Add(new Item { id = t.id, name = t.affiliationType, selected = selectedAffiliationType.Contains(strType) ? 1 : 0 });
            }
            // lstType = lstType.Where(l => l.id != 5).ToList();
            var aff_item = lstType.Where(l => l.id == 5).ToList();
            if (aff_item.Count > 0)
            {
                var PA_item = lstType[0];
                lstType.RemoveAt(0);
                lstType.Insert(2, PA_item);
            }

            //int[] selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder)
            //                                                          .Select(s => s.id).ToArray();

            //var pa_item = selectedAffiliationId.Where(l => l == 5).ToArray();
            //if (pa_item.Count() > 0)
            //{
            //    selectedAffiliationId = selectedAffiliationId.Where(l => l != 5).ToArray();
            //    selectedAffiliationId[selectedAffiliationId.GetUpperBound(0) + 1] = 5;
            //}
            var selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder)
                                                                      .Select(s => s.id).ToList();
            //var pa_item = selectedAffiliationId.con
            if (selectedAffiliationId.Contains(5))
            {
                //var PA_item = lstType[0];
                selectedAffiliationId.RemoveAt(0);
                selectedAffiliationId.Add(5);
            }
            int affiliationCount = 1;
            foreach (var item in selectedAffiliationId)
            {
                if (affiliationCount == 1)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Yes")
                    {
                        collegeInformation.affiliationSelected1 = 1;
                    }
                    else
                    {
                        collegeInformation.affiliationSelected1 = 2;
                    }
                }
                else if (affiliationCount == 2)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                   .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Yes")
                    {
                        collegeInformation.affiliationSelected3 = 1;
                    }
                    else
                    {
                        collegeInformation.affiliationSelected3 = 2;
                    }
                }

                else if (affiliationCount == 3)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                   .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Yes")
                    {
                        collegeInformation.affiliationSelected5 = 1;
                    }
                    else
                    {
                        collegeInformation.affiliationSelected5 = 2;
                    }
                }
                else if (affiliationCount == 4)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Conferred")
                    {
                        collegeInformation.affiliationSelected4 = 1;
                    }
                    else if (statusType == "Applied")
                    {
                        collegeInformation.affiliationSelected4 = 2;
                    }
                    else
                    {
                        collegeInformation.affiliationSelected4 = 3;
                    }
                }
                else if (affiliationCount == 5)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                     .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Conferred")
                    {
                        collegeInformation.affiliationSelected5 = 1;
                    }
                    else if (statusType == "Applied")
                    {
                        collegeInformation.affiliationSelected5 = 2;
                    }
                    else
                    {
                        collegeInformation.affiliationSelected5 = 3;
                    }
                }
                affiliationCount++;
            }
            collegeInformation.affiliationType = lstType;

            int rowIndex = 1;
            foreach (var type in lstType)
            {
                int affiliationType = type.id;

                if (rowIndex == 1)
                {
                    collegeInformation.affiliationId1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.id).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationFromDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationFromDate).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationToDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationToDate).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationDuration1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationDuration).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationfilepath1 =
                        db.jntuh_college_affiliation.Where(
                            a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                            .Select(a => a.filePath).FirstOrDefault();
                    var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1);
                    collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                    collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                }

                if (rowIndex == 2)
                {
                    collegeInformation.affiliationId3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.id).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationFromDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                               .Select(a => a.affiliationFromDate).ToArray()
                                                               .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationToDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationToDate).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationDuration3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationDuration).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationfilepath3 =
                      db.jntuh_college_affiliation.Where(
                          a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                          .Select(a => a.filePath).FirstOrDefault();
                    var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1);
                    collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                    collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();

                    //collegeInformation.affiliationFromDate2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                    //                                            .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                    //                                            .Select(s => s.ToString()).ToArray();
                    //collegeInformation.affiliationToDate2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                    //                                            .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                    //                                            .Select(s => s.ToString()).ToArray();
                    //collegeInformation.affiliationDuration2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                    //                                            .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                    //                                            .Select(s => s.ToString()).ToArray();

                    //var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType);
                    //collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                    //collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                }

                if (rowIndex == 3)
                {
                    collegeInformation.affiliationId5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.id).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.pa_affiliationFromDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationFromDate).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.pa_affiliationToDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationToDate).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.pa_affiliationDuration5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationDuration).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.pa_affiliationfilepath5 =
                        db.jntuh_college_affiliation.Where(
                            a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                            .Select(a => a.filePath).FirstOrDefault();
                    //var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).OrderByDescending(a => a.affiliationToDate).Take(1);
                    //collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                    //collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                }
                if (rowIndex == 4)
                {
                    collegeInformation.affiliationFromDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationFromDate).ToArray()
                                                                .Select(s => s.ToString()).ToArray();
                    collegeInformation.affiliationToDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationToDate).ToArray()
                                                                .Select(s => s.ToString()).ToArray();
                    collegeInformation.affiliationDuration4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                .Select(a => a.affiliationDuration).ToArray()
                                                                .Select(s => s.ToString()).ToArray();

                    var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1);
                    collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                    collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                }
                //if (rowIndex == 5)
                //{
                //    collegeInformation.affiliationFromDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).OrderByDescending(a => a.affiliationToDate).Take(1)
                //                                                .Select(a => a.affiliationFromDate).ToArray()
                //                                                .Select(s => s.ToString()).ToArray();
                //    collegeInformation.affiliationToDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).OrderByDescending(a => a.affiliationToDate).Take(1)
                //                                                .Select(a => a.affiliationToDate).ToArray()
                //                                                .Select(s => s.ToString()).ToArray();
                //    collegeInformation.affiliationDuration5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).OrderByDescending(a => a.affiliationToDate).Take(1)
                //                                                .Select(a => a.affiliationDuration).ToArray()
                //                                                .Select(s => s.ToString()).ToArray();

                //    var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).OrderByDescending(a => a.affiliationToDate).Take(1);
                //    collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                //    collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                //}

                rowIndex++;
            }

            if (collegeInformation.affiliationFromDate1 != null)
            {
                //collegeInformation.affiliationFromDate1 = new string[] { 
                //    collegeInformation.affiliationFromDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString() 
                //};
                collegeInformation.affiliationFromDate1 = collegeInformation.affiliationFromDate1 == null ? string.Empty :
                string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.affiliationFromDate1));
            }

            if (collegeInformation.affiliationFromDate2 != null)
            {
                collegeInformation.affiliationFromDate2 = new string[] { 
                    collegeInformation.affiliationFromDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate2.FirstOrDefault()).ToString()  
                };
            }

            if (collegeInformation.affiliationFromDate3 != null)
            {
                collegeInformation.affiliationFromDate3 = collegeInformation.affiliationFromDate3 == null ? string.Empty :
              string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.affiliationFromDate3));
            }

            if (collegeInformation.affiliationFromDate4 != null)
            {
                collegeInformation.affiliationFromDate4 = new string[] { 
                    collegeInformation.affiliationFromDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate4.FirstOrDefault()).ToString()  
                };
            }

            if (collegeInformation.pa_affiliationFromDate5 != null)
            {
                collegeInformation.pa_affiliationFromDate5 = collegeInformation.pa_affiliationFromDate5 == null ? string.Empty :
              string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.pa_affiliationFromDate5));
            }
            if (collegeInformation.affiliationToDate1 != null)
            {
                //collegeInformation.affiliationToDate1 = new string[] { 
                //    collegeInformation.affiliationToDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString() 
                //};
                collegeInformation.affiliationToDate1 = collegeInformation.affiliationToDate1 == null
                    ? string.Empty
                    : string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.affiliationToDate1));
            }

            if (collegeInformation.affiliationToDate2 != null)
            {
                collegeInformation.affiliationToDate2 = new string[] { 
                    collegeInformation.affiliationToDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate2.FirstOrDefault()).ToString()  
                };
            }

            if (collegeInformation.affiliationToDate3 != null)
            {
                collegeInformation.affiliationToDate3 = collegeInformation.affiliationToDate3 == null
                    ? string.Empty
                    : string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.affiliationToDate3));
            }

            if (collegeInformation.affiliationToDate4 != null)
            {
                collegeInformation.affiliationToDate4 = new string[] { 
                    collegeInformation.affiliationToDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate4.FirstOrDefault()).ToString()  
                };
            }

            if (collegeInformation.pa_affiliationToDate5 != null)
            {
                collegeInformation.pa_affiliationToDate5 = collegeInformation.pa_affiliationToDate5 == null
                    ? string.Empty
                    : string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.pa_affiliationToDate5));
            }
            if (collegeInformation.affiliationDuration1 != null)
            {
                //collegeInformation.affiliationDuration1 = new string[] { 
                //    collegeInformation.affiliationDuration1.Length == 0 ? string.Empty : collegeInformation.affiliationDuration1.FirstOrDefault()  
                //};
                collegeInformation.affiliationDuration1 = collegeInformation.affiliationDuration1 == null
                    ? string.Empty
                    : collegeInformation.affiliationDuration1.ToString();
            }
            if (collegeInformation.affiliationDuration2 != null)
            {
                collegeInformation.affiliationDuration2 = new string[] { 
                    collegeInformation.affiliationDuration2.Length == 0 ? string.Empty : collegeInformation.affiliationDuration2.FirstOrDefault()  
                };
            }
            if (collegeInformation.affiliationDuration3 != null)
            {
                collegeInformation.affiliationDuration3 = collegeInformation.affiliationDuration3 == null
                  ? string.Empty
                  : collegeInformation.affiliationDuration3.ToString();
            }
            if (collegeInformation.affiliationDuration4 != null)
            {
                collegeInformation.affiliationDuration4 = new string[] { 
                    collegeInformation.affiliationDuration4.Length == 0 ? string.Empty : collegeInformation.affiliationDuration4.FirstOrDefault()  
                };
            }
            if (collegeInformation.pa_affiliationDuration5 != null)
            {
                collegeInformation.pa_affiliationDuration5 = collegeInformation.pa_affiliationDuration5 == null
                  ? string.Empty
                  : collegeInformation.pa_affiliationDuration5.ToString();
            }

            //ViewBag.Status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
            //ViewBag.SubStatus = db.jntuh_college_status.Where(s => s.isActive == true && s.mainstatusId != 0).ToList();

            return View("Create", collegeInformation);
        }

        // POST: /CollegeInformation/Edit
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(CollegeInformation collegeInformation)
        {

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeInformation.id;
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            collegeInformation.id = userCollegeID;

            SaveCollegeInformation(collegeInformation);

            TempData["Success"] = "Updated successfully";

            return View("View");
            // Commented by Naushad Khan 
            // return View("Create", collegeInformation);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();


            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PAT") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }

                //ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            CollegeInformation collegeInformation = new CollegeInformation();
            if (userCollegeID == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Norecords = false;
                jntuh_college jntuh_college = db.jntuh_college.Find(userCollegeID);

                //collegeInformation.collegeStatusID = jntuh_college.collegeStatusID;

                //var collegestatus =
                //                 db.jntuh_college_status.Where(c => c.id == collegeInformation.collegeStatusID)
                //                     .Select(s => s.collegeStatus)
                //                     .FirstOrDefault();
                //if (collegestatus == "Minority")
                //{
                //    jntuh_college_minoritystatus minoritystatus =
                //        db.jntuh_college_minoritystatus.Where(r => r.collegeId == userCollegeID && r.statusTodate > todayDate)
                //            .Select(s => s)
                //            .FirstOrDefault();
                //    if (minoritystatus != null)
                //    {
                //        collegeInformation.collegeminorityStatus = db.jntuh_college_status.Where(s => s.id == minoritystatus.collegeStatusid).Select(s => s.collegeStatus).FirstOrDefault();
                //        collegeInformation.minortyid = minoritystatus.id;
                //        collegeInformation.academicYearid = minoritystatus.academicYearid;
                //        collegeInformation.collegesubstatusId = minoritystatus.collegeStatusid;
                //        DateTime fromdate = Convert.ToDateTime(minoritystatus.statusFromdate);
                //        collegeInformation.collegestatusfromdate = fromdate.ToString("dd/MM/yyyy").Split(' ')[0];
                //        DateTime todate = Convert.ToDateTime(minoritystatus.statusTodate);
                //        collegeInformation.collegestatustodate = todate.ToString("dd/MM/yyyy").Split(' ')[0];
                //        collegeInformation.collegestatusfilepath = minoritystatus.statusFile;

                //    }
                //}
                //collegeInformation.collegeStatus = db.jntuh_college_status.Where(s => s.id == collegeInformation.collegeStatusID).Select(s => s.collegeStatus).FirstOrDefault();

                string[] selectedAffiliationType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id).OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationTypeId).ToArray().Select(s => s.ToString()).ToArray();
                List<Item> lstType = new List<Item>();
                foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder).ToList())
                {
                    string strType = t.id.ToString();
                    lstType.Add(new Item { id = t.id, name = t.affiliationType, selected = selectedAffiliationType.Contains(strType) ? 1 : 0 });
                }
                // lstType = lstType.Where(l => l.id != 5).ToList();
                var aff_item = lstType.Where(l => l.id == 5).ToList();
                if (aff_item.Count > 0)
                {
                    var PA_item = lstType[0];
                    lstType.RemoveAt(0);
                    lstType.Insert(2, PA_item);
                }

                //int[] selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder)
                //                                                          .Select(s => s.id).ToArray();

                //var pa_item = selectedAffiliationId.Where(l => l == 5).ToArray();
                //if (pa_item.Count() > 0)
                //{
                //    selectedAffiliationId = selectedAffiliationId.Where(l => l != 5).ToArray();
                //    selectedAffiliationId[selectedAffiliationId.GetUpperBound(0) + 1] = 5;
                //}
                var selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder)
                                                                          .Select(s => s.id).ToList();
                //var pa_item = selectedAffiliationId.con
                if (selectedAffiliationId.Contains(5))
                {
                    //var PA_item = lstType[0];
                    selectedAffiliationId.RemoveAt(0);
                    selectedAffiliationId.Add(5);
                }
                int affiliationCount = 1;
                foreach (var item in selectedAffiliationId)
                {
                    if (affiliationCount == 1)
                    {
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                        .Select(a => a.affiliationStatus).FirstOrDefault();
                        if (statusType == "Yes")
                        {
                            collegeInformation.affiliationSelected1 = 1;
                        }
                        else
                        {
                            collegeInformation.affiliationSelected1 = 2;
                        }
                    }
                    else if (affiliationCount == 2)
                    {
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                       .Select(a => a.affiliationStatus).FirstOrDefault();
                        if (statusType == "Yes")
                        {
                            collegeInformation.affiliationSelected3 = 1;
                        }
                        else
                        {
                            collegeInformation.affiliationSelected3 = 2;
                        }
                    }

                    else if (affiliationCount == 3)
                    {
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                       .Select(a => a.affiliationStatus).FirstOrDefault();
                        if (statusType == "Yes")
                        {
                            collegeInformation.affiliationSelected5 = 1;
                        }
                        else
                        {
                            collegeInformation.affiliationSelected5 = 2;
                        }
                    }
                    else if (affiliationCount == 4)
                    {
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                        .Select(a => a.affiliationStatus).FirstOrDefault();
                        if (statusType == "Conferred")
                        {
                            collegeInformation.affiliationSelected4 = 1;
                        }
                        else if (statusType == "Applied")
                        {
                            collegeInformation.affiliationSelected4 = 2;
                        }
                        else
                        {
                            collegeInformation.affiliationSelected4 = 3;
                        }
                    }
                    else if (affiliationCount == 5)
                    {
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                         .Select(a => a.affiliationStatus).FirstOrDefault();
                        if (statusType == "Conferred")
                        {
                            collegeInformation.affiliationSelected5 = 1;
                        }
                        else if (statusType == "Applied")
                        {
                            collegeInformation.affiliationSelected5 = 2;
                        }
                        else
                        {
                            collegeInformation.affiliationSelected5 = 3;
                        }
                    }
                    affiliationCount++;
                }
                collegeInformation.affiliationType = lstType;

                int rowIndex = 1;
                foreach (var type in lstType)
                {
                    int affiliationType = type.id;

                    if (rowIndex == 1)
                    {
                        collegeInformation.affiliationId1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.id).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationFromDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationFromDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationToDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationToDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationDuration1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationfilepath1 =
                            db.jntuh_college_affiliation.Where(
                                a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                .Select(a => a.filePath).FirstOrDefault();
                        var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1);
                        collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                        collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                    }

                    if (rowIndex == 2)
                    {
                        collegeInformation.affiliationId3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.id).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationFromDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                   .Select(a => a.affiliationFromDate).ToArray()
                                                                   .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationToDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationToDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationDuration3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationfilepath3 =
                          db.jntuh_college_affiliation.Where(
                              a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                              .Select(a => a.filePath).FirstOrDefault();
                        var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1);
                        collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                        collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();

                        //collegeInformation.affiliationFromDate2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                        //                                            .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                        //                                            .Select(s => s.ToString()).ToArray();
                        //collegeInformation.affiliationToDate2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                        //                                            .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                        //                                            .Select(s => s.ToString()).ToArray();
                        //collegeInformation.affiliationDuration2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                        //                                            .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                        //                                            .Select(s => s.ToString()).ToArray();

                        //var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType);
                        //collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                        //collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                    }

                    if (rowIndex == 3)
                    {
                        collegeInformation.affiliationId5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.id).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.pa_affiliationFromDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationFromDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.pa_affiliationToDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationToDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.pa_affiliationDuration5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.pa_affiliationfilepath5 =
                            db.jntuh_college_affiliation.Where(
                                a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                .Select(a => a.filePath).FirstOrDefault();
                        //var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).OrderByDescending(a => a.affiliationToDate).Take(1);
                        //collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                        //collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                    }
                    if (rowIndex == 4)
                    {
                        collegeInformation.affiliationFromDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationFromDate).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();
                        collegeInformation.affiliationToDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationToDate).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();
                        collegeInformation.affiliationDuration4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1)
                                                                    .Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();

                        var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1);
                        collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                        collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                    }
                    //if (rowIndex == 5)
                    //{
                    //    collegeInformation.affiliationFromDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).OrderByDescending(a => a.affiliationToDate).Take(1)
                    //                                                .Select(a => a.affiliationFromDate).ToArray()
                    //                                                .Select(s => s.ToString()).ToArray();
                    //    collegeInformation.affiliationToDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).OrderByDescending(a => a.affiliationToDate).Take(1)
                    //                                                .Select(a => a.affiliationToDate).ToArray()
                    //                                                .Select(s => s.ToString()).ToArray();
                    //    collegeInformation.affiliationDuration5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).OrderByDescending(a => a.affiliationToDate).Take(1)
                    //                                                .Select(a => a.affiliationDuration).ToArray()
                    //                                                .Select(s => s.ToString()).ToArray();

                    //    var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).OrderByDescending(a => a.affiliationToDate).Take(1);
                    //    collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                    //    collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                    //}

                    rowIndex++;
                }

                if (collegeInformation.affiliationFromDate1 != null)
                {
                    //collegeInformation.affiliationFromDate1 = new string[] { 
                    //    collegeInformation.affiliationFromDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString() 
                    //};
                    collegeInformation.affiliationFromDate1 = collegeInformation.affiliationFromDate1 == null ? string.Empty :
                    string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.affiliationFromDate1));
                }

                if (collegeInformation.affiliationFromDate2 != null)
                {
                    collegeInformation.affiliationFromDate2 = new string[] { 
                    collegeInformation.affiliationFromDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate2.FirstOrDefault()).ToString()  
                };
                }

                if (collegeInformation.affiliationFromDate3 != null)
                {
                    collegeInformation.affiliationFromDate3 = collegeInformation.affiliationFromDate3 == null ? string.Empty :
                  string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.affiliationFromDate3));
                }

                if (collegeInformation.affiliationFromDate4 != null)
                {
                    collegeInformation.affiliationFromDate4 = new string[] { 
                    collegeInformation.affiliationFromDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate4.FirstOrDefault()).ToString()  
                };
                }

                if (collegeInformation.pa_affiliationFromDate5 != null)
                {
                    collegeInformation.pa_affiliationFromDate5 = collegeInformation.pa_affiliationFromDate5 == null ? string.Empty :
                  string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.pa_affiliationFromDate5));
                }
                if (collegeInformation.affiliationToDate1 != null)
                {
                    //collegeInformation.affiliationToDate1 = new string[] { 
                    //    collegeInformation.affiliationToDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString() 
                    //};
                    collegeInformation.affiliationToDate1 = collegeInformation.affiliationToDate1 == null
                        ? string.Empty
                        : string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.affiliationToDate1));
                }

                if (collegeInformation.affiliationToDate2 != null)
                {
                    collegeInformation.affiliationToDate2 = new string[] { 
                    collegeInformation.affiliationToDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate2.FirstOrDefault()).ToString()  
                };
                }

                if (collegeInformation.affiliationToDate3 != null)
                {
                    collegeInformation.affiliationToDate3 = collegeInformation.affiliationToDate3 == null
                        ? string.Empty
                        : string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.affiliationToDate3));
                }

                if (collegeInformation.affiliationToDate4 != null)
                {
                    collegeInformation.affiliationToDate4 = new string[] { 
                    collegeInformation.affiliationToDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate4.FirstOrDefault()).ToString()  
                };
                }

                if (collegeInformation.pa_affiliationToDate5 != null)
                {
                    collegeInformation.pa_affiliationToDate5 = collegeInformation.pa_affiliationToDate5 == null
                        ? string.Empty
                        : string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(collegeInformation.pa_affiliationToDate5));
                }
                if (collegeInformation.affiliationDuration1 != null)
                {
                    //collegeInformation.affiliationDuration1 = new string[] { 
                    //    collegeInformation.affiliationDuration1.Length == 0 ? string.Empty : collegeInformation.affiliationDuration1.FirstOrDefault()  
                    //};
                    collegeInformation.affiliationDuration1 = collegeInformation.affiliationDuration1 == null
                        ? string.Empty
                        : collegeInformation.affiliationDuration1.ToString();
                }
                if (collegeInformation.affiliationDuration2 != null)
                {
                    collegeInformation.affiliationDuration2 = new string[] { 
                    collegeInformation.affiliationDuration2.Length == 0 ? string.Empty : collegeInformation.affiliationDuration2.FirstOrDefault()  
                };
                }
                if (collegeInformation.affiliationDuration3 != null)
                {
                    collegeInformation.affiliationDuration3 = collegeInformation.affiliationDuration3 == null
                      ? string.Empty
                      : collegeInformation.affiliationDuration3.ToString();
                }
                if (collegeInformation.affiliationDuration4 != null)
                {
                    collegeInformation.affiliationDuration4 = new string[] { 
                    collegeInformation.affiliationDuration4.Length == 0 ? string.Empty : collegeInformation.affiliationDuration4.FirstOrDefault()  
                };
                }
                if (collegeInformation.pa_affiliationDuration5 != null)
                {
                    collegeInformation.pa_affiliationDuration5 = collegeInformation.pa_affiliationDuration5 == null
                      ? string.Empty
                      : collegeInformation.pa_affiliationDuration5.ToString();
                }

                //ViewBag.Status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
                //ViewBag.SubStatus = db.jntuh_college_status.Where(s => s.isActive == true && s.mainstatusId != 0).ToList();
            }

            return View("View", collegeInformation);
        }
    }
}
