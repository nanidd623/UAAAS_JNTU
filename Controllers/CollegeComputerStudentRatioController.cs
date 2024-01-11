using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using System.Data;
using System.Web.Configuration;
using System.IO;
using System.Data.OleDb;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeComputerStudentRatioController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();


        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int collegeComputerStudentRatioId = db.jntuh_college_computer_student_ratio.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            //int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID && editStatus.academicyearId == prAy &&
                                                                                    editStatus.IsCollegeEditable == true &&
                                                                                    editStatus.editFromDate <= todayDate &&
                                                                                    editStatus.editToDate >= todayDate)
                                                               .Select(editStatus => editStatus.id)
                                                               .FirstOrDefault();

            if (userCollegeID > 0 && collegeComputerStudentRatioId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeComputerStudentRatio");
            }
            if (userCollegeID > 0 && collegeComputerStudentRatioId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeComputerStudentRatio", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (collegeComputerStudentRatioId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CO") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeComputerStudentRatio");
            }

            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<ComputerStudentRatioDetails> computerStudentRatioDetails = new List<ComputerStudentRatioDetails>();
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "MAC-ADDRESS LIST").Select(e => e.id).FirstOrDefault();
            var macfile = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == enclosureId).Select(e => e.path).FirstOrDefault();

            foreach (var item in DegreeIds)
            {
                ComputerStudentRatioDetails details = new ComputerStudentRatioDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.availableComputers = 0;
                details.collegeId = userCollegeID;
                details.MacAddressList = macfile;
                computerStudentRatioDetails.Add(details);
            }

            ViewBag.Count = computerStudentRatioDetails.Count();
            return View(computerStudentRatioDetails);
        }

        private int GetIntake(int degreeId, int collegeId)
        {
            int totalIntake = 0;
            int duration = Convert.ToInt32(db.jntuh_degree.Where(d => d.id == degreeId).Select(d => d.degreeDuration).FirstOrDefault());
            int presentAcademicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.id == presentAcademicYearId).Select(a => a.actualYear).FirstOrDefault();
            int AcademicYearId1 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId2 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId3 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 2)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId4 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 3)).Select(a => a.id).FirstOrDefault();
            int[] specializationsId = (from d in db.jntuh_college_degree
                                       join de in db.jntuh_department on d.degreeId equals de.degreeId
                                       join s in db.jntuh_specialization on de.id equals s.departmentId
                                       join ProposedIntakeExisting in db.jntuh_college_intake_proposed on s.id equals ProposedIntakeExisting.specializationId
                                       where (d.degreeId == degreeId && d.isActive == true && d.collegeId == collegeId && ProposedIntakeExisting.collegeId == collegeId)
                                       select ProposedIntakeExisting.specializationId).Distinct().ToArray();
            //int[] specializations = specializationsId.Distinct().ToArray();
            foreach (var specializationId in specializationsId)
            {
                int totalIntake1 = 0;
                int totalIntake2 = 0;
                int totalIntake3 = 0;
                int totalIntake4 = 0;
                int totalIntake5 = 0;
                int[] shiftId1 = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == specializationId && e.academicYearId == AcademicYearId1).Select(e => e.shiftId).ToArray();

                foreach (var sId1 in shiftId1)
                {
                    totalIntake1 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId1 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake2 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == presentAcademicYearId && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake3 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId2 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake4 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId3 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake5 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId4 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                }

                if (duration >= 5)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4 + totalIntake5;
                }

                if (duration == 4)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4;
                }

                if (duration == 3)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3;
                }

                if (duration == 2)
                {
                    totalIntake += totalIntake1 + totalIntake2;
                }

                if (duration == 1)
                {
                    totalIntake += totalIntake1;
                }
            }

            return totalIntake;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<ComputerStudentRatioDetails> computerStudentRatioDetails, HttpPostedFileBase fileUploader)
        {
            TempData["Error"] = null;
            TempData["Success"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int ay1 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in computerStudentRatioDetails)
                {
                    userCollegeID = item.collegeId;
                }

            }
            SaveComputerStudentRatioDetails(computerStudentRatioDetails, fileUploader);

            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();


            List<ComputerStudentRatioDetails> computerStudentDetails = new List<ComputerStudentRatioDetails>();
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "MAC-ADDRESS LIST").Select(e => e.id).FirstOrDefault();
            var macfile = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == enclosureId && e.academicyearId == ay0).Select(e => e.path).FirstOrDefault();
            var previousmacfile = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == enclosureId && e.academicyearId == ay1).Select(e => e.path).FirstOrDefault();
            foreach (var item in DegreeIds)
            {
                ComputerStudentRatioDetails details = new ComputerStudentRatioDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.collegeId = userCollegeID;
                details.availableComputers = db.jntuh_college_computer_student_ratio.Where(computerStudenRatio => computerStudenRatio.collegeId == userCollegeID &&
                                                                                        computerStudenRatio.degreeId == item)
                                                                                  .Select(computerStudenRatio => computerStudenRatio.availableComputers)
                                                                                  .FirstOrDefault();
                details.MacAddressList = macfile;
                details.PreviousMacAddresspath = previousmacfile;
                computerStudentDetails.Add(details);
            }
            ViewBag.Count = computerStudentDetails.Count();
            return View(computerStudentDetails);
        }

        private void SaveComputerStudentRatioDetails(ICollection<ComputerStudentRatioDetails> computerStudentRatioDetails, HttpPostedFileBase fileUploader)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in computerStudentRatioDetails)
                {
                    userCollegeID = item.collegeId;
                }

            }

            var message = string.Empty;
            string fileName = string.Empty;
            var filepath = string.Empty;
            string macfile = null;
            int ActualCount = 0;
            int rows = 0;
            if (ModelState.IsValid)
            {
                foreach (ComputerStudentRatioDetails item in computerStudentRatioDetails)
                {
                    jntuh_college_computer_student_ratio details = new jntuh_college_computer_student_ratio();
                    details.degreeId = item.degreeId;
                    details.availableComputers = item.availableComputers;
                    details.collegeId = userCollegeID;
                    macfile = item.MacAddressList;
                    int existId = db.jntuh_college_computer_student_ratio.Where(ratio => ratio.collegeId == userCollegeID &&
                                                                                                       ratio.degreeId == item.degreeId).Select(ratio => ratio.id).FirstOrDefault();

                    if (existId == 0)
                    {
                        details.createdBy = userID;
                        details.createdOn = DateTime.Now;
                        db.jntuh_college_computer_student_ratio.Add(details);
                        db.SaveChanges();
                        message = "Save";
                    }
                    else
                    {
                        details.id = existId; ;
                        details.createdOn = db.jntuh_college_computer_student_ratio.Where(d => d.id == existId).Select(d => d.createdOn).FirstOrDefault();
                        details.createdBy = db.jntuh_college_computer_student_ratio.Where(d => d.id == existId).Select(d => d.createdBy).FirstOrDefault();
                        details.updatedBy = userID;
                        details.updatedOn = DateTime.Now;
                        db.Entry(details).State = EntityState.Modified;
                        message = "Update";
                    }
                    ActualCount = ActualCount + item.availableComputers;
                }
                if (message == "Update")
                {
                    rows = db.jntuh_college_macaddress.Where(m => m.collegeId == userCollegeID).Select(s => s).Count();
                    if (rows >= ActualCount || fileUploader != null)
                    {
                        db.SaveChanges();
                    }
                    else
                    {
                        TempData["Error"] = "Your upload MAC-ID's not match with total number of computers";
                        return;
                    }
                }
                //To Save File in jntuh_college_enclosures
                int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "MAC-ADDRESS LIST").Select(e => e.id).FirstOrDefault();

                var college_enclosures = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == ay0).Select(e => e).FirstOrDefault();
                jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                jntuh_college_enclosures.collegeID = userCollegeID;
                jntuh_college_enclosures.academicyearId = ay0;
                jntuh_college_enclosures.enclosureId = enclosureId;
                jntuh_college_enclosures.isActive = true;
                if (fileUploader != null)
                {
                    List<jntuh_college_macaddress> college_macaddress =
                      db.jntuh_college_macaddress.Where(m => m.collegeId == userCollegeID).Select(s => s).ToList();

                    if (college_macaddress.Count() > 0)
                    {
                        db.jntuh_college_macaddress.Where(d => d.collegeId == userCollegeID).ToList().ForEach(d => db.jntuh_college_macaddress.Remove(d));
                        db.SaveChanges();
                    }
                    string ext = Path.GetExtension(fileUploader.FileName);

                    fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "_MAC_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ext;

                    if (!Directory.Exists(Server.MapPath("~/Content/Upload/CollegeEnclosures/MAC")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Upload/CollegeEnclosures/MAC"));
                    }
                    fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/MAC"), fileName));
                    string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path.Combine(Server.MapPath("~/Content/Upload/CollegeEnclosures/MAC"), fileName) + ";Extended Properties=Excel 12.0;Persist Security Info=False";

                    //Create Connection to Excel work book                   
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Close();
                    //Create OleDbCommand to fetch data from Excel
                    excelConnection.Open();
                    OleDbCommand cmd1 = new OleDbCommand("select count(*) from [" + excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["Table_Name"].ToString() + "]", excelConnection);
                    rows = (int)cmd1.ExecuteScalar();
                    OleDbCommand cmd2 = new OleDbCommand("select * from [" + excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["Table_Name"].ToString() + "]", excelConnection);

                    DataTable dt = new DataTable();
                    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    oleda.SelectCommand = cmd2;
                    DataSet ds = new DataSet();
                    oleda.Fill(ds);
                    List<jntuh_college_macaddress> jntuh_college_macaddresslist = new List<jntuh_college_macaddress>();
                    dt = ds.Tables[0];

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(dt.Rows[i][0].ToString()) && !string.IsNullOrEmpty(dt.Rows[i][1].ToString()) && !string.IsNullOrEmpty(dt.Rows[i][2].ToString()) && !string.IsNullOrEmpty(dt.Rows[i][3].ToString()))
                            {
                                jntuh_college_macaddress macaddressdata = new jntuh_college_macaddress();
                                macaddressdata.collegeId = userCollegeID;
                                macaddressdata.macaddress = dt.Rows[i][1].ToString().Trim();
                                macaddressdata.location = dt.Rows[i][2].ToString().Trim();
                                macaddressdata.labname = dt.Rows[i][3].ToString().Trim();
                                macaddressdata.IsActive = true;
                                macaddressdata.createdBy = userCollegeID;
                                macaddressdata.createdOn = DateTime.Now;
                                jntuh_college_macaddresslist.Add(macaddressdata);
                                //db.jntuh_college_macaddress.Add(macaddressdata);
                                continue;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["Error"] = "Your upload MAC-ID's Excel not in Correct Format.";
                            return;
                        }
                    }
                    jntuh_college_macaddresslist.ForEach(d => db.jntuh_college_macaddress.Add(d));
                    db.SaveChanges();
                    excelConnection.Close();
                    if (rows >= ActualCount)
                    {
                        TempData["Success"] = "Rows count:" + rows + "  Actual count:" + ActualCount;
                        message = "Update1";
                        message = "ExcelCout";
                        jntuh_college_enclosures.path = fileName;
                    }

                }
                else if (macfile != null)
                {
                    fileName = macfile;
                    jntuh_college_enclosures.path = macfile;
                    rows = db.jntuh_college_macaddress.Where(m => m.collegeId == userCollegeID).Select(s => s).Count();
                }
                if (rows >= ActualCount)
                {

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
                }
                else
                {
                    List<jntuh_college_macaddress> college_macaddress =
                        db.jntuh_college_macaddress.Where(m => m.collegeId == userCollegeID).Select(s => s).ToList();

                    if (college_macaddress.Count() > 0)
                    {
                        db.jntuh_college_macaddress.Where(d => d.collegeId == userCollegeID).ToList().ForEach(d => db.jntuh_college_macaddress.Remove(d));
                        db.SaveChanges();
                    }
                    TempData["Error"] = "Your upload MAC-ID's not match with total number of computers";
                }

            }


            if (message == "Update")
            {
                //int dbcomputerscount =
                //         db.jntuh_college_computer_student_ratio.Where(a => a.collegeId == userCollegeID).Sum(o => o.availableComputers);
                if (rows >= ActualCount)
                {
                    TempData["Success"] = "Computer Student Ratio Details are Updated successfully";
                }

            }
            else if (message == "Update1")
            {
                //TempData["Success"] = "Computer Student Ratio Details are Updated successfully";
            }
            else
            {
                //int dbcomputerscount =
                //        db.jntuh_college_computer_student_ratio.Where(a => a.collegeId == userCollegeID).Sum(o => o.availableComputers);
                if (rows >= ActualCount)
                {
                    TempData["Success"] = "Computer Student Ratio Details are Updated successfully";
                }
            }

        }

        /// <summary>
        /// Showing MAC Address List to college corresponding there upload
        /// </summary>
        /// <param name="collegeId"></param>
        /// <returns></returns>
        public ActionResult MACAddressView()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            return View();

        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public JsonResult MACJson(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            List<jntuh_college_macaddress> college_macaddress =
                db.jntuh_college_macaddress.Where(m => m.collegeId == userCollegeID).Select(s => s).ToList();

            return Json(college_macaddress.OrderBy(s => s.labname).ToList(), "application/json", JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int ay1 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int collegeComputerStudentRatioId = db.jntuh_college_computer_student_ratio.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (collegeComputerStudentRatioId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeComputerStudentRatio");
            }
            DateTime todayDate = DateTime.Now.Date;
            //int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID && editStatus.academicyearId == prAy &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeComputerStudentRatio");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CO") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeComputerStudentRatio");
                }
            }

            //List<jntuh_college_degree> collegeDess = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && DegreeIds.Contains(d.degreeId) && d.isActive == true).ToList();

            List<ComputerStudentRatioDetails> computerStudentDetails = new List<ComputerStudentRatioDetails>();
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "MAC-ADDRESS LIST").Select(e => e.id).FirstOrDefault();
            var macfile = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == enclosureId && e.academicyearId == ay0).Select(e => e.path).FirstOrDefault();
            var previousmacfile = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == enclosureId && e.academicyearId == ay1).Select(e => e.path).FirstOrDefault();
            foreach (var item in DegreeIds)
            {
                ComputerStudentRatioDetails details = new ComputerStudentRatioDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.collegeId = userCollegeID;
                details.availableComputers = db.jntuh_college_computer_student_ratio.Where(computerStudenRatio => computerStudenRatio.collegeId == userCollegeID &&
                                                                                        computerStudenRatio.degreeId == item)
                                                                                  .Select(computerStudenRatio => computerStudenRatio.availableComputers)
                                                                                  .FirstOrDefault();
                details.MacAddressList = macfile;
                details.PreviousMacAddresspath = previousmacfile;
                computerStudentDetails.Add(details);
            }
            ViewBag.Count = computerStudentDetails.Count();
            ViewBag.Update = true;
            return View("Create", computerStudentDetails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<ComputerStudentRatioDetails> computerStudentRatioDetails, HttpPostedFileBase fileUploader)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int ay1 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in computerStudentRatioDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveComputerStudentRatioDetails(computerStudentRatioDetails, fileUploader);
            int academicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<ComputerStudentRatioDetails> computerStudentDetails = new List<ComputerStudentRatioDetails>();
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "MAC-ADDRESS LIST").Select(e => e.id).FirstOrDefault();
            var macfile = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == enclosureId).Select(e => e.path).FirstOrDefault();
            var previousmacfile = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == enclosureId && e.academicyearId == ay1).Select(e => e.path).FirstOrDefault();
            foreach (var item in DegreeIds)
            {
                ComputerStudentRatioDetails details = new ComputerStudentRatioDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.availableComputers = db.jntuh_college_computer_student_ratio.Where(computerStudenRatio => computerStudenRatio.collegeId == userCollegeID &&
                                                                                        computerStudenRatio.degreeId == item)
                                                                                  .Select(computerStudenRatio => computerStudenRatio.availableComputers)
                                                                                  .FirstOrDefault();
                details.MacAddressList = macfile;
                details.PreviousMacAddresspath = previousmacfile;
                computerStudentDetails.Add(details);
            }
            ViewBag.Count = computerStudentDetails.Count();
            ViewBag.Update = true;
            //return View("Create", computerStudentDetails);
            return View("View");
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int ay1 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int collegeComputerStudentRatioId = db.jntuh_college_computer_student_ratio.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int prAy =
                    db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CO") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<ComputerStudentRatioDetails> computerStudentDetails = new List<ComputerStudentRatioDetails>();
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "MAC-ADDRESS LIST").Select(e => e.id).FirstOrDefault();
            var macfile = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == enclosureId && e.academicyearId == ay0).Select(e => e.path).FirstOrDefault();
            var previousmacfile = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == enclosureId && e.academicyearId == ay1).Select(e => e.path).FirstOrDefault();
            foreach (var item in DegreeIds)
            {
                ComputerStudentRatioDetails details = new ComputerStudentRatioDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.availableComputers = db.jntuh_college_computer_student_ratio.Where(computerStudenRatio => computerStudenRatio.collegeId == userCollegeID &&
                                                                                        computerStudenRatio.degreeId == item)
                                                                                  .Select(computerStudenRatio => computerStudenRatio.availableComputers)
                                                                                  .FirstOrDefault();
                details.MacAddressList = macfile;
                details.PreviousMacAddresspath = previousmacfile;
                computerStudentDetails.Add(details);
            }
            ViewBag.Count = computerStudentDetails.Count();
            if (collegeComputerStudentRatioId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Count = computerStudentDetails.Count();
                ViewBag.Update = true;
            }
            return View("View", computerStudentDetails);
        }

        public ActionResult Delete()
        {
            try
            {
                if (Membership.GetUser() == null)
                {
                    return RedirectToAction("LogOn", "Account");
                }
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 375)
                {
                    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                if (userCollegeID != 0)
                {
                    jntuh_college_enclosures collegeEnclosures =
                        db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == 15 && e.academicyearId == ay0)
                            .Select(s => s)
                            .FirstOrDefault();
                    List<jntuh_college_macaddress> college_macaddress =
                        db.jntuh_college_macaddress.Where(m => m.collegeId == userCollegeID).Select(s => s).ToList();

                    if (collegeEnclosures != null)
                    {
                        db.jntuh_college_enclosures.Remove(collegeEnclosures);
                        db.SaveChanges();
                    }

                    if (college_macaddress.Count() > 0)
                    {
                        db.jntuh_college_macaddress.Where(d => d.collegeId == userCollegeID).ToList().ForEach(d => db.jntuh_college_macaddress.Remove(d));
                        db.SaveChanges();
                        if (collegeEnclosures.path != null)
                        {
                            System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/MAC"), collegeEnclosures.path));
                        }
                        TempData["Status"] = "File Delete Successfuly";
                    }
                    else
                    {
                        TempData["Status"] = "No Data Found";
                    }


                }
            }
            catch (Exception)
            {

                throw;
            }

            return RedirectToAction("Edit", "CollegeComputerStudentRatio");
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int collegeComputerStudentRatioId = db.jntuh_college_computer_student_ratio.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<ComputerStudentRatioDetails> computerStudentDetails = new List<ComputerStudentRatioDetails>();
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "MAC-ADDRESS LIST").Select(e => e.id).FirstOrDefault();
            var macfile = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID && e.enclosureId == enclosureId).Select(e => e.path).FirstOrDefault();

            foreach (var item in DegreeIds)
            {
                ComputerStudentRatioDetails details = new ComputerStudentRatioDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.availableComputers = db.jntuh_college_computer_student_ratio.Where(computerStudenRatio => computerStudenRatio.collegeId == userCollegeID &&
                                                                                        computerStudenRatio.degreeId == item)
                                                                                  .Select(computerStudenRatio => computerStudenRatio.availableComputers)
                                                                                  .FirstOrDefault();
                details.MacAddressList = macfile;

                computerStudentDetails.Add(details);
            }
            ViewBag.Count = computerStudentDetails.Count();
            if (collegeComputerStudentRatioId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Count = computerStudentDetails.Count();
                ViewBag.Update = true;
            }
            return View("UserView", computerStudentDetails);
        }
    }
}
