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

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class FacultyVerificationNewController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult Index(int? collegeid)
        {
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();

            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {
                string[] strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber).ToArray();

                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty
                                             .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeid))
                                             .ToList();

                var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
                {
                    id = a.id,
                    Type = a.type,
                    CollegeId = collegeid,
                    RegistrationNumber = a.RegistrationNumber,
                    UniqueID = a.UniqueID,
                    FirstName = a.FirstName,
                    MiddleName = a.MiddleName,
                    LastName = a.LastName,
                    GenderId = a.GenderId,
                    Email = a.Email,
                    facultyPhoto = a.Photo,
                    Mobile = a.Mobile,
                    PANNumber = a.PANNumber,
                    AadhaarNumber = a.AadhaarNumber,
                    isActive = a.isActive,
                    isApproved = a.isApproved,
                    department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault(),
                    SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count(),
                    SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count(),
                    isVerified = isFacultyVerified(a.id),
                    DeactivationReason = a.DeactivationReason,
                    updatedOn = a.updatedOn,
                    jntuh_registered_faculty_education = a.jntuh_registered_faculty_education
                }).ToList();

                teachingFaculty.AddRange(data);
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                return View(teachingFaculty);
            }

            return View(teachingFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyVerification(string fid)
        {

            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (fid != null)
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);

                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                regFaculty.Email = faculty.Email;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.GenderId = faculty.GenderId;
                if (faculty.DateOfBirth != null)
                {
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                }
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.PANNumber = faculty.PANNumber;
                regFaculty.facultyPANCardDocument = faculty.PANDocument;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                //regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.TotalExperience = faculty.TotalExperience;
                regFaculty.OrganizationName = faculty.OrganizationName;
                if (faculty.collegeId != null)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                }
                regFaculty.CollegeId = faculty.collegeId;
                if (faculty.DepartmentId != null)
                {
                    regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                }
                regFaculty.DepartmentId = faculty.DepartmentId;
                regFaculty.OtherDepartment = faculty.OtherDepartment;

                if (faculty.DesignationId != null)
                {
                    regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                }
                regFaculty.DesignationId = faculty.DesignationId;
                regFaculty.OtherDesignation = faculty.OtherDesignation;

                if (faculty.DateOfAppointment != null)
                {
                    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                }
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                {
                    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                }
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.GrossSalary = faculty.grosssalary;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.isActive = faculty.isActive;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.isView = true;
                regFaculty.DeactivationReason = faculty.DeactivationReason;


                regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                                                            .Select(e => new RegisteredFacultyEducation
                                                            {
                                                                educationId = e.id,
                                                                educationName = e.educationCategoryName,
                                                                studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                                                                specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                                                                passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                                                                percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                                                                division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                                                                university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                                                                place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                                                                facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
                                                            }).ToList();

                foreach (var item in regFaculty.FacultyEducation)
                {
                    if (item.division == null)
                        item.division = 0;
                }

                string registrationNumber = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
                int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();
                //Commented on 18-06-2018 by Narayana Reddy
                //int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();
                int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                //bool isValid = ShowHideLink(fID);

                //ViewBag.HideVerifyLink = isValid;

                //if (verificationOfficers.Contains(userId))
                //{
                //    if (isValid)
                //    {
                //        ViewBag.HideVerifyLink = true;
                //    }
                //    else
                //    {
                //        ViewBag.HideVerifyLink = false;
                //    }
                //}

                //if (verificationOfficers.Count() == 3)
                //{
                //    ViewBag.HideVerifyLink = true;
                //}

                ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
            }

            return View(regFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpGet]
        public ActionResult EditDepartment(string fid, int collegeId)
        {
            jntuh_registered_faculty faculty = new jntuh_registered_faculty();
            ViewBag.collegeId = collegeId;

            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;

            var CollegeDepts = db.jntuh_college_intake_existing.Where(s => s.collegeId == collegeId).Select(s => new
            {
                id = s.jntuh_specialization.jntuh_department.id,
                department = s.jntuh_specialization.jntuh_department.departmentName
            }).OrderBy(s => s.department).ToList();

            string[] aDepts = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };
            foreach (var dept in aDepts)
            {
                int deptId = db.jntuh_department.Where(d => d.departmentName == dept).Select(d => d.id).FirstOrDefault();

                if (deptId != 0)
                {
                    CollegeDepts.Add(new { id = deptId, department = dept });
                }

            }

            foreach (var item in CollegeDepts)
            {
                if (item.department == "B.Pharmacy" || item.department == "M.Pharmacy")
                {
                    var newitem = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => new
                    {
                        id = d.id,
                        department = d.departmentName
                    }).FirstOrDefault();

                    if (!existingDepts.Split(',').Contains(newitem.department))
                    {
                        depts.Add(new DistinctDepartment { id = newitem.id, departmentName = newitem.department });
                        existingDepts = existingDepts + "," + newitem.department;
                    }
                }
                else
                {
                    if (!existingDepts.Split(',').Contains(item.department))
                    {
                        depts.Add(new DistinctDepartment { id = item.id, departmentName = item.department });
                        existingDepts = existingDepts + "," + item.department;
                    }
                }
            }

            ViewBag.department = depts;
            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                faculty = db.jntuh_registered_faculty.Find(fID);
            }
            return PartialView("_EditDepartment", faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpPost]
        public ActionResult EditDepartment(jntuh_registered_faculty faculty, FormCollection fc)
        {
            int collegeId = 0;
            int departmentId = 0;
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (faculty != null && fc["collegeId"] != null && fc["departmentId"] != null)
            {
                collegeId = Convert.ToInt32(fc["collegeId"].ToString());
                departmentId = Convert.ToInt32(fc["departmentId"].ToString());
                jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Find(faculty.id);
                //////////////jntuh_registered_faculty.collegeId = collegeId;
                jntuh_registered_faculty.DepartmentId = departmentId;
                jntuh_registered_faculty.updatedBy = userId;
                jntuh_registered_faculty.updatedOn = DateTime.Now;
                db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { collegeid = collegeId });

        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpGet]
        public ActionResult DeactivateFaculty(string fid, int collegeId)
        {
            jntuh_registered_faculty faculty = new jntuh_registered_faculty();
            ViewBag.collegeId = collegeId;
            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                faculty = db.jntuh_registered_faculty.Find(fID);
                faculty.DeactivationReasons = db.jntuh_faculty_deactivation_reason.Where(r => r.isActive == true).Select(r => new FacultyDeactivationReason { id = r.id, reason = r.reasonForDeactivation, selected = false }).ToList();
            }
            return PartialView("_DeactivateFaculty", faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpPost]
        public ActionResult DeactivateFaculty(jntuh_registered_faculty faculty, FormCollection fc)
        {
            int collegeId = 0;
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (faculty != null && !string.IsNullOrEmpty(fc["collegeId"]) && !string.IsNullOrEmpty(fc["DeactivationReason"]))
            {
                collegeId = Convert.ToInt32(fc["collegeId"].ToString());
                jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Find(faculty.id);
                jntuh_registered_faculty.updatedBy = userId;
                jntuh_registered_faculty.updatedOn = DateTime.Now;

                jntuh_registered_faculty.isApproved = false;
                jntuh_registered_faculty.DeactivatedBy = userId;
                jntuh_registered_faculty.DeactivatedOn = DateTime.Now;
                string[] iReaons = fc["DeactivationReason"].Split(',');

                string dReason = string.Empty;
                foreach (var item in iReaons)
                {
                    int id = Convert.ToInt32(item);
                    dReason += db.jntuh_faculty_deactivation_reason.Find(id).reasonForDeactivation + ",";
                }

                dReason = dReason.Remove(dReason.Length - 1);

                jntuh_registered_faculty.DeactivationReason = dReason; // faculty.DeactivationReason;

                db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { collegeid = collegeId });

        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        //[HttpPost]
        public ActionResult ReactivateFaculty(string fid, int collegeId)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Find(fID);
                jntuh_registered_faculty.updatedBy = userId;
                jntuh_registered_faculty.updatedOn = DateTime.Now;

                jntuh_registered_faculty.isApproved = null;
                jntuh_registered_faculty.DeactivatedBy = null;
                jntuh_registered_faculty.DeactivatedOn = null;
                jntuh_registered_faculty.DeactivationReason = null;

                db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { collegeid = collegeId });

        }
        public bool ShowHideLink(int fid)
        {
            bool isValid = false;

            return isValid;
        }
        //public bool ShowHideLink_old(int fid)
        //{
        //    bool isValid = false;

        //    string registrationNumber = db.jntuh_registered_faculty.Where(of => of.id == fid).Select(of => of.RegistrationNumber).FirstOrDefault();
        //    int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();

        //    int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();

        //    int verifiedCount = verificationOfficers.Count();

        //    if (verifiedCount > 0 && verifiedCount <= 2)
        //    {
        //        if (verifiedCount == 2)
        //        {
        //            List<jntuh_college_faculty_verified> list1 = new List<jntuh_college_faculty_verified>();
        //            List<jntuh_college_faculty_verified> list2 = new List<jntuh_college_faculty_verified>();

        //            int index = 0;
        //            foreach (var officer in verificationOfficers)
        //            {
        //                if (index == 0)
        //                {
        //                    list1 = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId && v.VerificationOfficer == officer).Select(v => v).ToList();
        //                }

        //                if (index == 1)
        //                {
        //                    list2 = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId && v.VerificationOfficer == officer).Select(v => v).ToList();
        //                }

        //                index++;
        //            }

        //            isValid = CompareVerification(list1, list2);
        //        }
        //    }

        //    return isValid;
        //}

        //public bool CompareVerification(List<jntuh_college_faculty_verified> list1, List<jntuh_college_faculty_verified> list2)
        //{
        //    bool isValid = false;

        //    int misMatchCount = 0;

        //    foreach (var item1 in list1)
        //    {
        //        jntuh_college_faculty_verified item2 = list2.Where(l => l.LabelId == item1.LabelId && l.IsValid == item1.IsValid).FirstOrDefault();

        //        if (item2 == null)
        //        {
        //            misMatchCount++;
        //        }
        //    }

        //    if (misMatchCount == 0)
        //    {
        //        isValid = true;
        //    }

        //    return isValid;
        //}

        public bool isFacultyVerified(int fid)
        {
            bool isVerified = false;

            var faculty = db.jntuh_registered_faculty.Find(fid);

            if (faculty.isApproved != null)
            {
                isVerified = true;
            }

            return isVerified;
        }

        public bool isFacultyVerified_old(int fid)
        {
            bool isVerified = false;

            bool isValid = ShowHideLink(fid);

            isVerified = isValid;

            string registrationNumber = db.jntuh_registered_faculty.Where(of => of.id == fid).Select(of => of.RegistrationNumber).FirstOrDefault();
            int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();
            //Commented on 18-06-2018 by Narayana Reddy
            //int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();

            if (isValid)
            {
                isVerified = true;
            }
            else
            {
                isVerified = false;
            }
            //Commented on 18-06-2018 by Narayana Reddy
            //if (verificationOfficers.Count() == 3)
            //{
            //    isVerified = true;
            //}

            return isVerified;
        }

        //[Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        //[HttpGet]
        //public ActionResult CheckList(string fid)
        //{
        //    int facultyId = 0;
        //    int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    List<CollegFacultyVerification> jntuh_registered_faculty_labels = new List<CollegFacultyVerification>();
        //    if (fid != null)
        //    {
        //        int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
        //        string regNo = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
        //        facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == regNo).Select(of => of.id).FirstOrDefault();
        //        int count = db.jntuh_college_faculty_verified.Where(fv => fv.FacultyId == facultyId && fv.VerificationOfficer == userId).Select(fv => fv.VerificationOfficer).Distinct().Count();
        //        List<int> VerificationOfficerIDs = db.jntuh_college_faculty_verified.Where(fv => fv.FacultyId == facultyId).Select(fv => fv.VerificationOfficer).Distinct().ToList();

        //        int v3count = VerificationOfficerIDs.Where(f => VerificationOfficerIDs.Contains(userId)).Count();
        //        if (count == 0)
        //        {
        //            if (VerificationOfficerIDs.Count() == 2 && v3count == 0)
        //            {
        //                List<CollegFacultyVerification> list1 = new List<CollegFacultyVerification>();
        //                List<CollegFacultyVerification> list2 = new List<CollegFacultyVerification>();

        //                int index = 0;
        //                foreach (var officer in VerificationOfficerIDs)
        //                {
        //                    if (index == 0)
        //                    {
        //                        list1 = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId && v.VerificationOfficer == officer).Select(v => new CollegFacultyVerification { LabelId = v.LabelId, IsValid = v.IsValid }).ToList();
        //                    }

        //                    if (index == 1)
        //                    {
        //                        list2 = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId && v.VerificationOfficer == officer).Select(v => new CollegFacultyVerification { LabelId = v.LabelId, IsValid = v.IsValid }).ToList();
        //                    }

        //                    index++;
        //                }

        //                //int[] labelIDs = list1.Intersect(list2).ToArray();
        //                List<CollegFacultyVerification> list3 = new List<CollegFacultyVerification>();
        //                list3 = (from l1 in list1
        //                         join l2 in list2 on new { LabelId = l1.LabelId, Isvalid = l1.IsValid } equals new { LabelId = l2.LabelId, Isvalid = l2.IsValid }
        //                         select new CollegFacultyVerification
        //                         {
        //                             LabelId = l1.LabelId,
        //                             IsValid = l1.IsValid
        //                         }).ToList();

        //                jntuh_registered_faculty_labels = db.jntuh_registered_faculty_labels
        //                                                       .Where(fl => fl.IsActive == true)
        //                                                       .Select(fl => new CollegFacultyVerification
        //                                                       {
        //                                                           Id = 0,
        //                                                           VerificationOfficers = VerificationOfficerIDs,
        //                                                           LabelId = fl.Id,
        //                                                           LabelName = fl.LabelName,
        //                                                           IsValid = null,
        //                                                           loggedinUserId = userId
        //                                                       }).ToList();


        //                foreach (var item in list3)
        //                {
        //                    var items = jntuh_registered_faculty_labels.Where(l => l.LabelId == item.LabelId).FirstOrDefault();

        //                    if (items.LabelId == item.LabelId)
        //                    {
        //                        items.IsValid = item.IsValid;
        //                    }
        //                }


        //                //foreach (var items in list3)
        //                //{
        //                //    foreach (var item in jntuh_registered_faculty_labels)
        //                //    {
        //                //        if (items.LabelId == item.LabelId)
        //                //        {
        //                //            item.IsValid = items.IsValid;
        //                //        }
        //                //    }
        //                //}


        //            }
        //            else
        //            {
        //                jntuh_registered_faculty_labels = db.jntuh_registered_faculty_labels
        //                                                    .Where(fl => fl.IsActive == true)
        //                                                    .Select(fl => new CollegFacultyVerification
        //                                                    {
        //                                                        Id = 0,
        //                                                        VerificationOfficers = VerificationOfficerIDs,
        //                                                        LabelId = fl.Id,
        //                                                        LabelName = fl.LabelName,
        //                                                        IsValid = null,
        //                                                        loggedinUserId = userId
        //                                                    }).ToList();
        //            }
        //        }
        //        else
        //        {
        //            jntuh_registered_faculty_labels = db.jntuh_college_faculty_verified
        //                                                   .Where(fl => fl.jntuh_registered_faculty_labels.IsActive == true && fl.FacultyId == facultyId && fl.VerificationOfficer == userId)
        //                                                   .Select(fl => new CollegFacultyVerification
        //                                                   {
        //                                                       Id = fl.Id,
        //                                                       VerificationOfficers = VerificationOfficerIDs,
        //                                                       LabelId = fl.LabelId,
        //                                                       LabelName = fl.jntuh_registered_faculty_labels.LabelName,
        //                                                       IsValid = fl.IsValid,
        //                                                       loggedinUserId = userId
        //                                                   }).ToList();
        //        }

        //    }

        //    return View(jntuh_registered_faculty_labels);
        //}

        //[Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        //[HttpPost]
        //public ActionResult CheckList(string fid, List<CollegFacultyVerification> checkList)
        //{
        //    int fID = 0;
        //    if (fid != null)
        //    {
        //        fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
        //        string regNo = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
        //        int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == regNo).Select(of => of.id).FirstOrDefault();
        //        int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //        int count = db.jntuh_college_faculty_verified.Where(fv => fv.FacultyId == facultyId && fv.VerificationOfficer == userId).Count();
        //        if (checkList.Count() > 0)
        //        {
        //            //var checkList1 = checkList.Where(c => c.IsValid != null).ToList();
        //            int vcount = db.jntuh_college_faculty_verified.Where(fv => fv.FacultyId == facultyId).Select(fv => fv.VerificationOfficer).Distinct().Count();
        //            if (ModelState.IsValid || (ModelState.IsValid == false && vcount == 2))
        //            {
        //                foreach (var item in checkList.Where(c => c.IsValid != null).ToList())
        //                {
        //                    //jntuh_college_faculty_verified verified = new jntuh_college_faculty_verified();
        //                    //verified.FacultyId = facultyId;
        //                    //verified.LabelId = item.LabelId;
        //                    //verified.IsValid = item.IsValid;
        //                    //verified.VerificationOfficer = userId;
        //                    //if (count == 0)
        //                    //{
        //                    //    verified.CreatedBy = userId;
        //                    //    verified.CreatedOn = DateTime.Now;
        //                    //    db.jntuh_college_faculty_verified.Add(verified);
        //                    //}
        //                    //else
        //                    //{
        //                    //    verified.Id = item.Id;
        //                    //    verified.CreatedBy = db.jntuh_college_faculty_verified.Where(fv => fv.Id == item.Id).Select(fv => fv.CreatedBy).FirstOrDefault();
        //                    //    verified.CreatedOn = db.jntuh_college_faculty_verified.Where(fv => fv.Id == item.Id).Select(fv => fv.CreatedOn).FirstOrDefault();
        //                    //    verified.UpdatedBy = userId;
        //                    //    verified.UpdatedOn = DateTime.Now;
        //                    //    db.Entry(verified).State = EntityState.Modified;
        //                    //}
        //                    //db.SaveChanges();

        //                }
        //                TempData["Success"] = "Faculty verification completed";
        //            }
        //        }
        //    }
        //    return RedirectToAction("CheckList", new { fid = Utilities.EncryptString(fID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        //}

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpGet]
        public ActionResult Approve(string fid, int collegeId, bool pan, CollegFacultyVerification faculty)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));

                jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Find(fID);
                jntuh_registered_faculty.updatedBy = userId;
                jntuh_registered_faculty.updatedOn = DateTime.Now;

                jntuh_registered_faculty.isApproved = true;

                if (pan == false)
                {
                    jntuh_registered_faculty.DeactivationReason = "PAN NUMBER APPROVAL PENDING";
                }

                db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index", new { collegeid = collegeId });

            //CollegFacultyVerification faculty = new CollegFacultyVerification();

            //if (fid != null)
            //{
            //    int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            //    string regNo = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
            //    int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == regNo).Select(of => of.id).FirstOrDefault();

            //    faculty.FacultyId = facultyId;
            //}

            //return View(faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpPost]
        public ActionResult Approve1(string fid, int collegeId, bool pan, CollegFacultyVerification faculty)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));

                jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Find(fID);
                jntuh_registered_faculty.updatedBy = userId;
                jntuh_registered_faculty.updatedOn = DateTime.Now;

                jntuh_registered_faculty.isApproved = true;

                if (pan == false)
                {
                    jntuh_registered_faculty.DeactivationReason = "PAN NUMBER APPROVAL PENDING";
                }

                db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index", new { collegeid = collegeId });
        }

        //[Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        //public ActionResult FacultyVerificationStatus(int? collegeid, int? Value, string exportType)
        //{
        //    string strcollegeCode = string.Empty;
        //    ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();

        //    List<SelectListItem> statuslist = new List<SelectListItem>();
        //    statuslist.Add(new SelectListItem { Text = "Approved", Value = "1" });
        //    statuslist.Add(new SelectListItem { Text = "Not Approved", Value = "2" });
        //    ViewBag.statuslist = statuslist;

        //    List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();
        //    if (collegeid != null)
        //    {
        //        strcollegeCode = db.jntuh_college.Find(collegeid).collegeCode;
        //        var jntuh_designation = db.jntuh_designation.ToList();
        //        var jntuh_department = db.jntuh_department.ToList();

        //        int[] facultyIDs = db.jntuh_college_faculty_verified.Select(fv => fv.FacultyId).Distinct().ToArray();

        //        string[] strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid && facultyIDs.Contains(cf.id)).Select(cf => cf.RegistrationNumber).ToArray();
        //        List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
        //        jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeid)).ToList();

        //        var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
        //        {
        //            id = a.id,
        //            Type = a.type,
        //            //UserName=a.
        //            CollegeCode = strcollegeCode,
        //            RegistrationNumber = a.RegistrationNumber,
        //            UniqueID = a.UniqueID,
        //            FirstName = a.FirstName,
        //            MiddleName = a.MiddleName,
        //            LastName = a.LastName,
        //            GenderId = a.GenderId,
        //            FatherOrhusbandName = a.FatherOrHusbandName,
        //            MotherName = a.MotherName,
        //            DateOfBirth = a.DateOfBirth,
        //            WorkingStatus = a.WorkingStatus,
        //            OrganizationName = a.OrganizationName,
        //            designation = jntuh_designation.Where(d => d.id == a.DesignationId).Select(d => d.designation).FirstOrDefault(),
        //            OtherDesignation = a.OtherDesignation,
        //            department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault(),
        //            OtherDepartment = a.OtherDepartment,
        //            GrossSalary = a.grosssalary,
        //            DateOfAppointment = a.DateOfAppointment,
        //            isFacultyRatifiedByJNTU = a.isFacultyRatifiedByJNTU,
        //            DateOfRatification = a.DateOfRatification,
        //            ProceedingsNo = a.ProceedingsNumber,
        //            AICTEFacultyId = a.AICTEFacultyId,
        //            TotalExperience = a.TotalExperience,
        //            TotalExperiencePresentCollege = a.TotalExperiencePresentCollege,
        //            PANNumber = a.PANNumber,
        //            AadhaarNumber = a.AadhaarNumber,
        //            Mobile = a.Mobile,
        //            Email = a.Email,
        //            National = a.National,
        //            InterNational = a.InterNational,
        //            Citation = a.Citation,
        //            Awards = a.Awards,
        //            facultyPhoto = a.Photo,
        //            isActive = a.isActive,
        //            isApproved = a.isApproved,
        //            SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count(),
        //            SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count(),
        //            isValid = isFacultyApprovedStatus(a.id)
        //        }).ToList();

        //        teachingFaculty.AddRange(data);

        //        if (Value != null)
        //        {
        //            if (Value == 1)
        //            {
        //                teachingFaculty = teachingFaculty.Where(f => f.isValid == true).ToList();
        //            }
        //            else
        //            {
        //                teachingFaculty = teachingFaculty.Where(f => f.isValid == false).ToList();
        //            }
        //        }

        //        if (exportType == "Excel")
        //        {
        //            Response.ClearContent();
        //            Response.Buffer = true;
        //            Response.AddHeader("content-disposition", "attachment; filename=" + strcollegeCode + "-Faculty.xls");
        //            Response.ContentType = "application/vnd.ms-excel";
        //            return PartialView("_FacultyVerificationStatus", teachingFaculty);
        //        }
        //        return View(teachingFaculty);

        //    }

        //    return View(teachingFaculty);
        //}

        //public bool isFacultyApprovedStatus(int fid)
        //{
        //    bool isValid = false;
        //    string registrationNumber = db.jntuh_registered_faculty.Where(of => of.id == fid).Select(of => of.RegistrationNumber).FirstOrDefault();
        //    int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();
        //    int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();

        //    List<CollegFacultyVerification> list1 = new List<CollegFacultyVerification>();
        //    List<CollegFacultyVerification> list2 = new List<CollegFacultyVerification>();
        //    List<CollegFacultyVerification> list3 = new List<CollegFacultyVerification>();
        //    int index = 0;
        //    foreach (var officer in verificationOfficers)
        //    {
        //        if (index == 0)
        //        {
        //            list1 = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId && v.VerificationOfficer == officer).Select(v => new CollegFacultyVerification { LabelId = v.LabelId, IsValid = v.IsValid }).ToList();
        //        }

        //        if (index == 1)
        //        {
        //            list2 = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId && v.VerificationOfficer == officer).Select(v => new CollegFacultyVerification { LabelId = v.LabelId, IsValid = v.IsValid }).ToList();
        //        }

        //        if (index == 2)
        //        {
        //            list3 = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId && v.VerificationOfficer == officer).Select(v => new CollegFacultyVerification { LabelId = v.LabelId, IsValid = v.IsValid }).ToList();
        //        }

        //        index++;
        //    }

        //    var checklist = (from l1 in list1
        //                     join l2 in list2 on new { LabelId = l1.LabelId, Isvalid = l1.IsValid } equals new { LabelId = l2.LabelId, Isvalid = l2.IsValid }
        //                     select new CollegFacultyVerification
        //                     {
        //                         LabelId = l1.LabelId,
        //                         IsValid = l1.IsValid
        //                     }).ToList();


        //    if (verificationOfficers.Count() == 2)
        //    {
        //        if (checklist.Where(c => c.IsValid == false).Count() == 0)
        //        {
        //            isValid = true;
        //        }
        //        else
        //        {
        //            isValid = false;
        //        }
        //    }
        //    else if (verificationOfficers.Count() == 3)
        //    {
        //        if (checklist.Where(c => c.IsValid == false).Count() == 0)
        //        {
        //            if (list3.Where(l => l.IsValid == false).Count() == 0)
        //            {
        //                isValid = true;
        //            }
        //            else
        //            {
        //                isValid = false;
        //            }
        //        }
        //        else
        //        {
        //            isValid = false;
        //        }
        //    }
        //    return isValid;
        //}
        //Commented on 14-06-2018 by Narayana Reddy
        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //public ActionResult CollegeIntake(int? collegeId)
        //{
        //    List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

        //    var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
        //    {
        //        collegeId = c.id,
        //        collegeName = c.collegeCode + "-" + c.collegeName
        //    }).OrderBy(c => c.collegeName).ToList();
        //    ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
        //    //collegeId = 375;
        //    if (collegeId != null)
        //    {
        //        ViewBag.Status = true;
        //        int userCollegeID = (int)collegeId;

        //        ViewBag.collegeId = collegeId;
        //        var jntuh_academic_year = db.jntuh_academic_year.ToList();

        //        ViewBag.AcademicYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
        //        int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

        //        //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
        //        ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
        //        int AY0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

        //        ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
        //        ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
        //        ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
        //        ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
        //        ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

        //        int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //        int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //        int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //        int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
        //        int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
        //        int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

        //        List<jntuh_college_intake_existing_datentry2> intake = db.jntuh_college_intake_existing_datentry2.Where(i => i.collegeId == userCollegeID).ToList();

        //        int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
        //        var AICTEApprovalLettr = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();
        //        var jntuh_specialization = db.jntuh_specialization;
        //        var jntuh_department = db.jntuh_department;
        //        var jntuh_degree = db.jntuh_degree;
        //        var jntuh_shift = db.jntuh_shift;

        //        foreach (var item in intake)
        //        {
        //            CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
        //            newIntake.id = item.id;
        //            newIntake.collegeId = item.collegeId;
        //            newIntake.academicYearId = item.academicYearId;
        //            newIntake.shiftId = item.shiftId;
        //            newIntake.isActive = item.isActive;
        //            newIntake.isAffiliated = item.isAffiliated;
        //            newIntake.nbaFrom = item.nbaFrom;
        //            newIntake.nbaTo = item.nbaTo;
        //            newIntake.specializationId = item.specializationId;
        //            newIntake.Specialization = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
        //            newIntake.DepartmentID = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
        //            newIntake.Department = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
        //            newIntake.degreeID = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
        //            newIntake.Degree = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
        //            newIntake.degreeDisplayOrder = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
        //            newIntake.shiftId = item.shiftId;
        //            newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
        //            newIntake.AICTEApprovalLettr = AICTEApprovalLettr;
        //            collegeIntakeExisting.Add(newIntake);
        //        }

        //        collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
        //        foreach (var item in collegeIntakeExisting)
        //        {
        //            if (item.nbaFrom != null)
        //                item.nbaFromDate = Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
        //            if (item.nbaTo != null)
        //                item.nbaToDate = Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());

        //            //FLAG : 1 - Approved, 0 - Admitted
        //            jntuh_college_intake_existing_datentry2 details = db.jntuh_college_intake_existing_datentry2
        //                                                      .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
        //                                                      .Select(e => e)
        //                                                      .FirstOrDefault();
        //            if (details != null)
        //            {
        //                item.ApprovedIntake = details.approvedIntake;
        //                item.letterPath = details.approvalLetter;
        //                item.ProposedIntake = details.proposedIntake;
        //            }

        //            item.approvedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 1);
        //            item.admittedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 0);

        //            item.approvedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 1);
        //            item.admittedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 0);

        //            item.approvedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 1);
        //            item.admittedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 0);

        //            item.approvedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 1);
        //            item.admittedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 0);

        //            item.approvedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 1);
        //            item.admittedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 0);
        //        }
        //        collegeIntakeExisting = collegeIntakeExisting.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

        //    }
        //    else
        //    {
        //        ViewBag.Status = false;
        //    }
        //    return View(collegeIntakeExisting);

        //}

        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //[HttpGet]
        //public ActionResult AddEditCollegeIntake(int? id, int collegeId)
        //{
        //    CollegeIntakeExisting collegeIntakeExisting = new CollegeIntakeExisting();
        //    int userCollegeID = collegeId;
        //    if (id != null && userCollegeID == 0)
        //    {
        //        userCollegeID = db.jntuh_college_intake_existing_datentry2.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
        //    }

        //    ViewBag.IsUpdate = true;
        //    int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
        //    var AICTEApprovalLettr = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();
        //    collegeIntakeExisting.collegeId = userCollegeID;
        //    collegeIntakeExisting.AICTEApprovalLettr = AICTEApprovalLettr;

        //    var jntuh_academic_year = db.jntuh_academic_year.ToList();
        //    ViewBag.AcademicYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
        //    int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

        //    //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
        //    ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
        //    int AY0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

        //    ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
        //    ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
        //    ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
        //    ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
        //    ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

        //    if (id != null)
        //    {
        //        int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //        int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //        int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //        int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
        //        int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
        //        int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

        //        List<jntuh_college_intake_existing_datentry2> intake = db.jntuh_college_intake_existing_datentry2.Where(i => i.collegeId == userCollegeID && i.id == id).ToList();

        //        foreach (var item in intake)
        //        {
        //            collegeIntakeExisting.id = item.id;
        //            collegeIntakeExisting.collegeId = item.collegeId;
        //            collegeIntakeExisting.academicYearId = item.academicYearId;
        //            collegeIntakeExisting.shiftId = item.shiftId;
        //            collegeIntakeExisting.isActive = item.isActive;
        //            collegeIntakeExisting.isAffiliated = item.isAffiliated;
        //            collegeIntakeExisting.nbaFrom = item.nbaFrom;
        //            collegeIntakeExisting.nbaTo = item.nbaTo;
        //            collegeIntakeExisting.specializationId = item.specializationId;
        //            collegeIntakeExisting.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
        //            collegeIntakeExisting.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
        //            collegeIntakeExisting.Department = db.jntuh_department.Where(d => d.id == collegeIntakeExisting.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
        //            collegeIntakeExisting.degreeID = db.jntuh_department.Where(d => d.id == collegeIntakeExisting.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
        //            collegeIntakeExisting.Degree = db.jntuh_degree.Where(d => d.id == collegeIntakeExisting.degreeID).Select(d => d.degree).FirstOrDefault();
        //            collegeIntakeExisting.shiftId = item.shiftId;
        //            collegeIntakeExisting.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
        //        }

        //        if (collegeIntakeExisting.nbaFrom != null)
        //            collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaFrom.ToString());
        //        if (collegeIntakeExisting.nbaTo != null)
        //            collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaTo.ToString());

        //        //FLAG : 1 - Approved, 0 - Admitted
        //        jntuh_college_intake_existing_datentry2 details = db.jntuh_college_intake_existing_datentry2
        //                                                  .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == collegeIntakeExisting.specializationId && e.shiftId == collegeIntakeExisting.shiftId)
        //                                                  .Select(e => e)
        //                                                  .FirstOrDefault();
        //        if (details != null)
        //        {
        //            collegeIntakeExisting.ApprovedIntake = details.approvedIntake;
        //            collegeIntakeExisting.letterPath = details.approvalLetter;
        //            collegeIntakeExisting.ProposedIntake = details.proposedIntake;
        //        }

        //        collegeIntakeExisting.approvedIntake1 = GetIntake(userCollegeID, AY1, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
        //        collegeIntakeExisting.admittedIntake1 = GetIntake(userCollegeID, AY1, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

        //        collegeIntakeExisting.approvedIntake2 = GetIntake(userCollegeID, AY2, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
        //        collegeIntakeExisting.admittedIntake2 = GetIntake(userCollegeID, AY2, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

        //        collegeIntakeExisting.approvedIntake3 = GetIntake(userCollegeID, AY3, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
        //        collegeIntakeExisting.admittedIntake3 = GetIntake(userCollegeID, AY3, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

        //        collegeIntakeExisting.approvedIntake4 = GetIntake(userCollegeID, AY4, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
        //        collegeIntakeExisting.admittedIntake4 = GetIntake(userCollegeID, AY4, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

        //        collegeIntakeExisting.approvedIntake5 = GetIntake(userCollegeID, AY5, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
        //        collegeIntakeExisting.admittedIntake5 = GetIntake(userCollegeID, AY5, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

        //    }
        //    else
        //    {
        //        ViewBag.IsUpdate = false;
        //    }

        //    var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
        //                                                         (collegeDegree, degree) => new
        //                                                         {
        //                                                             collegeDegree.degreeId,
        //                                                             collegeDegree.collegeId,
        //                                                             collegeDegree.isActive,
        //                                                             degree.degree
        //                                                         })
        //                                                     .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
        //                                                     .Select(collegeDegree => new
        //                                                     {
        //                                                         collegeDegree.degreeId,
        //                                                         collegeDegree.degree
        //                                                     }).ToList();
        //    ViewBag.Degree = degrees.OrderBy(d => d.degree);
        //    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
        //    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
        //    ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
        //    ViewBag.Count = degrees.Count();
        //    return PartialView("_AddEditCollegeIntake", collegeIntakeExisting);

        //}

        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //[HttpPost]
        //public ActionResult AddEditCollegeIntake(CollegeIntakeExisting collegeIntakeExisting, string cmd)
        //{
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    int userCollegeID = collegeIntakeExisting.collegeId;

        //    if (collegeIntakeExisting.nbaFromDate != null)
        //        collegeIntakeExisting.nbaFrom = Convert.ToDateTime(Utilities.DDMMYY2MMDDYY(collegeIntakeExisting.nbaFromDate));
        //    if (collegeIntakeExisting.nbaToDate != null)
        //        collegeIntakeExisting.nbaTo = Convert.ToDateTime(Utilities.DDMMYY2MMDDYY(collegeIntakeExisting.nbaToDate));

        //    if (ModelState.IsValid)
        //    {
        //        collegeIntakeExisting.collegeId = userCollegeID;
        //        var jntuh_academic_year = db.jntuh_academic_year.ToList();
        //        int presentAY = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

        //        for (int i = -1; i < 5; i++)
        //        {
        //            int? approved = 0;
        //            int admitted = 0;
        //            int academicYear = 0;

        //            int? proposed = null;
        //            string letterPath = null;

        //            if (i == -1)
        //            {
        //                approved = collegeIntakeExisting.ApprovedIntake != null ? collegeIntakeExisting.ApprovedIntake : 0;
        //                admitted = 0;
        //                academicYear = presentAY + 1;
        //                //academicYear = presentAY + 1;

        //                letterPath = collegeIntakeExisting.letterPath;
        //                proposed = collegeIntakeExisting.ProposedIntake;
        //            }
        //            if (i == 0)
        //            {
        //                approved = collegeIntakeExisting.approvedIntake1;
        //                admitted = collegeIntakeExisting.admittedIntake1;
        //                academicYear = presentAY - i;
        //                //academicYear = presentAY + 1;
        //            }
        //            if (i == 1)
        //            {
        //                approved = collegeIntakeExisting.approvedIntake2;
        //                admitted = collegeIntakeExisting.admittedIntake2;
        //                academicYear = presentAY - i;
        //                //academicYear = presentAY;
        //            }
        //            if (i == 2)
        //            {
        //                approved = collegeIntakeExisting.approvedIntake3;
        //                admitted = collegeIntakeExisting.admittedIntake3;
        //                academicYear = presentAY - i;
        //                //academicYear = presentAY - 1;
        //            }
        //            if (i == 3)
        //            {
        //                approved = collegeIntakeExisting.approvedIntake4;
        //                admitted = collegeIntakeExisting.admittedIntake4;
        //                academicYear = presentAY - i;
        //                //academicYear = presentAY - 2;
        //            }
        //            if (i == 4)
        //            {
        //                approved = collegeIntakeExisting.approvedIntake5;
        //                admitted = collegeIntakeExisting.admittedIntake5;
        //                academicYear = presentAY - i;
        //                //academicYear = presentAY - 3;
        //            }

        //            jntuh_college_intake_existing_datentry2 jntuh_college_intake_existing = new jntuh_college_intake_existing_datentry2();
        //            jntuh_college_intake_existing.academicYearId = db.jntuh_academic_year.Where(a => a.actualYear == academicYear).Select(a => a.id).FirstOrDefault();

        //            var existingId = db.jntuh_college_intake_existing_datentry2.Where(p => p.specializationId == collegeIntakeExisting.specializationId
        //                                                                        && p.shiftId == collegeIntakeExisting.shiftId
        //                                                                        && p.collegeId == collegeIntakeExisting.collegeId
        //                                                                        && p.academicYearId == jntuh_college_intake_existing.academicYearId).Select(p => p.id).FirstOrDefault();
        //            int createdByu = Convert.ToInt32(db.jntuh_college_intake_existing_datentry2.Where(a => a.collegeId == userCollegeID && a.id == existingId).Select(a => a.createdBy).FirstOrDefault());
        //            DateTime createdonu = Convert.ToDateTime(db.jntuh_college_intake_existing_datentry2.Where(a => a.collegeId == userCollegeID && a.id == existingId).Select(a => a.createdOn).FirstOrDefault());

        //            if ((approved > 0 && i != -1) || (i != -1 && admitted > 0 && existingId == 0) || (existingId > 0) || (i == -1))
        //            {
        //                jntuh_college_intake_existing.id = collegeIntakeExisting.id;
        //                jntuh_college_intake_existing.collegeId = collegeIntakeExisting.collegeId;
        //                jntuh_college_intake_existing.academicYearId = jntuh_academic_year.Where(a => a.actualYear == academicYear).Select(a => a.id).FirstOrDefault();
        //                jntuh_college_intake_existing.specializationId = collegeIntakeExisting.specializationId;
        //                jntuh_college_intake_existing.shiftId = collegeIntakeExisting.shiftId;
        //                jntuh_college_intake_existing.approvedIntake = (int)approved;
        //                jntuh_college_intake_existing.admittedIntake = admitted;
        //                jntuh_college_intake_existing.approvalLetter = letterPath; //new
        //                jntuh_college_intake_existing.proposedIntake = proposed;  //new
        //                jntuh_college_intake_existing.nbaFrom = collegeIntakeExisting.nbaFrom;
        //                jntuh_college_intake_existing.nbaTo = collegeIntakeExisting.nbaTo;
        //                jntuh_college_intake_existing.isActive = true;
        //                jntuh_college_intake_existing.isAffiliated = collegeIntakeExisting.isAffiliated;

        //                if (existingId == 0)
        //                {
        //                    jntuh_college_intake_existing.createdBy = userID;
        //                    jntuh_college_intake_existing.createdOn = DateTime.Now;
        //                    db.jntuh_college_intake_existing_datentry2.Add(jntuh_college_intake_existing);
        //                }
        //                else
        //                {
        //                    jntuh_college_intake_existing.id = existingId;
        //                    jntuh_college_intake_existing.createdBy = createdByu;
        //                    jntuh_college_intake_existing.createdOn = createdonu;
        //                    jntuh_college_intake_existing.updatedBy = userID;
        //                    jntuh_college_intake_existing.updatedOn = DateTime.Now;
        //                    db.Entry(jntuh_college_intake_existing).State = EntityState.Modified;
        //                }
        //                db.SaveChanges();
        //            }
        //        }

        //        if (cmd == "Add")
        //        {
        //            TempData["Success"] = "Added successfully.";
        //        }
        //        else
        //        {
        //            TempData["Success"] = "Updated successfully.";
        //        }

        //        return RedirectToAction("CollegeIntake", new { collegeId = userCollegeID });
        //    }
        //    else
        //    {
        //        return RedirectToAction("CollegeIntake", new { collegeId = userCollegeID });
        //    }

        //}

        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //[HttpPost]
        //public ActionResult SaveAffiliation(List<CollegeIntakeExisting> collegeIntakeExisting)
        //{
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    int userCollegeID = collegeIntakeExisting[0].collegeId;

        //    foreach (var item in collegeIntakeExisting)
        //    {
        //        //if (item.isAffiliated)
        //        //{
        //        var jntuh_college_intake_existing = db.jntuh_college_intake_existing_datentry2.Find(item.id);
        //        jntuh_college_intake_existing.isAffiliated = item.isAffiliated;
        //        jntuh_college_intake_existing.updatedBy = userID;
        //        jntuh_college_intake_existing.updatedOn = DateTime.Now;
        //        db.Entry(jntuh_college_intake_existing).State = EntityState.Modified;
        //        //}
        //    }
        //    db.SaveChanges();

        //    return RedirectToAction("CollegeIntake", new { collegeId = userCollegeID });
        //}

        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //public ActionResult DeleteCollegeIntake(int id)
        //{
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
        //    if (userCollegeID == 0)
        //    {
        //        userCollegeID = db.jntuh_college_intake_existing_datentry2.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
        //    }
        //    int specid = db.jntuh_college_intake_existing_datentry2.Where(p => p.id == id).Select(p => p.specializationId).FirstOrDefault();
        //    int shiftid = db.jntuh_college_intake_existing_datentry2.Where(p => p.id == id).Select(p => p.shiftId).FirstOrDefault();
        //    List<jntuh_college_intake_existing_datentry2> jntuh_college_intake_existing = db.jntuh_college_intake_existing_datentry2.Where(p => p.specializationId == specid && p.shiftId == shiftid && p.collegeId == userCollegeID).ToList();
        //    foreach (var item in jntuh_college_intake_existing)
        //    {
        //        db.jntuh_college_intake_existing_datentry2.Remove(item);
        //        db.SaveChanges();
        //        TempData["Success"] = "College Intake Deleted successfully";
        //    }

        //    return RedirectToAction("CollegeIntake", new { collegeId = userCollegeID });
        //}

        //private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        //{
        //    int intake = 0;

        //    //approved
        //    if (flag == 1)
        //    {
        //        intake = db.jntuh_college_intake_existing_datentry2.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

        //    }
        //    else //admitted
        //    {
        //        intake = db.jntuh_college_intake_existing_datentry2.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
        //    }

        //    return intake;
        //}

        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //[HttpGet]
        //public ActionResult CollegeFacultyWithIntake(int? collegeId, string type)
        //{
        //    var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
        //    {
        //        collegeId = c.id,
        //        collegeName = c.collegeCode + "-" + c.collegeName
        //    }).OrderBy(c => c.collegeName).ToList();

        //    //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

        //    ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
        //    List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
        //    List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
        //    if (collegeId != null)
        //    {
        //        int[] collegeIDs = null;
        //        int facultystudentRatio = 0;
        //        decimal facultyRatio = 0m;
        //        if (collegeId != 0)
        //        {
        //            collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
        //        }
        //        else
        //        {
        //            collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
        //        }
        //        var jntuh_academic_year = db.jntuh_academic_year.ToList();
        //        var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
        //        var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
        //        var jntuh_degree = db.jntuh_degree.ToList();

        //        int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //        int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
        //        int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //        int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //        int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

        //        List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId)).ToList();
        //        foreach (var item in intake)
        //        {
        //            CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
        //            newIntake.id = item.id;
        //            newIntake.collegeId = item.collegeId;
        //            newIntake.academicYearId = item.academicYearId;
        //            newIntake.shiftId = item.shiftId;
        //            newIntake.isActive = item.isActive;
        //            newIntake.nbaFrom = item.nbaFrom;
        //            newIntake.nbaTo = item.nbaTo;
        //            newIntake.specializationId = item.specializationId;
        //            newIntake.Specialization = item.jntuh_specialization.specializationName;
        //            newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
        //            newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
        //            newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
        //            newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
        //            newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
        //            newIntake.shiftId = item.shiftId;
        //            newIntake.Shift = item.jntuh_shift.shiftName;
        //            collegeIntakeExisting.Add(newIntake);
        //        }
        //        collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

        //        //college Reg nos
        //        var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
        //        string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

        //        //education categoryIds UG,PG,PHD...........
        //        var jntuh_education_category = db.jntuh_education_category.ToList();

        //        //Reg nos related online facultyIds
        //        var jntuh_registered_faculty = db.jntuh_registered_faculty
        //                                         .Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId))
        //                                         .Select(rf => new
        //                                         {
        //                                             RegistrationNumber = rf.RegistrationNumber,
        //                                             Department = rf.jntuh_department.departmentName,
        //                                             HighestDegreeID = rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max()
        //                                         }).Where(e => e.HighestDegreeID != 0)
        //                                         .ToList()
        //                                         .Select(rf => new
        //                                         {
        //                                             RegistrationNumber = rf.RegistrationNumber,
        //                                             Department = rf.Department,
        //                                             HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
        //                                             Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
        //                                             SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault()
        //                                         }).Where(e => e.Department != null)
        //                                         .ToList();


        //        foreach (var item in collegeIntakeExisting)
        //        {
        //            CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
        //            int phdFaculty = 0;
        //            int pgFaculty = 0;
        //            int ugFaculty = 0;


        //            intakedetails.collegeId = item.collegeId;
        //            intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //            intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
        //            intakedetails.Degree = item.Degree;
        //            intakedetails.Department = item.Department;
        //            intakedetails.Specialization = item.Specialization;
        //            intakedetails.specializationId = item.specializationId;
        //            intakedetails.DepartmentID = item.DepartmentID;
        //            intakedetails.shiftId = item.shiftId;

        //            intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
        //            intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
        //            intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
        //            intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
        //            facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());
        //            if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
        //            }
        //            else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake1);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);

        //            }
        //            else if (item.Degree == "MCA")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
        //            }
        //            else //MAM MTM Pharm.D Pharm.D PB
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
        //            }
        //            intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //            intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

        //            string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
        //            if (strdegreetype == "UG")
        //            {
        //                if (item.Degree == "B.Pharmacy")
        //                {
        //                    ugFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
        //                }
        //                else
        //                {
        //                    ugFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.Recruitedfor == "UG").Count();
        //                }
        //            }
        //            if (strdegreetype == "PG")
        //            {
        //                if (item.Degree == "M.Pharmacy")
        //                {
        //                    pgFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
        //                }
        //                else
        //                {
        //                    pgFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
        //                }
        //            }
        //            if (item.Degree == "B.Pharmacy" || item.Degree == "M.Pharmacy")
        //            {
        //                phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy").Count();

        //            }
        //            else
        //            {
        //                phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Count();
        //            }


        //            intakedetails.phdFaculty = phdFaculty;
        //            intakedetails.pgFaculty = pgFaculty;
        //            intakedetails.ugFaculty = ugFaculty;
        //            intakedetails.totalFaculty = (ugFaculty + pgFaculty);
        //            //=============//


        //            intakedetailsList.Add(intakedetails);
        //        }
        //        intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

        //        string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others" };
        //        int btechdegreecount = intakedetailsList.Where(d => d.Degree == "B.Tech").Count();
        //        if (btechdegreecount != 0)
        //        {
        //            foreach (var department in strOtherDepartments)
        //            {
        //                int ugFaculty = jntuh_registered_faculty.Where(f => f.Department == department && f.Recruitedfor == "UG").Count();
        //                int pgFaculty = jntuh_registered_faculty.Where(f => f.Department == department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Count();
        //                int phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Count();
        //                intakedetailsList.Add(new CollegeFacultyWithIntakeReport { collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty });

        //            }
        //        }

        //        if (type == "Excel")
        //        {
        //            string strcollegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //            Response.ClearContent();
        //            Response.Buffer = true;
        //            Response.AddHeader("content-disposition", "attachment; filename=" + strcollegeCode + "-Faculty.xls");
        //            Response.ContentType = "application/vnd.ms-excel";
        //            return PartialView("_CollegeFacultyWithIntake", intakedetailsList);
        //        }
        //    }
        //    return View(intakedetailsList);
        //}

        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //[HttpGet]
        //public ActionResult CollegeFacultyWithIntakeNew(int iStartRow, int iPageSize, string type)
        //{
        //    var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
        //    {
        //        collegeId = c.id,
        //        collegeName = c.collegeCode + "-" + c.collegeName
        //    }).OrderBy(c => c.collegeName).ToList();

        //    //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

        //    ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();

        //    List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();

        //    //int[] collegeIDs = db.jntuh_college.Where(c => c.isActive == true && (cid == 0 || c.id == cid)).Select(c => c.id).Take(count).ToArray();

        //    //DEO Submitted colleges Ids
        //    int[] SubmittedCollegesId = db.jntuh_college_edit_status.Where(editStatus => editStatus.IsCollegeEditable == false)
        //                                                        .Select(editStatus => editStatus.collegeId).ToArray();

        //    int[] collegeIDs = db.jntuh_college.Where(c => c.isActive == true && SubmittedCollegesId.Contains(c.id)).OrderBy(c => c.collegeName).Select(c => c.id).ToArray();
        //    collegeIDs = Queryable.Skip(collegeIDs.AsQueryable(), iStartRow).Take(iPageSize).ToArray();

        //    foreach (var collegeId in collegeIDs)
        //    {
        //        List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

        //        var jntuh_college_faculty_deficiency = db.jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
        //        var jntuh_specialization = db.jntuh_specialization.ToList();

        //        int facultystudentRatio = 0;
        //        decimal facultyRatio = 0m;

        //        var jntuh_academic_year = db.jntuh_academic_year.ToList();
        //        var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
        //        var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
        //        var jntuh_degree = db.jntuh_degree.ToList();

        //        int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //        int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
        //        int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //        int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //        int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

        //        List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId).ToList();
        //        foreach (var item in intake)
        //        {
        //            CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
        //            newIntake.id = item.id;
        //            newIntake.collegeId = item.collegeId;
        //            newIntake.academicYearId = item.academicYearId;
        //            newIntake.shiftId = item.shiftId;
        //            newIntake.isActive = item.isActive;
        //            newIntake.nbaFrom = item.nbaFrom;
        //            newIntake.nbaTo = item.nbaTo;
        //            newIntake.specializationId = item.specializationId;
        //            newIntake.Specialization = item.jntuh_specialization.specializationName;
        //            newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
        //            newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
        //            newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
        //            newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
        //            newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
        //            newIntake.shiftId = item.shiftId;
        //            newIntake.Shift = item.jntuh_shift.shiftName;
        //            collegeIntakeExisting.Add(newIntake);
        //        }
        //        collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

        //        //college Reg nos
        //        var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
        //        string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

        //        //education categoryIds UG,PG,PHD...........

        //        int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

        //        var jntuh_education_category = db.jntuh_education_category.ToList();

        //        var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();
        //        //Reg nos related online facultyIds
        //        var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.DepartmentId != null && (rf.isApproved == null || rf.isApproved == true) && (rf.PANNumber != null || rf.AadhaarNumber != null))
        //                                         .Select(rf => new
        //                                         {
        //                                             RegistrationNumber = rf.RegistrationNumber,
        //                                             Department = rf.jntuh_department.departmentName,
        //                                             HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
        //                                             IsApproved = rf.isApproved,
        //                                             PanNumber = rf.PANNumber,
        //                                             AadhaarNumber = rf.AadhaarNumber,
        //                                             CollegeId = rf.collegeId
        //                                         }).ToList();
        //        jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID != 0).ToList();
        //        var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
        //                                         {
        //                                             RegistrationNumber = rf.RegistrationNumber,
        //                                             Department = rf.Department,
        //                                             HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
        //                                             Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
        //                                             SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
        //                                             PanNumber = rf.PanNumber,
        //                                             AadhaarNumber = rf.AadhaarNumber,
        //                                             CollegeId = rf.CollegeId
        //                                         }).Where(e => e.Department != null)
        //                                         .ToList();

        //        //collegeIntakeExisting = collegeIntakeExisting.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

        //        foreach (var item in collegeIntakeExisting)
        //        {
        //            CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
        //            int phdFaculty = 0;
        //            int pgFaculty = 0;
        //            int ugFaculty = 0;

        //            intakedetails.collegeId = item.collegeId;
        //            intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //            intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
        //            intakedetails.Degree = item.Degree;
        //            intakedetails.Department = item.Department;
        //            intakedetails.Specialization = item.Specialization;
        //            intakedetails.specializationId = item.specializationId;
        //            intakedetails.DepartmentID = item.DepartmentID;
        //            intakedetails.shiftId = item.shiftId;

        //            intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
        //            intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
        //            intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
        //            intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
        //            facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

        //            if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
        //            }
        //            else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);

        //            }
        //            else if (item.Degree == "MCA")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
        //            }
        //            else //MAM MTM Pharm.D Pharm.D PB
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
        //            }

        //            intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //            intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

        //            //====================================
        //            // intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(s => s.SpecializationId == item.specializationId).Count();

        //            string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
        //            if (strdegreetype == "UG")
        //            {
        //                if (item.Degree == "B.Pharmacy")
        //                {
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
        //                }
        //                else
        //                {
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.Recruitedfor == "UG").Count();
        //                }
        //            }

        //            if (strdegreetype == "PG")
        //            {
        //                if (item.Degree == "M.Pharmacy")
        //                {
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
        //                }
        //                else
        //                {
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
        //                }
        //            }
        //            intakedetails.id = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

        //            if (intakedetails.id > 0)
        //            {
        //                int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                if (swf != null)
        //                {
        //                    intakedetails.specializationWiseFaculty = (int)swf;
        //                }
        //                intakedetails.deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                intakedetails.shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
        //            }

        //            //============================================

        //            int noPanOrAadhaarcount = 0;

        //            if (item.Degree == "B.Pharmacy" || item.Degree == "M.Pharmacy")
        //            {
        //                ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == "Pharmacy").Count();
        //                pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy").Count();
        //                phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy").Count();
        //                noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
        //                intakedetails.Department = "Pharmacy";
        //            }
        //            else
        //            {
        //                ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == item.Department).Count();
        //                pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department).Count();
        //                phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Count();
        //                noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
        //            }

        //            intakedetails.phdFaculty = phdFaculty;
        //            intakedetails.pgFaculty = pgFaculty;
        //            intakedetails.ugFaculty = ugFaculty;
        //            intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
        //            intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
        //            //=============//

        //            intakedetailsList.Add(intakedetails);
        //        }

        //        intakedetailsList = intakedetailsList.OrderBy(ei => ei.collegeId).ThenBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

        //        string[] strOtherDepartments = { "z1English", "z2Mathematics", "z3Physics", "z4Chemistry", "z5Others" };
        //        int btechdegreecount = intakedetailsList.Where(d => d.Degree == "B.Tech").Count();
        //        if (btechdegreecount != 0)
        //        {
        //            foreach (var department1 in strOtherDepartments)
        //            {
        //                var department = department1.Replace("z1", "").Replace("z2", "").Replace("z3", "").Replace("z4", "").Replace("z5", "").ToString();
        //                int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
        //                int ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == department).Count();
        //                int pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department).Count();
        //                int phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Count();

        //                int facultydeficiencyId = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == speId && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                if (facultydeficiencyId == 0)
        //                {
        //                    intakedetailsList.Add(new CollegeFacultyWithIntakeReport { collegeId = (int)collegeId, Degree = "B.Tech", Department = department1, Specialization = department1, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1 });
        //                }
        //                else
        //                {
        //                    int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                    bool deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Deficiency).FirstOrDefault();
        //                    int shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Shortage).FirstOrDefault();
        //                    intakedetailsList.Add(new CollegeFacultyWithIntakeReport { id = facultydeficiencyId, collegeId = (int)collegeId, Degree = "B.Tech", Department = department1, Specialization = department1, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1, specializationWiseFaculty = (int)swf, deficiency = deficiency, shortage = shortage });
        //                }
        //            }
        //        }
        //    }

        //    if (type == "Excel")
        //    {
        //        //string strcollegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //        Response.ClearContent();
        //        Response.Buffer = true;
        //        Response.AddHeader("content-disposition", "attachment; filename=All-Colleges-Faculty-" + (iStartRow + 1) + "-" + (iStartRow + iPageSize) + ".xls");
        //        Response.ContentType = "application/vnd.ms-excel";
        //        return PartialView("_CollegeFacultyWithIntakeNew", intakedetailsList.Where(c => c.shiftId == 1).ToList());
        //    }
        //    return View(intakedetailsList.Where(c => c.shiftId == 1).ToList());
        //}

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        [HttpPost]
        public ActionResult CollegeFacultyWithIntakeNew(List<CollegeFacultyWithIntakeReport> facultyList)
        {
            int collegeId = facultyList.Select(c => c.collegeId).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var faculty = facultyList.Where(ld => ld.deficiency != null).ToList();
            if (faculty.Count() > 0)
            {
                if (ModelState.IsValid)
                {
                    foreach (var item in faculty)
                    {
                        jntuh_college_faculty_deficiency facultyDeficiency = new jntuh_college_faculty_deficiency();
                        facultyDeficiency.CollegeId = item.collegeId;
                        facultyDeficiency.SpecializationId = item.specializationId;
                        facultyDeficiency.ShiftId = item.shiftId;
                        facultyDeficiency.SpecializationWiseFaculty = item.specializationWiseFaculty;
                        facultyDeficiency.Deficiency = (bool)item.deficiency;
                        facultyDeficiency.Shortage = item.shortage;
                        facultyDeficiency.IsActive = true;
                        facultyDeficiency.SpecializationId = item.specializationId;
                        if (item.id == 0)
                        {
                            facultyDeficiency.CreatedBy = userID;
                            facultyDeficiency.CreatedOn = DateTime.Now;
                            db.jntuh_college_faculty_deficiency.Add(facultyDeficiency);
                        }
                        else
                        {
                            jntuh_college_faculty_deficiency facultyDeficiencyupdate = db.jntuh_college_faculty_deficiency.Find(item.id);
                            facultyDeficiencyupdate.SpecializationWiseFaculty = item.specializationWiseFaculty;
                            facultyDeficiencyupdate.Deficiency = (bool)item.deficiency;
                            facultyDeficiencyupdate.Shortage = item.shortage;
                            facultyDeficiencyupdate.UpdatedBy = userID;
                            facultyDeficiencyupdate.UpdatedOn = DateTime.Now;
                            db.Entry(facultyDeficiencyupdate).State = EntityState.Modified;
                        }
                    }
                    db.SaveChanges();
                }
                TempData["Success"] = "Data Saved";
            }
            return RedirectToAction("CollegeFacultyWithIntakeNew", new { collegeId = collegeId });
        }

        public class CollegeFacultyWithIntakeReport
        {
            public int id { get; set; }
            public int collegeId { get; set; }
            public int academicYearId { get; set; }
            public string collegeCode { get; set; }
            public string collegeName { get; set; }
            public string Degree { get; set; }
            public string Department { get; set; }
            public string Specialization { get; set; }
            public int shiftId { get; set; }
            public string Shift { get; set; }
            public int specializationId { get; set; }
            public int DepartmentID { get; set; }
            public int? degreeDisplayOrder { get; set; }

            public int approvedIntake1 { get; set; }
            public int approvedIntake2 { get; set; }
            public int approvedIntake3 { get; set; }
            public int approvedIntake4 { get; set; }
            public int totalIntake { get; set; }
            public decimal requiredFaculty { get; set; }
            public int phdFaculty { get; set; }
            public int pgFaculty { get; set; }
            public int ugFaculty { get; set; }
            public int totalFaculty { get; set; }
            public int specializationWiseFaculty { get; set; }
            public int facultyWithoutPANAndAadhaar { get; set; }

            public bool isActive { get; set; }
            public DateTime? nbaFrom { get; set; }
            public DateTime? nbaTo { get; set; }

            public bool? deficiency { get; set; }
            public int shortage { get; set; }

            //=====18-06-2015=====//
            public int FalseNameFaculty { get; set; }
            public int FalsePhotoFaculty { get; set; }
            public int FalsePANNumberFaculty { get; set; }
            public int FalseAadhaarNumberFaculty { get; set; }
            public int CertificatesIncompleteFaculty { get; set; }
            public int AbsentFaculty { get; set; }
            public int AvailableFaculty { get; set; }
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        [HttpGet]
        public ActionResult CollegeRegisteredFaculty(int? collegeId)
        {
            List<jntuh_registered_faculty> faculty = new List<jntuh_registered_faculty>();
            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();
            ViewBag.Colleges = colleges;

            if (collegeId != null)
            {
                ViewBag.collegeId = collegeId;
                string[] strRegnos = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).Select(cf => cf.RegistrationNumber).ToArray();
                faculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();
            }
            return View(faculty);
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        [HttpGet]
        public ActionResult AddEditCollegeFaculty(string regno, int collegeId)
        {
            CollegeFaculty faculty = new CollegeFaculty();
            string strRegistrationNumber = string.Empty;
            int userCollegeID = collegeId;
            faculty.collegeId = userCollegeID;
            if (regno != null)
            {
                strRegistrationNumber = UAAAS.Models.Utilities.DecryptString(regno, WebConfigurationManager.AppSettings["CryptoKey"]);
            }
            if (!string.IsNullOrEmpty(strRegistrationNumber))
            {
                int collegeFacultyId = db.jntuh_college_faculty_registered.Where(cf => cf.RegistrationNumber == strRegistrationNumber).Select(cf => cf.id).FirstOrDefault();
                var existingfaculty = db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber == strRegistrationNumber && (rf.collegeId == null || rf.collegeId == userCollegeID)).Select(rf => rf).FirstOrDefault();
                faculty.id = collegeFacultyId;
                faculty.facultyFirstName = existingfaculty.FirstName;
                faculty.facultyLastName = existingfaculty.LastName;
                faculty.facultySurname = existingfaculty.MiddleName;
                if (existingfaculty.DesignationId != null)
                {
                    faculty.facultyDesignationId = (int)existingfaculty.DesignationId;
                    faculty.facultyOtherDesignation = existingfaculty.OtherDesignation;
                    faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                }
                else
                {
                    faculty.designation = String.Empty;
                }
                if (existingfaculty.DepartmentId != null)
                {
                    faculty.facultyDepartmentId = (int)existingfaculty.DepartmentId;
                    faculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                }
                else
                {
                    faculty.department = String.Empty;
                }
            }

            ViewBag.PGSpecializations = db.jntuh_college_intake_existing
                                         .Where(e => e.collegeId == userCollegeID && e.jntuh_specialization.jntuh_department.jntuh_degree.id != 4 && e.jntuh_specialization.jntuh_department.jntuh_degree.id != 5)
                                         .Select(e => new { id = e.jntuh_specialization.id, spec = e.jntuh_specialization.specializationName })
                                         .GroupBy(e => new { e.id, e.spec })
                                         .OrderBy(e => e.Key.spec)
                                         .Select(e => new { id = e.Key.id, spec = e.Key.spec }).ToList();

            return PartialView("_AddEditCollegeFaculty", faculty);
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        [HttpPost]
        public ActionResult AddEditCollegeFaculty(CollegeFaculty faculty)
        {
            TempData["Error"] = null;

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = faculty.collegeId;

            ViewBag.PGSpecializations = db.jntuh_college_intake_existing
                                         .Where(e => e.collegeId == userCollegeID && e.jntuh_specialization.jntuh_department.jntuh_degree.id != 4 && e.jntuh_specialization.jntuh_department.jntuh_degree.id != 5)
                                         .Select(e => new { id = e.jntuh_specialization.id, spec = e.jntuh_specialization.specializationName })
                                         .GroupBy(e => new { e.id, e.spec })
                                         .OrderBy(e => e.Key.spec)
                                         .Select(e => new { id = e.Key.id, spec = e.Key.spec }).ToList();

            jntuh_registered_faculty isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();

            if (isRegisteredFaculty == null)
            {
                TempData["Error"] = "Invalid Faculty Registration Number.";
                return RedirectToAction("CollegeRegisteredFaculty", "FacultyVerification", new { collegeId = userCollegeID });
            }

            if (isRegisteredFaculty.WorkingStatus == true)
            {
                if (userCollegeID != isRegisteredFaculty.collegeId && isRegisteredFaculty.collegeId != null)
                {
                    TempData["Error"] = "Faculty is already working in other JNTUH affiliated college.";
                    return RedirectToAction("CollegeRegisteredFaculty", "FacultyVerification", new { collegeId = userCollegeID });
                }
            }

            jntuh_college_faculty_registered isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();

            if (isExistingFaculty != null)
            {
                if (userCollegeID != isExistingFaculty.collegeId && isExistingFaculty.collegeId != null)
                {
                    TempData["Error"] = "Faculty is already working in other JNTUH affiliated college.";
                }
                else if (userCollegeID == isExistingFaculty.collegeId)
                {
                    TempData["Error"] = "Faculty is already working in your college";
                }

                return RedirectToAction("CollegeRegisteredFaculty", "FacultyVerification", new { collegeId = userCollegeID });
            }

            if (TempData["Error"] == null)
            {
                //jntuh_college_faculty oldFac = db.jntuh_college_faculty.Find(faculty.id);

                jntuh_college_faculty_registered eFaculty = new jntuh_college_faculty_registered();
                eFaculty.collegeId = userCollegeID;
                eFaculty.RegistrationNumber = faculty.FacultyRegistrationNumber;
                eFaculty.IdentifiedFor = faculty.facultyRecruitedFor;
                eFaculty.SpecializationId = faculty.SpecializationId;
                eFaculty.createdBy = userID;
                eFaculty.createdOn = DateTime.Now;

                //if (oldFac != null )
                //{
                //    eFaculty.existingFacultyId = faculty.id;
                //}
                //else
                //{
                //    eFaculty.existingFacultyId = null;
                //}

                db.jntuh_college_faculty_registered.Add(eFaculty);
                db.SaveChanges();
                TempData["Success"] = "Faculty added Successfully.";
            }

            return RedirectToAction("CollegeRegisteredFaculty", "FacultyVerification", new { collegeId = userCollegeID });
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult DeleteCollegeFaculty(string regno, int collegeId)
        {
            int userCollegeID = collegeId;
            string strRegistrationNumber = string.Empty;
            if (regno != null)
            {
                strRegistrationNumber = UAAAS.Models.Utilities.DecryptString(regno, WebConfigurationManager.AppSettings["CryptoKey"]);
            }
            var faculty = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId && cf.RegistrationNumber == strRegistrationNumber).Select(cf => cf).FirstOrDefault();
            if (faculty != null)
            {
                db.jntuh_college_faculty_registered.Remove(faculty);
                db.SaveChanges();
                TempData["Success"] = "deleted successfully";
            }
            return RedirectToAction("CollegeRegisteredFaculty", "FacultyVerification", new { collegeId = userCollegeID });

        }

        public ActionResult FileUpload(HttpPostedFileBase fileUploader, string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(collegeId);
            }
            //To Save File in jntuh_college_enclosures
            string fileName = string.Empty;
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
            var college_enclosures = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e).FirstOrDefault();
            jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
            jntuh_college_enclosures.collegeID = userCollegeID;
            jntuh_college_enclosures.enclosureId = enclosureId;
            jntuh_college_enclosures.isActive = true;

            if (fileUploader != null)
            {
                string ext = Path.GetExtension(fileUploader.FileName);
                fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "_APL_" + enclosureId + ext;
                fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures"), fileName));
                jntuh_college_enclosures.path = fileName;
            }
            else if (!string.IsNullOrEmpty(college_enclosures.path))
            {
                fileName = college_enclosures.path;
                jntuh_college_enclosures.path = fileName;
            }

            if (college_enclosures == null)
            {
                jntuh_college_enclosures.createdBy = userID;
                jntuh_college_enclosures.createdOn = DateTime.Now;
                db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                db.SaveChanges();
            }
            else
            {
                college_enclosures.path = fileName;
                college_enclosures.updatedBy = userID;
                college_enclosures.updatedOn = DateTime.Now;
                db.Entry(college_enclosures).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("CollegeIntake", new { collegeId = userCollegeID });

        }

        public ActionResult Deactivations()
        {
            List<jntuh_registered_faculty> filteredFaculty = db.jntuh_registered_faculty.Where(f => f.isApproved == false).ToList();

            DeactivationReport report = new DeactivationReport();
            report.Total = db.jntuh_college_faculty_registered.Count();
            report.Deactivations = filteredFaculty.Count();
            report.Name = filteredFaculty.Where(f => f.DeactivationReason.Contains("False Name")).Count();
            report.Photo = filteredFaculty.Where(f => f.DeactivationReason.Contains("False Photo")).Count();
            report.PAN = filteredFaculty.Where(f => f.DeactivationReason.Contains("False PAN Number")).Count();
            report.Aadhaar = filteredFaculty.Where(f => f.DeactivationReason.Contains("False Aadhaar Number")).Count();
            report.Certificates = filteredFaculty.Where(f => f.DeactivationReason.Contains("Certificate(s) Incomplete")).Count();
            report.Absent = filteredFaculty.Where(f => f.DeactivationReason.Contains("Absent")).Count();

            return View(report);
        }

        public class DeactivationReport
        {
            public int Total { get; set; }
            public int Deactivations { get; set; }
            public int Absent { get; set; }
            public int Name { get; set; }
            public int Photo { get; set; }
            public int PAN { get; set; }
            public int Aadhaar { get; set; }
            public int Certificates { get; set; }
            public int Multiple { get; set; }
        }

        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        [HttpGet]
        public ActionResult PharmacyFacultyWithIntake(int? collegeId, string type)
        {
            int[] PharmacydegreeIds = db.jntuh_degree.Where(d => d.degree == "B.Pharmacy" || d.degree == "M.Pharmacy").Select(d => d.id).ToArray();

            var colleges = (from c in db.jntuh_college
                            join cd in db.jntuh_college_degree on c.id equals cd.collegeId
                            join ce in db.jntuh_college_edit_status on c.id equals ce.collegeId
                            where (c.isActive == true && cd.isActive == true && ce.IsCollegeEditable == false && PharmacydegreeIds.Contains(cd.degreeId))
                            select new
                            {
                                collegeId = c.id,
                                collegeName = c.collegeCode + "-" + c.collegeName
                            }).OrderBy(c => c.collegeName).Distinct().ToList();


            //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            if (collegeId != null)
            {
                var jntuh_specialization = db.jntuh_specialization.ToList();

                int[] PharmacySpecializatioIDs = jntuh_specialization.Where(s => PharmacydegreeIds.Contains(s.jntuh_department.jntuh_degree.id)).Select(s => s.id).ToArray();

                int[] collegeIDs = null;
                int facultystudentRatio = 0;
                decimal facultyRatio = 0m;
                if (collegeId != 0)
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
                }
                else
                {
                    collegeIDs = colleges.Select(c => c.collegeId).Take(3).ToArray();
                }
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
                var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
                var jntuh_degree = db.jntuh_degree.ToList();

                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => PharmacySpecializatioIDs.Contains(i.specializationId) && collegeIDs.Contains(i.collegeId)).ToList();
                foreach (var item in intake)
                {
                    CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                    newIntake.id = item.id;
                    newIntake.collegeId = item.collegeId;
                    newIntake.academicYearId = item.academicYearId;
                    newIntake.shiftId = item.shiftId;
                    newIntake.isActive = item.isActive;
                    newIntake.nbaFrom = item.nbaFrom;
                    newIntake.nbaTo = item.nbaTo;
                    newIntake.specializationId = item.specializationId;
                    newIntake.Specialization = item.jntuh_specialization.specializationName;
                    newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
                    newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                    newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                    newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                    newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = item.jntuh_shift.shiftName;
                    collegeIntakeExisting.Add(newIntake);
                }
                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

                //college Reg nos
                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => (cf.collegeId == collegeId)).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //education categoryIds UG,PG,PHD...........



                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber)
                                                                    && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();

                //if (collegeId==0)
                //{
                //jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => (cf.collegeId == collegeId || collegeIDs.Contains(cf.collegeId))).ToList();
                //strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                //    registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber)
                //                                                    && (rf.collegeId == null || collegeIDs.Contains(rf.collegeId))).ToList();
                //}

                foreach (var item in collegeIntakeExisting)
                {
                    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;

                    //intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    //intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    //intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                    //intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);

                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }
                    else //MAM MTM Pharm.D Pharm.D PB
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }

                    intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;


                    if (item.Degree == "B.Pharmacy" || item.Degree == "M.Pharmacy")
                    {
                        intakedetails.Department = "Pharmacy";
                    }



                    intakedetails.AvailableFaculty = registeredFaculty.Count();
                    intakedetails.FalseNameFaculty = registeredFaculty.Where(r => r.isApproved == false && r.DeactivationReason == "False Name").Count();
                    intakedetails.FalsePhotoFaculty = registeredFaculty.Where(r => r.isApproved == false && r.DeactivationReason == "False Photo").Count();
                    intakedetails.FalsePANNumberFaculty = registeredFaculty.Where(r => r.isApproved == false && r.DeactivationReason == "False PAN Number").Count();
                    intakedetails.FalseAadhaarNumberFaculty = registeredFaculty.Where(r => r.isApproved == false && r.DeactivationReason == "False Aadhaar Number").Count();
                    intakedetails.CertificatesIncompleteFaculty = registeredFaculty.Where(r => r.isApproved == false && r.DeactivationReason == "Certificate(s) Incomplete").Count();
                    intakedetails.AbsentFaculty = registeredFaculty.Where(r => r.isApproved == false && r.DeactivationReason == "Absent").Count();
                    //=============//

                    intakedetailsList.Add(intakedetails);
                }

                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                if (type == "Excel")
                {
                    string strcollegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=" + strcollegeCode + "-Faculty.xls");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("_PharmacyFacultyWithIntake", intakedetailsList.Where(c => c.shiftId == 1).ToList());
                }
            }
            return View(intakedetailsList.Where(c => c.shiftId == 1).ToList());
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult CollegeLabs(int? collegeId)
        {
            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();
            ViewBag.Colleges = colleges.OrderBy(c => c.collegeName).ToList();

            List<Lab> lstlaboratories = new List<Lab>();

            if (collegeId != null)
            {
                ViewBag.Status = true;
                int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e.specializationId).Distinct().ToArray();
                List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                            .Where(l => specializationIds.Contains(l.SpecializationID))
                                                            .Select(l => new Lab
                                                            {
                                                                EquipmentID = l.id,
                                                                degreeId = l.DegreeID,
                                                                degree = l.jntuh_degree.degree,
                                                                degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
                                                                departmentId = l.DepartmentID,
                                                                department = l.jntuh_department.departmentName,
                                                                specializationId = l.SpecializationID,
                                                                specializationName = l.jntuh_specialization.specializationName,
                                                                year = l.Year,
                                                                Semester = l.Semester,
                                                                Labcode = l.Labcode,
                                                                LabName = l.LabName,
                                                                EquipmentName = l.EquipmentName
                                                            })
                                                            .OrderBy(l => l.degreeDisplayOrder)
                                                            .ThenBy(l => l.department)
                                                            .ThenBy(l => l.specializationName)
                                                            .ThenBy(l => l.year).ThenBy(l => l.Semester)
                                                            .ToList();

                var collegeEquipments = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeId).Select(l => l.EquipmentID).Distinct().ToArray();

                var list = collegeLabMaster.Where(c => !collegeEquipments.Contains(c.EquipmentID)).Select(c => new { id = c.EquipmentID, LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName, Degree = c.degree, Department = c.department, Specialization = c.specializationName, Year = c.year, Semester = c.Semester, SpecializationId = c.specializationId })
                                           .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

                var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeId && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

                list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

                var jntuh_college_laboratories = db.jntuh_college_laboratories_dataentry2.AsNoTracking().Where(l => l.CollegeID == collegeId).ToList();

                if (list.Count() > 0)
                {
                    foreach (var item in list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList())
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
                        lstlabs.EquipmentID = item.id; //(int)collegeLabMaster.Where(l => l.specializationId == item.SpecializationId && l.year == item.Year && l.Semester == item.Semester && l.Labcode == item.LabCode && l.LabName == item.LabName && l.EquipmentName == item.EquipmentName).Select(l => l.id).FirstOrDefault();
                        lstlabs.degree = item.Degree;
                        lstlabs.department = item.Department;
                        lstlabs.specializationName = item.Specialization;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.Year;
                        lstlabs.Labcode = item.LabCode;
                        lstlabs.LabName = item.LabName;
                        lstlabs.EquipmentName = item.EquipmentName;
                        lstlabs.LabEquipmentName = item.EquipmentName;
                        lstlabs.collegeId = (int)collegeId;
                        lstlabs.EquipmentNo = 1;
                        lstlabs.RandomCode = "";
                        lstlaboratories.Add(lstlabs);
                    }
                }
            }
            else
            {
                ViewBag.Status = false;
            }

            return View(lstlaboratories);
        }

        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //[HttpPost]
        //public ActionResult SaveLabs(List<Lab> collegeLabs)
        //{
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    int userCollegeID = collegeLabs[0].collegeId;
        //    var jntuh_college_laboratories_dataentrys2 = db.jntuh_college_laboratories_dataentry2.Where(C=>C.CollegeID==userCollegeID).Select(C=>C);
        //    foreach (var laboratories in collegeLabs)
        //    {
        //        if (laboratories.isAffiliated)
        //        {
        //            if (laboratories.EquipmentUniqueID == null)
        //            {
        //                laboratories.EquipmentUniqueID = string.Empty;
        //            }

        //            jntuh_college_laboratories_dataentry2 jntuh_college_laboratories = new jntuh_college_laboratories_dataentry2();
        //            jntuh_college_laboratories.CollegeID = laboratories.collegeId;
        //            jntuh_college_laboratories.EquipmentID = laboratories.EquipmentID;
        //            jntuh_college_laboratories.Make = laboratories.Make;
        //            jntuh_college_laboratories.Model = laboratories.Model;
        //            jntuh_college_laboratories.EquipmentUniqueID = laboratories.EquipmentUniqueID;
        //            jntuh_college_laboratories.EquipmentName = laboratories.EquipmentName;
        //            jntuh_college_laboratories.AvailableUnits = laboratories.AvailableUnits;
        //            jntuh_college_laboratories.AvailableArea = laboratories.AvailableArea;
        //            jntuh_college_laboratories.RoomNumber = laboratories.RoomNumber;
        //            jntuh_college_laboratories.EquipmentNo = laboratories.EquipmentNo;
        //            jntuh_college_laboratories.isActive = true;
        //            jntuh_college_laboratories.isAffiliated = true;

        //            var existingID = jntuh_college_laboratories_dataentrys2.Where(c => c.CollegeID == laboratories.collegeId && c.EquipmentID == laboratories.EquipmentID && c.EquipmentNo == laboratories.EquipmentNo).Select(c => c).FirstOrDefault();

        //            if (existingID == null)
        //            {
        //                jntuh_college_laboratories.createdBy = userID;
        //                jntuh_college_laboratories.createdOn = DateTime.Now;
        //                db.jntuh_college_laboratories_dataentry2.Add(jntuh_college_laboratories);
        //                db.SaveChanges();
        //                TempData["Success"] = "Lab Added Successfully.";
        //            }
        //            else
        //            {
        //                TempData["Success"] = "Lab already exists.";
        //            }
        //        }
        //    }

        //    db.SaveChanges();

        //    return RedirectToAction("CollegeLabs", new { collegeId = userCollegeID });
        //}

        [HttpGet]
        public ActionResult GetFacultyDetails(string RegistrationNumber)
        {
            if (!string.IsNullOrEmpty(RegistrationNumber))
            {
                try
                {
                    List<jntuh_registered_faculty> jntuh_registered_faculty =
                        db.jntuh_registered_faculty.AsNoTracking()
                            .Where(e => e.RegistrationNumber == RegistrationNumber)
                            .Select(e => e)
                            .ToList();
                    if (jntuh_registered_faculty.Count != 0)
                    {
                        List<jntuh_college_faculty_registered> College_faculty_registration =
                            db.jntuh_college_faculty_registered.AsNoTracking()
                                .Where(e => e.RegistrationNumber == RegistrationNumber)
                                .Select(e => e)
                                .ToList();
                        if (College_faculty_registration.Count != 0)
                        {
                            var Data = (from reg in jntuh_registered_faculty
                                        join clgfaculty in College_faculty_registration on reg.RegistrationNumber equals clgfaculty.RegistrationNumber 
                            
                                        select new
                                        {
                                            Name = reg.FirstName + " " + reg.LastName + " " + reg.MiddleName,
                                            DesignationId = Convert.ToInt32(reg.DesignationId),
                                            DepartmentId = clgfaculty.DepartmentId == null ? Convert.ToInt32(reg.DepartmentId) : Convert.ToInt32(clgfaculty.DepartmentId),
                                            CollegeId = Convert.ToInt32(clgfaculty.collegeId),
                                            EmailId = reg.Email,
                                            MobileNo = reg.Mobile,
                                            RegNumber = reg.RegistrationNumber
                                        }).FirstOrDefault();
                            return Json( Data,JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var Data = (from reg in jntuh_registered_faculty select  new
                            {
                                Name=reg.FirstName+" "+reg.LastName+" "+reg.MiddleName,
                                DesignationId=Convert.ToInt32(reg.DesignationId),
                                DepartmentId=Convert.ToInt32(reg.DepartmentId),
                                CollegeId=Convert.ToInt32(reg.collegeId),
                                EmailId=reg.Email,
                                MobileNo=reg.Mobile,
                                RegNumber=reg.RegistrationNumber
                            }).FirstOrDefault();
                            return Json(Data, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else
                    {
                        return Json("Registration Number not found.", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {

                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
              //  return null;
            }
            else
            {
                return Json("Please enter Registration Number.", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetColleges()
        {
            var colleges = (from clg in db.jntuh_college
                join edit in db.jntuh_college_edit_status on clg.id equals edit.collegeId
                where edit.IsCollegeEditable==false
                select new
                {
                    CollegeId=clg.id,
                    CollegeName=clg.collegeCode+"-"+clg.collegeName
                }).ToList();
            return Json(colleges, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDepartments()
        {
            var Department = db.jntuh_department.Where(e => e.isActive == true).Select(e => new
            {
                DeptId=e.id,
                DeptName=e.departmentName
            }).ToList();
            return Json(Department, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDesignation()
        {
            var DesignationList = db.jntuh_designation.Where(e => e.isActive == true).Select(e => new
            {
                DesignationId = e.id,
                DesignationName = e.designation
            }).ToList();
            return Json(DesignationList, JsonRequestBehavior.AllowGet);
        }




    }
}
