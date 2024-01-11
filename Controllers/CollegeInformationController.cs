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

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeInformationController : BaseController
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
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (userCollegeID > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeInformation");
            }
            if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (userCollegeID == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            CollegeInformation collegeInformation = new CollegeInformation();

            string[] strSelected = new string[] { };
            List<Item> lstAffiliationType = new List<Item>();

            foreach (var type in db.jntuh_college_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.DisplayOrder).ToList())
            {
                string strType = type.id.ToString();
                lstAffiliationType.Add(new Item { id = type.id, name = type.collegeAffiliationType, selected = strSelected.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.collegeAffiliationType = lstAffiliationType;

            List<Item> lstCollegeType = new List<Item>();

            foreach (var type in db.jntuh_college_type.Where(s => s.isActive == true))
            {
                string strType = type.id.ToString();
                lstCollegeType.Add(new Item { id = type.id, name = type.collegeType, selected = strSelected.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.collegeType = lstCollegeType;

            List<Item> lstDegree = new List<Item>();

            foreach (var d in db.jntuh_degree.Where(s => s.isActive == true).OrderBy(s => s.degreeDisplayOrder))
            {
                string strType = d.id.ToString();
                lstDegree.Add(new Item { id = d.id, name = d.degree, selected = strSelected.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.degree = lstDegree;

            List<Item> lstType = new List<Item>();

            foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder))
            {
                string strType = t.id.ToString();
                lstType.Add(new Item { id = t.id, name = t.affiliationType, selected = strSelected.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.affiliationType = lstType;

            //collegeInformation.affiliationFromDate1 = new string[] { string.Empty };
            collegeInformation.affiliationFromDate2 = new string[] { string.Empty };
            //collegeInformation.affiliationFromDate3 = new string[] { string.Empty };
            collegeInformation.affiliationFromDate4 = new string[] { string.Empty };
            collegeInformation.affiliationFromDate5 = new string[] { string.Empty };

            //collegeInformation.affiliationToDate1 = new string[] { string.Empty };
            collegeInformation.affiliationToDate2 = new string[] { string.Empty };
            //collegeInformation.affiliationToDate3 = new string[] { string.Empty };
            collegeInformation.affiliationToDate4 = new string[] { string.Empty };
            collegeInformation.affiliationToDate5 = new string[] { string.Empty };

            //collegeInformation.affiliationDuration1 = new string[] { string.Empty };
            //collegeInformation.affiliationDuration2 = new string[] { string.Empty };
            //collegeInformation.affiliationDuration3 = new string[] { string.Empty };
            collegeInformation.affiliationDuration4 = new string[] { string.Empty };
            collegeInformation.affiliationDuration5 = new string[] { string.Empty };

            var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();
            var pgIds = new int[] { 1, 2, 3, 6 };
            ViewBag.pgcetStatus = false;
            var pgstats = DegreeIds.Where(i => pgIds.Contains(i)).ToList();
            if (pgstats.Count > 0)
            {
                ViewBag.pgcetStatus = true;
            }

            ViewBag.Status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
            ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
            ViewBag.AffiliationType = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(f => f.displayOrder).ToList();

            return View(collegeInformation);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDistrictList(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }
            var districtList = this.GetDistricts(Convert.ToInt32(id));

            var myData = districtList.Select(a => new SelectListItem()
            {
                Text = a.districtName,
                Value = a.id.ToString(),
            });

            return Json(myData, JsonRequestBehavior.AllowGet);
        }

        private IList<jntuh_district> GetDistricts(int id)
        {
            return db.jntuh_district.Where(d => d.stateId == id).ToList();
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
                //if (ModelState.IsValid)
                //{
                //get current logged in user id
                int createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                //create college object
                jntuh_college jntuh_college = new jntuh_college();
                jntuh_college.id = collegeInformation.id;
                jntuh_college.collegeCode = collegeInformation.collegeCode;
                jntuh_college.collegeName = collegeInformation.collegeName;
                jntuh_college.collegeTypeID = Convert.ToInt32(collegeInformation.collegeTypeID

                                             .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s).FirstOrDefault());

                jntuh_college.collegeStatusID = collegeInformation.collegeStatusID;
                //jntuh_college.societyName = string.Empty;
                jntuh_college.eamcetCode = collegeInformation.eamcetCode;
                jntuh_college.icetCode = collegeInformation.icetCode;
                //New Fields Added on 04-02-2020

                jntuh_college.pgcetCode = collegeInformation.pgcetCode == null ? string.Empty : collegeInformation.pgcetCode.Trim();
                jntuh_college.aicteId = collegeInformation.aicteid == null ? string.Empty : collegeInformation.aicteid.Trim();

                jntuh_college.collegeAffiliationTypeID = 9;
                //jntuh_college.collegeAffiliationTypeID = Convert.ToInt32(collegeInformation.collegeAffiliationTypeID
                //                                                        .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s).FirstOrDefault());
                string affiliation = db.jntuh_college_affiliation_type.Where(f => f.isActive == true && f.id == jntuh_college.collegeAffiliationTypeID)
                                                                      .OrderBy(s => s.DisplayOrder).Select(f => f.collegeAffiliationType)
                                                                      .FirstOrDefault();
                //if (affiliation == "Any other Category (Specify)")
                //{
                //    jntuh_college.otherCategory = collegeInformation.otherCategory;
                //}
                jntuh_college.isActive = true;


                if (collegeID == 0)     //if college id = 0; then insert the college record
                {
                    jntuh_college.createdBy = createdBy;
                    jntuh_college.createdOn = DateTime.Now;
                    db1.jntuh_college.Add(jntuh_college);
                }
                else                    //if college id exists then modify the existing college record
                {
                    jntuh_college.id = collegeID;
                    jntuh_college.createdBy = db.jntuh_college.Where(c => c.id == collegeID).Select(c => c.createdBy).FirstOrDefault();
                    jntuh_college.createdOn = db.jntuh_college.Where(c => c.id == collegeID).Select(c => c.createdOn).FirstOrDefault();
                    jntuh_college.updatedBy = createdBy;
                    jntuh_college.updatedOn = DateTime.Now;
                    jntuh_college.isPermant = db.jntuh_college.Where(c => c.id == collegeID).Select(c => c.isPermant).FirstOrDefault();
                    jntuh_college.isActive = db.jntuh_college.Where(c => c.id == collegeID).Select(c => c.isActive).FirstOrDefault();
                    jntuh_college.isClosed = db.jntuh_college.Where(c => c.id == collegeID).Select(c => c.isClosed).FirstOrDefault();
                    jntuh_college.isDeleted = db.jntuh_college.Where(c => c.id == collegeID).Select(c => c.isDeleted).FirstOrDefault();
                    //jntuh_college.isNew = db.jntuh_college.Where(c => c.id == collegeID).Select(c => c.isNew).FirstOrDefault();
                    db1.Entry(jntuh_college).State = EntityState.Modified;

                }
                try
                {
                    db1.SaveChanges();


                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}",
                                validationError.PropertyName,
                                validationError.ErrorMessage);
                        }
                    }
                }


                // db.SaveChanges();

                //get the college id after inserting the colleg record
                collegeID = jntuh_college.id;

                #region College Status Minority/ Non-Minority Saving written by Narayana Reddy
                var collegestatus =
                db.jntuh_college_status.Where(c => c.id == collegeInformation.collegeStatusID)
                    .Select(s => s.collegeStatus)
                    .FirstOrDefault();
                if (collegestatus != "Minority")
                {
                    jntuh_college.collegeStatusID = collegeInformation.collegeStatusID;
                    var minoritystatus =
                           db.jntuh_college_minoritystatus.Where(r => r.collegeId == userCollegeID)
                               .Select(s => s).FirstOrDefault();
                    if (minoritystatus != null)
                    {
                        db.jntuh_college_minoritystatus.Remove(minoritystatus);
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (collegeInformation.collegestatusfile != null || collegeInformation.collegestatusfilepath != null)
                    {

                        string collegestatusfilepath = "~/Content/Upload/College/CollegeStatus";
                        if (!Directory.Exists(Server.MapPath(collegestatusfilepath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(collegestatusfilepath));
                        }
                        if (collegeInformation.collegestatusfile != null)
                        {
                            var ext = Path.GetExtension(collegeInformation.collegestatusfile.FileName);
                            if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                            {
                                if (collegeInformation.collegestatusfilepath == null)
                                {
                                    if (collegeInformation.collegestatusfilepath == null)
                                    {
                                        string fileName = collegeInformation.collegeCode + "-" +
                                                          DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                          "MS";
                                        collegeInformation.collegestatusfile.SaveAs(string.Format("{0}/{1}{2}",
                                            Server.MapPath(collegestatusfilepath), fileName, ext));
                                        collegeInformation.collegestatusfilepath = string.Format("{0}{1}", fileName,
                                            ext);
                                    }
                                    else
                                    {
                                        collegeInformation.collegestatusfile.SaveAs(string.Format("{0}",
                                            Server.MapPath(collegestatusfilepath), collegeInformation.collegestatusfilepath));
                                        collegeInformation.collegestatusfilepath =
                                            collegeInformation.collegestatusfilepath;
                                    }
                                }
                            }
                        }
                        else
                        {
                            collegeInformation.collegestatusfilepath = collegeInformation.collegestatusfilepath;
                        }


                        var minoritystatus =
                            db.jntuh_college_minoritystatus.Where(r => r.collegeId == userCollegeID)
                                .Select(s => s).FirstOrDefault();
                        if (minoritystatus == null)
                        {
                            jntuh_college_minoritystatus collegeminoritystatus = new jntuh_college_minoritystatus();
                            collegeminoritystatus.collegeId = collegeID;
                            collegeminoritystatus.academicYearid = ay0;
                            collegeminoritystatus.collegeStatusid = collegeInformation.collegesubstatusId;
                            collegeminoritystatus.statusFromdate = Utilities.DDMMYY2MMDDYY(collegeInformation.collegestatusfromdate.ToString());
                            collegeminoritystatus.statusTodate = Utilities.DDMMYY2MMDDYY(collegeInformation.collegestatustodate.ToString());
                            collegeminoritystatus.statusFile = collegeInformation.collegestatusfilepath;
                            collegeminoritystatus.isActive = true;
                            collegeminoritystatus.createdOn = DateTime.Now;
                            collegeminoritystatus.createdBy = userID;
                            collegeminoritystatus.updatedOn = null;
                            collegeminoritystatus.updatedBy = null;
                            db.jntuh_college_minoritystatus.Add(collegeminoritystatus);
                            db.SaveChanges();
                        }
                        else
                        {
                            minoritystatus.academicYearid = ay0;
                            minoritystatus.collegeStatusid = collegeInformation.collegesubstatusId;
                            minoritystatus.statusFromdate = Utilities.DDMMYY2MMDDYY(collegeInformation.collegestatusfromdate.ToString());
                            minoritystatus.statusTodate = Utilities.DDMMYY2MMDDYY(collegeInformation.collegestatustodate.ToString());
                            minoritystatus.statusFile = collegeInformation.collegestatusfilepath;
                            minoritystatus.updatedOn = DateTime.Now;
                            minoritystatus.updatedBy = userID;
                            db.Entry(minoritystatus).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                    }

                }
                #endregion

                //if college id exists, insert other records into other tables
                if (collegeID != 0)
                {
                    //create college address object
                    jntuh_address jntuh_address = new jntuh_address();
                    jntuh_address.collegeId = collegeID;
                    jntuh_address.addressTye = collegeInformation.addressTye;
                    jntuh_address.address = collegeInformation.address;
                    jntuh_address.townOrCity = collegeInformation.townOrCity;
                    jntuh_address.mandal = collegeInformation.mandal;
                    jntuh_address.districtId = collegeInformation.districtId;
                    jntuh_address.stateId = collegeInformation.stateId;
                    jntuh_address.pincode = collegeInformation.pincode;
                    jntuh_address.fax = collegeInformation.fax;
                    jntuh_address.landline = collegeInformation.landline;
                    jntuh_address.mobile = collegeInformation.mobile;
                    jntuh_address.email = collegeInformation.email;
                    jntuh_address.website = collegeInformation.website;
                    if (collegeInformation.website == null)
                    {
                        jntuh_address.website = string.Empty;
                    }


                    var addressID = db.jntuh_address.Where(a => a.collegeId == collegeID && a.addressTye == "COLLEGE").Select(a => a.id).FirstOrDefault();

                    if (addressID == 0)      //if address id = 0; then insert the college address record
                    {
                        jntuh_address.createdBy = createdBy;
                        jntuh_address.createdOn = DateTime.Now;
                        db.jntuh_address.Add(jntuh_address);
                    }
                    else                        //if address id exists then modify the existing college address record
                    {
                        jntuh_address.id = addressID;
                        jntuh_address.createdBy = db.jntuh_address.Where(a => a.id == addressID).Select(a => a.createdBy).FirstOrDefault();
                        jntuh_address.createdOn = db.jntuh_address.Where(a => a.id == addressID).Select(a => a.createdOn).FirstOrDefault();
                        jntuh_address.updatedBy = createdBy;
                        jntuh_address.updatedOn = DateTime.Now;
                        db.Entry(jntuh_address).State = EntityState.Modified;
                    }

                    db.SaveChanges();

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

                    int[] degreeId = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).Select(d => d.id).ToArray();
                    foreach (var id in degreeId)
                    {
                        jntuh_college_degree collegeDegree = new jntuh_college_degree();
                        collegeDegree.id = id;
                        collegeDegree.collegeId = userCollegeID;
                        collegeDegree.degreeId = db.jntuh_college_degree.Where(d => d.id == id).Select(d => d.degreeId).FirstOrDefault();
                        collegeDegree.isActive = false;
                        collegeDegree.createdBy = db.jntuh_college_degree.Where(d => d.id == id).Select(d => d.createdBy).FirstOrDefault();
                        collegeDegree.createdOn = db.jntuh_college_degree.Where(d => d.id == id).Select(d => d.createdOn).FirstOrDefault();
                        collegeDegree.updatedBy = db.jntuh_college_degree.Where(d => d.id == id).Select(d => d.updatedBy).FirstOrDefault();
                        collegeDegree.updatedOn = db.jntuh_college_degree.Where(d => d.id == id).Select(d => d.updatedOn).FirstOrDefault();
                        jntuh_college_degree existing = db.jntuh_college_degree.Find(id);
                        ((IObjectContextAdapter)db).ObjectContext.Detach(existing);
                        db.Entry(collegeDegree).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    foreach (string degree in collegeInformation.degreeId.Where(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        //create college degree object
                        jntuh_college_degree jntuh_college_degree = new jntuh_college_degree();
                        jntuh_college_degree.collegeId = collegeID;
                        jntuh_college_degree.degreeId = Convert.ToInt32(degree);
                        jntuh_college_degree.isActive = true;

                        var degreeID = db.jntuh_college_degree
                                              .Where(d => d.collegeId == userCollegeID && d.degreeId == jntuh_college_degree.degreeId)
                                              .Select(d => d.id).FirstOrDefault();

                        if (degreeID == 0)   //if degree id = 0; then insert the college degree record
                        {
                            jntuh_college_degree.createdBy = userID;
                            jntuh_college_degree.createdOn = DateTime.Now;
                            db.jntuh_college_degree.Add(jntuh_college_degree);
                        }
                        else                    //if degree id exists then modify the existing college degree record
                        {
                            jntuh_college_degree.id = degreeID;
                            jntuh_college_degree.createdBy = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.id == degreeID).Select(d => d.createdBy).FirstOrDefault();
                            jntuh_college_degree.createdOn = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.id == degreeID).Select(d => d.createdOn).FirstOrDefault();
                            jntuh_college_degree.updatedBy = createdBy;
                            jntuh_college_degree.updatedOn = DateTime.Now;
                            jntuh_college_degree existing = db.jntuh_college_degree.Find(degreeID);
                            ((IObjectContextAdapter)db).ObjectContext.Detach(existing);
                            db.Entry(jntuh_college_degree).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                    }
                }
            }
            //}
            string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
            //  var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            //after postback
            string[] selectedCollegeAffiliation = collegeInformation.collegeAffiliationTypeID.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s).ToArray();
            List<Item> lstAffiliationType = new List<Item>();
            foreach (var type in db.jntuh_college_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.DisplayOrder).ToList())
            {
                string strType = type.id.ToString();
                lstAffiliationType.Add(new Item { id = type.id, name = type.collegeAffiliationType, selected = selectedCollegeAffiliation.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.collegeAffiliationType = lstAffiliationType;

            string[] selectedCollegeType = collegeInformation.collegeTypeID.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s).ToArray();
            List<Item> lstCollegeType = new List<Item>();
            foreach (var type in db.jntuh_college_type.Where(s => s.isActive == true))
            {
                string strType = type.id.ToString();
                lstCollegeType.Add(new Item { id = type.id, name = type.collegeType, selected = selectedCollegeType.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.collegeType = lstCollegeType;

            string[] selectedCollegeDegree = collegeInformation.degreeId.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s).ToArray();
            List<Item> lstDegree = new List<Item>();
            foreach (var d in db.jntuh_degree.Where(s => s.isActive == true).OrderBy(s => s.degreeDisplayOrder))
            {
                string strType = d.id.ToString();
                lstDegree.Add(new Item { id = d.id, name = d.degree, selected = selectedCollegeDegree.Contains(strType) ? 1 : 0 });
            }
            collegeInformation.degree = lstDegree;

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

            ViewBag.Status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
            ViewBag.SubStatus = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
            ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
            ViewBag.AffiliationType = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(f => f.displayOrder).ToList();
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
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();
            var pgIds = new int[] { 1, 2, 3, 6 };
            ViewBag.pgcetStatus = false;
            var pgstats = DegreeIds.Where(i => pgIds.Contains(i)).ToList();
            if (pgstats.Count > 0)
            {
                ViewBag.pgcetStatus = true;
            }
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeInformation");
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

                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CF") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeInformation");
                }
            }

            CollegeInformation collegeInformation = new CollegeInformation();

            jntuh_college jntuh_college = db.jntuh_college.Find(userCollegeID);
            if (jntuh_college != null)
            {
                collegeInformation.id = jntuh_college.id;
                collegeInformation.collegeCode = jntuh_college.collegeCode;
                collegeInformation.collegeName = jntuh_college.collegeName;
                collegeInformation.collegeStatusID = jntuh_college.collegeStatusID;
                collegeInformation.eamcetCode = jntuh_college.eamcetCode;
                collegeInformation.icetCode = jntuh_college.icetCode;
                //New Fields
                collegeInformation.pgcetCode = String.IsNullOrEmpty(jntuh_college.pgcetCode) ? String.Empty : jntuh_college.pgcetCode;
                collegeInformation.aicteid = String.IsNullOrEmpty(jntuh_college.aicteId) ? String.Empty : jntuh_college.aicteId;

               // collegeInformation.otherCategory = jntuh_college.otherCategory;
                collegeInformation.createdBy = jntuh_college.createdBy;
                collegeInformation.createdOn = jntuh_college.createdOn;
                collegeInformation.updatedBy = jntuh_college.createdBy;
                collegeInformation.updatedOn = jntuh_college.createdOn;

                var collegestatus =
                  db.jntuh_college_status.Where(c => c.id == collegeInformation.collegeStatusID)
                      .Select(s => s.collegeStatus)
                      .FirstOrDefault();
                if (collegestatus == "Minority")
                {
                    jntuh_college_minoritystatus minoritystatus =
                        db.jntuh_college_minoritystatus.Where(r => r.collegeId == userCollegeID)
                            .Select(s => s)
                            .FirstOrDefault();
                    if (minoritystatus != null)
                    {
                        collegeInformation.collegeminorityStatus =
                            db.jntuh_college_status.Where(r => r.id == minoritystatus.collegeStatusid)
                                .Select(s => s.collegeStatus)
                                .FirstOrDefault();
                        collegeInformation.minortyid = minoritystatus.id;
                        collegeInformation.academicYearid = minoritystatus.academicYearid;
                        collegeInformation.collegesubstatusId = minoritystatus.collegeStatusid;
                        DateTime fromdate = Convert.ToDateTime(minoritystatus.statusFromdate);
                        collegeInformation.collegestatusfromdate = fromdate.ToString("dd/MM/yyyy").Split(' ')[0];
                        DateTime todate = Convert.ToDateTime(minoritystatus.statusTodate);
                        collegeInformation.collegestatustodate = todate.ToString("dd/MM/yyyy").Split(' ')[0];
                        collegeInformation.collegestatusfilepath = minoritystatus.statusFile;

                    }
                }
            }

            jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == jntuh_college.id && a.addressTye == "COLLEGE").Select(a => a).ToList().FirstOrDefault();
            if (jntuh_address != null)
            {
                collegeInformation.address = jntuh_address.address;
                collegeInformation.townOrCity = jntuh_address.townOrCity;
                collegeInformation.mandal = jntuh_address.mandal;
                collegeInformation.stateId = jntuh_address.stateId;
                collegeInformation.districtId = jntuh_address.districtId;
                collegeInformation.pincode = jntuh_address.pincode;
                collegeInformation.fax = jntuh_address.fax;
                collegeInformation.landline = jntuh_address.landline;
                collegeInformation.mobile = jntuh_address.mobile;
                collegeInformation.email = jntuh_address.email;
                collegeInformation.website = jntuh_address.website;
            }

            //after postback
            string[] selectedCollegeAffiliation = jntuh_college.collegeAffiliationTypeID.ToString().Split(' ');
            List<Item> lstAffiliationType = new List<Item>();
            foreach (var type in db.jntuh_college_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.DisplayOrder).ToList())
            {
                string strType = type.id.ToString();
                lstAffiliationType.Add(new Item { id = type.id, name = type.collegeAffiliationType, selected = selectedCollegeAffiliation.Contains(strType) ? 1 : 0 });

            }
            // collegeInformation.collegeTypeID=
            collegeInformation.collegeAffiliationType = lstAffiliationType;

            string[] selectedCollegeType = jntuh_college.collegeTypeID.ToString().Split(' ');
            List<Item> lstCollegeType = new List<Item>();
            foreach (var type in db.jntuh_college_type.Where(s => s.isActive == true))
            {
                string strType = type.id.ToString();
                lstCollegeType.Add(new Item { id = type.id, name = type.collegeType, selected = selectedCollegeType.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.collegeType = lstCollegeType;

            string[] selectedCollegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == jntuh_college.id && d.isActive == true).Select(d => d.degreeId).ToArray().Select(s => s.ToString()).ToArray();
            List<Item> lstDegree = new List<Item>();
            foreach (var d in db.jntuh_degree.Where(s => s.isActive == true).OrderBy(s => s.degreeDisplayOrder))
            {
                string strType = d.id.ToString();
                lstDegree.Add(new Item { id = d.id, name = d.degree, selected = selectedCollegeDegree.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.degree = lstDegree;

            string[] selectedAffiliationType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id).OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationTypeId).ToArray().Select(s => s.ToString()).ToArray();
            List<Item> lstType = new List<Item>();
            foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder).ToList())
            {
                string strType = t.id.ToString();
                lstType.Add(new Item { id = t.id, name = t.affiliationType, selected = selectedAffiliationType.Contains(strType) ? 1 : 0 });
            }
            int[] selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder)
                                                                      .Select(s => s.id).ToArray();
            int affiliationCount = 1;
            foreach (var item in selectedAffiliationId)
            {
                if (affiliationCount == 1)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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

                }
                else if (affiliationCount == 4)
                {
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                    string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                    collegeInformation.affiliationFromDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationToDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationDuration1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationfilepath1 =
                        db.jntuh_college_affiliation.Where(
                            a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                            .OrderByDescending(a => a.affiliationTypeId).Select(a => a.filePath).FirstOrDefault();
                    var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType);
                    collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                    collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                }

                if (rowIndex == 2)
                {
                    collegeInformation.affiliationFromDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                               .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                               .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationToDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationDuration3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                .Select(s => s.ToString()).FirstOrDefault();
                    collegeInformation.affiliationfilepath3 =
                      db.jntuh_college_affiliation.Where(
                          a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                          .OrderByDescending(a => a.affiliationTypeId).Select(a => a.filePath).FirstOrDefault();
                    var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType);
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

                }
                if (rowIndex == 4)
                {
                    collegeInformation.affiliationFromDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                .Select(s => s.ToString()).ToArray();
                    collegeInformation.affiliationToDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                .Select(s => s.ToString()).ToArray();
                    collegeInformation.affiliationDuration4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                .Select(s => s.ToString()).ToArray();

                    var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType);
                    collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                    collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                }
                if (rowIndex == 5)
                {
                    collegeInformation.affiliationFromDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                .Select(s => s.ToString()).ToArray();
                    collegeInformation.affiliationToDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                .Select(s => s.ToString()).ToArray();
                    collegeInformation.affiliationDuration5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                .Select(s => s.ToString()).ToArray();

                    var jntuhCollegeAffiliation = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType);
                    collegeInformation.affiliationGrade = jntuhCollegeAffiliation.Select(a => a.affiliationGrade).FirstOrDefault();
                    collegeInformation.affiliationCGPA = jntuhCollegeAffiliation.Select(a => a.CGPA).FirstOrDefault();
                }

                rowIndex++;
            }

            if (collegeInformation.affiliationFromDate1 != null)
            {
                //collegeInformation.affiliationFromDate1 = new string[] { 
                //    collegeInformation.affiliationFromDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString() 
                //};
                collegeInformation.affiliationFromDate1 = collegeInformation.affiliationFromDate1 == null ? string.Empty :
                Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.ToString());
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
              Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate3.ToString());
            }

            if (collegeInformation.affiliationFromDate4 != null)
            {
                collegeInformation.affiliationFromDate4 = new string[] { 
                    collegeInformation.affiliationFromDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate4.FirstOrDefault()).ToString()  
                };
            }

            if (collegeInformation.affiliationFromDate5 != null)
            {
                collegeInformation.affiliationFromDate5 = new string[] { 
                    collegeInformation.affiliationFromDate5.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate5.FirstOrDefault()).ToString()  
                };
            }
            if (collegeInformation.affiliationToDate1 != null)
            {
                //collegeInformation.affiliationToDate1 = new string[] { 
                //    collegeInformation.affiliationToDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString() 
                //};
                collegeInformation.affiliationToDate1 = collegeInformation.affiliationToDate1 == null
                    ? string.Empty
                    : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.ToString());
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
                    : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate3.ToString());
            }

            if (collegeInformation.affiliationToDate4 != null)
            {
                collegeInformation.affiliationToDate4 = new string[] { 
                    collegeInformation.affiliationToDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate4.FirstOrDefault()).ToString()  
                };
            }

            if (collegeInformation.affiliationToDate5 != null)
            {
                collegeInformation.affiliationToDate5 = new string[] { 
                    collegeInformation.affiliationToDate5.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate5.FirstOrDefault()).ToString()  
                };
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
            if (collegeInformation.affiliationDuration5 != null)
            {
                collegeInformation.affiliationDuration5 = new string[] { 
                    collegeInformation.affiliationDuration5.Length == 0 ? string.Empty : collegeInformation.affiliationDuration5.FirstOrDefault()  
                };
            }

            ViewBag.Status = db.jntuh_college_status.Where(s => s.isActive == true && s.mainstatusId == 0).ToList();
            ViewBag.SubStatus = db.jntuh_college_status.Where(s => s.isActive == true && s.mainstatusId != 0).ToList();
            ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
            ViewBag.AffiliationType = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(f => f.displayOrder).ToList();

            collegeInformation.collegeStatus = db.jntuh_college_status.Where(s => s.id == collegeInformation.collegeStatusID).Select(s => s.collegeStatus).FirstOrDefault();
            ViewBag.StateName = db.jntuh_state.Where(s => s.id == collegeInformation.stateId).Select(s => s.stateName).FirstOrDefault();
            ViewBag.DistrictName = db.jntuh_district.Where(s => s.id == collegeInformation.districtId).Select(s => s.districtName).FirstOrDefault();
            //  return View("View");
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
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {

                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CF") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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
                if (jntuh_college != null)
                {
                    collegeInformation.id = jntuh_college.id;
                    collegeInformation.collegeCode = jntuh_college.collegeCode;
                    collegeInformation.collegeName = jntuh_college.collegeName;
                    collegeInformation.collegeStatusID = jntuh_college.collegeStatusID;
                    collegeInformation.eamcetCode = jntuh_college.eamcetCode;
                    collegeInformation.icetCode = jntuh_college.icetCode;

                    collegeInformation.pgcetCode = String.IsNullOrEmpty(jntuh_college.pgcetCode) ? String.Empty : jntuh_college.pgcetCode;
                    collegeInformation.aicteid = String.IsNullOrEmpty(jntuh_college.aicteId) ? String.Empty : jntuh_college.aicteId;

                    //collegeInformation.otherCategory = jntuh_college.otherCategory;
                    collegeInformation.createdBy = jntuh_college.createdBy;
                    collegeInformation.createdOn = jntuh_college.createdOn;
                    collegeInformation.updatedBy = jntuh_college.createdBy;
                    collegeInformation.updatedOn = jntuh_college.createdOn;

                    var collegestatus =
                 db.jntuh_college_status.Where(c => c.id == collegeInformation.collegeStatusID)
                     .Select(s => s.collegeStatus)
                     .FirstOrDefault();
                    if (collegestatus == "Minority")
                    {
                        jntuh_college_minoritystatus minoritystatus =
                            db.jntuh_college_minoritystatus.Where(r => r.collegeId == userCollegeID)
                                .Select(s => s)
                                .FirstOrDefault();
                        if (minoritystatus != null)
                        {
                            collegeInformation.collegeminorityStatus = db.jntuh_college_status.Where(s => s.id == minoritystatus.collegeStatusid).Select(s => s.collegeStatus).FirstOrDefault();
                            collegeInformation.minortyid = minoritystatus.id;
                            collegeInformation.academicYearid = minoritystatus.academicYearid;
                            collegeInformation.collegesubstatusId = minoritystatus.collegeStatusid;
                            DateTime fromdate = Convert.ToDateTime(minoritystatus.statusFromdate);
                            collegeInformation.collegestatusfromdate = fromdate.ToString("dd/MM/yyyy").Split(' ')[0];
                            DateTime todate = Convert.ToDateTime(minoritystatus.statusTodate);
                            collegeInformation.collegestatustodate = todate.ToString("dd/MM/yyyy").Split(' ')[0];
                            collegeInformation.collegestatusfilepath = minoritystatus.statusFile;

                        }
                    }
                }

                jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == jntuh_college.id && a.addressTye == "COLLEGE").Select(a => a).ToList().FirstOrDefault();
                if (jntuh_address != null)
                {
                    collegeInformation.address = jntuh_address.address;
                    collegeInformation.townOrCity = jntuh_address.townOrCity;
                    collegeInformation.mandal = jntuh_address.mandal;
                    collegeInformation.stateId = jntuh_address.stateId;
                    collegeInformation.districtId = jntuh_address.districtId;
                    collegeInformation.pincode = jntuh_address.pincode;
                    collegeInformation.fax = jntuh_address.fax;
                    collegeInformation.landline = jntuh_address.landline;
                    collegeInformation.mobile = jntuh_address.mobile;
                    collegeInformation.email = jntuh_address.email;
                    collegeInformation.website = jntuh_address.website;
                }

                //after postback
                string[] selectedCollegeAffiliation = jntuh_college.collegeAffiliationTypeID.ToString().Split(' ');
                List<Item> lstAffiliationType = new List<Item>();
                foreach (var type in db.jntuh_college_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.DisplayOrder).ToList())
                {
                    string strType = type.id.ToString();
                    lstAffiliationType.Add(new Item { id = type.id, name = type.collegeAffiliationType, selected = selectedCollegeAffiliation.Contains(strType) ? 1 : 0 });

                }

                collegeInformation.collegeAffiliationType = lstAffiliationType;

                string[] selectedCollegeType = jntuh_college.collegeTypeID.ToString().Split(' ');
                List<Item> lstCollegeType = new List<Item>();
                foreach (var type in db.jntuh_college_type.Where(s => s.isActive == true))
                {
                    string strType = type.id.ToString();
                    lstCollegeType.Add(new Item { id = type.id, name = type.collegeType, selected = selectedCollegeType.Contains(strType) ? 1 : 0 });
                }

                collegeInformation.collegeType = lstCollegeType;

                string[] selectedCollegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == jntuh_college.id && d.isActive == true).Select(d => d.degreeId).ToArray().Select(s => s.ToString()).ToArray();
                List<Item> lstDegree = new List<Item>();
                foreach (var d in db.jntuh_degree.Where(s => s.isActive == true).OrderBy(s => s.degreeDisplayOrder))
                {
                    string strType = d.id.ToString();
                    lstDegree.Add(new Item { id = d.id, name = d.degree, selected = selectedCollegeDegree.Contains(strType) ? 1 : 0 });
                }

                collegeInformation.degree = lstDegree;

                string[] selectedAffiliationType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id).OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationTypeId).ToArray().Select(s => s.ToString()).ToArray();
                List<Item> lstType = new List<Item>();
                foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder).ToList())
                {
                    string strType = t.id.ToString();
                    lstType.Add(new Item { id = t.id, name = t.affiliationType, selected = selectedAffiliationType.Contains(strType) ? 1 : 0 });
                }
                int[] selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder)
                                                                          .Select(s => s.id).ToArray();
                int affiliationCount = 1;
                foreach (var item in selectedAffiliationId)
                {
                    if (affiliationCount == 1)
                    {
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                        collegeInformation.affiliationFromDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationToDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationDuration1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationfilepath1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.filePath).FirstOrDefault();

                        collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                        collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                    }

                    if (rowIndex == 2)
                    {
                        collegeInformation.affiliationFromDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                     .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                     .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationToDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationDuration3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationfilepath3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.filePath).FirstOrDefault();
                        collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                        collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                    }



                    rowIndex++;
                }

                if (collegeInformation.affiliationFromDate1 != null)
                {
                    //    collegeInformation.affiliationFromDate1 = new string[] { 
                    //    collegeInformation.affiliationFromDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString() 
                    //};
                    collegeInformation.affiliationfilepath1 = collegeInformation.affiliationfilepath1;
                    collegeInformation.affiliationFromDate1 = collegeInformation.affiliationFromDate1 == null ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.ToString());
                }

                if (collegeInformation.affiliationFromDate2 != null)
                {
                    collegeInformation.affiliationFromDate2 = new string[] { 
                    collegeInformation.affiliationFromDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate2.FirstOrDefault()).ToString()  
                };
                }

                if (collegeInformation.affiliationFromDate3 != null)
                {
                    collegeInformation.affiliationfilepath3 = collegeInformation.affiliationfilepath3;
                    collegeInformation.affiliationFromDate3 = collegeInformation.affiliationFromDate3 == null ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate3.ToString());
                }


                if (collegeInformation.affiliationToDate1 != null)
                {
                    //    collegeInformation.affiliationToDate1 = new string[] { 
                    //    collegeInformation.affiliationToDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString() 
                    //};
                    collegeInformation.affiliationToDate1 = collegeInformation.affiliationToDate1 == null ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.ToString());
                }

                if (collegeInformation.affiliationToDate2 != null)
                {
                    collegeInformation.affiliationToDate2 = new string[] { 
                    collegeInformation.affiliationToDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate2.FirstOrDefault()).ToString()  
                };
                }

                if (collegeInformation.affiliationToDate3 != null)
                {
                    collegeInformation.affiliationToDate3 = collegeInformation.affiliationToDate3 == null ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate3.ToString());
                }

                if (collegeInformation.affiliationToDate4 != null)
                {
                    collegeInformation.affiliationToDate4 = new string[] { 
                    collegeInformation.affiliationToDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate4.FirstOrDefault()).ToString()  
                };
                }

                if (collegeInformation.affiliationToDate5 != null)
                {
                    collegeInformation.affiliationToDate5 = new string[] { 
                    collegeInformation.affiliationToDate5.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate5.FirstOrDefault()).ToString()  
                };
                }
                if (collegeInformation.affiliationDuration1 != null)
                {
                    //    collegeInformation.affiliationDuration1 = new string[] { 
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
                if (collegeInformation.affiliationDuration5 != null)
                {
                    collegeInformation.affiliationDuration5 = new string[] { 
                    collegeInformation.affiliationDuration5.Length == 0 ? string.Empty : collegeInformation.affiliationDuration5.FirstOrDefault()  
                };
                }


                ViewBag.Status = db.jntuh_college_status.Where(s => s.isActive == true && s.mainstatusId == 0).ToList();
                ViewBag.SubStatus = db.jntuh_college_status.Where(s => s.isActive == true && s.mainstatusId != 0).ToList();
                ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
                ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
                ViewBag.AffiliationType = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(f => f.displayOrder).ToList();

                collegeInformation.collegeStatus = db.jntuh_college_status.Where(s => s.id == collegeInformation.collegeStatusID).Select(s => s.collegeStatus).FirstOrDefault();

                ViewBag.StateName = db.jntuh_state.Where(s => s.id == collegeInformation.stateId).Select(s => s.stateName).FirstOrDefault();
                ViewBag.DistrictName = db.jntuh_district.Where(s => s.id == collegeInformation.districtId).Select(s => s.districtName).FirstOrDefault();
            }

            return View("View", collegeInformation);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            CollegeInformation collegeInformation = new CollegeInformation();
            if (userCollegeID != 0)
            {
                jntuh_college jntuh_college = db.jntuh_college.Find(userCollegeID);
                if (jntuh_college != null)
                {
                    collegeInformation.id = jntuh_college.id;
                    collegeInformation.collegeCode = jntuh_college.collegeCode;
                    collegeInformation.collegeName = jntuh_college.collegeName;
                    collegeInformation.collegeStatusID = jntuh_college.collegeStatusID;
                    collegeInformation.eamcetCode = jntuh_college.eamcetCode;
                    collegeInformation.icetCode = jntuh_college.icetCode;
                    //collegeInformation.otherCategory = jntuh_college.otherCategory;
                    collegeInformation.createdBy = jntuh_college.createdBy;
                    collegeInformation.createdOn = jntuh_college.createdOn;
                    collegeInformation.updatedBy = jntuh_college.createdBy;
                    collegeInformation.updatedOn = jntuh_college.createdOn;
                }

                jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == jntuh_college.id && a.addressTye == "COLLEGE").Select(a => a).ToList().FirstOrDefault();
                if (jntuh_address != null)
                {
                    collegeInformation.address = jntuh_address.address;
                    collegeInformation.townOrCity = jntuh_address.townOrCity;
                    collegeInformation.mandal = jntuh_address.mandal;
                    collegeInformation.stateId = jntuh_address.stateId;
                    collegeInformation.districtId = jntuh_address.districtId;
                    collegeInformation.pincode = jntuh_address.pincode;
                    collegeInformation.fax = jntuh_address.fax;
                    collegeInformation.landline = jntuh_address.landline;
                    collegeInformation.mobile = jntuh_address.mobile;
                    collegeInformation.email = jntuh_address.email;
                    collegeInformation.website = jntuh_address.website;
                }
                string[] selectedCollegeAffiliation = jntuh_college.collegeAffiliationTypeID.ToString().Split(' ');
                List<Item> lstAffiliationType = new List<Item>();
                foreach (var type in db.jntuh_college_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.DisplayOrder).ToList())
                {
                    string strType = type.id.ToString();
                    lstAffiliationType.Add(new Item { id = type.id, name = type.collegeAffiliationType, selected = selectedCollegeAffiliation.Contains(strType) ? 1 : 0 });

                }

                collegeInformation.collegeAffiliationType = lstAffiliationType;

                string[] selectedCollegeType = jntuh_college.collegeTypeID.ToString().Split(' ');
                List<Item> lstCollegeType = new List<Item>();
                foreach (var type in db.jntuh_college_type.Where(s => s.isActive == true))
                {
                    string strType = type.id.ToString();
                    lstCollegeType.Add(new Item { id = type.id, name = type.collegeType, selected = selectedCollegeType.Contains(strType) ? 1 : 0 });
                }

                collegeInformation.collegeType = lstCollegeType;

                string[] selectedCollegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == jntuh_college.id && d.isActive == true).Select(d => d.degreeId).ToArray().Select(s => s.ToString()).ToArray();
                List<Item> lstDegree = new List<Item>();
                foreach (var d in db.jntuh_degree.Where(s => s.isActive == true).OrderBy(s => s.degreeDisplayOrder))
                {
                    string strType = d.id.ToString();
                    lstDegree.Add(new Item { id = d.id, name = d.degree, selected = selectedCollegeDegree.Contains(strType) ? 1 : 0 });
                }

                collegeInformation.degree = lstDegree;

                string[] selectedAffiliationType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id).OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationTypeId).ToArray().Select(s => s.ToString()).ToArray();
                List<Item> lstType = new List<Item>();
                foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder).ToList())
                {
                    string strType = t.id.ToString();
                    lstType.Add(new Item { id = t.id, name = t.affiliationType, selected = selectedAffiliationType.Contains(strType) ? 1 : 0 });
                }
                int[] selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder)
                                                                          .Select(s => s.id).ToArray();
                int affiliationCount = 1;
                foreach (var item in selectedAffiliationId)
                {
                    if (affiliationCount == 1)
                    {
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                        string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
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
                        collegeInformation.affiliationFromDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationToDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationDuration1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();

                        collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                        collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();

                    }

                    if (rowIndex == 2)
                    {
                        collegeInformation.affiliationFromDate2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();
                        collegeInformation.affiliationToDate2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();
                        collegeInformation.affiliationDuration2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();
                        collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                        collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();

                    }

                    if (rowIndex == 3)
                    {
                        collegeInformation.affiliationFromDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationToDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationDuration3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).FirstOrDefault();
                        collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                        collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();

                    }
                    if (rowIndex == 4)
                    {
                        collegeInformation.affiliationFromDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();
                        collegeInformation.affiliationDuration4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();
                        collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                        collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();

                    }
                    if (rowIndex == 5)
                    {
                        collegeInformation.affiliationFromDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();
                        collegeInformation.affiliationToDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();
                        collegeInformation.affiliationDuration5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                    .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                    .Select(s => s.ToString()).ToArray();
                        collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                        collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();

                    }

                    rowIndex++;
                }

                if (collegeInformation.affiliationFromDate1 != null)
                {
                    //    collegeInformation.affiliationFromDate1 = new string[] { 
                    //    collegeInformation.affiliationFromDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString() 
                    //};
                    collegeInformation.affiliationFromDate1 = collegeInformation.affiliationFromDate1 == null
                        ? string.Empty
                        : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.ToString());
                }

                if (collegeInformation.affiliationFromDate2 != null)
                {
                    collegeInformation.affiliationFromDate2 = new string[] { 
                    collegeInformation.affiliationFromDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate2.FirstOrDefault()).ToString()  
                };
                }

                if (collegeInformation.affiliationFromDate3 != null)
                {
                    collegeInformation.affiliationFromDate3 = collegeInformation.affiliationFromDate3 == null
                         ? string.Empty
                         : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate3.ToString());
                }

                if (collegeInformation.affiliationFromDate4 != null)
                {
                    collegeInformation.affiliationFromDate4 = new string[] { 
                    collegeInformation.affiliationFromDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate4.FirstOrDefault()).ToString()  
                };
                }

                if (collegeInformation.affiliationFromDate5 != null)
                {
                    collegeInformation.affiliationFromDate5 = new string[] { 
                    collegeInformation.affiliationFromDate5.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate5.FirstOrDefault()).ToString()  
                };
                }
                if (collegeInformation.affiliationToDate1 != null)
                {
                    //    collegeInformation.affiliationToDate1 = new string[] { 
                    //    collegeInformation.affiliationToDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString() 
                    //};
                    collegeInformation.affiliationToDate1 = collegeInformation.affiliationToDate1 == null
                        ? string.Empty
                        : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.ToString());
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
                        : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate3.ToString());
                }

                if (collegeInformation.affiliationToDate5 != null)
                {
                    collegeInformation.affiliationToDate5 = new string[] { 
                    collegeInformation.affiliationToDate5.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate5.FirstOrDefault()).ToString()  
                };
                }
                if (collegeInformation.affiliationDuration1 != null)
                {
                    //    collegeInformation.affiliationDuration1 = new string[] { 
                    //    collegeInformation.affiliationDuration1.Length == 0 ? string.Empty : collegeInformation.affiliationDuration1.FirstOrDefault()  
                    //};
                    collegeInformation.affiliationDuration1 = collegeInformation.affiliationDuration1 == null ? string.Empty : collegeInformation.affiliationDuration1.ToString();
                }
                if (collegeInformation.affiliationDuration2 != null)
                {
                    collegeInformation.affiliationDuration2 = new string[] { 
                    collegeInformation.affiliationDuration2.Length == 0 ? string.Empty : collegeInformation.affiliationDuration2.FirstOrDefault()  
                };
                }
                if (collegeInformation.affiliationDuration3 != null)
                {
                    collegeInformation.affiliationDuration3 = collegeInformation.affiliationDuration3 == null ? string.Empty : collegeInformation.affiliationDuration3.ToString();
                }
                if (collegeInformation.affiliationDuration4 != null)
                {
                    collegeInformation.affiliationDuration4 = new string[] { 
                    collegeInformation.affiliationDuration4.Length == 0 ? string.Empty : collegeInformation.affiliationDuration4.FirstOrDefault()  
                };
                }
                if (collegeInformation.affiliationDuration5 != null)
                {
                    collegeInformation.affiliationDuration5 = new string[] { 
                    collegeInformation.affiliationDuration5.Length == 0 ? string.Empty : collegeInformation.affiliationDuration5.FirstOrDefault()  
                };
                }
                ViewBag.StatusName = db.jntuh_college_status.Where(s => s.id == collegeInformation.collegeStatusID).Select(s => s.collegeStatus).FirstOrDefault();
                ViewBag.StateName = db.jntuh_state.Where(s => s.id == collegeInformation.stateId).Select(s => s.stateName).FirstOrDefault();
                ViewBag.DistrictName = db.jntuh_district.Where(s => s.id == collegeInformation.districtId).Select(s => s.districtName).FirstOrDefault();
            }
            return View("UserView", collegeInformation);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult PA_Create(string collegeId)
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
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            //if (userCollegeID > 0 && Roles.IsUserInRole("College"))
            //{
            //    return RedirectToAction("View", "CollegeInformation");
            //}
            //if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            //{
            //    return RedirectToAction("Edit", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            //}

            //if (userCollegeID == 0 && status == 0 && Roles.IsUserInRole("College"))
            //{
            //    ViewBag.NotUpload = true;
            //}
            //else
            //{
            //    ViewBag.NotUpload = false;
            //}
            CollegeInformation collegeInformation = new CollegeInformation();

            string[] strSelected = new string[] { };
            List<Item> lstAffiliationType = new List<Item>();

            foreach (var type in db.jntuh_college_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.DisplayOrder).ToList())
            {
                string strType = type.id.ToString();
                lstAffiliationType.Add(new Item { id = type.id, name = type.collegeAffiliationType, selected = strSelected.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.collegeAffiliationType = lstAffiliationType;

            List<Item> lstCollegeType = new List<Item>();

            foreach (var type in db.jntuh_college_type.Where(s => s.isActive == true))
            {
                string strType = type.id.ToString();
                lstCollegeType.Add(new Item { id = type.id, name = type.collegeType, selected = strSelected.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.collegeType = lstCollegeType;

            List<Item> lstDegree = new List<Item>();

            foreach (var d in db.jntuh_degree.Where(s => s.isActive == true).OrderBy(s => s.degreeDisplayOrder))
            {
                string strType = d.id.ToString();
                lstDegree.Add(new Item { id = d.id, name = d.degree, selected = strSelected.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.degree = lstDegree;

            List<Item> lstType = new List<Item>();

            foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder))
            {
                string strType = t.id.ToString();
                lstType.Add(new Item { id = t.id, name = t.affiliationType, selected = strSelected.Contains(strType) ? 1 : 0 });
            }

            collegeInformation.affiliationType = lstType;

            //collegeInformation.affiliationFromDate1 = new string[] { string.Empty };
            collegeInformation.affiliationFromDate2 = new string[] { string.Empty };
            //collegeInformation.affiliationFromDate3 = new string[] { string.Empty };
            collegeInformation.affiliationFromDate4 = new string[] { string.Empty };
            collegeInformation.affiliationFromDate5 = new string[] { string.Empty };

            //collegeInformation.affiliationToDate1 = new string[] { string.Empty };
            collegeInformation.affiliationToDate2 = new string[] { string.Empty };
            //collegeInformation.affiliationToDate3 = new string[] { string.Empty };
            collegeInformation.affiliationToDate4 = new string[] { string.Empty };
            collegeInformation.affiliationToDate5 = new string[] { string.Empty };

            //collegeInformation.affiliationDuration1 = new string[] { string.Empty };
            //collegeInformation.affiliationDuration2 = new string[] { string.Empty };
            //collegeInformation.affiliationDuration3 = new string[] { string.Empty };
            collegeInformation.affiliationDuration4 = new string[] { string.Empty };
            collegeInformation.affiliationDuration5 = new string[] { string.Empty };

            var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();
            var pgIds = new int[] { 1, 2, 3, 6 };
            ViewBag.pgcetStatus = false;
            var pgstats = DegreeIds.Where(i => pgIds.Contains(i)).ToList();
            if (pgstats.Count > 0)
            {
                ViewBag.pgcetStatus = true;
            }

            ViewBag.Status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
            ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
            ViewBag.AffiliationType = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(f => f.displayOrder).ToList();
            //return RedirectToAction("Create", "AffiliationTypes");
            return View(collegeInformation);
        }
    }
}
