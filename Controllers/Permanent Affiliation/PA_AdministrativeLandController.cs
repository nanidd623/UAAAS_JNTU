using System;
using System.Collections.Generic;
using System.Data;
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
    public class PA_AdministrativeLandController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            int[] area = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").Select(r => r.id).ToArray();
            int collegeAreaId = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && area.Contains(a.areaRequirementId)).Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (userCollegeID > 0 && collegeAreaId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "PA_AdministrativeLand");
            }
            if (userCollegeID > 0 && collegeAreaId > 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Edit", "PA_AdministrativeLand", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (collegeAreaId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PAL") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_AdministrativeLand");
            }
            var jntuhProgramType = db.jntuh_program_type.AsNoTracking().ToList();
            List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
                                     .Select(r => new AdminLand
                                     {
                                         id = r.id,
                                         requirementType = r.requirementType,
                                         programId = r.programId,
                                         requiredRooms = r.requiredRooms,
                                         requiredRoomsCalculation = r.requiredRoomsCalculation,
                                         requiredArea = r.requiredArea,
                                         requiredAreaCalculation = r.requiredAreaCalculation,
                                         areaTypeDescription = r.areaTypeDescription,
                                         areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                                         jntuh_program_type = jntuhProgramType.Where(p => p.id == r.programId).FirstOrDefault(),
                                         availableRooms = null,
                                         availableArea = null,
                                         supportingdoc = null,
                                         supportingdocpath = null,
                                         collegeId = userCollegeID
                                     }).ToList();
            //AdministrativeLand adminLand = new AdministrativeLand();
            //adminLand.adminLand = land;
            ViewBag.Count = land.Count();
            return View(land);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<AdminLand> adminLand)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (adminLand != null)
                {
                    foreach (var item in adminLand)
                    {
                        userCollegeID = (int)item.collegeId;
                    }
                }
            }
            SaveArea(adminLand);
            TempData["Success"] = "Added successfully";
            List<AdminLand> land = new List<AdminLand>();
            var collegeArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID).ToList();
            var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
                                 .Select(r => r).ToList();
            foreach (var r in areaRequirements)
            {
                var newLand = new AdminLand
                {
                    id = r.id,
                    requirementType = r.requirementType,
                    programId = r.programId,
                    requiredRooms = r.requiredRooms,
                    requiredRoomsCalculation = r.requiredRoomsCalculation,
                    requiredArea = r.requiredArea,
                    requiredAreaCalculation = r.requiredAreaCalculation,
                    areaTypeDescription = r.areaTypeDescription,
                    areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                    jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                    availableRooms = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                    availableArea = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
                    supportingdocpath = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.supportingdocument).FirstOrDefault(),
                    collegeId = userCollegeID
                };
                land.Add(newLand);
            }
            //List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
            //                         .Select(r => new AdminLand
            //                         {
            //                             id = r.id,
            //                             requirementType = r.requirementType,
            //                             programId = r.programId,
            //                             requiredRooms = r.requiredRooms,
            //                             requiredRoomsCalculation = r.requiredRoomsCalculation,
            //                             requiredArea = r.requiredArea,
            //                             requiredAreaCalculation = r.requiredAreaCalculation,
            //                             areaTypeDescription = r.areaTypeDescription,
            //                             areaTypeDisplayOrder = r.areaTypeDisplayOrder,
            //                             jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
            //                             availableRooms = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
            //                             availableArea = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
            //                             supportingdocpath = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.supportingdocument).FirstOrDefault(),
            //                             collegeId = userCollegeID
            //                         }).ToList();

            ViewBag.Count = land.Count();
            return View(land);
        }

        private void SaveArea(ICollection<AdminLand> adminLand)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (adminLand != null)
                {
                    foreach (var item in adminLand)
                    {
                        userCollegeID = (int)item.collegeId;
                    }
                }
            }
            var collegeCode = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault();
            if (ModelState.IsValid)
            {
                foreach (AdminLand item in adminLand)
                {
                    jntuh_college_area area = new jntuh_college_area();
                    area.areaRequirementId = item.id;
                    if (userCollegeID == 0)
                    {
                        area.collegeId = (int)item.collegeId;
                    }
                    else
                    {
                        area.collegeId = userCollegeID;
                    }
                    area.availableRooms = item.availableRooms;
                    area.availableArea = item.availableArea;

                    if (item.specializationID == null)
                    {
                        area.specializationID = 0;
                    }
                    else
                    {
                        area.specializationID = item.specializationID;
                    }

                    var collegeArea = db.jntuh_college_area.AsNoTracking().Where(a => a.collegeId == userCollegeID && a.areaRequirementId == item.id).Select(a => a).FirstOrDefault();
                    if (collegeArea == null)
                    {
                        area.createdBy = userID;
                        area.createdOn = DateTime.Now;
                        if (item.supportingdoc != null)
                        {
                            const string affiliationfile = "~/Content/Upload/College/AdministrativeLand";
                            if (!Directory.Exists(Server.MapPath(affiliationfile)))
                            {
                                Directory.CreateDirectory(Server.MapPath(affiliationfile));
                            }
                            var ext = Path.GetExtension(item.supportingdoc.FileName);
                            if (ext != null && (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF")))
                            {
                                if (item.supportingdocpath == null)
                                {
                                    string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "AdmLnd";
                                    item.supportingdoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                                    item.supportingdocpath = string.Format("{0}{1}", fileName, ext);
                                    area.supportingdocument = item.supportingdocpath;
                                }
                                else
                                {
                                    item.supportingdoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), item.supportingdocpath));
                                    area.supportingdocument = item.supportingdocpath;
                                }
                            }
                        }
                        if ((item.availableRooms != null) && (item.availableArea != null))
                        {
                            collegeArea = area;
                            db.jntuh_college_area.Add(collegeArea);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        var createdBy = Convert.ToInt32(collegeArea.createdBy);
                        var createdon = Convert.ToDateTime(collegeArea.createdOn);
                        var supprtngdocpath = collegeArea.supportingdocument;
                        if (item.supportingdoc != null)
                        {
                            const string affiliationfile = "~/Content/Upload/College/AdministrativeLand";
                            if (!Directory.Exists(Server.MapPath(affiliationfile)))
                            {
                                Directory.CreateDirectory(Server.MapPath(affiliationfile));
                            }
                            var ext = Path.GetExtension(item.supportingdoc.FileName);
                            if (ext != null && (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF")))
                            {
                                if (item.supportingdocpath == null)
                                {
                                    string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "InsLand";
                                    item.supportingdoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                                    item.supportingdocpath = string.Format("{0}{1}", fileName, ext);
                                    area.supportingdocument = item.supportingdocpath;
                                }
                                else
                                {
                                    item.supportingdoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), item.supportingdocpath));
                                    area.supportingdocument = item.supportingdocpath;
                                }
                            }
                        }
                        else
                        {
                            area.supportingdocument = supprtngdocpath;
                        }
                        if (createdBy != 0)
                        {
                            area.createdBy = createdBy;
                            area.createdOn = createdon;
                        }

                        area.id = collegeArea.id;
                        area.updatedBy = userID;
                        area.updatedOn = DateTime.Now;
                        collegeArea = area;
                        db.Entry(collegeArea).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            int[] area = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").Select(r => r.id).ToArray();
            int collegeAreaId = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && area.Contains(a.areaRequirementId)).Select(a => a.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (collegeAreaId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create");
            }
            else if (collegeAreaId == 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Create", "PA_AdministrativeLand", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_AdministrativeLand");
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PAL") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_AdministrativeLand");
                }
            }

            ////RAMESH:To-DisableEdit
            //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
            //{
            //    ViewBag.IsEditable = false;
            //    return RedirectToAction("View", "PA_AdministrativeLand");
            //}
            //else
            //{
            //    ViewBag.IsEditable = true;
            //}
            List<AdminLand> land = new List<AdminLand>();
            var collegeArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID).ToList();
            var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
                                 .Select(r => r).ToList();
            foreach (var r in areaRequirements)
            {
                var newLand = new AdminLand
                {
                    id = r.id,
                    requirementType = r.requirementType,
                    programId = r.programId,
                    requiredRooms = r.requiredRooms,
                    requiredRoomsCalculation = r.requiredRoomsCalculation,
                    requiredArea = r.requiredArea,
                    requiredAreaCalculation = r.requiredAreaCalculation,
                    areaTypeDescription = r.areaTypeDescription,
                    areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                    jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                    availableRooms = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                    availableArea = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
                    supportingdocpath = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.supportingdocument).FirstOrDefault(),
                    collegeId = userCollegeID
                };
                land.Add(newLand);
            }
            //List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
            //                         .Select(r => new AdminLand
            //                         {
            //                             id = r.id,
            //                             requirementType = r.requirementType,
            //                             programId = r.programId,
            //                             requiredRooms = r.requiredRooms,
            //                             requiredRoomsCalculation = r.requiredRoomsCalculation,
            //                             requiredArea = r.requiredArea,
            //                             requiredAreaCalculation = r.requiredAreaCalculation,
            //                             areaTypeDescription = r.areaTypeDescription,
            //                             areaTypeDisplayOrder = r.areaTypeDisplayOrder,
            //                             jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
            //                             availableRooms = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
            //                             availableArea = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
            //                             supportingdocpath = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.supportingdocument).FirstOrDefault(),
            //                             collegeId = userCollegeID
            //                         }).ToList();
            ViewBag.Count = land.Count();
            return View("Create", land);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<AdminLand> adminLand)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (adminLand != null)
                {
                    foreach (var item in adminLand)
                    {
                        userCollegeID = (int)item.collegeId;
                    }
                }
            }
            SaveArea(adminLand);
            TempData["Success"] = "Updated successfully";
            List<AdminLand> land = new List<AdminLand>();
            var collegeArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID).ToList();
            var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
                                 .Select(r => r).ToList();
            foreach (var r in areaRequirements)
            {
                var newLand = new AdminLand
                {
                    id = r.id,
                    requirementType = r.requirementType,
                    programId = r.programId,
                    requiredRooms = r.requiredRooms,
                    requiredRoomsCalculation = r.requiredRoomsCalculation,
                    requiredArea = r.requiredArea,
                    requiredAreaCalculation = r.requiredAreaCalculation,
                    areaTypeDescription = r.areaTypeDescription,
                    areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                    jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                    availableRooms = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                    availableArea = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
                    supportingdocpath = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.supportingdocument).FirstOrDefault(),
                    collegeId = userCollegeID
                };
                land.Add(newLand);
            }
            //List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
            //                         .Select(r => new AdminLand
            //                         {
            //                             id = r.id,
            //                             requirementType = r.requirementType,
            //                             programId = r.programId,
            //                             requiredRooms = r.requiredRooms,
            //                             requiredRoomsCalculation = r.requiredRoomsCalculation,
            //                             requiredArea = r.requiredArea,
            //                             requiredAreaCalculation = r.requiredAreaCalculation,
            //                             areaTypeDescription = r.areaTypeDescription,
            //                             areaTypeDisplayOrder = r.areaTypeDisplayOrder,
            //                             jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
            //                             availableRooms = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
            //                             availableArea = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
            //                             supportingdocpath = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.supportingdocument).FirstOrDefault(),
            //                             collegeId = userCollegeID
            //                         }).ToList();
            ViewBag.Count = land.Count();
            return View("View");
            // return View("Create", land);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int[] area = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").Select(r => r.id).ToArray();
            int collegeAreaId = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && area.Contains(a.areaRequirementId)).Select(a => a.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ////RAMESH:To-DisableEdit
                //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
                //{
                //    ViewBag.IsEditable = false;
                //    //return RedirectToAction("Index", "OtherColleges");
                //}
                //else
                //{
                //    ViewBag.IsEditable = true;
                //}
                ////ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PAL") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            List<AdminLand> land = new List<AdminLand>();
            var collegeArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID).ToList();
            var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
                                 .Select(r => r).ToList();
            foreach (var r in areaRequirements)
            {
                var newLand = new AdminLand
                {
                    id = r.id,
                    requirementType = r.requirementType,
                    programId = r.programId,
                    requiredRooms = r.requiredRooms,
                    requiredRoomsCalculation = r.requiredRoomsCalculation,
                    requiredArea = r.requiredArea,
                    requiredAreaCalculation = r.requiredAreaCalculation,
                    areaTypeDescription = r.areaTypeDescription,
                    areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                    jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                    availableRooms = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                    availableArea = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
                    supportingdocpath = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.supportingdocument).FirstOrDefault()
                };
                land.Add(newLand);
            }
            //List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
            //                             .Select(r => new AdminLand
            //                             {
            //                                 id = r.id,
            //                                 requirementType = r.requirementType,
            //                                 programId = r.programId,
            //                                 requiredRooms = r.requiredRooms,
            //                                 requiredRoomsCalculation = r.requiredRoomsCalculation,
            //                                 requiredArea = r.requiredArea,
            //                                 requiredAreaCalculation = r.requiredAreaCalculation,
            //                                 areaTypeDescription = r.areaTypeDescription,
            //                                 areaTypeDisplayOrder = r.areaTypeDisplayOrder,
            //                                 jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
            //                                 availableRooms = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
            //                                 availableArea = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
            //                                 supportingdocpath = collegeArea.Where(a => a.areaRequirementId == r.id).Select(a => a.supportingdocument).FirstOrDefault()
            //                             }).ToList();
            if (collegeAreaId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Count = land.Count();
            }
            return View("View", land);
        }

        /// <summary>
        /// College Open Land Screen Getting Action
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeFreeSpace()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //int userCollegeID = 375;

            AdminLand land = new AdminLand();
            jntuh_college_area jntuh_college_area =
                db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == 93)
                    .Select(s => s)
                    .FirstOrDefault();
            if (userCollegeID != 0)
            {
                if (jntuh_college_area != null)
                {
                    land.availableArea = jntuh_college_area.availableArea;
                }
            }

            return View(land);
        }

        /// <summary>
        /// College Open land Screen Posting Action and Saving in DB
        /// </summary>
        /// <param name="land"></param>
        /// <returns>one time adding in db</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeFreeSpace(AdminLand land)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //int userCollegeID = 375;
            if (userCollegeID != 0 && land.availableArea != 0 && land.availableArea != null && userCollegeID != null)
            {
                jntuh_college_area freeland = new jntuh_college_area();
                freeland.areaRequirementId =
                    db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "OPENLAND")
                        .Select(s => s.id)
                        .FirstOrDefault();
                freeland.areaRequirementId = freeland.areaRequirementId;
                freeland.availableArea = land.availableArea;
                freeland.availableRooms = null;
                freeland.collegeId = userCollegeID;
                freeland.createdBy = userID;
                freeland.createdOn = DateTime.Now;
                freeland.updatedBy = null;
                freeland.updatedOn = null;
                freeland.specializationID = 0;
                freeland.shiftId = 0;
                db.jntuh_college_area.Add(freeland);
                db.SaveChanges();
                TempData["Success"] = "College Open Land Area Information (In Acres) Added Successfully.";
                return View(land);
            }
            else
            {
                return RedirectToAction("CollegeFreeSpace");
            }

            //if (userCollegeID != 0 && land.availableArea == 0)
            //{
            //    TempData["Error"] = "Enter Above 0.00 Value.";
            //    return RedirectToAction("CollegeFreeSpace");
            //}
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int[] area = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").Select(r => r.id).ToArray();
            int collegeAreaId = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && area.Contains(a.areaRequirementId)).Select(a => a.id).FirstOrDefault();

            List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
                                         .Select(r => new AdminLand
                                         {
                                             id = r.id,
                                             requirementType = r.requirementType,
                                             programId = r.programId,
                                             requiredRooms = r.requiredRooms,
                                             requiredRoomsCalculation = r.requiredRoomsCalculation,
                                             requiredArea = r.requiredArea,
                                             requiredAreaCalculation = r.requiredAreaCalculation,
                                             areaTypeDescription = r.areaTypeDescription,
                                             areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                                             jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                                             availableRooms = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                                             availableArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault()
                                         }).ToList();
            if (collegeAreaId == 0)
            {
                ViewBag.NoRecords = true;
            }
            else
            {
                ViewBag.Count = land.Count();
            }
            return View("UserView", land);
        }
    }
}
