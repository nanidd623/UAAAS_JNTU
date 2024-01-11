using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using UAAAS.Models;
using UAAAS.Mailers;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class PublicController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //Filter Colleges
        public ActionResult FindCollege()
        {
            CollegeFilter filter = new CollegeFilter();
            ViewBag.District = db.jntuh_district.Where(d => d.isActive).OrderBy(d => d.districtName)
                                 .Select(d => new
                                 {
                                     Id = d.id,
                                     Name = d.districtName
                                 }).ToList();

            ViewBag.Degree = db.jntuh_degree.Where(d => d.isActive).OrderBy(d => d.degreeDisplayOrder)
                                 .Select(d => new
                                 {
                                     Id = d.id,
                                     Name = d.degree
                                 }).ToList();

            return View(filter);
        }

        //Filter Colleges
        [HttpPost]
        public ActionResult FindCollege(CollegeFilter filter)
        {
            ViewBag.District = db.jntuh_district.Where(d => d.isActive).OrderBy(d => d.districtName)
                                 .Select(d => new
                                 {
                                     Id = d.id,
                                     Name = d.districtName
                                 }).ToList();

            ViewBag.Degree = db.jntuh_degree.Where(d => d.isActive).OrderBy(d => d.degreeDisplayOrder)
                                 .Select(d => new
                                 {
                                     Id = d.id,
                                     Name = d.degree
                                 }).ToList();

            return View(filter);
        }

        //CollegeInformation for public
        public ActionResult CollegeInformation(string id)
        {
            int collegeId = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));

            CollegeDetails details = new CollegeDetails();
            PrincipalDetails principal = new PrincipalDetails();

            if (collegeId > 0)
            {
                var college = db.jntuh_college.Find(collegeId);
                details.Code = college.collegeCode;
                details.Name = college.collegeName;
                details.Id = collegeId;
                details.EamcetCode = college.eamcetCode;
                details.IcetCode = college.icetCode;
                details.PgcetCode = college.pgcetCode;

                jntuh_address address = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == collegeId).Select(a => a).FirstOrDefault();
                details.Address = address.address;
                details.TownOrCity = address.townOrCity;
                details.Mandal = address.mandal;
                details.Pincode = address.pincode.ToString();
                details.District = db.jntuh_district.Find(address.districtId).districtName;
                details.Website = address.website;
                details.EstablishedYear = db.jntuh_college_establishment.Where(e => e.collegeId == collegeId).Select(e => e.instituteEstablishedYear).FirstOrDefault().ToString();
                details.Photos = db.jntuh_college_document.Where(a => a.collegeId == collegeId)
                    .Select(a => new CollegeDocuments
                    {
                        id = a.id,
                        collegeId = collegeId,
                        documentId = a.documentId,
                        documentName = db.jntuh_documents_required.Where(d => d.id == a.documentId).Select(d => d.documentName).FirstOrDefault(),
                        scannedCopy = a.scannedCopy
                    }).OrderBy(d => d.documentId).ToList();
                details.Mobile = address.mobile;
                details.LandLine = address.mobile;
                details.Email = address.email;

                jntuh_college_principal_registered _principal = db.jntuh_college_principal_registered.AsNoTracking().Where(p => p.collegeId == collegeId).Select(p => p).FirstOrDefault();

                if (_principal != null)
                {
                    jntuh_registered_faculty prinicipaldetails = db.jntuh_registered_faculty.Where(p => p.RegistrationNumber == _principal.RegistrationNumber).Select(p => p).FirstOrDefault();
                    if (prinicipaldetails != null)
                    {
                        principal.FirstName = prinicipaldetails.FirstName;
                        principal.LastName = prinicipaldetails.MiddleName;
                        principal.SurName = prinicipaldetails.LastName;
                        principal.Department = db.jntuh_department.Where(d => d.id == prinicipaldetails.DepartmentId).Select(d => d.departmentName).SingleOrDefault();
                        principal.DateofAppointment = prinicipaldetails.DateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(prinicipaldetails.DateOfAppointment.ToString()).ToString();
                        principal.Photo = prinicipaldetails.Photo;
                    }
                }

                var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
                int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                DateTime todayDate = DateTime.Now.Date;

                var editToDate = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == collegeId && editStatus.academicyearId == ay0).Select(a => a.editToDate).FirstOrDefault();

                if (editToDate == null)
                {
                    principal.LastUpdatedDate = "--/--/----";
                }
                else if (editToDate >= todayDate)
                {
                    principal.LastUpdatedDate = Utilities.MMDDYY2DDMMYY(todayDate.ToString()).ToString();
                }
                else
                {
                    principal.LastUpdatedDate = Utilities.MMDDYY2DDMMYY(editToDate.ToString()).ToString();
                }
            }
            //return View(details);
            return View(model: new Tuple<CollegeDetails, PrincipalDetails>(details, principal));
        }

        //Batch Wise Performance for public
        public ActionResult BatchPerformance(string id)
        {
            int collegeId = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));

            List<BatchWisePerformance> lstPerformance = new List<BatchWisePerformance>();

            if (collegeId > 0)
            {
                string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;

                List<jntuh_college_batch_performance> batchPerformance = db.jntuh_college_batch_performance.Where(p => p.collegeCode == collegeCode).OrderByDescending(p => p.batch).Select(p => p).ToList();

                foreach (jntuh_college_batch_performance item in batchPerformance)
                {
                    BatchWisePerformance performance = new BatchWisePerformance();
                    performance.Batch = item.batch;
                    performance.Duration = item.batch + "-" + (int.Parse(item.batch) + 3).ToString().GetLast(2);
                    performance.Enrolled = item.enrolledStudents;
                    performance.Passed = item.passedStudents;

                    decimal enrolled = 0, passed = 0;

                    if (item.enrolledStudents != null)
                    {
                        enrolled = decimal.Parse(item.enrolledStudents.ToString());
                    }

                    if (item.passedStudents != null)
                    {
                        if (item.passedStudents > item.enrolledStudents)
                        {
                            passed = decimal.Parse(item.enrolledStudents.ToString());
                            performance.Passed = item.enrolledStudents;
                        }
                        else
                        {
                            passed = decimal.Parse(item.passedStudents.ToString());
                        }
                    }

                    if ((enrolled != 0 && passed != 0) && (enrolled >= passed))
                    {
                        performance.Percentage = Math.Round((passed / enrolled) * 100, 0);
                    }

                    lstPerformance.Add(performance);
                }

            }

            return View(lstPerformance);
        }

        //JQuery Request - To show academic performance graphs
        public JsonResult GetDataAssets(string id)
        {
            List<object> data = new List<object>();
            data.Add(new object[] { "Batches", "Enrolled Students", "Passed Students", "Pass Percentages" });

            int collegeId = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));

            string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;

            List<jntuh_college_batch_performance> batchPerformance = db.jntuh_college_batch_performance.Where(p => p.collegeCode == collegeCode).OrderByDescending(p => p.batch).Select(p => p).ToList();

            foreach (jntuh_college_batch_performance item in batchPerformance)
            {
                BatchWisePerformance performance = new BatchWisePerformance();
                performance.Batch = item.batch;
                performance.Duration = item.batch + "-" + (int.Parse(item.batch) + 3).ToString().GetLast(2);
                performance.Enrolled = item.enrolledStudents;
                performance.Passed = item.passedStudents;

                decimal enrolled = 0, passed = 0;

                if (item.enrolledStudents != null)
                {
                    enrolled = decimal.Parse(item.enrolledStudents.ToString());
                }

                if (item.passedStudents != null)
                {
                    if (item.passedStudents > item.enrolledStudents)
                    {
                        passed = decimal.Parse(item.enrolledStudents.ToString());
                        performance.Passed = item.enrolledStudents;
                    }
                    else
                    {
                        passed = decimal.Parse(item.passedStudents.ToString());
                    }
                }

                if ((enrolled != 0 && passed != 0) && (enrolled >= passed))
                {
                    performance.Percentage = Math.Round((passed / enrolled) * 100, 0);
                }

                data.Add(new object[] { performance.Duration, performance.Enrolled, performance.Passed, performance.Percentage });
            }

            return Json(data);
        }

        //JQuery Request - To bind specializations
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDepartments(string degree)
        {
            int degreeId;

            if (degree == string.Empty)
            {
                degreeId = 0;
            }
            else
            {
                degreeId = int.Parse(degree);
            }

            var departments = db.jntuh_department.Where(s => s.isActive == true)
                                    .Join(db.jntuh_degree, de => de.degreeId, d => d.id, (de, d) => new { de.id, de.departmentName, degreeId = d.id })
                                    .Where(n => n.degreeId == degreeId).Select(n => new
                                    {
                                        Name = n.departmentName,
                                        Id = n.id
                                    }).OrderBy(n => n.Name).ToList();

            var myData = departments.Select(a => new SelectListItem()
            {
                Text = a.Name.ToString(),
                Value = a.Id.ToString(),
            });

            return Json(myData, JsonRequestBehavior.AllowGet);
        }

        //JQuery Request - To bind specializations
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetSpecializations(string department)
        {
            int departmentId;

            if (department == string.Empty)
            {
                departmentId = 0;
            }
            else
            {
                departmentId = int.Parse(department);
            }

            var specializations = db.jntuh_specialization.Where(s => s.isActive == true)
                                    .Join(db.jntuh_department, s => s.departmentId, d => d.id, (s, d) => new { s.id, s.specializationName, departmentId = d.id })
                                    .Where(n => n.departmentId == departmentId).Select(n => new
                                    {
                                        Name = n.specializationName,
                                        Id = n.id
                                    }).OrderBy(n => n.Name).ToList();

            var myData = specializations.Select(a => new SelectListItem()
            {
                Text = a.Name.ToString(),
                Value = a.Id.ToString(),
            });

            return Json(myData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PublicColleges()
        {
            var distinctColleges = db.jntuh_approvedadmitted_intake.Select(i => i.collegeId).Distinct().ToList();
            List<jntuh_college> colleges = db.jntuh_college.Where(c => distinctColleges.Contains(c.id)).Select(c => c).ToList();
            return View(colleges);

            #region Previous Code

            //List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => c).ToList();
            //return View(colleges);

            #endregion
        }

        public ActionResult TeachingFaculty(string collegeId)
        {
            return RedirectToAction("Index", "Home");
            List<jntuh_registered_faculty> regFaculty = null;
            try
            {
                int userCollegeID = 0;
                if (collegeId.Trim().Length > 10 || collegeId.Contains("="))
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                else
                    userCollegeID = Convert.ToInt32(collegeId);
                ViewBag.CollegeName = db.jntuh_college.Find(userCollegeID).collegeName;
                ViewBag.CollegeCode = db.jntuh_college.Find(userCollegeID).collegeCode;
                string[] strRegNos = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == userCollegeID).Select(cf => cf.RegistrationNumber).ToArray();
                regFaculty = db.jntuh_registered_faculty.Where(rf => strRegNos.Contains(rf.RegistrationNumber)).Select(rf => rf).ToList();

                //List<jntuh_registered_faculty> regFacultyNull = regFaculty.Where(a => a.jntuh_department == null).ToList();
                //List<jntuh_registered_faculty> regFacultyWithoutNull = regFaculty.Where(a => a.jntuh_department != null).OrderBy(a => a.jntuh_department.departmentName).ToList();

                //regFaculty = regFacultyWithoutNull.Union(regFacultyNull).ToList();
            }
            catch (Exception EX)
            {
                // LogController Errorlog = new LogController();
                // string ErrorMessage = "Controller: Public , Action : TeachingFaculty" + "College Id Is :" + collegeId + "," + "Error:" + EX.Message;
                //  Errorlog.Log(ErrorMessage);
                throw;

            }


            return View(regFaculty);
        }

        public ActionResult Affiliations()
        {
            int[] CollegeIds = db.jntuh_college_news
                         .Where(n => n.title.Contains("Grant of Affiliation is available at your portal for download."))
                         .Select(c => c.collegeId)
                         .ToArray();

            List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true && CollegeIds.Contains(c.id)).Select(c => c).ToList();
            return View(colleges);
        }
        public ActionResult JointInspectionReports()
        {
            List<int> numbers = new List<int> { 42, 63, 115, 125, 140, 151, 158, 182, 201, 255, 260, 278, 308, 335, 336, 368, 369, 370, 371, 377, 385, 401, 416, 443 };

            //List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true  && CollegeIds.Contains(c.id)).Select(c => c).ToList();
            List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true && numbers.Contains(c.id)).Select(c => c).ToList();
            return View(colleges);
        }

        public ActionResult AadhaarEnabledBiometricSyatem()
        {
            return View();
        }
        public ActionResult AutonomousCollegsyllabus()
        {
            List<int> numbers = new List<int> { 11, 26, 32, 38, 68, 108, 109, 134, 171, 179, 180, 183, 192, 196, 198, 335, 399 };

            //List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true  && CollegeIds.Contains(c.id)).Select(c => c).ToList();
            List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true && numbers.Contains(c.id)).Select(c => c).ToList();
            return View(colleges);
        }
        public ActionResult CollegesReports()
        {
            //var numbers = new List<int> {33, 42, 63, 115, 125, 140, 151, 158, 182, 201, 255, 260, 278, 308, 335, 336, 368, 369, 370, 371, 377, 385, 401, 416, 443 };
            var numbers = db.jntuh_college_edit_status.Where(C => C.IsCollegeEditable == false).Select(C => C.collegeId).ToList();
            var numbers1 = db.jntuh_college_edit_status.Where(C => C.IsCollegeEditable == false).Select(C => C.collegeId).ToArray();
            var CollegeReinspectionIds = new[] { 125, 140, 151 };
            var collegeRecommendationIds = new[] { 201, 255, 260 };
            var CollegeAffiliationIds = new[] { 125, 201, 308, 335, 336 };

            var CollegeReports =
                db.jntuh_college.Where(c => c.isActive && numbers.Contains(c.id)).Select(c =>
                            new Collegecodesreports()
                            {
                                CollegeId = c.id,
                                CollegeCode = c.collegeCode,
                                CollegeName = c.collegeName,

                            }).ToList();
            CollegeReports.ToList().ForEach(i => i.ReinspectionIds = CollegeReinspectionIds);
            CollegeReports.ToList().ForEach(i => i.RecommendationIds = numbers1);
            CollegeReports.ToList().ForEach(i => i.AffiliationIds = numbers1);
            return View(CollegeReports);
        }


        public ActionResult CollegeNameAndAddress()
        {
            var collegeIds = db.jntuh_college_intake_existing.Where(e => e.academicYearId == 11 && e.approvedIntake != 0 && e.collegeId != 375).Select(e => e.collegeId).Distinct().ToArray();
            List<CollegeAddress> CollegeNameAndAddress = (from coll in db.jntuh_college
                                                          join addr in db.jntuh_address on coll.id equals addr.collegeId
                                                          join distr in db.jntuh_district on addr.districtId equals distr.id
                                                          join stat in db.jntuh_state on addr.stateId equals stat.id

                                                          where collegeIds.Contains(coll.id) && addr.addressTye == "COLLEGE"
                                                          select new CollegeAddress
                                                          {
                                                              id = coll.id,
                                                              CollegeName = coll.collegeName,
                                                              CollegeCode = coll.collegeCode,
                                                              EamcetCode = coll.eamcetCode,
                                                              Address = addr.address,
                                                              Email = addr.email,
                                                              Landline = addr.landline,
                                                              Website = addr.website,
                                                              TownorCity = addr.townOrCity,
                                                              Mandal = addr.mandal,
                                                              Pincode = addr.pincode,
                                                              fax = addr.fax,
                                                              Mobile = addr.mobile,
                                                              District = distr.districtName,
                                                              State = stat.stateName,
                                                              //Issubmitted = edit.isSubmitted
                                                          }).ToList();
            return View(CollegeNameAndAddress);

        }

        public ActionResult OldColleges()
        {
            List<OldColleges> oldCollegesList = new List<OldColleges>();
            List<jntuh_college> collegesList = db.jntuh_college.ToList();

            oldCollegesList = (from o in db.jntuh_oladcolleges
                               //join c in db.jntuh_college on o.collegecode equals c.collegeCode
                               where o.collegecode != "ZZ"
                               select new OldColleges
                               {
                                   sno = o.sno,
                                   //CollegeId = collegesList.Where(c => c.collegeCode == o.collegecode).Select(c => c.id).FirstOrDefault(),
                                   CollegeCode = o.collegecode,
                                   CollegeName = o.collegename,
                                   Address = o.address,
                                   District = o.district,
                                   YearOfEstablishment = o.yearofestablishment,
                                   YearOfClosing = o.yearofclosing,
                                   Website = o.website,
                                   CollegeStatus = o.collegestatus
                               }).OrderBy(o => o.sno).ToList();

            foreach (var item in oldCollegesList)
            {
                item.CollegeId = collegesList.Where(c => c.collegeCode == item.CollegeCode).Select(c => c.id).FirstOrDefault();
            }

            return View(oldCollegesList);
        }

        public ActionResult OldCollegeInformation(string collegeCode)
        {
            string collegeCC = Utilities.DecryptString(collegeCode, WebConfigurationManager.AppSettings["CryptoKey"]);

            CollegeDetails details = new CollegeDetails();
            PrincipalDetails principal = new PrincipalDetails();

            if (!string.IsNullOrEmpty(collegeCC))
            {
                var college = db.jntuh_oladcolleges.Where(c => c.collegecode == collegeCC).FirstOrDefault();
                details.Code = college.collegecode;
                details.Name = college.collegename;
                details.Id = college.sno;
                details.EamcetCode = "";
                details.IcetCode = "";
                details.PgcetCode = "";

                //jntuh_address address = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == collegeId).Select(a => a).FirstOrDefault();
                details.Address = college.address;
                details.TownOrCity = "";
                details.Mandal = "";
                details.Pincode = "";
                details.District = college.district;
                details.Website = college.website;
                details.EstablishedYear = Convert.ToString(college.yearofestablishment);
                //details.Photos = db.jntuh_college_document.Where(a => a.collegeId == collegeId)
                //    .Select(a => new CollegeDocuments
                //    {
                //        id = a.id,
                //        collegeId = collegeId,
                //        documentId = a.documentId,
                //        documentName = db.jntuh_documents_required.Where(d => d.id == a.documentId).Select(d => d.documentName).FirstOrDefault(),
                //        scannedCopy = a.scannedCopy
                //    }).OrderBy(d => d.documentId).ToList();
                //details.Mobile = address.mobile;
                //details.LandLine = address.mobile;
                //details.Email = address.email;

                //jntuh_college_principal_registered _principal = db.jntuh_college_principal_registered.AsNoTracking().Where(p => p.collegeId == collegeId).Select(p => p).FirstOrDefault();

                //if (_principal != null)
                //{
                //    jntuh_registered_faculty prinicipaldetails = db.jntuh_registered_faculty.Where(p => p.RegistrationNumber == _principal.RegistrationNumber).Select(p => p).FirstOrDefault();
                //    if (prinicipaldetails != null)
                //    {
                //        principal.FirstName = prinicipaldetails.FirstName;
                //        principal.LastName = prinicipaldetails.MiddleName;
                //        principal.SurName = prinicipaldetails.LastName;
                //        principal.Department = db.jntuh_department.Where(d => d.id == prinicipaldetails.DepartmentId).Select(d => d.departmentName).SingleOrDefault();
                //        principal.DateofAppointment = prinicipaldetails.DateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(prinicipaldetails.DateOfAppointment.ToString()).ToString();
                //        principal.Photo = prinicipaldetails.Photo;
                //    }
                //}

                //var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
                //int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                //int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                //DateTime todayDate = DateTime.Now.Date;

                //var editToDate = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == collegeId && editStatus.academicyearId == ay0).Select(a => a.editToDate).FirstOrDefault();

                //if (editToDate == null)
                //{
                //    principal.LastUpdatedDate = "--/--/----";
                //}
                //else if (editToDate >= todayDate)
                //{
                //    principal.LastUpdatedDate = Utilities.MMDDYY2DDMMYY(todayDate.ToString()).ToString();
                //}
                //else
                //{
                //    principal.LastUpdatedDate = Utilities.MMDDYY2DDMMYY(editToDate.ToString()).ToString();
                //}
            }
            //return View(details);
            return View(model: new Tuple<CollegeDetails, PrincipalDetails>(details, principal));
        }

        public ActionResult ActiveInactiveColleges()
        {
            var collegeIds = db.jntuh_college_intake_existing.Where(e => e.academicYearId == 11 && e.approvedIntake != 0 && e.collegeId != 375).Select(e => e.collegeId).Distinct().ToArray();
            List<CollegeAddress> CollegeNameAndAddress = (from coll in db.jntuh_college
                                                          join addr in db.jntuh_address on coll.id equals addr.collegeId
                                                          join distr in db.jntuh_district on addr.districtId equals distr.id
                                                          join stat in db.jntuh_state on addr.stateId equals stat.id

                                                          where collegeIds.Contains(coll.id) && addr.addressTye == "COLLEGE"
                                                          select new CollegeAddress
                                                          {
                                                              id = coll.id,
                                                              CollegeName = coll.collegeName,
                                                              CollegeCode = coll.collegeCode,
                                                              EamcetCode = coll.eamcetCode,
                                                              Address = addr.address,
                                                              Email = addr.email,
                                                              Landline = addr.landline,
                                                              Website = addr.website,
                                                              TownorCity = addr.townOrCity,
                                                              Mandal = addr.mandal,
                                                              Pincode = addr.pincode,
                                                              fax = addr.fax,
                                                              Mobile = addr.mobile,
                                                              District = distr.districtName,
                                                              State = stat.stateName,
                                                              //Issubmitted = edit.isSubmitted
                                                          }).ToList();
            return View(CollegeNameAndAddress);
        }

        public ActionResult AY2015To16()
        {
            var collegeIds = db.jntuh_college_intake_existing.Where(e => e.academicYearId == 3 && e.approvedIntake != 0 && e.collegeId != 375).Select(e => e.collegeId).Distinct().ToArray();
            List<CollegeAddress> CollegeNameAndAddress = (from coll in db.jntuh_college
                                                          join addr in db.jntuh_address on coll.id equals addr.collegeId
                                                          join distr in db.jntuh_district on addr.districtId equals distr.id
                                                          join stat in db.jntuh_state on addr.stateId equals stat.id

                                                          where collegeIds.Contains(coll.id) && addr.addressTye == "COLLEGE"
                                                          select new CollegeAddress
                                                          {
                                                              id = coll.id,
                                                              CollegeName = coll.collegeName,
                                                              CollegeCode = coll.collegeCode,
                                                              EamcetCode = coll.eamcetCode,
                                                              Address = addr.address,
                                                              Email = addr.email,
                                                              Landline = addr.landline,
                                                              Website = addr.website,
                                                              TownorCity = addr.townOrCity,
                                                              Mandal = addr.mandal,
                                                              Pincode = addr.pincode,
                                                              fax = addr.fax,
                                                              Mobile = addr.mobile,
                                                              District = distr.districtName,
                                                              State = stat.stateName,
                                                              //Issubmitted = edit.isSubmitted
                                                          }).ToList();
            return View(CollegeNameAndAddress);
        }

        public ActionResult AY2016To17()
        {
            var collegeIds = db.jntuh_college_intake_existing.Where(e => e.academicYearId == 8 && e.approvedIntake != 0 && e.collegeId != 375).Select(e => e.collegeId).Distinct().ToArray();
            List<CollegeAddress> CollegeNameAndAddress = (from coll in db.jntuh_college
                                                          join addr in db.jntuh_address on coll.id equals addr.collegeId
                                                          join distr in db.jntuh_district on addr.districtId equals distr.id
                                                          join stat in db.jntuh_state on addr.stateId equals stat.id

                                                          where collegeIds.Contains(coll.id) && addr.addressTye == "COLLEGE"
                                                          select new CollegeAddress
                                                          {
                                                              id = coll.id,
                                                              CollegeName = coll.collegeName,
                                                              CollegeCode = coll.collegeCode,
                                                              EamcetCode = coll.eamcetCode,
                                                              Address = addr.address,
                                                              Email = addr.email,
                                                              Landline = addr.landline,
                                                              Website = addr.website,
                                                              TownorCity = addr.townOrCity,
                                                              Mandal = addr.mandal,
                                                              Pincode = addr.pincode,
                                                              fax = addr.fax,
                                                              Mobile = addr.mobile,
                                                              District = distr.districtName,
                                                              State = stat.stateName,
                                                              //Issubmitted = edit.isSubmitted
                                                          }).ToList();
            return View(CollegeNameAndAddress);
        }

        public ActionResult AY2017To18()
        {
            var collegeIds = db.jntuh_college_intake_existing.Where(e => e.academicYearId == 9 && e.approvedIntake != 0 && e.collegeId != 375).Select(e => e.collegeId).Distinct().ToArray();
            List<CollegeAddress> CollegeNameAndAddress = (from coll in db.jntuh_college
                                                          join addr in db.jntuh_address on coll.id equals addr.collegeId
                                                          join distr in db.jntuh_district on addr.districtId equals distr.id
                                                          join stat in db.jntuh_state on addr.stateId equals stat.id

                                                          where collegeIds.Contains(coll.id) && addr.addressTye == "COLLEGE"
                                                          select new CollegeAddress
                                                          {
                                                              id = coll.id,
                                                              CollegeName = coll.collegeName,
                                                              CollegeCode = coll.collegeCode,
                                                              EamcetCode = coll.eamcetCode,
                                                              Address = addr.address,
                                                              Email = addr.email,
                                                              Landline = addr.landline,
                                                              Website = addr.website,
                                                              TownorCity = addr.townOrCity,
                                                              Mandal = addr.mandal,
                                                              Pincode = addr.pincode,
                                                              fax = addr.fax,
                                                              Mobile = addr.mobile,
                                                              District = distr.districtName,
                                                              State = stat.stateName,
                                                              //Issubmitted = edit.isSubmitted
                                                          }).ToList();
            return View(CollegeNameAndAddress);
        }

        public ActionResult AY2018To19()
        {
            var collegeIds = db.jntuh_college_intake_existing.Where(e => e.academicYearId == 10 && e.approvedIntake != 0 && e.collegeId != 375).Select(e => e.collegeId).Distinct().ToArray();
            List<CollegeAddress> CollegeNameAndAddress = (from coll in db.jntuh_college
                                                          join addr in db.jntuh_address on coll.id equals addr.collegeId
                                                          join distr in db.jntuh_district on addr.districtId equals distr.id
                                                          join stat in db.jntuh_state on addr.stateId equals stat.id

                                                          where collegeIds.Contains(coll.id) && addr.addressTye == "COLLEGE"
                                                          select new CollegeAddress
                                                          {
                                                              id = coll.id,
                                                              CollegeName = coll.collegeName,
                                                              CollegeCode = coll.collegeCode,
                                                              EamcetCode = coll.eamcetCode,
                                                              Address = addr.address,
                                                              Email = addr.email,
                                                              Landline = addr.landline,
                                                              Website = addr.website,
                                                              TownorCity = addr.townOrCity,
                                                              Mandal = addr.mandal,
                                                              Pincode = addr.pincode,
                                                              fax = addr.fax,
                                                              Mobile = addr.mobile,
                                                              District = distr.districtName,
                                                              State = stat.stateName,
                                                              //Issubmitted = edit.isSubmitted
                                                          }).ToList();
            return View(CollegeNameAndAddress);
        }

        public ActionResult AY2020To21()
        {
            var collegeIds = db.jntuh_college_intake_existing.Where(e => e.academicYearId == 12 && e.approvedIntake != 0 && e.collegeId != 375).Select(e => e.collegeId).Distinct().ToArray();
            List<CollegeAddress> CollegeNameAndAddress = (from coll in db.jntuh_college
                                                          join addr in db.jntuh_address on coll.id equals addr.collegeId
                                                          join distr in db.jntuh_district on addr.districtId equals distr.id
                                                          join stat in db.jntuh_state on addr.stateId equals stat.id

                                                          where collegeIds.Contains(coll.id) && addr.addressTye == "COLLEGE"
                                                          select new CollegeAddress
                                                          {
                                                              id = coll.id,
                                                              CollegeName = coll.collegeName,
                                                              CollegeCode = coll.collegeCode,
                                                              EamcetCode = coll.eamcetCode,
                                                              Address = addr.address,
                                                              Email = addr.email,
                                                              Landline = addr.landline,
                                                              Website = addr.website,
                                                              TownorCity = addr.townOrCity,
                                                              Mandal = addr.mandal,
                                                              Pincode = addr.pincode,
                                                              fax = addr.fax,
                                                              Mobile = addr.mobile,
                                                              District = distr.districtName,
                                                              State = stat.stateName,
                                                              //Issubmitted = edit.isSubmitted
                                                          }).ToList();
            return View(CollegeNameAndAddress);
        }

        public ActionResult AcademicYearWiseColleges(string academicYearId)
        {
            List<CollegeAddress> CollegeNameAndAddress = new List<CollegeAddress>();
            ViewBag.AcademicYear = null;
            ViewBag.AcademicYearsList = db.jntuh_academic_year.Where(a => a.actualYear > 2013).Select(a => new { id = a.id, academicYear = a.academicYear }).ToList();
            if (academicYearId != null)
            {
                int intAcademicYearId = Convert.ToInt32(academicYearId);
                var collegeIds = db.jntuh_college_intake_existing.Where(e => e.academicYearId == intAcademicYearId && e.approvedIntake != 0 && e.collegeId != 375).Select(e => e.collegeId).Distinct().ToArray();
                CollegeNameAndAddress = (from coll in db.jntuh_college
                                         join addr in db.jntuh_address on coll.id equals addr.collegeId
                                         join distr in db.jntuh_district on addr.districtId equals distr.id
                                         join stat in db.jntuh_state on addr.stateId equals stat.id

                                         where collegeIds.Contains(coll.id) && addr.addressTye == "COLLEGE"
                                         select new CollegeAddress
                                         {
                                             id = coll.id,
                                             CollegeName = coll.collegeName,
                                             CollegeCode = coll.collegeCode,
                                             EamcetCode = coll.eamcetCode,
                                             Address = addr.address,
                                             Email = addr.email,
                                             Landline = addr.landline,
                                             Website = addr.website,
                                             TownorCity = addr.townOrCity,
                                             Mandal = addr.mandal,
                                             Pincode = addr.pincode,
                                             fax = addr.fax,
                                             Mobile = addr.mobile,
                                             District = distr.districtName,
                                             State = stat.stateName,
                                             //Issubmitted = edit.isSubmitted
                                         }).ToList();
                ViewBag.AcademicYearId = academicYearId;
                ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.id == intAcademicYearId).Select(a => a.academicYear).FirstOrDefault();
            }
            return View(CollegeNameAndAddress);
        }

        public ActionResult CollegeCourses(string collegeId, string academicYearId)
        {
            List<CollegeAddress> CollegeNameAndAddress = new List<CollegeAddress>();
            int collegeid = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            int academicyearid = Convert.ToInt32(Utilities.DecryptString(academicYearId, WebConfigurationManager.AppSettings["CryptoKey"]));

            var Specializations = db.jntuh_specialization.ToList();

            List<CollegeCourses> collegeCourses = (from d in db.jntuh_degree
                                                   join dep in db.jntuh_department on d.id equals dep.degreeId
                                                   join s in db.jntuh_specialization on dep.id equals s.departmentId
                                                   join i in db.jntuh_approvedadmitted_intake on s.id equals i.SpecializationId
                                                   where i.AcademicYearId == academicyearid && i.collegeId == collegeid
                                                   select new CollegeCourses
                                                   {
                                                       DegreeId = d.id,
                                                       DegreeName = d.degree,
                                                       SpecializationId = i.SpecializationId,
                                                       SpecializationName = s.specializationName,
                                                       ApprovedIntake = i.ApprovedIntake,
                                                       DegreeDisplayOrder = (int)d.degreeDisplayOrder
                                                   }).OrderBy(c => c.DegreeDisplayOrder).ToList();

            CollegeDetails details = new CollegeDetails();
            if (collegeid > 0)
            {
                var college = db.jntuh_college.Find(collegeid);
                details.Code = college.collegeCode;
                details.Name = college.collegeName;
                details.Id = collegeid;
                details.EamcetCode = college.eamcetCode;
                details.IcetCode = college.icetCode;
                details.PgcetCode = college.pgcetCode;

                jntuh_address address = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == collegeid).Select(a => a).FirstOrDefault();
                details.Address = address.address;
                details.TownOrCity = address.townOrCity;
                details.Mandal = address.mandal;
                details.Pincode = address.pincode.ToString();
                details.District = db.jntuh_district.Find(address.districtId).districtName;
                details.Website = address.website;
                details.EstablishedYear = db.jntuh_college_establishment.Where(e => e.collegeId == collegeid).Select(e => e.instituteEstablishedYear).FirstOrDefault().ToString();
                //details.Photos = db.jntuh_college_document.Where(a => a.collegeId == collegeId)
                //    .Select(a => new CollegeDocuments
                //    {
                //        id = a.id,
                //        collegeId = collegeId,
                //        documentId = a.documentId,
                //        documentName = db.jntuh_documents_required.Where(d => d.id == a.documentId).Select(d => d.documentName).FirstOrDefault(),
                //        scannedCopy = a.scannedCopy
                //    }).OrderBy(d => d.documentId).ToList();
                details.Mobile = address.mobile;
                details.LandLine = address.mobile;
                details.Email = address.email;
            }

            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.id == academicyearid).Select(a => a.academicYear).FirstOrDefault();
            return View(model: new Tuple<CollegeDetails, List<CollegeCourses>>(details, collegeCourses));
            //return View(collegeCourses);
        }
    }

    public class Collegecodesreports
    {
        public string CollegeCode { get; set; }
        public int CollegeId { get; set; }
        public string CollegeName { get; set; }
        public int[] ReinspectionIds { get; set; }
        public int[] RecommendationIds { get; set; }
        public int[] AffiliationIds { get; set; }
    }

    public class CollegeAddress
    {
        public int id { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public string EamcetCode { get; set; }
        public string Email { get; set; }
        public string Landline { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string TownorCity { get; set; }
        public string Mandal { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public int Pincode { get; set; }
        public string fax { get; set; }
        public string Mobile { get; set; }
        public bool? Issubmitted { get; set; }
    }

    public class CollegeCourses
    {
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }
        public int DegreeDisplayOrder { get; set; }
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        public int ApprovedIntake { get; set; }
    }

    public class OldColleges
    {
        public int sno { get; set; }
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        public int? YearOfEstablishment { get; set; }
        public int? YearOfClosing { get; set; }
        public string Website { get; set; }
        public string CollegeStatus { get; set; }
    }

    public class TeachingFaculty
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Designation { get; set; }
        public string DepartmentName { get; set; }
        public string OtherDepartment { get; set; }
        public DateTime? DateOfAppointment { get; set; }
        public string Photo { get; set; }

    }
}
