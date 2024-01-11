using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class PA_InstructionalLandController : BaseController
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
            int[] area = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL").Select(r => r.id).ToArray();
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
            else if (userCollegeID == 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (userCollegeID > 0 && collegeAreaId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "PA_InstructionalLand");
            }
            if (userCollegeID > 0 && collegeAreaId > 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Edit", "PA_InstructionalLand", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (collegeAreaId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PIL") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_InstructionalLand");
            }

            List<string> collegeDegrees = db.jntuh_college_degree
                                         .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
                                         .Where(s => s.d.isActive == true && s.cd.collegeId == userCollegeID)
                                         .OrderBy(s => s.d.degreeDisplayOrder)
                                         .Select(s => s.d.degree).ToList();

            List<AdminLand> land = new List<AdminLand>();

            foreach (string degree in collegeDegrees)
            {
                var programType = db.jntuh_program_type.Where(p => p.programType == degree).FirstOrDefault();

                ////RAMESH: Commented to remove specialization wise records for - M.Tech & M.Pharmacy

                //if (string.Equals(degree.ToUpper(), "M.PHARMACY") || string.Equals(degree.ToUpper(), "M.TECH"))
                //{
                //    var collegeDegreeSpecializations = db.jntuh_college_intake_proposed
                //                                         .Join(db.jntuh_specialization, prop => prop.specializationId, spec => spec.id, (prop, spec) => new { prop, spec })
                //                                         .Join(db.jntuh_department, a => a.spec.departmentId, dep => dep.id, (a, dep) => new { a, dep })
                //                                         .Join(db.jntuh_degree, b => b.dep.degreeId, d => d.id, (b, d) => new { b, d })
                //                                         .Where(s => s.b.a.spec.isActive == true && s.b.a.prop.collegeId == userCollegeID && s.d.degree == degree)
                //                                         .OrderBy(s => new { s.b.a.spec.specializationName, s.b.a.prop.shiftId })
                //                                         .Select(s => s).ToList();

                //    foreach (var specialization in collegeDegreeSpecializations)
                //    {
                //        var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
                //                         .Select(r => r).ToList();
                //        foreach (var areaRequirement in areaRequirements)
                //        {
                //            AdminLand newLand = new AdminLand();
                //            newLand.id = areaRequirement.id;
                //            newLand.requirementType = areaRequirement.requirementType;
                //            newLand.programId = areaRequirement.programId;
                //            newLand.specializationID = specialization.b.a.prop.specializationId;
                //            if (specialization.b.a.prop.shiftId == 2)
                //            {
                //                newLand.specializationName = specialization.b.a.spec.specializationName + " - 2";
                //            }
                //            else
                //            {
                //                newLand.specializationName = specialization.b.a.spec.specializationName;
                //            }

                //            newLand.requiredRooms = areaRequirement.requiredRooms;
                //            newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
                //            newLand.requiredArea = areaRequirement.requiredArea;
                //            newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
                //            newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
                //            newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
                //            newLand.jntuh_program_type = programType;
                //            newLand.availableRooms = null;
                //            newLand.availableArea = null;
                //            newLand.collegeId = userCollegeID;
                //            land.Add(newLand);
                //        }
                //    }
                //}
                //else
                //{
                var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
                                 .Select(r => r).ToList();
                foreach (var areaRequirement in areaRequirements)
                {
                    AdminLand newLand = new AdminLand();
                    newLand.id = areaRequirement.id;
                    newLand.requirementType = areaRequirement.requirementType;
                    newLand.programId = areaRequirement.programId;
                    newLand.requiredRooms = areaRequirement.requiredRooms;
                    newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
                    newLand.requiredArea = areaRequirement.requiredArea;
                    newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
                    newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
                    newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
                    newLand.jntuh_program_type = programType;
                    newLand.availableRooms = null;
                    newLand.availableArea = null;
                    newLand.supportingdoc = null;
                    newLand.supportingdocpath = null;
                    newLand.collegeId = userCollegeID;
                    land.Add(newLand);
                }
                //}
            }

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
            return RedirectToAction("View", new { id = userCollegeID });

            //List<string> collegeDegrees = db.jntuh_college_degree
            //                             .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
            //                             .Where(s => s.d.isActive == true && s.cd.collegeId == userCollegeID)
            //                             .OrderBy(s => s.d.degreeDisplayOrder)
            //                             .Select(s => s.d.degree).ToList();

            //List<AdminLand> land = new List<AdminLand>();

            //foreach (string degree in collegeDegrees)
            //{
            //    var programType = db.jntuh_program_type.Where(p => p.programType == degree).FirstOrDefault();

            //    if (string.Equals(degree.ToUpper(), "M.PHARMACY") || string.Equals(degree.ToUpper(), "M.TECH"))
            //    {
            //        var collegeDegreeSpecializations = db.jntuh_college_intake_proposed
            //                                             .Join(db.jntuh_specialization, prop => prop.specializationId, spec => spec.id, (prop, spec) => new { prop, spec })
            //                                             .Join(db.jntuh_department, a => a.spec.departmentId, dep => dep.id, (a, dep) => new { a, dep })
            //                                             .Join(db.jntuh_degree, b => b.dep.degreeId, d => d.id, (b, d) => new { b, d })
            //                                             .Where(s => s.b.a.spec.isActive == true && s.b.a.prop.collegeId == userCollegeID && s.d.degree == degree)
            //                                             .OrderBy(s => s.b.a.spec.specializationName)
            //                                             .Select(s => new { Text = s.b.a.spec.specializationName, Value = s.b.a.spec.id }).ToList();

            //        collegeDegreeSpecializations = collegeDegreeSpecializations.GroupBy(s => s.Text).Select(s => new { Text = s.First().Text, Value = s.First().Value }).ToList();

            //        foreach (var specialization in collegeDegreeSpecializations)
            //        {
            //            var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
            //                             .Select(r => r).ToList();
            //            foreach (var areaRequirement in areaRequirements)
            //            {
            //                AdminLand newLand = new AdminLand();
            //                newLand.id = areaRequirement.id;
            //                newLand.requirementType = areaRequirement.requirementType;
            //                newLand.programId = areaRequirement.programId;
            //                newLand.specializationID = specialization.Value;
            //                //if (specialization.b.a.prop.shiftId == 2)
            //                //{
            //                //    newLand.specializationName = specialization.b.a.spec.specializationName + " - 2";
            //                //}
            //                //else
            //                //{
            //                newLand.specializationName = specialization.Text;
            //                //}

            //                newLand.requiredRooms = areaRequirement.requiredRooms;
            //                newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
            //                newLand.requiredArea = areaRequirement.requiredArea;
            //                newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
            //                newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
            //                newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
            //                newLand.jntuh_program_type = programType;
            //                newLand.availableRooms = null;
            //                newLand.availableArea = null;
            //                newLand.collegeId = userCollegeID;
            //                land.Add(newLand);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
            //                         .Select(r => r).ToList();
            //        foreach (var areaRequirement in areaRequirements)
            //        {
            //            AdminLand newLand = new AdminLand();
            //            newLand.id = areaRequirement.id;
            //            newLand.requirementType = areaRequirement.requirementType;
            //            newLand.programId = areaRequirement.programId;
            //            newLand.requiredRooms = areaRequirement.requiredRooms;
            //            newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
            //            newLand.requiredArea = areaRequirement.requiredArea;
            //            newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
            //            newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
            //            newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
            //            newLand.jntuh_program_type = programType;
            //            newLand.availableRooms = null;
            //            newLand.availableArea = null;
            //            newLand.collegeId = userCollegeID;
            //            land.Add(newLand);
            //        }
            //    }
            //}

            //ViewBag.Count = land.Count();
            //return View(land);
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

                    jntuh_college_area collegeArea = db.jntuh_college_area.AsNoTracking().Where(a => a.collegeId == userCollegeID && a.areaRequirementId == item.id && a.specializationID == item.specializationID).Select(a => a).FirstOrDefault();
                    if (collegeArea == null)
                    {
                        area.createdBy = userID;
                        area.createdOn = DateTime.Now;
                        if (item.supportingdoc != null)
                        {
                            const string affiliationfile = "~/Content/Upload/College/InstructionalLand";
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
                        if ((item.availableRooms != null) && (item.availableArea != null))
                        {
                            collegeArea = area;
                            db.jntuh_college_area.Add(collegeArea);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        //var collegeEditArea = db.jntuh_college_area.AsNoTracking().Where(a => a.collegeId == userCollegeID && a.areaRequirementId == item.id && a.specializationID == item.specializationID).Select(a => a);
                        var createdBy = Convert.ToInt32(collegeArea.createdBy);
                        var createdon = Convert.ToDateTime(collegeArea.createdOn);
                        var supprtngdocpath = collegeArea.supportingdocument;
                        if (item.supportingdoc != null)
                        {
                            const string affiliationfile = "~/Content/Upload/College/InstructionalLand";
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
                        //db.Entry(area).CurrentValues.SetValues(area);
                        db.SaveChanges();
                    }
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID =
                        Convert.ToInt32(Utilities.DecryptString(collegeId,
                            WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }


            int[] area =
                db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL")
                    .Select(r => r.id)
                    .ToArray();
            int collegeAreaId =
                db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && area.Contains(a.areaRequirementId))
                    .Select(a => a.id)
                    .FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            else if (userCollegeID == 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Create", "CollegeInformation",
                    new
                    {
                        collegeId =
                            Utilities.EncryptString(userCollegeID.ToString(),
                                WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
            }
            if (collegeAreaId == 0)
            {
                return RedirectToAction("Create", "PA_InstructionalLand");
            }
            else if (collegeAreaId == 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Create", "PA_InstructionalLand",
                    new
                    {
                        collegeId =
                            Utilities.EncryptString(userCollegeID.ToString(),
                                WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_InstructionalLand");
            }
            else
            {
                bool isPageEditable =
                    db.jntuh_pa_college_screens_assigned.Where(
                        a => a.jntuh_college_screens.ScreenCode.Equals("PIL") && a.CollegeId == userCollegeID)
                        .Select(a => a.IsEditable)
                        .FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_InstructionalLand");
                }
            }

            ////RAMESH:To-DisableEdit
            //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
            //{
            //    ViewBag.IsEditable = false;
            //    return RedirectToAction("View", "InstructionalLand");
            //}
            //else
            //{
            //    ViewBag.IsEditable = true;
            //}

            //List<string> collegeDegrees = db.jntuh_college_degree
            //                             .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
            //                             .Where(s => s.d.isActive == true && s.cd.collegeId == userCollegeID)
            //                             .OrderBy(s => s.d.degreeDisplayOrder)
            //                             .Select(s => s.d.degree).ToList();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            List<string> collegeDegrees = (from ie in db.jntuh_college_intake_existing
                                           join s in db.jntuh_specialization on ie.specializationId equals s.id
                                           join d in db.jntuh_department on s.departmentId equals d.id
                                           join de in db.jntuh_degree on d.degreeId equals de.id
                                           where ie.academicYearId == (prAy - 1) && (ie.aicteApprovedIntake != 0 || ie.approvedIntake != 0) && ie.collegeId == userCollegeID
                                           orderby de.degreeDisplayOrder
                                           select de.degree
                ).Distinct().ToList();


            List<AdminLand> land = new List<AdminLand>();

            foreach (string degree in collegeDegrees)
            {
                var programType = db.jntuh_program_type.Where(p => p.programType == degree).FirstOrDefault();

                ////RAMESH: Commented to remove specialization wise records for - M.Tech & M.Pharmacy

                //if (string.Equals(degree.ToUpper(), "M.PHARMACY") || string.Equals(degree.ToUpper(), "M.TECH"))
                //{
                //    var collegeDegreeSpecializations = db.jntuh_college_intake_proposed
                //                                          .Join(db.jntuh_specialization, prop => prop.specializationId, spec => spec.id, (prop, spec) => new { prop, spec })
                //                                          .Join(db.jntuh_department, a => a.spec.departmentId, dep => dep.id, (a, dep) => new { a, dep })
                //                                          .Join(db.jntuh_degree, b => b.dep.degreeId, d => d.id, (b, d) => new { b, d })
                //                                          .Where(s => s.b.a.spec.isActive == true && s.b.a.prop.collegeId == userCollegeID && s.d.degree == degree)
                //                                          .OrderBy(s => s.b.a.spec.specializationName)
                //                                          .Select(s => new { Text = s.b.a.spec.specializationName, Value = s.b.a.spec.id }).ToList();

                //    collegeDegreeSpecializations = collegeDegreeSpecializations.GroupBy(s => s.Text).Select(s => new { Text = s.First().Text, Value = s.First().Value }).ToList();

                //    foreach (var specialization in collegeDegreeSpecializations)
                //    {
                //        var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
                //                         .Select(r => r).ToList();
                //        foreach (var areaRequirement in areaRequirements)
                //        {
                //            AdminLand newLand = new AdminLand();
                //            newLand.id = areaRequirement.id;
                //            newLand.requirementType = areaRequirement.requirementType;
                //            newLand.programId = areaRequirement.programId;
                //            newLand.specializationID = specialization.Value;
                //            //if (specialization.b.a.prop.shiftId == 2)
                //            //{
                //            //    newLand.specializationName = specialization.b.a.spec.specializationName + " - 2";
                //            //}
                //            //else
                //            //{
                //            newLand.specializationName = specialization.Text;
                //            //}

                //            newLand.requiredRooms = areaRequirement.requiredRooms;
                //            newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
                //            newLand.requiredArea = areaRequirement.requiredArea;
                //            newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
                //            newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
                //            newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
                //            newLand.jntuh_program_type = programType;
                //            newLand.availableRooms = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == newLand.specializationID).Select(a => a.availableRooms).FirstOrDefault();
                //            newLand.availableArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == newLand.specializationID).Select(a => a.availableArea).FirstOrDefault();
                //            newLand.collegeId = userCollegeID;
                //            land.Add(newLand);
                //        }
                //    }
                //}
                //else
                //{
                var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
                                 .Select(r => r).ToList();
                var collegeArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.specializationID == 0).ToList();
                foreach (var areaRequirement in areaRequirements)
                {
                    AdminLand newLand = new AdminLand();
                    newLand.id = areaRequirement.id;
                    newLand.requirementType = areaRequirement.requirementType;
                    newLand.programId = areaRequirement.programId;
                    newLand.requiredRooms = areaRequirement.requiredRooms;
                    newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
                    newLand.requiredArea = areaRequirement.requiredArea;
                    newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
                    newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
                    newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
                    newLand.jntuh_program_type = programType;
                    newLand.availableRooms = collegeArea.Where(a => a.areaRequirementId == areaRequirement.id).Select(a => a.availableRooms).FirstOrDefault();
                    newLand.availableArea = collegeArea.Where(a => a.areaRequirementId == areaRequirement.id).Select(a => a.availableArea).FirstOrDefault();
                    newLand.supportingdocpath = collegeArea.Where(a => a.areaRequirementId == areaRequirement.id).Select(a => a.supportingdocument).FirstOrDefault();
                    newLand.collegeId = userCollegeID;
                    land.Add(newLand);
                }
                //}
            }

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
            return RedirectToAction("View", new { id = userCollegeID });

            //List<string> collegeDegrees = db.jntuh_college_degree
            //                             .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
            //                             .Where(s => s.d.isActive == true && s.cd.collegeId == userCollegeID)
            //                             .OrderBy(s => s.d.degreeDisplayOrder)
            //                             .Select(s => s.d.degree).ToList();

            //List<AdminLand> land = new List<AdminLand>();

            //foreach (string degree in collegeDegrees)
            //{
            //    var programType = db.jntuh_program_type.Where(p => p.programType == degree).FirstOrDefault();

            //    if (string.Equals(degree.ToUpper(), "M.PHARMACY") || string.Equals(degree.ToUpper(), "M.TECH"))
            //    {
            //        var collegeDegreeSpecializations = db.jntuh_college_intake_proposed
            //                                             .Join(db.jntuh_specialization, prop => prop.specializationId, spec => spec.id, (prop, spec) => new { prop, spec })
            //                                             .Join(db.jntuh_department, a => a.spec.departmentId, dep => dep.id, (a, dep) => new { a, dep })
            //                                             .Join(db.jntuh_degree, b => b.dep.degreeId, d => d.id, (b, d) => new { b, d })
            //                                             .Where(s => s.b.a.spec.isActive == true && s.b.a.prop.collegeId == userCollegeID && s.d.degree == degree)
            //                                             .OrderBy(s => s.b.a.spec.specializationName)
            //                                             .Select(s => new { Text = s.b.a.spec.specializationName, Value = s.b.a.spec.id }).ToList();

            //        collegeDegreeSpecializations = collegeDegreeSpecializations.GroupBy(s => s.Text).Select(s => new { Text = s.First().Text, Value = s.First().Value }).ToList();

            //        foreach (var specialization in collegeDegreeSpecializations)
            //        {
            //            var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
            //                             .Select(r => r).ToList();
            //            foreach (var areaRequirement in areaRequirements)
            //            {
            //                AdminLand newLand = new AdminLand();
            //                newLand.id = areaRequirement.id;
            //                newLand.requirementType = areaRequirement.requirementType;
            //                newLand.programId = areaRequirement.programId;
            //                newLand.specializationID = specialization.Value;
            //                //if (specialization.b.a.prop.shiftId == 2)
            //                //{
            //                //    newLand.specializationName = specialization.b.a.spec.specializationName + " - 2";
            //                //}
            //                //else
            //                //{
            //                newLand.specializationName = specialization.Text;
            //                //}

            //                newLand.requiredRooms = areaRequirement.requiredRooms;
            //                newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
            //                newLand.requiredArea = areaRequirement.requiredArea;
            //                newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
            //                newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
            //                newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
            //                newLand.jntuh_program_type = programType;
            //                newLand.availableRooms = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == newLand.specializationID).Select(a => a.availableRooms).FirstOrDefault();
            //                newLand.availableArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == newLand.specializationID).Select(a => a.availableArea).FirstOrDefault();
            //                newLand.collegeId = userCollegeID;
            //                land.Add(newLand);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
            //                         .Select(r => r).ToList();
            //        foreach (var areaRequirement in areaRequirements)
            //        {
            //            AdminLand newLand = new AdminLand();
            //            newLand.id = areaRequirement.id;
            //            newLand.requirementType = areaRequirement.requirementType;
            //            newLand.programId = areaRequirement.programId;
            //            newLand.requiredRooms = areaRequirement.requiredRooms;
            //            newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
            //            newLand.requiredArea = areaRequirement.requiredArea;
            //            newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
            //            newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
            //            newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
            //            newLand.jntuh_program_type = programType;
            //            newLand.availableRooms = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == 0).Select(a => a.availableRooms).FirstOrDefault();
            //            newLand.availableArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == 0).Select(a => a.availableArea).FirstOrDefault();
            //            newLand.collegeId = userCollegeID;
            //            land.Add(newLand);
            //        }
            //    }
            //}

            //ViewBag.Count = land.Count();
            //return View("Create", land);
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

            int[] area = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL").Select(r => r.id).ToArray();
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
                //    //return RedirectToAction("View", "PA_InstructionalLand");
                //}
                //else
                //{
                //    ViewBag.IsEditable = true;
                //}
                ////ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PIL") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            //List<string> collegeDegrees = db.jntuh_college_degree
            //                              .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
            //                              .Where(s => s.d.isActive == true && s.cd.collegeId == userCollegeID)
            //                              .OrderBy(s => s.d.degreeDisplayOrder)
            //                              .Select(s => s.d.degree).ToList();
            List<string> collegeDegrees = (from ie in db.jntuh_college_intake_existing
                                           join s in db.jntuh_specialization on ie.specializationId equals s.id
                                           join d in db.jntuh_department on s.departmentId equals d.id
                                           join de in db.jntuh_degree on d.degreeId equals de.id
                                           where ie.academicYearId == (prAy - 1) && (ie.aicteApprovedIntake != 0 || ie.approvedIntake != 0) && ie.collegeId == userCollegeID
                                           orderby de.degreeDisplayOrder
                                           select de.degree
                ).Distinct().ToList();

            List<AdminLand> land = new List<AdminLand>();

            foreach (string degree in collegeDegrees)
            {
                var programType = db.jntuh_program_type.Where(p => p.programType == degree).FirstOrDefault();

                ////RAMESH: Commented to remove specialization wise records for - M.Tech & M.Pharmacy

                //if (string.Equals(degree.ToUpper(), "M.PHARMACY") || string.Equals(degree.ToUpper(), "M.TECH"))
                //{
                //    var collegeDegreeSpecializations = db.jntuh_college_intake_proposed
                //                                         .Join(db.jntuh_specialization, prop => prop.specializationId, spec => spec.id, (prop, spec) => new { prop, spec })
                //                                         .Join(db.jntuh_department, a => a.spec.departmentId, dep => dep.id, (a, dep) => new { a, dep })
                //                                         .Join(db.jntuh_degree, b => b.dep.degreeId, d => d.id, (b, d) => new { b, d })
                //                                         .Where(s => s.b.a.spec.isActive == true && s.b.a.prop.collegeId == userCollegeID && s.d.degree == degree)
                //                                         .OrderBy(s => s.b.a.spec.specializationName)
                //                                         .Select(s => new { Text = s.b.a.spec.specializationName, Value = s.b.a.spec.id }).ToList();

                //    collegeDegreeSpecializations = collegeDegreeSpecializations.GroupBy(s => s.Text).Select(s => new { Text = s.First().Text, Value = s.First().Value }).ToList();

                //    foreach (var specialization in collegeDegreeSpecializations)
                //    {
                //        var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
                //                         .Select(r => r).ToList();
                //        foreach (var areaRequirement in areaRequirements)
                //        {
                //            AdminLand newLand = new AdminLand();
                //            newLand.id = areaRequirement.id;
                //            newLand.requirementType = areaRequirement.requirementType;
                //            newLand.programId = areaRequirement.programId;
                //            newLand.specializationID = specialization.Value;
                //            //if (specialization.b.a.prop.shiftId == 2)
                //            //{
                //            //    newLand.specializationName = specialization.b.a.spec.specializationName + " - 2";
                //            //}
                //            //else
                //            //{
                //            newLand.specializationName = specialization.Text;
                //            //}

                //            newLand.requiredRooms = areaRequirement.requiredRooms;
                //            newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
                //            newLand.requiredArea = areaRequirement.requiredArea;
                //            newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
                //            newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
                //            newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
                //            newLand.jntuh_program_type = programType;
                //            newLand.availableRooms = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == newLand.specializationID).Select(a => a.availableRooms).FirstOrDefault();
                //            newLand.availableArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == newLand.specializationID).Select(a => a.availableArea).FirstOrDefault();
                //            newLand.collegeId = userCollegeID;
                //            land.Add(newLand);
                //        }
                //    }
                //}
                //else
                //{
                var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
                                 .Select(r => r).ToList();
                var collegeArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.specializationID == 0).ToList();
                foreach (var areaRequirement in areaRequirements)
                {
                    AdminLand newLand = new AdminLand();
                    newLand.id = areaRequirement.id;
                    newLand.requirementType = areaRequirement.requirementType;
                    newLand.programId = areaRequirement.programId;
                    newLand.requiredRooms = areaRequirement.requiredRooms;
                    newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
                    newLand.requiredArea = areaRequirement.requiredArea;
                    newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
                    newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
                    newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
                    newLand.jntuh_program_type = programType;
                    newLand.availableRooms = collegeArea.Where(a => a.areaRequirementId == areaRequirement.id).Select(a => a.availableRooms).FirstOrDefault();
                    newLand.availableArea = collegeArea.Where(a => a.areaRequirementId == areaRequirement.id).Select(a => a.availableArea).FirstOrDefault();
                    newLand.supportingdocpath = collegeArea.Where(a => a.areaRequirementId == areaRequirement.id).Select(a => a.supportingdocument).FirstOrDefault();
                    newLand.collegeId = userCollegeID;
                    land.Add(newLand);
                }
                //}
            }
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

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int[] area = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL").Select(r => r.id).ToArray();
            int collegeAreaId = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && area.Contains(a.areaRequirementId)).Select(a => a.id).FirstOrDefault();

            List<string> collegeDegrees = db.jntuh_college_degree
                                         .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
                                         .Where(s => s.d.isActive == true && s.cd.collegeId == userCollegeID)
                                         .OrderBy(s => s.d.degreeDisplayOrder)
                                         .Select(s => s.d.degree).ToList();

            List<AdminLand> land = new List<AdminLand>();

            foreach (string degree in collegeDegrees)
            {
                var programType = db.jntuh_program_type.Where(p => p.programType == degree).FirstOrDefault();

                if (string.Equals(degree.ToUpper(), "M.PHARMACY") || string.Equals(degree.ToUpper(), "M.TECH"))
                {
                    var collegeDegreeSpecializations = db.jntuh_college_intake_proposed
                                                         .Join(db.jntuh_specialization, prop => prop.specializationId, spec => spec.id, (prop, spec) => new { prop, spec })
                                                         .Join(db.jntuh_department, a => a.spec.departmentId, dep => dep.id, (a, dep) => new { a, dep })
                                                         .Join(db.jntuh_degree, b => b.dep.degreeId, d => d.id, (b, d) => new { b, d })
                                                         .Where(s => s.b.a.spec.isActive == true && s.b.a.prop.collegeId == userCollegeID && s.d.degree == degree)
                                                         .OrderBy(s => s.b.a.spec.specializationName)
                                                         .Select(s => new { Text = s.b.a.spec.specializationName, Value = s.b.a.spec.id }).ToList();

                    collegeDegreeSpecializations = collegeDegreeSpecializations.GroupBy(s => s.Text).Select(s => new { Text = s.First().Text, Value = s.First().Value }).ToList();

                    foreach (var specialization in collegeDegreeSpecializations)
                    {
                        var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
                                         .Select(r => r).ToList();
                        foreach (var areaRequirement in areaRequirements)
                        {
                            AdminLand newLand = new AdminLand();
                            newLand.id = areaRequirement.id;
                            newLand.requirementType = areaRequirement.requirementType;
                            newLand.programId = areaRequirement.programId;
                            newLand.specializationID = specialization.Value;
                            //if (specialization.b.a.prop.shiftId == 2)
                            //{
                            //    newLand.specializationName = specialization.b.a.spec.specializationName + " - 2";
                            //}
                            //else
                            //{
                            newLand.specializationName = specialization.Text;
                            //}

                            newLand.requiredRooms = areaRequirement.requiredRooms;
                            newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
                            newLand.requiredArea = areaRequirement.requiredArea;
                            newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
                            newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
                            newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
                            newLand.jntuh_program_type = programType;
                            newLand.availableRooms = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == newLand.specializationID).Select(a => a.availableRooms).FirstOrDefault();
                            newLand.availableArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == newLand.specializationID).Select(a => a.availableArea).FirstOrDefault();
                            newLand.collegeId = userCollegeID;
                            land.Add(newLand);
                        }
                    }
                }
                else
                {
                    var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
                                     .Select(r => r).ToList();
                    foreach (var areaRequirement in areaRequirements)
                    {
                        AdminLand newLand = new AdminLand();
                        newLand.id = areaRequirement.id;
                        newLand.requirementType = areaRequirement.requirementType;
                        newLand.programId = areaRequirement.programId;
                        newLand.requiredRooms = areaRequirement.requiredRooms;
                        newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
                        newLand.requiredArea = areaRequirement.requiredArea;
                        newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
                        newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
                        newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
                        newLand.jntuh_program_type = programType;
                        newLand.availableRooms = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == 0).Select(a => a.availableRooms).FirstOrDefault();
                        newLand.availableArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == areaRequirement.id && a.specializationID == 0).Select(a => a.availableArea).FirstOrDefault();
                        newLand.collegeId = userCollegeID;
                        land.Add(newLand);
                    }
                }
            }

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
