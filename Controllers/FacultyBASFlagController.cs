using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    public class FacultyBASFlagController : BaseController
    {
        //Written By Siva
        // GET: /BASFlagDeficiencyFaculty/
        uaaasDBContext db = new uaaasDBContext();
          

        #region Code Written By Siva
        [Authorize(Roles = "Admin")]
        public ActionResult BasFlagDeficiencyFacultyData()
        {
            #region CollegeWise BAS Flag Faculty Data  Code Written BY Siva

            //  var collegeids =db.jntuh_college_edit_status.Where(e=>e.IsCollegeEditable==false).Select(e=>e.collegeId).ToList();
            //  //var collegeids =new int[]{ 2, 4, 6, 7, 8,198 };
            //  List<jntuh_college_basreport> basFlagTotal = db.jntuh_college_basreport.Where(e=>collegeids.Contains(e.collegeId)).Select(e => e).ToList();
            //  var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e=>collegeids.Contains(e.collegeId)).Select(e=>e).ToList();
            //  var jntuh_college = db.jntuh_college.Where(e=>e.isActive ==true && collegeids.Contains(e.id)).ToList();
            //  List<BasFlagClass> BasClass = new List<BasFlagClass>();

            //  int days =Convert.ToInt32(ConfigurationManager.AppSettings["Days"]);
            //  int months = Convert.ToInt32(ConfigurationManager.AppSettings["Months"]); ;
            ////  string[] months = { "July", "August", "September", "October", "November", "December", "January", "February", };

            //  foreach (var college in jntuh_college)
            //  {
            //    BasFlagClass BasFacultyClass = new BasFlagClass();
            //      int CollegeWiseFacultyBasCount =0;
            //     var Faculty = jntuh_college_faculty_registered.Where(e=>e.collegeId ==college.id).Select(e=>e).ToList();
            //      foreach (var EachFaculty in Faculty)
            //      {
            //          bool FalgBas =true;
            //          int FacultyBasFlagCount =0;
            //          var BasData = basFlagTotal.Where(e=>e.RegistrationNumber.Trim() == EachFaculty.RegistrationNumber.Trim()).Select(e=>e).ToList();
            //          var FacultyMonthCount = 0;
            //          FacultyMonthCount = BasData.Count();
            //          foreach (var item in BasData)
            //          {
            //              int? totalworkingdays =item.totalworkingDays;
            //              int? totalpresentdays =item.NoofPresentDays;
            //              int? RequiredPresentDays =totalworkingdays-days;
            //              if (totalpresentdays >= RequiredPresentDays)
            //              {
            //                  FacultyBasFlagCount++;
            //              }
            //              else
            //              {

            //              }


            //          }
            //          var ReqiredMonthCount = FacultyMonthCount - months;
            //          if (FacultyBasFlagCount >= ReqiredMonthCount)
            //          {
            //              FalgBas = false;
            //          }
            //          else
            //          {
            //              FalgBas = true;
            //          }
            //          if(FalgBas == true)
            //          {
            //              CollegeWiseFacultyBasCount++;
            //          }

            //      }

            //      BasFacultyClass.CollegeCode = college.collegeCode;
            //      BasFacultyClass.CollegeName = college.collegeName;
            //      BasFacultyClass.TotalTeachingFaculty = Faculty.Count();
            //      BasFacultyClass.BasFlagFacultyCount = CollegeWiseFacultyBasCount;
            //      BasClass.Add(BasFacultyClass);
            //  }
            //  Response.ClearContent();
            //  Response.Buffer = true;
            //  Response.AddHeader("content-disposition", "attachment; filename=BasFaculty.xls");
            //  Response.ContentType = "application/vnd.ms-excel";
            //  return View("~/Views/BASFlagDeficiencyFaculty/_ExcelFileDoenload.cshtml", BasClass.ToList());
            #endregion
            #region CollegeWise Without BAS Flag Faculty Data  Code Written BY Siva

            var collegeids = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).Select(e => e.collegeId).ToList();
            // var collegeids = new int[] { 67 };
            List<jntuh_college_basreport> basFlagTotal = db.jntuh_college_basreport.Where(e => collegeids.Contains(e.collegeId)).Select(e => e).ToList();
            var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => collegeids.Contains(e.collegeId)).Select(e => e).ToList();
            var jntuh_college = db.jntuh_college.Where(e => e.isActive == true && collegeids.Contains(e.id)).ToList();
            List<BasFlagClass> BasClass = new List<BasFlagClass>();

            int days = Convert.ToInt32(ConfigurationManager.AppSettings["Days"]);
            int months = Convert.ToInt32(ConfigurationManager.AppSettings["Months"]); ;
            //  string[] months = { "July", "August", "September", "October", "November", "December", "January", "February", };

            foreach (var college in jntuh_college)
            {
                BasFlagClass BasFacultyClass = new BasFlagClass();
                int CollegeWiseFacultyBasCount = 0;
                var Faculty = jntuh_college_faculty_registered.Where(e => e.collegeId == college.id).Select(e => e).ToList();
                foreach (var EachFaculty in Faculty)
                {
                    bool FalgBas = true;
                    int FacultyBasFlagCount = 0;
                    var BasData = basFlagTotal.Where(e => e.RegistrationNumber.Trim() == EachFaculty.RegistrationNumber.Trim()).Select(e => e).ToList();
                    var FacultyMonthCount = 0;
                    FacultyMonthCount = BasData.Count();
                    foreach (var item in BasData)
                    {
                        int? totalworkingdays = item.totalworkingDays;
                        int? totalpresentdays = item.NoofPresentDays;
                        int? RequiredPresentDays = days;
                        if (totalpresentdays >= RequiredPresentDays)
                        {
                            FacultyBasFlagCount++;
                        }
                        else
                        {

                        }


                    }
                    var ReqiredMonthCount = FacultyMonthCount - months;
                    if (FacultyBasFlagCount >= ReqiredMonthCount)
                    {
                        FalgBas = false;
                    }
                    else
                    {
                        FalgBas = true;
                    }
                    //if (FalgBas == true)
                    //{
                    //    CollegeWiseFacultyBasCount++;
                    //}
                    if (FalgBas == false)
                    {
                        CollegeWiseFacultyBasCount++;
                    }

                }

                BasFacultyClass.CollegeCode = college.collegeCode;
                BasFacultyClass.CollegeName = college.collegeName;
                BasFacultyClass.TotalTeachingFaculty = Faculty.Count();
                BasFacultyClass.BasFlagFacultyCount = CollegeWiseFacultyBasCount;
                BasClass.Add(BasFacultyClass);
            }
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=BasFaculty.xls");
            Response.ContentType = "application/vnd.ms-excel";
            return View("~/Views/BASFlagDeficiencyFaculty/_ExcelFileDoenload.cshtml", BasClass.ToList());
            #endregion
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CollegeWiseBASFlagCleared(int? collegeId, string command)
        {
            //Written By Siva
            var Colleges = db.jntuh_college_edit_status.Join(db.jntuh_college, s => s.collegeId, c => c.id, (s, c) => new { s = s, c = c }).Where(e => e.s.IsCollegeEditable == false && e.c.isActive == true)
                .Select(a => new
                {
                    collegeId = a.c.id,
                    collegeName = a.c.collegeCode + "-" + a.c.collegeName
                }).ToList();

            ViewBag.CollegeList = Colleges;

            List<SelectListItem> MonthNameAndIds = new List<SelectListItem>();
            MonthNameAndIds.Add(new SelectListItem() { Text = "January", Value = "1" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "February", Value = "2" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "March", Value = "3" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "April", Value = "4" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "May", Value = "5" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "June", Value = "6" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "July", Value = "7" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "August", Value = "8" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "September", Value = "9" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "October", Value = "10" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "November", Value = "11" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "December", Value = "12" });



            var SelectedMonths = MonthNameAndIds.Where(e=>e.Text == "July" ||  e.Text == "August" || e.Text == "September" || e.Text == "October" || e.Text == "November" || e.Text == "December").Select(e=>e.Text).ToArray();

            if (collegeId != null)
            {

                #region CollegeWise Without BAS Flag Faculty Data  Code Written BY Siva

                List<jntuh_college_basreport> basFlagTotal = db.jntuh_college_basreport.Where(z => z.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college = db.jntuh_college.FirstOrDefault(e => e.id == collegeId);

                int days = Convert.ToInt32(ConfigurationManager.AppSettings["Days"]);
                int months = Convert.ToInt32(ConfigurationManager.AppSettings["Months"]);
                int MarchDaysCount = Convert.ToInt32(ConfigurationManager.AppSettings["MarchDays"]);

                List<CollegeWiseBasFlagFaculty> CollegeWiseBasFlagFaculty = new List<CollegeWiseBasFlagFaculty>();

                int CollegeWiseFacultyBasCount = 0;
                foreach (var EachFaculty in jntuh_college_faculty_registered)
                {
                    bool FalgBas = true;
                    int FacultyBasFlagCount = 0;
                    var FacultyMonthCount = 0;

                    var BasData = basFlagTotal.Where(e => e.RegistrationNumber.Trim() == EachFaculty.RegistrationNumber.Trim()).Select(e => e).ToList();

                    CollegeWiseBasFlagFaculty BasFacultyClass = new CollegeWiseBasFlagFaculty();
                    BasFacultyClass.CollegeCode = jntuh_college.collegeCode;
                    BasFacultyClass.CollegeName = jntuh_college.collegeName;
                    BasFacultyClass.Registraionnumber = EachFaculty.RegistrationNumber;
                    string date = BasData.Select(e => e.joiningDate).FirstOrDefault().ToString();
                    BasFacultyClass.BASJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");

                    FacultyMonthCount = BasData.Count();

                    var BasDataNew = BasData.Where(m=>SelectedMonths.Contains(m.month)).Select(e=>e.month).Count();
                    var NotRequiredMonths = BasData.Where(m => m.month == "January" || m.month == "February" || m.month == "March").Select(e => e.month).Count();
                    foreach (var item in BasData)
                    {
                        int? totalworkingdays = item.totalworkingDays;
                        int? totalpresentdays = item.NoofPresentDays;
                        int? RequiredPresentDays = days;

                        if(item.month == "March")
                        {
                            if(totalpresentdays >= MarchDaysCount)
                            {
                                BasFacultyClass.March = item.month;
                            }
                        }
                        else if(item.month == "February")
                        {
                            if(BasFacultyClass.BASJoiningDate == "22-02-2018" || BasFacultyClass.BASJoiningDate == "26-02-2018" ||BasFacultyClass.BASJoiningDate == "28-02-2018")
                            {
                                BasFacultyClass.February = item.month;
                               //  FacultyBasFlagCount++;
                            }
                            else
                            {
                                if(totalpresentdays >= RequiredPresentDays)
                                {
                                    BasFacultyClass.February = item.month;
                                   // FacultyBasFlagCount++;
                                }
                            }
                        }
                        else if (totalpresentdays >= RequiredPresentDays)
                        {
                            
                            if (item.month == "July")
                            {
                               
                                BasFacultyClass.July = item.month;
                                FacultyBasFlagCount++;

                            }
                            else if (item.month == "August")
                            {
                                BasFacultyClass.August = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "September")
                            {
                                BasFacultyClass.September = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "October")
                            {
                                BasFacultyClass.October = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "November")
                            {
                                BasFacultyClass.November = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "December")
                            {
                                BasFacultyClass.December = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "January")
                            {
                                BasFacultyClass.January = item.month;
                               // FacultyBasFlagCount++;
                            }
                            //else if (item.month == "February")
                            //{
                            //    BasFacultyClass.February = item.month;
                            //}
                            // else if (item.month == "March")
                            //{
                            //    BasFacultyClass.March = item.month;
                            //}

                        }
                        else
                        {

                        }


                    }

                    if (BasDataNew == 0)
                    {
                        if (NotRequiredMonths == 1)
                        {
                            if (BasFacultyClass.March != null)
                            {
                                FalgBas = false;
                            }
                            else
                            {
                                FalgBas = true;
                            }
                        }
                        else if (NotRequiredMonths == 2)
                        {
                            if (BasFacultyClass.February != null && BasFacultyClass.March != null)
                            {
                                FalgBas = false;
                            }
                            else
                            {
                                FalgBas = true;
                            }
                        }
                        else if (NotRequiredMonths == 3)
                        {
                            if (BasFacultyClass.January != null && BasFacultyClass.February != null && BasFacultyClass.March != null)
                            {
                                FalgBas = false;
                            }
                            else
                            {
                                FalgBas = true;
                            }
                        }
                         
                       
                    }
                    else
                    {
                        var ReqiredMonthCount = BasDataNew - months;
                        if (FacultyBasFlagCount >= ReqiredMonthCount)
                        {
                            if (NotRequiredMonths == 1)
                            {
                                if (BasFacultyClass.March != null)
                                {
                                    FalgBas = false;
                                }
                                else
                                {
                                    FalgBas = true;
                                }
                            }
                            else if (NotRequiredMonths == 2)
                            {
                                if (BasFacultyClass.February != null && BasFacultyClass.March != null)
                                {
                                    FalgBas = false;
                                }
                                else
                                {
                                    FalgBas = true;
                                }
                            }
                            else if (NotRequiredMonths == 3)
                            {
                                if (BasFacultyClass.January != null && BasFacultyClass.February != null && BasFacultyClass.March != null)
                                {
                                    FalgBas = false;
                                }
                                else
                                {
                                    FalgBas = true;
                                }
                            }
                           // FalgBas = false;
                        }
                        else
                        {
                            FalgBas = true;
                        }
                    }

                    if (FalgBas == false)
                    {
                        BasFacultyClass.Cleared = "Cleared";
                        CollegeWiseFacultyBasCount++;
                    }

                    CollegeWiseBasFlagFaculty.Add(BasFacultyClass);
                }


                #endregion
                if (command == "Print")
                {
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename = '" + jntuh_college.collegeCode + "'_CollegeBASFacultyList.xls");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/BASFlagDeficiencyFaculty/_BASFlagFacultyDataExcel.cshtml", CollegeWiseBasFlagFaculty);

                }
                else
                {
                    return View(CollegeWiseBasFlagFaculty);
                }
            }
            else
            {
                return View();
            }

        }

        [Authorize(Roles = "Admin")]
        public ActionResult AllCollegesBASFlagNotClearedFaculty(string command)
        {
            //Written By Siva
            #region Old Code
            //var collegeids = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).Select(e => e.collegeId).ToList();
            //// var collegeids = new int[] { 4 };
            //List<jntuh_college_basreport> basFlagTotal = db.jntuh_college_basreport.Where(e => collegeids.Contains(e.collegeId)).Select(e => e).ToList();
            //var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => collegeids.Contains(e.collegeId)).Select(e => e).ToList();
            //var jntuh_college = db.jntuh_college.Where(e => e.isActive == true && collegeids.Contains(e.id)).ToList();

            //int days = Convert.ToInt32(ConfigurationManager.AppSettings["Days"]);
            //int months = Convert.ToInt32(ConfigurationManager.AppSettings["Months"]);

            //List<CollegeWiseBasFlagFaculty> CollegeWiseBasFlagFaculty = new List<CollegeWiseBasFlagFaculty>();

            //foreach (var college in jntuh_college)
            //{

            //    int CollegeWiseFacultyBasCount = 0;
            //    var Faculty = jntuh_college_faculty_registered.Where(e => e.collegeId == college.id).Select(e => e).ToList();
            //    foreach (var EachFaculty in Faculty)
            //    {

            //        var BasData = basFlagTotal.Where(e => e.RegistrationNumber.Trim() == EachFaculty.RegistrationNumber.Trim()).Select(e => e).ToList();

            //        CollegeWiseBasFlagFaculty BasFacultyClass = new CollegeWiseBasFlagFaculty();
            //        BasFacultyClass.CollegeCode = college.collegeCode;
            //        BasFacultyClass.CollegeName = college.collegeName;
            //        BasFacultyClass.Registraionnumber = EachFaculty.RegistrationNumber;
            //        string date = BasData.Select(e => e.joiningDate).FirstOrDefault().ToString();
            //        if (BasData.Count == 0)
            //        {

            //        }
            //        else
            //        {
            //            BasFacultyClass.BASJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
            //        }




            //        bool FalgBas = true;
            //        int FacultyBasFlagCount = 0;
            //        var FacultyMonthCount = 0;
            //        FacultyMonthCount = BasData.Count();
            //        foreach (var item in BasData)
            //        {
            //            int? totalworkingdays = item.totalworkingDays;
            //            int? totalpresentdays = item.NoofPresentDays;
            //            int? RequiredPresentDays = days;
            //            if (totalpresentdays >= RequiredPresentDays)
            //            {
            //                FacultyBasFlagCount++;
            //                if (item.month == "July")
            //                {
            //                    BasFacultyClass.July = item.month;

            //                }
            //                else if (item.month == "August")
            //                {
            //                    BasFacultyClass.August = item.month;
            //                }
            //                else if (item.month == "September")
            //                {
            //                    BasFacultyClass.September = item.month;
            //                }
            //                else if (item.month == "October")
            //                {
            //                    BasFacultyClass.October = item.month;
            //                }
            //                else if (item.month == "November")
            //                {
            //                    BasFacultyClass.November = item.month;
            //                }
            //                else if (item.month == "December")
            //                {
            //                    BasFacultyClass.December = item.month;
            //                }
            //                else if (item.month == "January")
            //                {
            //                    BasFacultyClass.January = item.month;
            //                }
            //                else if (item.month == "February")
            //                {
            //                    BasFacultyClass.February = item.month;
            //                }
            //                //else if (item.month == "March")
            //                //{
            //                //    BasFacultyClass.February = item.month;
            //                //}
            //            }
            //            else
            //            {

            //            }


            //        }
            //        var ReqiredMonthCount = FacultyMonthCount - months;
            //        if (FacultyBasFlagCount >= ReqiredMonthCount)
            //        {
            //            FalgBas = false;
            //        }
            //        else
            //        {
            //            FalgBas = true;
            //        }

            //        if (FalgBas == false)
            //        {
            //            BasFacultyClass.Cleared = "Cleared";
            //            CollegeWiseFacultyBasCount++;
            //        }
            //        else
            //        {
            //            BasFacultyClass.Cleared = "NotCleared";
            //        }
            //        CollegeWiseBasFlagFaculty.Add(BasFacultyClass);
            //    }

            //}
            //if (command == "Print")
            //{
            //    Response.ClearContent();
            //    Response.Buffer = true;
            //    Response.AddHeader("content-disposition", "attachment; filename =CollegeWiseBASFlagNotFacultyList.xls");
            //    Response.ContentType = "application/vnd.ms-excel";
            //    return PartialView("~/Views/BASFlagDeficiencyFaculty/_BASFlagFacultyDataExcel.cshtml", CollegeWiseBasFlagFaculty.Where(e => e.Cleared == "NotCleared").ToList());
            //}
            //else
            //{
            //    return View(CollegeWiseBasFlagFaculty.Where(e => e.Cleared == "NotCleared").ToList());
            //}
            #endregion

            List<SelectListItem> MonthNameAndIds = new List<SelectListItem>();
            MonthNameAndIds.Add(new SelectListItem() { Text = "January", Value = "1" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "February", Value = "2" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "March", Value = "3" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "April", Value = "4" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "May", Value = "5" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "June", Value = "6" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "July", Value = "7" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "August", Value = "8" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "September", Value = "9" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "October", Value = "10" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "November", Value = "11" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "December", Value = "12" });

            var collegeids = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).Select(e => e.collegeId).ToList();
            // var collegeids = new int[] { 4 };
            List<jntuh_college_basreport> basFlagTotal = db.jntuh_college_basreport.Where(e => collegeids.Contains(e.collegeId)&&(e.year==2018||e.year==2019)).Select(e => e).ToList();
            var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => collegeids.Contains(e.collegeId)).Select(e => e).ToList();
            var jntuh_college = db.jntuh_college.Where(e => e.isActive == true && collegeids.Contains(e.id)).ToList();

            var SelectedMonths = MonthNameAndIds.Where(e => e.Text == "July" || e.Text == "August" || e.Text == "September" || e.Text == "October" || e.Text == "November" || e.Text == "December").Select(e => e.Text).ToArray();

            int days = Convert.ToInt32(ConfigurationManager.AppSettings["Days"]);
            int months = Convert.ToInt32(ConfigurationManager.AppSettings["Months"]);
            int MarchDaysCount = Convert.ToInt32(ConfigurationManager.AppSettings["MarchDays"]);

            List<CollegeWiseBasFlagFaculty> CollegeWiseBasFlagFaculty = new List<CollegeWiseBasFlagFaculty>();

            foreach (var college in jntuh_college)
            {

                int CollegeWiseFacultyBasCount = 0;
                var Faculty = jntuh_college_faculty_registered.Where(e => e.collegeId == college.id).Select(e => e).ToList();
                foreach (var EachFaculty in Faculty)
                {

                    var BasData = basFlagTotal.Where(e => e.RegistrationNumber.Trim() == EachFaculty.RegistrationNumber.Trim()).Select(e => e).ToList();

                    CollegeWiseBasFlagFaculty BasFacultyClass = new CollegeWiseBasFlagFaculty();
                    BasFacultyClass.CollegeCode = college.collegeCode;
                    BasFacultyClass.CollegeName = college.collegeName;
                    BasFacultyClass.Registraionnumber = EachFaculty.RegistrationNumber;
                    string date = BasData.Select(e => e.joiningDate).FirstOrDefault().ToString();
                    if (BasData.Count == 0)
                    {

                    }
                    else
                    {
                        BasFacultyClass.BASJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                    }




                    bool FalgBas = true;
                    int FacultyBasFlagCount = 0;
                    var FacultyMonthCount = 0;
                    FacultyMonthCount = BasData.Count();

                    var BasDataNew = BasData.Where(m => SelectedMonths.Contains(m.month)).Select(e => e.month).Count();
                    var NotRequiredMonths = BasData.Where(m => m.month == "January" || m.month == "February" || m.month == "March").Select(e => e.month).Count();

                    foreach (var item in BasData)
                    {
                        int? totalworkingdays = item.totalworkingDays;
                        int? totalpresentdays = item.NoofPresentDays;
                        int? RequiredPresentDays = days;

                        if (item.month == "March" && item.year == 2019)
                        {
                            if (totalpresentdays >= MarchDaysCount)
                            {
                                BasFacultyClass.March = item.month;
                            }
                        }
                        else if (item.month == "February" && item.year == 2019)
                        {
                            //if (BasFacultyClass.BASJoiningDate == "22-02-2018" || BasFacultyClass.BASJoiningDate == "26-02-2018" || BasFacultyClass.BASJoiningDate == "28-02-2018")
                            if (BasFacultyClass.BASJoiningDate == "22-02-2019" || BasFacultyClass.BASJoiningDate == "26-02-2019" || BasFacultyClass.BASJoiningDate == "28-02-2019")
                            {
                                BasFacultyClass.February = item.month;
                            }
                            else
                            {
                                if (totalpresentdays >= RequiredPresentDays)
                                {
                                    BasFacultyClass.February = item.month;
                                }
                            }
                        }
                        else if (totalpresentdays >= RequiredPresentDays)
                        {

                            if (item.month == "July"&&item.year==2018)
                            {

                                BasFacultyClass.July = item.month;
                                FacultyBasFlagCount++;

                            }
                            else if (item.month == "August" && item.year == 2018)
                            {
                                BasFacultyClass.August = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "September" && item.year == 2018)
                            {
                                BasFacultyClass.September = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "October" && item.year == 2018)
                            {
                                BasFacultyClass.October = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "November" && item.year == 2018)
                            {
                                BasFacultyClass.November = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "December" && item.year == 2018)
                            {
                                BasFacultyClass.December = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "January" && item.year == 2019)
                            {
                                BasFacultyClass.January = item.month;
                            }
                            //else if (item.month == "February")
                            //{
                            //    BasFacultyClass.February = item.month;
                            //}
                            // else if (item.month == "March")
                            //{
                            //    BasFacultyClass.March = item.month;
                            //}

                        }
                        else
                        {

                        }


                    }

                    if (BasDataNew == 0)
                    {
                        if (NotRequiredMonths == 1)
                        {
                            if (BasFacultyClass.March != null)
                            {
                                FalgBas = false;
                            }
                            else
                            {
                                FalgBas = true;
                            }
                        }
                        else if (NotRequiredMonths == 2)
                        {
                            if (BasFacultyClass.February != null && BasFacultyClass.March != null)
                            {
                                FalgBas = false;
                            }
                            else
                            {
                                FalgBas = true;
                            }
                        }
                        else if (NotRequiredMonths == 3)
                        {
                            if (BasFacultyClass.January != null && BasFacultyClass.February != null && BasFacultyClass.March != null)
                            {
                                FalgBas = false;
                            }
                            else
                            {
                                FalgBas = true;
                            }
                        }


                    }
                    else
                    {
                        var ReqiredMonthCount = BasDataNew - months;
                        if (FacultyBasFlagCount >= ReqiredMonthCount)
                        {
                            if (NotRequiredMonths == 1)
                            {
                                if (BasFacultyClass.March != null)
                                {
                                    FalgBas = false;
                                }
                                else
                                {
                                    FalgBas = true;
                                }
                            }
                            else if (NotRequiredMonths == 2)
                            {
                                if (BasFacultyClass.February != null && BasFacultyClass.March != null)
                                {
                                    FalgBas = false;
                                }
                                else
                                {
                                    FalgBas = true;
                                }
                            }
                            else if (NotRequiredMonths == 3)
                            {
                                if (BasFacultyClass.January != null && BasFacultyClass.February != null && BasFacultyClass.March != null)
                                {
                                    FalgBas = false;
                                }
                                else
                                {
                                    FalgBas = true;
                                }
                            }
                            // FalgBas = false;
                        }
                        else
                        {
                            FalgBas = true;
                        }
                    }

                    if (FalgBas == false)
                    {
                        BasFacultyClass.Cleared = "Cleared";
                        CollegeWiseFacultyBasCount++;
                    }
                    else
                    {
                        BasFacultyClass.Cleared = "NotCleared";
                    }
                    CollegeWiseBasFlagFaculty.Add(BasFacultyClass);
                }
            }
            if (command == "Print")
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename =CollegeWiseBASFlagNotFacultyList.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/BASFlagDeficiencyFaculty/_BASFlagFacultyDataExcel.cshtml", CollegeWiseBasFlagFaculty.Where(e => e.Cleared == "NotCleared").ToList());
            }
            else
            {
                return View(CollegeWiseBasFlagFaculty.Where(e => e.Cleared == "NotCleared").ToList());
            }

        }

        [Authorize(Roles="Admin")]
        public ActionResult AllCollegesPrinicipalsBASFlag(string command)
        {
            var CollegeIds = db.jntuh_college_edit_status.Where(s => s.IsCollegeEditable == false).Select(e => e.collegeId).ToList();
            var Colleges = db.jntuh_college.Where(s => CollegeIds.Contains(s.id) && s.isActive == true).Select(e => e).ToList();

            var Prinicipals = db.jntuh_college_principal_registered.Where(e => CollegeIds.Contains(e.collegeId)).Select(e => e).ToList();
            var PrinicipalRegnos = Prinicipals.Select(e => e.RegistrationNumber).ToArray();
            var PrinicipalRegisteredData = db.jntuh_registered_faculty.Where(f => PrinicipalRegnos.Contains(f.RegistrationNumber)).Select(e => e).ToList();

            var jntuh_college_basreport = db.jntuh_college_basreport.Where(e => PrinicipalRegnos.Contains(e.RegistrationNumber)).Select(e => e).ToList();

            List<SelectListItem> MonthNameAndIds = new List<SelectListItem>();
            MonthNameAndIds.Add(new SelectListItem() { Text = "January", Value = "1" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "February", Value = "2" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "March", Value = "3" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "April", Value = "4" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "May", Value = "5" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "June", Value = "6" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "July", Value = "7" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "August", Value = "8" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "September", Value = "9" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "October", Value = "10" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "November", Value = "11" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "December", Value = "12" });



            var SelectedMonths = MonthNameAndIds.Where(e => e.Text == "July" || e.Text == "August" || e.Text == "September" || e.Text == "October" || e.Text == "November" || e.Text == "December").Select(e => e.Text).ToArray();
            
            int days = Convert.ToInt32(ConfigurationManager.AppSettings["Days"]);
            int months = Convert.ToInt32(ConfigurationManager.AppSettings["Months"]);
            int MarchDaysCount = Convert.ToInt32(ConfigurationManager.AppSettings["MarchDays"]);

            List<CollegeWiseBasFlagFacultyNew> CollegeWiseBasFlagFaculty = new List<CollegeWiseBasFlagFacultyNew>();

            foreach (var Prinicipal in Prinicipals)
            {
                   bool FalgBas = true;
                   int FacultyBasFlagCount = 0;
                   var FacultyMonthCount = 0;

                  var BasData = jntuh_college_basreport.Where(e => e.RegistrationNumber.Trim() == Prinicipal.RegistrationNumber.Trim()).Select(e => e).ToList();

                  CollegeWiseBasFlagFacultyNew BasFacultyClass = new CollegeWiseBasFlagFacultyNew();
                    BasFacultyClass.CollegeCode = Colleges.Where(e=>e.id == Prinicipal.collegeId).Select(e=>e.collegeCode).FirstOrDefault();
                    BasFacultyClass.CollegeName = Colleges.Where(e => e.id == Prinicipal.collegeId).Select(e => e.collegeName).FirstOrDefault();
                    BasFacultyClass.Registraionnumber = Prinicipal.RegistrationNumber;
                    string date = BasData.Select(e => e.joiningDate).FirstOrDefault().ToString();
                    if (BasData.Count == 0)
                    {

                    }
                    else
                    {
                        BasFacultyClass.BASJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                    }

                    FacultyMonthCount = BasData.Count();

                    var BasDataNew = BasData.Where(m=>SelectedMonths.Contains(m.month)).Select(e=>e.month).Count();
                    var NotRequiredMonths = BasData.Where(m => m.month == "January" || m.month == "February" || m.month == "March").Select(e => e.month).Count();
                    foreach (var item in BasData)
                    {
                        int? totalworkingdays = item.totalworkingDays;
                        int? totalpresentdays = item.NoofPresentDays;
                        int? RequiredPresentDays = days;

                        if (item.month == "July")
                        {
                            BasFacultyClass.JulyPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "August")
                        {
                            BasFacultyClass.AugustPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "September")
                        {
                            BasFacultyClass.SeptemberPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "October")
                        {
                            BasFacultyClass.OctoberPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "November")
                        {
                            BasFacultyClass.NovemberPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "December")
                        {
                            BasFacultyClass.DecemberPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "January")
                        {
                            BasFacultyClass.JanuaryPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "February")
                        {
                            BasFacultyClass.FebruaryPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "March")
                        {
                            BasFacultyClass.MarchPresentDays = item.NoofPresentDays;
                        }


                        if(item.month == "March")
                        {
                            if(totalpresentdays >= MarchDaysCount)
                            {
                                BasFacultyClass.March = item.month;
                            }
                        }
                        else if(item.month == "February")
                        {
                            if(BasFacultyClass.BASJoiningDate == "22-02-2018" || BasFacultyClass.BASJoiningDate == "26-02-2018" ||BasFacultyClass.BASJoiningDate == "28-02-2018")
                            {
                                BasFacultyClass.February = item.month;
                            }
                            else
                            {
                                if(totalpresentdays >= RequiredPresentDays)
                                {
                                    BasFacultyClass.February = item.month;
                                }
                            }
                        }
                        else if (totalpresentdays >= RequiredPresentDays)
                        {
                            
                            if (item.month == "July")
                            {
                               
                                BasFacultyClass.July = item.month;
                                FacultyBasFlagCount++;

                            }
                            else if (item.month == "August")
                            {
                                BasFacultyClass.August = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "September")
                            {
                                BasFacultyClass.September = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "October")
                            {
                                BasFacultyClass.October = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "November")
                            {
                                BasFacultyClass.November = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "December")
                            {
                                BasFacultyClass.December = item.month;
                                FacultyBasFlagCount++;
                            }
                            else if (item.month == "January")
                            {
                                BasFacultyClass.January = item.month;
                            }
                            //else if (item.month == "February")
                            //{
                            //    BasFacultyClass.February = item.month;
                            //}
                            // else if (item.month == "March")
                            //{
                            //    BasFacultyClass.March = item.month;
                            //}

                        }
                        else
                        {

                        }


                    }

                    if (BasDataNew == 0)
                    {
                        if (NotRequiredMonths == 1)
                        {
                            if (BasFacultyClass.March != null)
                            {
                                FalgBas = false;
                            }
                            else
                            {
                                FalgBas = true;
                            }
                        }
                        else if (NotRequiredMonths == 2)
                        {
                            if (BasFacultyClass.February != null && BasFacultyClass.March != null)
                            {
                                FalgBas = false;
                            }
                            else
                            {
                                FalgBas = true;
                            }
                        }
                        else if (NotRequiredMonths == 3)
                        {
                            if (BasFacultyClass.January != null && BasFacultyClass.February != null && BasFacultyClass.March != null)
                            {
                                FalgBas = false;
                            }
                            else
                            {
                                FalgBas = true;
                            }
                        }
                         
                       
                    }
                    else
                    {
                        var ReqiredMonthCount = BasDataNew - months;
                        if (FacultyBasFlagCount >= ReqiredMonthCount)
                        {
                            if (NotRequiredMonths == 1)
                            {
                                if (BasFacultyClass.March != null)
                                {
                                    FalgBas = false;
                                }
                                else
                                {
                                    FalgBas = true;
                                }
                            }
                            else if (NotRequiredMonths == 2)
                            {
                                if (BasFacultyClass.February != null && BasFacultyClass.March != null)
                                {
                                    FalgBas = false;
                                }
                                else
                                {
                                    FalgBas = true;
                                }
                            }
                            else if (NotRequiredMonths == 3)
                            {
                                if (BasFacultyClass.January != null && BasFacultyClass.February != null && BasFacultyClass.March != null)
                                {
                                    FalgBas = false;
                                }
                                else
                                {
                                    FalgBas = true;
                                }
                            }
                           // FalgBas = false;
                        }
                        else
                        {
                            FalgBas = true;
                        }
                    }

                    if (FalgBas == false)
                    {
                        BasFacultyClass.Cleared = "Cleared";
                       // CollegeWiseFacultyBasCount++;
                    }

                    CollegeWiseBasFlagFaculty.Add(BasFacultyClass);
                }
            if (command == "Print")
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename =AllCollegesPrinicipalsBASFlag.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/BASFlagDeficiencyFaculty/_PrinicipalsBASFlagView.cshtml", CollegeWiseBasFlagFaculty);

            }
            else
            {
                return View(CollegeWiseBasFlagFaculty);
            }


           
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AllCollegesBASFlagFacultyCount(string command)
        {
            //Written By Siva
            var collegeids = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).Select(e => e.collegeId).ToList();
            // var collegeids = new int[] { 4 };
            List<jntuh_college_basreport> basFlagTotal = db.jntuh_college_basreport.Where(e => collegeids.Contains(e.collegeId)).Select(e => e).ToList();
            var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => collegeids.Contains(e.collegeId)).Select(e => e).ToList();
            var jntuh_college = db.jntuh_college.Where(e => e.isActive == true && collegeids.Contains(e.id)).ToList();

            int days = Convert.ToInt32(ConfigurationManager.AppSettings["Days"]);
            int months = Convert.ToInt32(ConfigurationManager.AppSettings["Months"]);

            List<CollegeWiseBasFlagFaculty> CollegeWiseBasFlagFaculty = new List<CollegeWiseBasFlagFaculty>();

            foreach (var college in jntuh_college)
            {

                int CollegeWiseFacultyBasCount = 0;
                var Faculty = jntuh_college_faculty_registered.Where(e => e.collegeId == college.id).Select(e => e).ToList();
                foreach (var EachFaculty in Faculty)
                {

                    var BasData = basFlagTotal.Where(e => e.RegistrationNumber.Trim() == EachFaculty.RegistrationNumber.Trim()).Select(e => e).ToList();

                    CollegeWiseBasFlagFaculty BasFacultyClass = new CollegeWiseBasFlagFaculty();
                    BasFacultyClass.CollegeCode = college.collegeCode;
                    BasFacultyClass.CollegeName = college.collegeName;
                    BasFacultyClass.Registraionnumber = EachFaculty.RegistrationNumber;
                    string date = BasData.Select(e => e.joiningDate).FirstOrDefault().ToString();
                    if (BasData.Count == 0)
                    {

                    }
                    else
                    {
                        BasFacultyClass.BASJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                    }




                    bool FalgBas = true;
                    int FacultyBasFlagCount = 0;
                    var FacultyMonthCount = 0;
                    FacultyMonthCount = BasData.Count();
                    foreach (var item in BasData)
                    {
                        int? totalworkingdays = item.totalworkingDays;
                        int? totalpresentdays = item.NoofPresentDays;
                        int? RequiredPresentDays = days;
                        if (totalpresentdays >= RequiredPresentDays)
                        {
                            FacultyBasFlagCount++;
                            if (item.month == "July")
                            {
                                BasFacultyClass.July = item.month;

                            }
                            else if (item.month == "August")
                            {
                                BasFacultyClass.August = item.month;
                            }
                            else if (item.month == "September")
                            {
                                BasFacultyClass.September = item.month;
                            }
                            else if (item.month == "October")
                            {
                                BasFacultyClass.October = item.month;
                            }
                            else if (item.month == "November")
                            {
                                BasFacultyClass.November = item.month;
                            }
                            else if (item.month == "December")
                            {
                                BasFacultyClass.December = item.month;
                            }
                            else if (item.month == "January")
                            {
                                BasFacultyClass.January = item.month;
                            }
                            else if (item.month == "February")
                            {
                                BasFacultyClass.February = item.month;
                            }

                        }
                        else
                        {

                        }


                    }
                    var ReqiredMonthCount = FacultyMonthCount - months;
                    if (FacultyBasFlagCount >= ReqiredMonthCount)
                    {
                        FalgBas = false;
                    }
                    else
                    {
                        FalgBas = true;
                    }

                    if (FalgBas == false)
                    {
                        BasFacultyClass.Cleared = "Cleared";
                        CollegeWiseFacultyBasCount++;
                    }
                    else
                    {
                        BasFacultyClass.Cleared = "NotCleared";
                    }
                    CollegeWiseBasFlagFaculty.Add(BasFacultyClass);
                }

            }
            var CollegeWiseFacultyCount = CollegeWiseBasFlagFaculty.AsEnumerable().GroupBy(e => new { e.CollegeCode }).Select(e => e.First()).ToList();

            List<BasFlagClass> FacultyList = new List<BasFlagClass>();
            foreach (var item in CollegeWiseFacultyCount)
            {
                BasFlagClass obj = new BasFlagClass();
                obj.CollegeCode = item.CollegeCode;
                obj.CollegeName = item.CollegeName;
                obj.TotalTeachingFaculty = CollegeWiseBasFlagFaculty.Where(e => e.CollegeCode == item.CollegeCode).Select(e => e.Registraionnumber).Count(); ;
                obj.BasFlagFacultyCount = CollegeWiseBasFlagFaculty.Where(e => e.CollegeCode == item.CollegeCode && e.Cleared == "NotCleared").Select(e => e.Cleared).Count();
                FacultyList.Add(obj);
            }

            if (command == "Print")
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename =CollegeWiseBASFlagNotFacultyList.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/BASFlagDeficiencyFaculty/_BASFlagFacultyDataExcel.cshtml", FacultyList);
            }
            else
            {
                return View(FacultyList);
            }

        }

        public class BasFlagClass
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public string Registraionnumber { get; set; }
            public int TotalTeachingFaculty { get; set; }
            public int BasFlagFacultyCount { get; set; }
            public int Count { get; set; }

        }

        public class CollegeWiseBasFlagFaculty
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public string Registraionnumber { get; set; }
            public string BASJoiningDate { get; set; }
            public string July { get; set; }
            public string August { get; set; }
            public string September { get; set; }
            public string October { get; set; }
            public string November { get; set; }
            public string December { get; set; }
            public string January { get; set; }
            public string February { get; set; }
            public string March { get; set; }
            public string Cleared { get; set; }
        }

        public class CollegeWiseBasFlagFacultyNew
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public string Registraionnumber { get; set; }
            public string Name { get; set; }
            public string Mobile { get; set; }
            public string Email { get; set; }
            public string Department { get; set; }
            public string BASJoiningDate { get; set; }
            public int? TotalWorkingDays { get; set; }
            public int? TotalPresentDays { get; set; }
            public string July { get; set; }
            public int? JulyPresentDays { get; set; }
            public string August { get; set; }
            public int? AugustPresentDays { get; set; }
            public string September { get; set; }
            public int? SeptemberPresentDays { get; set; }
            public string October { get; set; }
            public int? OctoberPresentDays { get; set; }
            public string November { get; set; }
            public int? NovemberPresentDays { get; set; }
            public string December { get; set; }
            public int? DecemberPresentDays { get; set; }
            public string January { get; set; }
            public int? JanuaryPresentDays { get; set; }
            public string February { get; set; }
            public int? FebruaryPresentDays { get; set; }
            public string March { get; set; }
            public int? MarchPresentDays { get; set; }
            public string Cleared { get; set; }
        }

        #region Appeal Reverification Bas Flag Faculty
        
        public ActionResult AppealFacultyVerification(int? collegeId)
        {
            var AppealcollegesIds = db.jntuh_appeal_faculty_registered.Where(e => e.NOtificationReport == null).Select(e => e.collegeId).Distinct().ToList();
            var Appealcolleges = db.jntuh_college.Where(e => AppealcollegesIds.Contains(e.id)).Select(e => e).ToList();
            var colleges = Appealcolleges.Select(e => new
            {
                collegeId = e.id,
                collegeName = e.collegeCode +"-"+e.collegeName
            }).ToList();
            ViewBag.colleges = colleges;

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if(collegeId != null)
            {
               
                var appeal_reverification_Faculty = db.jntuh_appeal_faculty_registered.Where(e =>e.collegeId == collegeId && e.AppealReverificationSupportingDocument != null).Select(e => e).ToList();

                var AppealRegnos = appeal_reverification_Faculty.Select(e => e.RegistrationNumber).ToList();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeId).Select(P => P.RegistrationNumber.Trim()).ToArray();
                var jntuh_department = db.jntuh_department.Where(e=>e.isActive ==true).Select(e=>e).ToList();
                var jntuh_specialization = db.jntuh_specialization.Where(e=>e.isActive ==true).Select(e=>e).ToList();

                var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(r => AppealRegnos.Contains(r.RegistrationNumber)).Select(e => e).ToList();

                foreach (var a in jntuh_registered_faculty)
                {
                    string Reason = null;
                    FacultyRegistration faculty = new FacultyRegistration();
                    faculty.RegistrationNumber = a.RegistrationNumber;
                    faculty.FirstName = a.FirstName;
                    faculty.MiddleName = a.MiddleName;
                    faculty.LastName = a.LastName;
                    faculty.DepartmentId = appeal_reverification_Faculty.Where(e => e.RegistrationNumber == faculty.RegistrationNumber).Select(e=>e.DepartmentId).FirstOrDefault();
                    faculty.department = faculty.DepartmentId != null ? jntuh_department.Where(e => e.id == faculty.DepartmentId).Select(e => e.departmentName).FirstOrDefault() : null;
                    faculty.SpecializationId = appeal_reverification_Faculty.Where(e => e.RegistrationNumber == faculty.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    faculty.SpecializationName = faculty.SpecializationId != null ? jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() : null;
                    faculty.IdentfiedFor = appeal_reverification_Faculty.Where(e => e.RegistrationNumber == faculty.RegistrationNumber).Select(e => e.IdentifiedFor).FirstOrDefault();
                    faculty.IncomeTaxFileview = appeal_reverification_Faculty.Where(e => e.RegistrationNumber == faculty.RegistrationNumber).Select(e => e.AppealReverificationSupportingDocument).FirstOrDefault();
                    if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                        faculty.Principal = "Principal";
                    else
                        faculty.Principal = "";

                    //Faculty Flags
                    faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                    faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                    faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false;
                    faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false;
                    faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                    //faculty.MultipleReginSamecoll = a.MultipleRegInSameCollege != null ? (bool)a.MultipleRegInSameCollege : false;
                    faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null ? (bool)a.Xeroxcopyofcertificates : false;
                    faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false;
                    faculty.NOrelevantUgFlag = a.NoRelevantUG == "Yes" ? true : false;
                    faculty.NOrelevantPgFlag = a.NoRelevantPG == "Yes" ? true : false;
                    faculty.NOrelevantPhdFlag = a.NORelevantPHD == "Yes" ? true : false;
                    //faculty.NoForm16Verification = a.Noform16Verification != null ? (bool)a.Noform16Verification : false;
                    faculty.NoSCM17Flag = a.NoSCM != null ? (bool)a.NoSCM : false;
                    //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false;
                    faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null ? (bool)(a.PhdUndertakingDocumentstatus) : false;
                    faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                    faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                    //faculty.AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false;
                    //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null ? (bool)(a.SamePANUsedByMultipleFaculty) : false;
                    //faculty.BasstatusOld = a.BASStatusOld;
                    //Basstatus Column Consider as Aadhaar Flag 
                    //faculty.Basstatus = a.BASStatus;
                    faculty.Deactivedby = a.DeactivatedBy;
                    faculty.DeactivedOn = a.DeactivatedOn;
                    faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD == true ? true : false;
                    faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG == true ? true : false;


                    if (faculty.Absent == true)
                        Reason += "Absent";

                    if (faculty.Type == "Adjunct")
                    {
                        if (Reason != null)
                            Reason += ",Adjunct Faculty";
                        else
                            Reason += "Adjunct Faculty";
                    }

                    if (faculty.XeroxcopyofcertificatesFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Xerox copyof certificates";
                        else
                            Reason += "Xerox copyof certificates";
                    }

                    if (faculty.NOrelevantUgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant UG";
                        else
                            Reason += "NO Relevant UG";
                    }

                    if (faculty.NOrelevantPgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PG";
                        else
                            Reason += "NO Relevant PG";
                    }

                    if (faculty.NOrelevantPhdFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PHD";
                        else
                            Reason += "NO Relevant PHD";
                    }

                    if (faculty.NOTQualifiedAsPerAICTE == true)
                    {
                        if (Reason != null)
                            Reason += ",NOT Qualified AsPerAICTE";
                        else
                            Reason += "NOT Qualified AsPerAICTE";
                    }

                    if (faculty.InvalidPANNo == true)
                    {
                        if (Reason != null)
                            Reason += ",InvalidPANNumber";
                        else
                            Reason += "InvalidPANNumber";
                    }

                    if (faculty.InCompleteCeritificates == true)
                    {
                        if (Reason != null)
                            Reason += ",InComplete Ceritificates";
                        else
                            Reason += "InComplete Ceritificates";
                    }

                    if (faculty.NoSCM == true)
                    {
                        if (Reason != null)
                            Reason += ",NoSCM";
                        else
                            Reason += "NoSCM";
                    }

                    if (faculty.OriginalCertificatesnotshownFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Original Certificates notshown";
                        else
                            Reason += "Original Certificates notshown";
                    }

                    if (faculty.PANNumber == null)
                    {
                        if (Reason != null)
                            Reason += ",No PANNumber";
                        else
                            Reason += "No PANNumber";
                    }

                    if (faculty.NotIdentityFiedForAnyProgramFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NotIdentityFied ForAnyProgram";
                        else
                            Reason += "NotIdentityFied ForAnyProgram";
                    }

                    if (faculty.SamePANUsedByMultipleFaculty == true)
                    {
                        if (Reason != null)
                            Reason += ",SamePANUsedByMultipleFaculty";
                        else
                            Reason += "SamePANUsedByMultipleFaculty";
                    }

                    if (faculty.Basstatus == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",No/Invalid Aadhaar Document";
                        else
                            Reason += "No/Invalid Aadhaar Document";
                    }

                    if (faculty.BasstatusOld == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",BAS Flag";
                        else
                            Reason += "BAS Flag";
                    }

                    if (faculty.OriginalsVerifiedUG == true)
                    {
                        if (Reason != null)
                            Reason += ",Complaint PHD Faculty";
                        else
                            Reason += "Complaint PHD Faculty";
                    }

                    if (faculty.OriginalsVerifiedPHD == true)
                    {
                        if (Reason != null)
                            Reason += ",No Guide Sign in PHD Thesis";
                        else
                            Reason += "No Guide Sign in PHD Thesis";
                    }

                    faculty.DeactivationReason = Reason == null ? null : Reason;

                    teachingFaculty.Add(faculty);

                }
                return View(teachingFaculty.Where(e => e.BasstatusOld == "Yes").ToList());
            }


            return View(teachingFaculty);
        }

        //Written By Siva
        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult GetFacultyBASDetailsView(string RegistarationNumber)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeID = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == RegistarationNumber).Select(e => e.collegeId).FirstOrDefault();
            if (RegistarationNumber != null)
            {
                var FacultyBASData = db.jntuh_college_basreport.Where(e => e.RegistrationNumber == RegistarationNumber).Select(e => e).ToList();
                if (FacultyBASData.Count() != 0 && FacultyBASData != null)
                {
                    GetFacultyBASDetails Faculty = new GetFacultyBASDetails();
                    Faculty.RegistarationNumber = RegistarationNumber;
                    string date = FacultyBASData.Select(e => e.joiningDate).FirstOrDefault().ToString();

                    Faculty.BasJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                    Faculty.TotalWorkingDays = FacultyBASData.Select(e => e.totalworkingDays).Sum();
                    Faculty.TotalPresentDays = FacultyBASData.Select(e => e.NoofPresentDays).Sum();

                    foreach (var item in FacultyBASData)
                    {
                        if (item.month == "July")
                        {
                            Faculty.JulyTotalDays = item.totalworkingDays;
                            Faculty.JulyPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "August")
                        {
                            Faculty.AugustTotalDays = item.totalworkingDays;
                            Faculty.AugustPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "September")
                        {
                            Faculty.SeptemberTotalDays = item.totalworkingDays;
                            Faculty.SeptemberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "October")
                        {
                            Faculty.OctoberTotalDays = item.totalworkingDays;
                            Faculty.OctoberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "November")
                        {
                            Faculty.NovemberTotalDays = item.totalworkingDays;
                            Faculty.NovemberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "December")
                        {
                            Faculty.DecemberTotalDays = item.totalworkingDays;
                            Faculty.DecemberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "January")
                        {
                            Faculty.JanuaryTotalDays = item.totalworkingDays;
                            Faculty.JanuaryPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "February")
                        {
                            Faculty.FebruaryTotalDays = item.totalworkingDays;
                            Faculty.FebruaryPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "March")
                        {
                            Faculty.MarchTotalDays = item.totalworkingDays;
                            Faculty.MarchPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "April")
                        {
                            Faculty.AprilTotalDays = item.totalworkingDays;
                            Faculty.AprilPresentDays = item.NoofPresentDays;
                        }
                    }


                    return PartialView("~/Views/FacultyBASFlag/_GetFacultyBASDetails.cshtml", Faculty);
                }
                else
                {
                    return RedirectToAction("AppealFacultyVerification", new { collegeId = collegeID });
                }
            }
            else
            {
                return RedirectToAction("AppealFacultyVerification", new { collegeId = collegeID });
            }
            // return View();
        }

        public class GetFacultyBASDetails
        {
            public string RegistarationNumber { get; set; }
            public string BasJoiningDate { get; set; }
            public int? JulyTotalDays { get; set; }
            public int? AugustTotalDays { get; set; }
            public int? SeptemberTotalDays { get; set; }
            public int? OctoberTotalDays { get; set; }
            public int? NovemberTotalDays { get; set; }
            public int? DecemberTotalDays { get; set; }
            public int? JanuaryTotalDays { get; set; }
            public int? FebruaryTotalDays { get; set; }
            public int? MarchTotalDays { get; set; }
            public int? AprilTotalDays { get; set; }
            public int? JulyPresentDays { get; set; }
            public int? AugustPresentDays { get; set; }
            public int? SeptemberPresentDays { get; set; }
            public int? OctoberPresentDays { get; set; }
            public int? NovemberPresentDays { get; set; }
            public int? DecemberPresentDays { get; set; }
            public int? JanuaryPresentDays { get; set; }
            public int? FebruaryPresentDays { get; set; }
            public int? MarchPresentDays { get; set; }
            public int? AprilPresentDays { get; set; }
            public int? TotalWorkingDays { get; set; }
            public int? TotalPresentDays { get; set; }
        }

        #endregion


        public ActionResult StaticRegistrationNumbersBASFlagChecking(string command)
        {
            //Written By Siva
            #region OldCode
           
            string[] RegNos  = new string[] { "0007-180208-115525", "0009-150410-223407", "00150404-122843", "00150404-132143", "00150404-142150", "00150406-124851", "00150407-105515", "0017-150408-155908", "0017-160301-122830", "0033-160306-192726", "0037-150418-151850", "0043-170522-131138", "0045-180129-222401", "0055-150409-105615", "0062-150414-084913", "0063-171227-194032", "0065-161128-150056", "0070-160311-123513", "0070-170110-094030", "0076-150412-140821", "0078-170523-170724", "0079-180207-200335", "0086-170117-142511", "0089-160314-175435", "0089-161209-145730", "0091-150427-145248", "0104-150414-142722", "0105-180110-112802", "01150330-195657", "01150330-222845", "01150331-125001", "01150331-151224", "01150331-201207", "01150402-162629", "01150404-110704", "01150406-161150", "01150407-151610", "01150407-151833", "0123-150410-101600", "0138-150408-131246", "0149-150408-213453", "0159-150419-103024", "0164-171222-094759", "0171-150407-213332", "0183-150623-110537", "0183-161025-172827", "0188-170914-104044", "0191-171129-114721", "0197-170114-211327", "0206-160527-190725", "0209-150410-101942", "0212-150418-165845", "0214-160301-202618", "02150331-120556", "02150402-134808", "02150403-220700", "02150404-110529", "02150404-113151", "02150406-222518", "0228-170523-212632", "0229-150508-113304", "0232-160312-200514", "0237-170208-160211", "0249-170523-120150", "0254-150408-125530", "0255-150418-113623", "0261-150420-121809", "0264-170523-191942", "0266-160320-155426", "0271-160529-123746", "0278-150420-155450", "0289-150409-112844", "0290-161128-113918", "0294-150427-103920", "0297-161126-112913", "0298-180202-155940", "0304-150423-124746", "0311-150415-172025", "0311-170122-203935", "0314-150420-143233", "03150331-231935", "03150401-222957", "03150404-221644", "03150406-164545", "0316-170108-121812", "0321-150413-212744", "0328-150409-104223", "0330-150430-152111", "0334-150409-132111", "0338-150505-123322", "0340-170213-144245", "0343-150416-102005", "0349-150624-185052", "0354-170523-153347", "0364-170131-014034", "0368-161227-154357", "0370-150418-131438", "0377-150409-133622", "0383-170112-114722", "0387-171025-174106", "0389-171116-123400", "0391-160303-162613", "0398-160315-143304", "0398-170521-100904", "0399-161228-131657", "0406-150418-131001", "0411-161202-112915", "0411-180127-105509", "04150402-162346", "04150404-125957", "04150404-162859", "04150404-164306", "04150407-044411", "04150407-173505", "0422-160304-224140", "0429-150420-110211", "0433-170522-175603", "0435-150420-195826", "0438-171122-122044", "0452-160310-154804", "0457-150408-215923", "0462-160305-131508", "0472-160313-111912", "0479-180110-220632", "0481-161025-134621", "0494-150411-212235", "0501-150415-145453", "0508-150427-161539", "0512-161123-121032", "05150404-114612", "05150404-143934", "05150404-150310", "05150404-160107", "05150406-114345", "05150407-141345", "0518-170201-100649", "0520-180102-220148", "0522-150427-225629", "0527-170623-161203", "0534-160222-124511", "0553-171226-122921", "0554-150409-140823", "0564-170208-180025", "0565-170207-220306", "0567-161212-102048", "0574-150410-111014", "0579-150506-133602", "0591-150424-142958", "0595-180201-161147", "0598-150408-175353", "0611-180208-172255", "0612-160919-173944", "0612-161210-123014", "0614-160314-122412", "06150331-111246", "06150331-130643", "06150401-152726", "06150402-153653", "06150404-113145", "06150404-132713", "06150405-115535", "06150406-115949", "06150406-133044", "06150407-113602", "06150407-131623", "06150407-133809", "06150407-142347", "0619-180131-095845", "0622-150413-150200", "0627-171208-145014", "0635-170131-011730", "0640-150412-202441", "0647-170521-101358", "0653-160220-110651", "0661-161031-001234", "0664-160314-171217", "0666-150408-135631", "0670-150419-142418", "0678-150409-134601", "0680-160529-145540", "0684-150427-123326", "0690-160224-143435", "0695-160209-105750", "0695-170116-211154", "0695-180202-135315", "0697-170521-115623", "0704-180208-133828", "0707-161025-140625", "0714-150408-163208", "07150330-103619", "07150331-132917", "07150402-114612", "07150402-165852", "07150404-125837", "07150407-000328", "07150407-134148", "0723-170206-142206", "0723-170523-223421", "0732-150413-223536", "0755-180208-162637", "0756-150411-130143", "0765-171222-151335", "0769-160305-224725", "0772-160309-102912", "0778-150410-125550", "0788-180208-101027", "0790-170213-204405", "0791-150411-144110", "0793-160529-180203", "0795-171124-160448", "0796-170522-142021", "0798-171220-133506", "0804-170913-180210", "0807-150408-125629", "0810-150421-120331", "0814-160314-164124", "08150404-232135", "08150407-111738", "08150407-134210", "0816-150409-144756", "0816-151231-123436", "0819-180208-214939", "0827-161022-175318", "0837-161129-122004", "0843-150415-195740", "0846-170111-120917", "0851-150409-170037", "0861-150413-142903", "0861-170916-141035", "0869-171227-162556", "0871-160315-145356", "0874-161103-150002", "0879-160314-164629", "0884-160315-001044", "0895-171222-113335", "0897-170915-161247", "0899-150419-090852", "0900-170523-223048", "0902-150506-165503", "0903-160203-143127", "0913-150408-140109", "09150330-185602", "09150330-233548", "09150331-151951", "09150401-182955", "09150402-132227", "0918-150421-113133", "0920-171212-144956", "0931-160204-115104", "0936-150420-114612", "0941-150506-162018", "0947-170126-071811", "0948-160223-154330", "0954-171222-165544", "0962-160105-143546", "0963-150413-125053", "0972-170126-091920", "0972-170131-005308", "0973-150408-164314", "0973-161030-093220", "0978-170913-201423", "0996-170131-023348", "1001-150411-152829", "1011-150408-152618", "10150403-133250", "1016-170213-095525", "1019-150413-125218", "1023-170522-160534", "1025-171227-133210", "1028-150505-134104", "1034-150411-145802", "1038-150408-121059", "1039-160313-141051", "1044-171215-150733", "1046-161126-133019", "1059-170125-103246", "1061-160306-221302", "1062-170123-140150", "1065-161205-164740", "1067-150412-120929", "1075-161207-155320", "1076-180220-234151", "1082-160210-102657", "1089-150409-125700", "1089-170208-175418", "1093-180201-120151", "1098-160223-134532", "1112-161023-142238", "1112-170116-130834", "11150405-154738", "11150405-230141", "11150406-114427", "11150407-103414", "11150407-105535", "11150407-143836", "1131-160305-165031", "1134-160306-155739", "1148-160224-190015", "1154-160309-121215", "1161-180208-115655", "1167-170203-110751", "1184-180206-172014", "1187-160210-145559", "1192-170207-012809", "1193-150417-233334", "1193-180208-162745", "1194-161205-171645", "1195-170521-131948", "1195-170915-155908", "1196-150409-212536", "12150331-022559", "12150401-143701", "12150402-111935", "12150402-152647", "12150402-153633", "12150403-161135", "12150404-113532", "12150404-154539", "12150407-102002", "1225-161118-155245", "1226-160528-130834", "1227-170520-154157", "1235-160206-123159", "1235-170131-182202", "1236-161128-123648", "1237-171013-150434", "1239-160307-230546", "1244-150625-160443", "1250-180208-164904", "1255-170208-172403", "1258-180130-143059", "1260-170102-141915", "1263-160303-180642", "1266-150411-023607", "1267-150420-140505", "1272-150425-191320", "1277-150413-124141", "1279-161228-105341", "1280-170203-113618", "1286-170131-201713", "1291-180206-135319", "1297-160314-172232", "1298-170523-121831", "1303-160315-103412", "1304-160223-103618", "1312-161026-160254", "1313-171123-150641", "13150402-163345", "13150403-162842", "13150404-123131", "13150406-152951", "13150407-160822", "13150407-162554", "1324-150413-200901", "1325-160112-185306", "1328-150410-102032", "1332-170201-103806", "1334-160302-111355", "1335-170520-121434", "1336-170911-162939", "1339-150425-223809", "1348-150420-110623", "1348-180202-145152", "1362-170119-154826", "1373-150426-233659", "1375-171206-145411", "1377-150412-112802", "1379-150409-161635", "1380-170523-161434", "1382-170521-153441", "1386-171018-151234", "1412-161221-102343", "14150402-115407", "14150402-120027", "14150402-121235", "14150402-155349", "14150404-072853", "14150404-153743", "14150406-102925", "14150406-150508", "1422-180129-150230", "1423-160219-094426", "1423-170201-231848", "1426-150411-125711", "1434-150415-215340", "1439-150408-145337", "1439-150413-123239", "1445-160204-134145", "1459-180103-215657", "1466-150419-190407", "1479-171218-143112", "1480-150410-132808", "1482-150408-160212", "1493-170915-144826", "1497-171011-105355", "1500-150408-113059", "1510-170117-111457", "1511-150409-125904", "15150403-161534", "15150403-231653", "15150404-165417", "15150405-122355", "15150405-150802", "15150407-113619", "15150407-155302", "1516-150417-230501", "1517-160128-162616", "1519-171223-161429", "1523-150408-150209", "1538-160527-181237", "1539-150410-142048", "1539-170522-113513", "1540-180106-094729", "1546-150417-152331", "1550-170203-110650", "1551-160302-115953", "1566-160529-204221", "1572-150410-114653", "1572-171215-162418", "1583-150413-163325", "1585-180127-144655", "1586-150424-152459", "1592-160320-134143", "1593-150409-141240", "1595-180201-180017", "1597-160528-152545", "1603-180131-191237", "1608-150409-104757", "16150403-102930", "16150406-120431", "16150406-121938", "16150406-123557", "16150406-130251", "16150406-153238", "16150407-122941", "16150407-133453", "1620-161024-152052", "1623-170128-131044", "1632-170111-132912", "1633-170523-190006", "1636-170117-152946", "1643-150411-123804", "1653-150410-171027", "1657-180202-162022", "1658-160314-135529", "1665-160529-234630", "1693-170213-114848", "1696-150413-145115", "1703-180201-161103", "1704-180201-132810", "1706-150413-151751", "1708-170109-145010", "1715-170918-131805", "17150331-181635", "17150401-154407", "17150402-124709", "17150404-114317", "17150406-115454", "17150406-145030", "1717-171026-163553", "1719-170102-125403", "1720-150408-122540", "1724-160313-120949", "1724-170116-125032", "1732-161125-134800", "1736-161207-142623", "1738-161128-150415", "1739-170602-135119", "1740-150411-173641", "1744-161123-171157", "1745-150419-203504", "1745-151222-114535", "1747-170623-161422", "1747-180208-144342", "1774-170209-191821", "1776-151231-121925", "1778-161022-132131", "1778-170213-154848", "1783-160217-122916", "1786-161025-180517", "1791-150417-215632", "1792-180201-113733", "1799-170523-145346", "1803-170522-170602", "1804-150504-223903", "1812-161025-154059", "1814-150418-121540", "1815-170208-201455", "18150401-145131", "18150403-235223", "18150407-150227", "18150407-183429", "1821-161220-155939", "1828-150420-120347", "1834-170131-041404", "1834-180208-210350", "1855-150417-193607", "1874-171101-112851", "1881-160307-111846", "1884-180201-111249", "1886-161215-110840", "1887-150409-204914", "1889-160319-164942", "1898-150416-103606", "1902-170131-181438", "19150331-095841", "19150402-125919", "19150403-074232", "19150404-105700", "19150407-121546", "1923-150411-103741", "1928-160306-195235", "1932-160229-073352", "1933-170131-011734", "1938-171227-190729", "1941-150408-195342", "1959-160529-184524", "1968-150420-124535", "1981-170207-181640", "1985-160229-144455", "1987-170103-151550", "1988-161213-123229", "2002-170911-162313", "2006-160219-113546", "20150401-191342", "20150402-112655", "20150403-232819", "20150404-015659", "20150404-111254", "20150405-113026", "20150406-164452", "2023-170915-153502", "2025-170523-133203", "2026-150409-165352", "2027-150410-121044", "2031-161231-114645", "2041-160215-133636", "2051-150410-135522", "2054-160219-143542", "2054-170127-102825", "2054-170201-122900", "2073-150408-154940", "2080-150413-104303", "2084-170129-104621", "2097-150408-114705", "2098-151228-160426", "2112-160719-110859", "2115-150415-093549", "21150330-130127", "21150330-230024", "21150330-230655", "21150403-133944", "21150406-170045", "21150407-111354", "21150407-114517", "21150407-154836", "2117-180201-163235", "2129-160319-161407", "2136-170126-072826", "2139-170126-101355", "2147-160129-155537", "2152-150408-105423", "2167-170112-111334", "2173-151229-143024", "2195-150408-123749", "2198-160609-105544", "2214-150410-121050", "2215-150414-145324", "22150331-155326", "22150401-125835", "22150407-112351", "22150407-122732", "2220-170201-061419", "2223-150408-123716", "2231-160528-214659", "2234-171221-111633", "2237-170208-172133", "2238-150427-144228", "2243-161210-144310", "2257-150428-055331", "2257-170113-190851", "2264-150502-173109", "2275-160222-140546", "2285-170116-203354", "2286-170107-145605", "2295-170103-155344", "2299-150409-154511", "2305-150413-144834", "2310-160304-201247", "2315-150407-224158", "23150331-092724", "23150401-101417", "23150402-094942", "23150403-145605", "23150404-113644", "2324-171208-105321", "2333-170117-120120", "2336-161027-130400", "2338-160529-082400", "2344-180206-103028", "2351-150507-075609", "2353-170203-160228", "2355-170131-191226", "2357-160105-123059", "2368-150420-161213", "2368-150507-190142", "2368-170104-151452", "2386-150620-152649", "2386-151218-155645", "2387-180108-143722", "2392-180201-105143", "2396-171207-162649", "2399-170202-134902", "2409-180208-160922", "2413-170520-123446", "2415-150409-150012", "2415-160218-123944", "24150330-154256", "24150331-105215", "24150403-155438", "24150404-144929", "24150404-145458", "24150405-131544", "24150406-113409", "24150407-144012", "2422-150419-214353", "2422-170127-062248", "2423-160222-130130", "2425-180208-152024", "2426-160313-095146", "2439-150410-193012", "2439-160211-123708", "2440-170112-101852", "2447-150413-140800", "2447-170202-055024", "2451-150408-161052", "2453-170118-113454", "2455-150411-144816", "2459-161227-100217", "2460-171220-144420", "2468-150410-103024", "2468-151222-143411", "2477-150413-174417", "2478-170914-151129", "2482-160205-161712", "2488-160108-160128", "2488-170201-141648", "2488-170914-144209", "2489-170207-162302", "2493-150418-142538", "2500-160307-125857", "25150331-103600", "25150404-152451", "25150406-152025", "2518-150417-130617", "2526-170211-140330", "2529-160129-145834", "2533-160310-110814", "2534-171117-123628", "2552-160305-142949", "2560-150408-211950", "2563-170203-111533", "2578-160213-110706", "2580-160309-104648", "2589-160315-121543", "2605-160310-131957", "2606-170913-110531", "2611-170207-130847", "26150328-102440", "26150331-100957", "26150402-142054", "26150403-155145", "26150404-133450", "26150406-182344", "26150407-125656", "2618-150416-174141", "2625-151218-121808", "2627-180202-101038", "2639-170201-061450", "2641-150412-134821", "2644-170105-122433", "2657-170602-182604", "2660-160310-180718", "2661-180202-172741", "2665-170104-132243", "2672-170112-132640", "2677-150408-174826", "2694-150622-111010", "2697-150415-115426", "2697-180130-124807", "2699-150409-154605", "2704-170520-130953", "2705-150506-170154", "2705-160218-125920", "2709-170131-184117", "2709-170520-111218", "27150331-114004", "27150331-122933", "27150331-153714", "27150401-135253", "27150403-141104", "27150404-105355", "27150406-132203", "27150407-121048", "2718-160307-011834", "2724-150408-171821", "2726-150408-010119", "2727-150419-111029", "2736-170208-161002", "2741-170520-103020", "2747-150411-113720", "2748-150413-133433", "2753-161020-113059", "2759-150419-112924", "2767-170523-225515", "2770-161028-140111", "2772-160226-193511", "2773-170109-102839", "2775-150421-132007", "2776-150426-174034", "2784-161027-112616", "2787-150427-172144", "2802-150409-125545", "2803-160727-013739", "2809-150408-115117", "2812-180208-171137", "28150406-114527", "28150406-134111", "28150407-132807", "28150407-193405", "2817-160225-103934", "2818-170127-111644", "2822-150409-103812", "2823-180202-111019", "2825-150420-202531", "2831-151216-143314", "2832-160305-123343", "2836-150408-154433", "2839-150414-142831", "2840-150428-120751", "2847-180205-145703", "2860-170522-154549", "2867-150408-181929", "2870-150415-145856", "2872-150417-112832", "2875-160307-112908", "2876-150413-192246", "2880-160218-223616", "2883-161206-120759", "2886-161117-110449", "2892-160320-155417", "2901-171208-150453", "2903-160305-155631", "2909-160129-100443", "29150402-115939", "29150403-194155", "29150404-175402", "29150406-103116", "29150406-121824", "29150406-142128", "29150406-184245", "2919-170120-152602", "2921-171009-152053", "2924-150411-160319", "2924-160320-132525", "2931-150411-173542", "2944-161022-132123", "2947-170208-064336", "2949-160309-110305", "2951-170202-052356", "2954-150416-103048", "2961-160307-111238", "2971-161024-163038", "2976-150409-150941", "2982-170523-172149", "2984-170520-154553", "2991-150410-135537", "2997-171223-162403", "2999-170122-190110", "3011-150506-143432", "3015-150418-164955", "3015-161105-102504", "30150331-124825", "30150401-172054", "30150402-161632", "30150404-172610", "30150404-185435", "30150406-101910", "30150407-124348", "30150407-162249", "3022-150408-105758", "3023-150407-213026", "3024-160308-162234", "3024-170914-132719", "3040-170915-115017", "3042-150410-130143", "3042-161025-155320", "3046-150408-123016", "3050-170116-142118", "3052-171114-104657", "3055-161125-201020", "3068-160305-201328", "3071-171124-124104", "3077-161025-092554", "3079-160528-224153", "3108-160320-201556", "3109-180201-163126", "31150404-102907", "31150404-140528", "31150404-145725", "31150404-195745", "31150406-132701", "3119-171016-164221", "3126-170131-010521", "3127-160529-195347", "3127-170126-034042", "3132-150426-111616", "3133-150410-105936", "3138-180131-233327", "3146-150410-162710", "3149-170109-134623", "3150-150413-162254", "3163-150413-211938", "3164-170208-062428", "3173-170914-150723", "3174-170207-172505", "3175-170913-105811", "3176-170601-161536", "3176-180202-133056", "3186-171227-091033", "3193-160302-125505", "3194-151223-155854", "3198-170523-121112", "3206-180202-181333", "3210-160108-115118", "3210-170131-042425", "32150330-184849", "32150331-101020", "32150331-122023", "32150331-140308", "32150403-191223", "32150404-153853", "32150404-172740", "32150406-151851", "3216-150420-102634", "3221-180208-163912", "3226-150409-144409", "3226-150515-114327", "3228-180202-190533", "3229-150419-022130", "3248-150427-153018", "3252-150408-154953", "3254-170208-051838", "3259-161221-153152", "3264-161024-213028", "3280-170201-144304", "3284-160315-130736", "3285-150413-145210", "3286-171227-145359", "3292-150420-162826", "3296-170118-103603", "33150331-120220", "33150402-123137", "33150403-121738", "33150404-154141", "33150406-143448", "33150407-111134", "33150407-130622", "3332-160309-114126", "3333-161213-134400", "3335-180201-145236", "3345-150409-122730", "3346-160213-161530", "3357-171218-144646", "3370-160315-134028", "3371-160303-174942", "3371-170129-075834", "3388-170129-082213", "3403-180201-115811", "3407-150410-145259", "34150330-174403", "34150401-111127", "34150401-142710", "34150406-131850", "3417-150410-102238", "3420-170523-203751", "3422-180201-131330", "3425-150418-120359", "3431-160219-135556", "3432-161231-121013", "3432-170129-072851", "3433-160302-170919", "3436-161231-133451", "3439-160220-111619", "3448-170131-225634", "3453-150412-185020", "3465-150411-165219", "3465-160120-121726", "3467-150409-155417", "3473-180201-170519", "3477-171227-113508", "3483-170126-095859", "3487-170912-102813", "3489-150422-155607", "3495-180201-150702", "3503-160314-112537", "3509-150410-115632", "3510-170111-130947", "35150331-203743", "35150402-142741", "35150404-152522", "35150405-223509", "35150406-172430", "35150407-142303", "3531-161124-122237", "3532-170213-192053", "3539-150408-131514", "3540-180208-183821", "3556-150409-105506", "3558-171129-141433", "3572-170523-162953", "3573-170207-162957", "3582-150413-152652", "3591-150410-125429", "3592-160313-222024", "3593-170207-001055", "3611-150415-152649", "3612-161202-125003", "3615-150411-112857", "3615-160312-133633", "36150403-130305", "36150406-161812", "36150407-100632", "36150407-121220", "3616-150410-102804", "3624-150409-132438", "3624-161027-121204", "3627-160314-170421", "3630-160306-210237", "3634-150428-100300", "3639-160315-010107", "3645-150506-161115", "3649-150409-094050", "3654-160310-153344", "3659-170131-043726", "3664-170207-113804", "3668-150419-121933", "3675-170117-101707", "3677-151228-155128", "3697-180124-130440", "3699-150410-132226", "3711-160320-172323", "3714-150408-151324", "37150331-140447", "37150331-201236", "37150404-135532", "37150404-142550", "37150406-133923", "37150406-142515", "3719-150409-222430", "3724-170213-125740", "3730-180207-164802", "3743-150409-112440", "3743-170126-121220", "3748-160320-163418", "3759-170131-051039", "3759-170207-200452", "3772-161221-161618", "3774-150408-122959", "3774-170212-091314", "3780-150424-125834", "3786-150408-101716", "3792-170915-171037", "3800-160108-113339", "3804-170203-234126", "38150331-113831", "38150331-114808", "38150402-131017", "38150403-104851", "38150406-113008", "38150406-151849", "38150406-153922", "38150407-114922", "38150407-141045", "3820-180207-153748", "3824-160315-163757", "3825-151218-135715", "3826-170107-145702", "3826-170129-115321", "3826-171227-233704", "3827-160311-182024", "3828-161019-161619", "3830-171206-150315", "3832-160221-111516", "3844-170208-174725", "3846-150408-114505", "3847-150410-113219", "3855-170213-144041", "3864-171223-150936", "3869-150410-152514", "3870-160314-121218", "3885-160209-102257", "3890-170523-223725", "3893-170522-131419", "3907-150420-152803", "3915-150506-163007", "39150402-122047", "39150404-213627", "39150406-112650", "39150406-124038", "39150407-121907", "39150407-125323", "3918-150420-101926", "3930-160204-155854", "3931-160526-154811", "3936-150418-164408", "3939-160526-122855", "3952-150408-115449", "3953-160102-113841", "3961-150424-120438", "3962-150515-123610", "3976-170915-180353", "3983-150511-233602", "3992-150419-080217", "4005-160216-090715", "4007-150415-145649", "4007-160310-100844", "40150402-170443", "40150406-124742", "40150406-151446", "40150407-125423", "4024-150414-201804", "4032-180201-151029", "4036-170208-040826", "4045-150418-063749", "4046-150408-180421", "4049-150501-153313", "4049-160225-135523", "4054-170130-052316", "4054-170204-110401", "4066-170116-153454", "4075-171128-125052", "4079-180208-231222", "4098-161213-112929", "4099-170522-122816", "4100-150421-121555", "4101-150411-105236", "4101-150504-170027", "4107-150425-151419", "4110-160120-144233", "4112-171222-171302", "41150402-133226", "41150404-113040", "41150406-010905", "4135-150424-152211", "4141-150427-143138", "4142-171224-215950", "4144-150408-152803", "4147-150415-121607", "4153-170115-172006", "4157-170522-165424", "4157-170913-131650", "4179-180202-163132", "4189-180201-202900", "4207-161020-164022", "4212-170520-154800", "4213-161128-093740", "4215-161223-144848", "4215-180208-161720", "42150331-113515", "42150406-152311", "42150407-110147", "42150407-123232", "42150407-154522", "4232-170522-141133", "4235-170201-133626", "4243-160306-074048", "4245-150408-132734", "4248-180202-171805", "4253-150418-162514", "4256-170105-144438", "4258-150413-171320", "4265-170522-122220", "4269-180207-145304", "4274-150408-160332", "4275-160308-133315", "4286-160528-145039", "4287-170523-172633", "4297-180201-101513", "4298-160316-104022", "4300-170208-163059", "4307-160217-123734", "4310-150415-235250", "4310-170126-130653", "43150330-144912", "43150330-161340", "43150331-121811", "43150402-150152", "43150402-162415", "43150404-143444", "43150406-101205", "4316-160222-150902", "4336-150409-162429", "4342-170111-091417", "4345-160524-165457", "4349-180123-140200", "4357-161210-150550", "4359-150419-125809", "4361-170520-161917", "4368-180202-145156", "4370-170125-102826", "4373-170915-165932", "4374-170104-125349", "4375-160229-131312", "4388-170205-120331", "4404-160528-230641", "4406-170523-181600", "4411-170523-114449", "4415-160228-114657", "44150401-163328", "44150402-125710", "44150403-104659", "44150407-122156", "4418-171220-150248", "4422-180201-144914", "4423-160229-120013", "4423-170522-124702", "4428-170127-105214", "4430-170109-140217", "4439-170913-131636", "4440-160312-085119", "4454-160315-024113", "4455-171223-165021", "4460-180131-172933", "4463-170912-151337", "4463-180201-140846", "4464-170126-024023", "4473-170115-183328", "4479-170117-130507", "4482-161125-142436", "4499-170522-103724", "4501-170119-144342", "4512-180119-225729", "45150330-135118", "45150331-161436", "45150406-142306", "45150407-075620", "4518-160229-153912", "4531-160229-113726", "4534-160311-130611", "4535-161128-152900", "4538-170106-121618", "4539-180201-172827", "4540-160309-153739", "4556-170523-162349", "4560-170116-192548", "4563-161027-123114", "4570-150427-094955", "4571-150504-181413", "4577-150408-152856", "4584-160305-122810", "4585-161201-160209", "4586-151222-194918", "4587-150408-154522", "4590-150416-143638", "4602-170204-162205", "4610-150407-200049", "4610-150410-155151", "46150406-125500", "46150407-112830", "46150407-144715", "4618-150409-121045", "4622-170120-134908", "4623-161126-131853", "4625-160526-220611", "4626-180208-134747", "4628-170205-124643", "4632-160315-144107", "4648-150408-212254", "4649-150414-120316", "4651-160222-151703", "4651-180111-002201", "4652-170523-121022", "4656-171122-114730", "4662-150427-075211", "4680-161104-140308", "4681-150410-101558", "4683-161129-151048", "4683-170113-154219", "4684-150420-135135", "4686-150409-125921", "4686-170211-143137", "4688-160306-003949", "4695-170213-170659", "4697-160217-203935", "4699-150410-110411", "4702-160311-180754", "4712-160313-143055", "47150331-113435", "47150331-182034", "47150402-093652", "47150402-131222", "47150404-190607", "47150406-163118", "47150407-132746", "47150407-152328", "4728-180202-082643", "4731-160306-121731", "4731-170130-143850", "4731-170522-102502", "4735-170915-161133", "4743-170522-160430", "4744-150410-141034", "4744-160220-155006", "4751-160130-144156", "4758-150411-123622", "4763-160226-172443", "4768-160219-134349", "4789-161128-112441", "4801-170126-125032", "4802-150409-000442", "4803-170131-180155", "4811-171226-140908", "4814-160217-165341", "48150331-044557", "48150402-130357", "48150405-232624", "48150406-111721", "48150406-132519", "48150407-125105", "48150407-154050", "4822-150410-114424", "4831-170522-151851", "4841-160315-184911", "4843-170108-135437", "4854-150414-125905", "4857-150411-163252", "4859-160313-194649", "4861-170201-151335", "4865-171016-113947", "4872-180202-103956", "4874-160215-165007", "4876-160213-150901", "4877-170116-111735", "4881-161224-112706", "4896-150619-162217", "4898-160130-112028", "4900-180206-120242", "4903-170207-115247", "4906-180104-104507", "49150404-115121", "49150404-142008", "49150404-170305", "49150406-114716", "49150406-145609", "49150407-114156", "49150407-143839", "4917-171022-202121", "4929-150409-144539", "4935-170117-102242", "4938-170202-095110", "4945-150409-184906", "4946-170523-231520", "4956-151229-124632", "4964-160314-114954", "4970-150411-140828", "4971-160128-140101", "4973-150408-132054", "4973-170113-202737", "4974-150408-150749", "4977-170213-145706", "4978-161025-153504", "4983-150619-153638", "4984-160303-115900", "4999-160118-121910", "50150327-201344", "50150331-145021", "50150331-190305", "50150405-080016", "50150405-134030", "50150407-175359", "5020-160223-110315", "5025-160229-183358", "5025-180201-191319", "5034-150409-110833", "5038-160224-131100", "5043-150408-125708", "5050-150409-132929", "5056-180207-170351", "5057-171021-105147", "5065-161231-130124", "5086-160529-112201", "5086-161102-112239", "5088-150502-135231", "5092-180202-050911", "5100-180202-154235", "5102-160306-195354", "5106-180208-194841", "5107-161025-144741", "5111-150408-175736", "51150405-112628", "51150406-150532", "51150406-154756", "51150407-103019", "5118-150421-095123", "5118-170106-120828", "5119-170913-145107", "5132-160302-122835", "5140-171023-143126", "5142-160306-155551", "5149-160106-233105", "5153-150420-172013", "5174-160320-154659", "5198-150410-113217", "5202-150506-171710", "5203-150409-142249", "5203-170122-115917", "52150404-154809", "52150404-201425", "52150406-133435", "52150407-152647", "5216-160225-115159", "5218-150420-155112", "5218-170121-063910", "5223-150412-122604", "5228-170520-115145", "5231-151222-124931", "5231-180202-165111", "5232-150410-132702", "5233-150408-152752", "5235-160223-164927", "5237-170520-164625", "5240-160307-120829", "5252-180127-160522", "5254-160217-135516", "5255-170520-175435", "5256-150420-121814", "5256-171220-130151", "5261-161114-101437", "5270-170208-012320", "5271-170208-060658", "5273-150413-152031", "5280-160422-215925", "5283-170916-110540", "5286-150413-164156", "5290-170117-121901", "5293-170213-160813", "5294-150410-135203", "5297-161126-162209", "5299-160312-183316", "5308-180202-115849", "5311-170914-151625", "53150331-204441", "53150401-204835", "53150402-115601", "53150404-145759", "53150406-161513", "53150407-173532", "5329-150410-150448", "5332-170913-131123", "5333-170106-122129", "5342-180201-122118", "5346-150408-162529", "5346-150409-103615", "5358-160314-115856", "5377-150416-143530", "5384-151231-113215", "5390-150420-124958", "5406-161201-144923", "5408-160315-171742", "5410-161221-154840", "5411-150409-114451", "5414-150414-135918", "5414-160222-121906", "54150331-085207", "54150331-143516", "54150402-102202", "54150404-101332", "54150407-113636", "54150407-145020", "5417-150416-173107", "5417-150418-113930", "5450-150623-165900", "5454-161126-134359", "5455-170914-112243", "5456-171113-110854", "5465-160318-162400", "5470-170522-142615", "5474-160225-110911", "5480-180202-112059", "5491-171103-142932", "5502-170120-125609", "5503-180208-144157", "5509-150414-174240", "55150401-173858", "55150403-155536", "55150404-164801", "55150406-110725", "55150406-124635", "5516-150413-163541", "5526-150422-175939", "5527-150416-221102", "5532-170206-234514", "5536-170105-143357", "5540-150409-151342", "5548-160128-224323", "5549-150507-183713", "5574-150422-123317", "5576-150410-101336", "5576-160529-153845", "5582-180131-143419", "5590-150413-174841", "5598-160817-100637", "5603-150410-113109", "5609-150409-144212", "5609-170523-235819", "5610-160311-113147", "5614-150420-155913", "56150331-144109", "56150331-171430", "56150402-155057", "56150406-112840", "56150406-132532", "56150407-104220", "5617-150513-152145", "5630-150420-125100", "5632-150504-143830", "5632-160212-144142", "5641-180202-114133", "5645-170126-130728", "5659-171206-155619", "5662-160529-185837", "5666-150422-095935", "5668-150408-211212", "5670-170522-125002", "5672-180202-153808", "5679-171012-101129", "5685-150409-131541", "5687-161214-141826", "5693-150507-191303", "5697-161021-135336", "57150331-103126", "57150401-123553", "57150406-004107", "57150406-141835", "57150406-145821", "57150406-150411", "57150407-125915", "5716-161220-161902", "5725-150505-114121", "5726-160311-111601", "5730-170522-100531", "5736-171118-102011", "5738-150413-125429", "5753-171224-192424", "5767-160310-115054", "5772-160224-120936", "5772-160315-153243", "5775-170201-064952", "5777-150413-125906", "5777-150415-093419", "5783-160213-133323", "5784-171106-142610", "5785-170523-181523", "5791-170110-125752", "5801-150409-143524", "5807-171011-144105", "5807-180127-114604", "5812-150408-173001", "58150402-145745", "58150405-142724", "5817-160217-161421", "5826-150408-151029", "5826-150413-164352", "5826-170913-103020", "5829-150511-222513", "5831-170106-110151", "5853-150420-170627", "5853-160310-120216", "5859-150409-160341", "5867-170602-152447", "5870-170131-185010", "5889-161025-155838", "5889-170204-180351", "5893-160529-230109", "5893-170129-094042", "5905-180203-132313", "5910-160310-105743", "5913-170201-122857", "5913-170212-101154", "5914-170523-202326", "59150401-102004", "59150402-121114", "59150406-115722", "59150406-153330", "5916-180127-115342", "5923-150502-143512", "5924-170520-140225", "5926-150407-215837", "5931-150413-155104", "5931-160229-122123", "5945-171226-180924", "5951-150409-153728", "5958-150413-154246", "5963-161230-134120", "5966-150410-145859", "5970-170102-115818", "5974-180215-131145", "5990-161024-161122", "5990-180202-173614", "5992-170203-094437", "6000-150414-182704", "6009-150408-162747", "6015-170522-131621", "60150402-142433", "60150402-151814", "60150405-201023", "60150406-102734", "60150406-210110", "60150407-112825", "60150407-125343", "6022-150505-152515", "6023-161105-103235", "6049-180130-180018", "6051-150524-174209", "6053-171221-173151", "6056-161025-162035", "6065-150516-161613", "6066-150414-155209", "6069-150411-130050", "6076-170112-142605", "6082-150408-155301", "6085-160301-113156", "6087-150422-221630", "6089-171020-132823", "6090-150415-112755", "6090-170914-125045", "6092-170913-151305", "6093-160529-124758", "6093-180202-121519", "6097-170127-072854", "6100-150414-111354", "6104-170914-124210", "6105-160305-173122", "6108-170128-041206", "6110-180219-170550", "6113-161213-144347", "61150330-214829", "61150404-141824", "61150404-163123", "61150404-163421", "61150405-121514", "61150406-154123", "61150407-120333", "61150407-143545", "6117-170122-165007", "6118-160113-100901", "6121-170128-094759", "6127-170201-122205", "6128-170112-142512", "6139-150408-225135", "6143-150427-153323", "6161-160302-130228", "6180-171017-135900", "6195-150409-145559", "6197-180131-175206", "6200-150418-144716", "6206-150409-165306", "6213-161022-184405", "6213-170201-120909", "62150331-112856", "62150331-155134", "62150403-100019", "62150404-101335", "62150404-160151", "62150406-113441", "62150406-164412", "62150406-190933", "62150407-131841", "6217-170112-095414", "6222-160220-124116", "6222-160526-113115", "6245-170127-105030", "6247-170912-140539", "6248-170914-115407", "6251-170522-175143", "6253-171201-152124", "6260-170520-125955", "6275-160312-223645", "6275-170109-110637", "6279-160220-155410", "6281-150626-112130", "6283-150413-135346", "6283-150413-140008", "6286-171204-120854", "6307-180208-211616", "6310-150410-104450", "6310-170111-095925", "6315-150421-164537", "63150328-184136", "63150401-151206", "63150405-105142", "63150406-103802", "63150407-143633", "6317-170131-201009", "6319-170120-141113", "6329-150408-140045", "6336-170914-140628", "6346-150417-165042", "6377-150409-103655", "6378-160222-113410", "6386-150507-161158", "6387-160319-165638", "6394-150411-124909", "6398-150408-125955", "6403-170130-041622", "6403-170916-124035", "6404-170207-161917", "6404-171221-160659", "6410-170118-160550", "64150402-135939", "64150404-113919", "64150406-014615", "6419-150416-160033", "6422-160305-115826", "6425-170913-144201", "6432-180201-143834", "6441-150414-142344", "6445-180201-175912", "6447-161229-061433", "6452-180124-143253", "6468-150409-124940", "6481-180201-093758", "6483-170118-154709", "6489-170112-153338", "6491-161229-101555", "6503-170201-145903", "6513-170121-071548", "65150330-234021", "65150404-142726", "65150406-144550", "6523-170918-163733", "6525-170125-120940", "6529-150419-111624", "6532-170104-113621", "6542-150408-154656", "6545-180208-143606", "6553-160314-010349", "6553-170201-092800", "6557-180201-192130", "6557-180202-145448", "6562-150423-155047", "6575-160227-133435", "6586-160113-102256", "6590-170201-091037", "6597-150410-152806", "6605-150428-152517", "66150402-143755", "66150404-195728", "66150406-154147", "66150407-134734", "6620-180201-154426", "6625-161024-141332", "6631-160305-114438", "6641-170208-130829", "6647-150409-104416", "6648-150408-132922", "6660-170523-164407", "6661-160216-162733", "6662-161025-185352", "6666-170131-055211", "6670-170122-151816", "6672-160128-161546", "6672-160529-170707", "6676-150410-113019", "6678-170131-180933", "6678-170208-142356", "6679-171221-123106", "6683-160517-122817", "6683-170108-190235", "6690-180130-151009", "6700-170522-125315", "6701-160227-154452", "6701-171225-202147", "6705-160524-103804", "6706-160305-183000", "6709-150413-140634", "6711-161231-160817", "67150402-152106", "67150402-155800", "67150406-154925", "67150407-123931", "67150407-152411", "6719-150416-192442", "6722-170915-131506", "6726-170121-140429", "6731-161024-120556", "6732-170110-124840", "6734-170116-122937", "6741-161022-121423", "6744-150412-204559", "6755-171222-155054", "6756-170520-123238", "6758-150410-152127", "6775-150409-164545", "6775-160223-100217", "6775-160527-113422", "6775-170916-094129", "6785-150408-131328", "6785-161031-114837", "6797-150424-233740", "6800-150408-155528", "6801-170128-134104", "6803-160223-164826", "6814-170128-121951", "68150402-113151", "68150402-144056", "68150403-015716", "68150404-113848", "68150406-152433", "68150407-140826", "68150407-144135", "68150407-180808", "6821-161130-144758", "6824-150408-124106", "6835-180208-222923", "6847-160309-144345", "6856-150605-145326", "6860-160131-154850", "6861-150422-153721", "6863-171214-115350", "6865-170102-162623", "6870-150410-115811", "6880-150409-151334", "6887-180127-160016", "6900-151223-152448", "6905-170911-160320", "6907-170110-124333", "69150402-105714", "69150404-145213", "69150404-162418", "69150404-171926", "69150405-124021", "69150406-144209", "69150406-174533", "6916-180221-145748", "6936-150414-142652", "6938-150411-145450", "6939-161229-125054", "6945-150408-105112", "6945-150408-110500", "6946-160528-100557", "6947-160304-161325", "6950-170522-184238", "6950-180201-112952", "6953-180202-135203", "6954-170520-130548", "6957-170915-155204", "6965-150414-114950", "6967-170522-130121", "6970-150417-090333", "6978-160307-121248", "6980-150408-142820", "6981-170112-104726", "6991-150420-130530", "6991-160108-153948", "6996-151223-150556", "6999-161220-164130", "7004-160219-111846", "7008-160307-141820", "7009-180126-210613", "70150402-150611", "70150403-145538", "70150405-151757", "70150406-160033", "70150407-142347", "7016-161024-152425", "7021-151217-115954", "7022-161025-113405", "7025-180208-155728", "7030-170107-122049", "7040-161229-114700", "7050-170131-192946", "7061-150420-105321", "7065-170915-154035", "7073-150418-124548", "7077-171128-114354", "7090-150417-130402", "7094-150427-205136", "7105-170208-083051", "7111-170111-113028", "7114-170213-180625", "7115-171212-162116", "71150401-160927", "71150402-122058", "71150402-153329", "71150404-131652", "71150404-134718", "71150404-233209", "7116-180201-174044", "7130-150428-112901", "7136-170918-135738", "7137-150408-140831", "7138-170127-100321", "7140-160310-133505", "7148-171125-120143", "7149-150507-192126", "7152-160225-110807", "7160-150407-204742", "7163-170129-021621", "7164-150422-204237", "7167-150408-170456", "7173-170104-134234", "7178-180208-173403", "7186-150422-130450", "7190-150416-102348", "7195-180129-130106", "7197-150417-181337", "7206-160311-141220", "7211-150409-131146", "7211-160302-145355", "72150330-203208", "72150401-095414", "72150401-171750", "72150401-224326", "72150402-163454", "72150403-071321", "72150404-140928", "72150404-150052", "72150404-151751", "72150407-143538", "7216-150408-153009", "7219-170523-202302", "7227-170207-190900", "7233-150519-175453", "7253-160309-112152", "7254-150409-111929", "7258-150409-102513", "7262-170211-180641", "7263-170128-114432", "7264-160225-131221", "7269-170523-150603", "7274-160116-143639", "7274-160320-191731", "7275-160301-132339", "7283-160314-173516", "7286-170104-160426", "7293-150414-132411", "7301-150414-112533", "7308-170207-114320", "7313-150409-150255", "73150403-160105", "73150403-184501", "73150407-124535", "73150407-125907", "73150407-153702", "7316-150408-120539", "7329-171227-111349", "7330-150408-112254", "7331-150414-124517", "7331-150422-101654", "7331-170913-110206", "7332-171214-192349", "7344-150418-125845", "7344-170208-165015", "7349-150409-145546", "7355-150413-180225", "7356-160225-112618", "7356-161103-155127", "7362-150412-131129", "7376-150502-153240", "7377-150414-181559", "7379-170125-103830", "7383-150408-170052", "7385-180202-115303", "7391-160304-125258", "7392-150507-152523", "7392-170911-165820", "7393-170210-175855", "7393-170523-205444", "7395-160308-154146", "7396-170523-211803", "7401-170201-135706", "7406-170127-105338", "7413-170523-224350", "74150329-123802", "74150404-132947", "74150407-110315", "7440-160315-210929", "7442-160529-180103", "7443-170523-213227", "7447-160223-160623", "7454-171009-094302", "7461-150427-072900", "7461-151223-122616", "7479-150421-105533", "7484-171223-135049", "7490-150411-125727", "7490-170110-120834", "7490-180202-170112", "7492-150417-151225", "7503-160312-222447", "7503-161121-152932", "7504-160307-120525", "75150329-075458", "75150330-224454", "75150331-181534", "75150401-123052", "75150406-114801", "75150407-174713", "7516-150416-103352", "7516-160130-171629", "7519-160309-120141", "7521-150701-152543", "7528-180104-151357", "7534-150410-120811", "7541-170914-102048", "7542-160310-161113", "7546-171227-153645", "7553-170204-164547", "7555-170601-152305", "7558-150408-191150", "7558-160213-151019", "7565-160320-175703", "7567-160601-153731", "7584-160218-124328", "7589-150622-220108", "7594-150430-103524", "7595-161207-140122", "7598-150414-111151", "7613-160315-162203", "76150330-131131", "76150402-125750", "76150402-165017", "76150404-132522", "76150404-141308", "76150406-121030", "7617-150420-164551", "7617-180207-150550", "7623-161229-113737", "7624-160311-164633", "7631-150410-124109", "7639-170202-140733", "7654-150425-192119", "7656-170520-113447", "7657-161207-110654", "7659-171227-132507", "7660-180202-173444", "7663-150408-120134", "7664-170110-135411", "7672-170201-130726", "7682-161028-141703", "7684-161024-224045", "7685-160129-134207", "7690-160529-171804", "7691-160804-172710", "7697-161231-094117", "7703-170208-120019", "7708-160311-153502", "77150403-144321", "77150404-153002", "77150405-153609", "77150406-101642", "7726-150413-123823", "7727-161104-160427", "7727-180208-134717", "7730-150418-150918", "7735-150412-124002", "7735-170202-021128", "7735-170207-015614", "7737-150423-151340", "7737-180130-121442", "7741-180131-160943", "7749-150415-234927", "7749-151219-181423", "7757-170109-141614", "7758-161024-102822", "7762-170213-183458", "7766-150408-114311", "7770-170105-142123", "7780-180202-171308", "7783-160204-153400", "7783-171223-170550", "7788-161025-163652", "7802-180130-105715", "7804-150410-150606", "7805-160227-114301", "7811-160307-100456", "7811-170522-185130", "78150331-095755", "78150331-155131", "78150403-113956", "78150404-162446", "78150405-203601", "78150407-121739", "78150407-133428", "78150407-162814", "7825-160127-150135", "7831-150507-204325", "7832-171227-095236", "7837-161128-193550", "7837-170108-205317", "7837-171225-073920", "7846-150424-014125", "7846-170207-161315", "7852-150410-181459", "7855-180207-144034", "7859-170207-230403", "7860-150409-093946", "7860-180207-125814", "7865-160228-110547", "7876-170125-123105", "7892-150622-113440", "7892-170208-120939", "7893-170206-154526", "7905-170203-112653", "7906-160306-151937", "79150402-112951", "79150403-133318", "79150406-120644", "7919-160528-201710", "7925-180104-111645", "7948-150623-175333", "7952-160304-174835", "7957-160305-202129", "7964-151223-150848", "7972-150424-152550", "7984-170103-155512", "7986-151221-121137", "7992-161022-112313", "7993-160123-141250", "7999-170131-184403", "7999-170915-105640", "8003-161219-105150", "8004-170911-225045", "8008-150702-132923", "80150331-104357", "80150404-161319", "80150406-145457", "8045-180202-145914", "8051-170201-192944", "8052-160317-163026", "8053-170107-153808", "8064-180207-165114", "8068-171121-113931", "8084-170106-141141", "8088-170103-103649", "8096-150410-101848", "8097-170112-011609", "81150402-112837", "81150406-145946", "8123-180207-180154", "8127-150409-121259", "8129-170916-103002", "8132-150409-172532", "8148-170523-140155", "8162-150419-091405", "8166-180201-155956", "8175-180219-111532", "8184-150418-160647", "8196-180202-172813", "8198-170131-190831", "8199-160218-202931", "8199-161128-133048", "8212-170127-111118", "8214-180208-155336", "82150401-193318", "82150402-165457", "82150404-100029", "82150404-140906", "82150405-165030", "82150406-004933", "82150406-112355", "8217-150410-144924", "8223-150413-130432", "8232-160309-131947", "8234-170105-124758", "8235-180201-145327", "8236-171224-222547", "8242-160525-114043", "8246-150425-182432", "8251-170201-063604", "8264-150408-150608", "8266-170126-100222", "8273-150413-133528", "8276-150620-203646", "8276-171207-141230", "8286-160529-184616", "8288-160302-131732", "8296-150408-112959", "8310-150504-134212", "8313-160209-172511", "8314-160529-155357", "83150331-223450", "83150404-160632", "83150404-162428", "83150406-122012", "83150406-124406", "83150406-131744", "83150406-133331", "83150406-144419", "83150406-190942", "8320-160308-132419", "8338-170117-120418", "8339-150504-133749", "8340-151223-164445", "8348-170202-174618", "8348-170522-171005", "8349-151216-215143", "8353-150426-150841", "8363-170915-160832", "8363-170918-150425", "8366-160129-224526", "8382-150419-134710", "8386-180131-163104", "8394-170107-134712", "8397-170522-122330", "8412-170213-000212", "8413-170111-131021", "84150406-134458", "84150406-143933", "84150406-155716", "84150407-164929", "8423-150407-204612", "8423-170201-112533", "8423-180202-132307", "8429-150421-124351", "8441-160319-161629", "8446-170119-141054", "8450-170522-150835", "8453-150417-180221", "8456-150408-105204", "8458-170126-070010", "8464-170129-101557", "8485-150501-153652", "8486-170117-154715", "8488-170521-214919", "8495-180202-090027", "8497-161125-135857", "8500-160203-162140", "8504-170523-150741", "8505-161031-162642", "8505-170522-170146", "8507-180202-193045", "8509-170108-174702", "85150330-182657", "85150331-125201", "85150401-152830", "85150402-161743", "85150403-230538", "85150404-104708", "85150404-121036", "85150404-150450", "85150405-220535", "85150407-161927", "8516-170523-172116", "8519-150428-103953", "8519-170120-131034", "8523-160218-125327", "8525-150524-215348", "8526-150408-163516", "8529-180219-160550", "8559-150408-174854", "8559-150414-164627", "8566-170131-183003", "8569-150426-133149", "8569-170913-124549", "8574-150506-123244", "8579-160109-111853", "8583-160314-145126", "8585-180130-140303", "8586-171220-144129", "8592-150424-101416", "8594-160226-115349", "8594-170914-131547", "8601-150407-215242", "8608-150409-110811", "8614-150502-134914", "86150329-133252", "86150330-214902", "86150404-152523", "86150406-150924", "86150407-162748", "86150407-165203", "8617-150507-162025", "8622-171125-123346", "8628-160306-231747", "8630-150409-133956", "8631-170126-075712", "8635-170131-235318", "8636-150416-130230", "8638-170131-052904", "8640-150504-132831", "8641-170523-145306", "8649-160525-151623", "8656-170119-144222", "8657-160307-045533", "8667-180208-205639", "8671-170523-202702", "8686-150419-155102", "8690-150409-173054", "8710-160123-150322", "8713-150415-223120", "87150330-162853", "87150405-172820", "8732-180201-124258", "8733-161025-105702", "8733-171120-114422", "8735-150410-160617", "8736-150504-162728", "8741-150413-125346", "8741-180202-162322", "8742-161026-113357", "8748-150409-122037", "8748-150418-152733", "8763-171222-101259", "8769-160130-190541", "8771-160209-123747", "8772-150409-121657", "8776-160524-170553", "8777-161128-190236", "8779-161025-113529", "8784-170213-172950", "8787-160223-142327", "8789-160528-172939", "8792-150408-140040", "8792-160316-121428", "8795-160203-175048", "8801-150408-072304", "8803-170523-161809", "8804-170120-124339", "88150401-143550", "88150402-132154", "88150405-055945", "88150406-163814", "88150407-105745", "88150407-123130", "8817-180128-202102", "8818-150416-131333", "8835-150409-215651", "8843-160308-105802", "8851-170208-160447", "8860-170210-174656", "8866-150407-222739", "8869-171222-152359", "8883-170213-145216", "8884-171226-224049", "8886-160309-111629", "8894-180123-123932", "8905-150414-105738", "8905-171007-194736", "89150330-163653", "89150402-152459", "89150402-164401", "89150403-074443", "89150406-220242", "8916-180202-115622", "8923-161209-154209", "8923-180201-192015", "8934-170520-161054", "8945-150414-190947", "8966-170127-095009", "8966-171028-153855", "8968-161217-114914", "8979-160611-115937", "8981-170131-025200", "8990-150410-151058", "8990-180201-112944", "8993-180201-172018", "8994-160227-143825", "9003-150420-141939", "9006-150409-113541", "90150401-100832", "90150402-113500", "90150406-150306", "90150406-160310", "90150407-101404", "90150407-133919", "90150407-144628", "9034-161025-121526", "9048-160525-200023", "9053-151223-174358", "9063-160121-151725", "9063-180208-220957", "9064-150425-180139", "9079-150410-103817", "9083-150413-130544", "9084-180131-155141", "9091-150409-165116", "9099-160529-181342", "9101-150414-152316", "9107-160305-165217", "91150331-110143", "91150402-124503", "91150404-163458", "91150406-023716", "91150406-101941", "91150406-141429", "91150407-125932", "91150407-164824", "9117-150417-111650", "9122-180204-153030", "9123-151218-225853", "9126-170915-115651", "9127-150429-155346", "9130-150408-153601", "9132-170208-133934", "9145-150507-182141", "9157-160203-163715", "9162-180208-203320", "9165-150413-144341", "9173-170118-120624", "9174-170522-071728", "9175-161210-122028", "9185-171224-202234", "9188-171102-144554", "9191-150408-103715", "9193-160204-123623", "9196-170128-035827", "9199-160309-144048", "9202-170118-101846", "9208-160213-100842", "9213-170131-224323", "92150331-160512", "92150401-140923", "92150401-162534", "92150401-202528", "92150402-152307", "92150405-183642", "92150406-102807", "92150407-114946", "92150407-153555", "9217-170106-124756", "9218-170126-104946", "9232-170119-104104", "9234-160215-121151", "9239-170109-115744", "9241-170913-144954", "9242-170523-172200", "9251-150414-062519", "9251-160305-194801", "9257-160226-122157", "9257-170916-062524", "9257-171227-172832", "9263-160209-165332", "9268-160306-002552", "9268-160528-134441", "9270-151221-162216", "9281-170523-112402", "9285-180202-153742", "9293-150409-152758", "9296-160225-161725", "9298-160306-173510", "9312-170125-121922", "93150401-074526", "93150402-120841", "93150402-131530", "93150407-151138", "9324-171225-083429", "9325-160129-142318", "9330-170520-144722", "9333-160313-223222", "9349-160310-130516", "9352-150408-112832", "9369-150417-214628", "9371-150410-115605", "9371-160302-155838", "9376-170131-185206", "9377-150419-145913", "9377-161126-125717", "9378-150408-150232", "9382-160314-115007", "9387-150411-133252", "9387-160301-152443", "9393-160311-170945", "9400-160124-113800", "9400-170129-112153", "9403-180207-133701", "9408-150418-064356", "9408-170131-052211", "94150330-113934", "94150331-122435", "94150331-164036", "94150331-225018", "94150401-151409", "94150402-111241", "94150402-115132", "94150402-165951", "94150403-212613", "94150405-001740", "94150405-004032", "94150405-111226", "94150406-114354", "94150407-115002", "94150407-130011", "9420-150409-104558", "9420-150501-093802", "9438-170119-132009", "9444-160314-162918", "9447-180212-120756", "9448-171220-142737", "9450-160302-134955", "9470-180206-132125", "9477-150411-120806", "9481-161206-124104", "9484-171223-154701", "9499-150411-114632", "9501-160303-113550", "9510-170112-102057", "9515-170120-113720", "95150331-110148", "95150404-122605", "95150404-214824", "95150406-144038", "95150406-150453", "95150406-190716", "95150407-160455", "95150407-165536", "9521-150413-110745", "9521-160305-153603", "9524-160527-113642", "9526-170102-112112", "9544-160119-115501", "9548-171011-150852", "9549-160309-161222", "9552-160320-103611", "9552-171124-165313", "9562-180202-121510", "9565-171215-115921", "9568-170207-150317", "9573-170522-165517", "9579-150409-143445", "9583-180202-045640", "9585-150420-105758", "9590-160306-003250", "9595-150408-135620", "9595-180202-145749", "9604-170615-133142", "9610-170109-100316", "9612-150426-155734", "9614-150414-150648", "96150401-170945", "96150405-090312", "96150406-213444", "96150407-121047", "96150407-181227", "9628-150409-172430", "9629-150408-125905", "9638-160311-171407", "9643-150414-154048", "9651-160314-214710", "9662-180204-211536", "9665-161207-131759", "9674-170522-151400", "9679-150408-140728", "9683-160221-140840", "9683-170601-162530", "9685-150507-145511", "9692-150413-174620", "9698-171026-131429", "9704-161210-115658", "9708-150410-144632", "9711-160301-110434", "97150331-094004", "97150331-154428", "97150401-122002", "97150402-120847", "97150402-154036", "97150404-203443", "97150406-124918", "97150407-113543", "97150407-114851", "97150407-123426", "9723-180131-105948", "9725-170207-021441", "9727-161126-122908", "9733-150415-095034", "9745-160529-150403", "9745-171225-131721", "9747-171009-130842", "9750-150420-141510", "9760-150512-160713", "9760-170916-124258", "9768-171227-113501", "9770-170201-133826", "9784-150410-120836", "9785-170104-124821", "9785-170601-153856", "9790-150409-121614", "9795-150409-135353", "9801-170110-065949", "9805-150416-115518", "9805-160316-211552", "9806-180207-174847", "9807-150412-180800", "98150403-213601", "98150404-151104", "98150405-185754", "98150406-133128", "98150406-145704", "98150407-100015", "9817-150413-181840", "9818-171227-144625", "9828-150501-044104", "9835-160126-200806", "9837-170520-132409", "9839-150426-103701", "9840-161216-115247", "9841-170213-080749", "9853-160314-115912", "9857-170916-131945", "9859-170107-105738", "9862-151217-115409", "9862-170128-132409", "9867-180208-122028", "9869-150408-120428", "9872-171020-153028", "9874-161022-140522", "9878-150430-140038", "9878-180201-105606", "9882-180203-163139", "9884-170105-162951", "9891-160225-100923", "9901-170105-125815", "9902-150409-124700", "9905-170112-093839", "9906-160125-181402", "9914-150522-154121", "9918-170122-172510", "9922-170523-135124", "9926-150413-123146", "9927-150411-101844", "9928-150413-161333", "9931-150409-123511", "9932-180201-151804", "9946-150410-132838", "9980-170102-120339", "9986-170111-100814" };
           
            List<jntuh_college_basreport> basFlagTotal = db.jntuh_college_basreport.Where(z => RegNos.Contains(z.RegistrationNumber)).Select(e => e).ToList();
            var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => RegNos.Contains(e.RegistrationNumber)).Select(e => e).ToList();
            var Jntuh_registered_faculty = db.jntuh_registered_faculty.Where(r => RegNos.Contains(r.RegistrationNumber)).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).ToList();

            int days = Convert.ToInt32(ConfigurationManager.AppSettings["Days"]);
            int months = Convert.ToInt32(ConfigurationManager.AppSettings["Months"]); ;

            List<SelectListItem> MonthNameAndIds = new List<SelectListItem>();
            MonthNameAndIds.Add(new SelectListItem() { Text = "January", Value = "1" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "February", Value = "2" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "March", Value = "3" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "April", Value = "4" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "May", Value = "5" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "June", Value = "6" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "July", Value = "7" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "August", Value = "8" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "September", Value = "9" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "October", Value = "10" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "November", Value = "11" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "December", Value = "12" });

            List<CollegeWiseBasFlagFacultyNew> CollegeWiseBasFlagFaculty = new List<CollegeWiseBasFlagFacultyNew>();

            var SelectedMonths = MonthNameAndIds.Where(e => e.Text == "July" || e.Text == "August" || e.Text == "September" || e.Text == "October" || e.Text == "November" || e.Text == "December").Select(e => e.Text).ToArray();

            int CollegeWiseFacultyBasCount = 0;
            foreach (var EachFaculty in RegNos)
            {
                bool FalgBas = true;
                int FacultyBasFlagCount = 0;
                var FacultyMonthCount = 0;
                var FacultyData = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == EachFaculty).Select(e => e).FirstOrDefault();
                var BasData = basFlagTotal.Where(e => e.RegistrationNumber.Trim() == EachFaculty).Select(e => e).ToList();

                CollegeWiseBasFlagFacultyNew BasFacultyClass = new CollegeWiseBasFlagFacultyNew();
                BasFacultyClass.CollegeCode = jntuh_college.Where(e => e.id == FacultyData.collegeId).Select(e => e.collegeCode).FirstOrDefault();
                BasFacultyClass.CollegeName = jntuh_college.Where(e => e.id == FacultyData.collegeId).Select(e => e.collegeName).FirstOrDefault();
                BasFacultyClass.Registraionnumber = EachFaculty;
                BasFacultyClass.Name = Jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == EachFaculty.Trim()).Select(e => e.FirstName + " " + e.MiddleName + " " + e.LastName).FirstOrDefault();
                BasFacultyClass.Mobile = Jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == EachFaculty.Trim()).Select(e => e.Mobile).FirstOrDefault();
                BasFacultyClass.Email = Jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == EachFaculty.Trim()).Select(e => e.Email).FirstOrDefault();
                BasFacultyClass.Department = jntuh_department.Where(e => e.id == FacultyData.DepartmentId).Select(e => e.departmentName).FirstOrDefault();
                string date = BasData.Select(e => e.joiningDate).FirstOrDefault().ToString();
                if (date == null || date == "")
                {

                }
                else
                {
                    BasFacultyClass.BASJoiningDate = Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                }
                BasFacultyClass.TotalWorkingDays = BasData.Select(e => e.totalworkingDays).Sum();
                BasFacultyClass.TotalPresentDays = BasData.Select(e => e.NoofPresentDays).Sum();
                FacultyMonthCount = BasData.Count();

                var BasDataNew = BasData.Where(m => SelectedMonths.Contains(m.month)).Select(e => e.month).Count();
                var NotRequiredMonths = BasData.Where(m => m.month == "January" || m.month == "February" || m.month == "March").Select(e => e.month).Count();

                foreach (var item in BasData)
                {
                    int? totalworkingdays = item.totalworkingDays;
                    int? totalpresentdays = item.NoofPresentDays;
                    int? RequiredPresentDays = days;

                    if (item.month == "July")
                    {
                        BasFacultyClass.JulyPresentDays = item.NoofPresentDays;
                    }
                    else if (item.month == "August")
                    {
                        BasFacultyClass.AugustPresentDays = item.NoofPresentDays;
                    }
                    else if (item.month == "September")
                    {
                        BasFacultyClass.SeptemberPresentDays = item.NoofPresentDays;
                    }
                    else if (item.month == "October")
                    {
                        BasFacultyClass.OctoberPresentDays = item.NoofPresentDays;
                    }
                    else if (item.month == "November")
                    {
                        BasFacultyClass.NovemberPresentDays = item.NoofPresentDays;
                    }
                    else if (item.month == "December")
                    {
                        BasFacultyClass.DecemberPresentDays = item.NoofPresentDays;
                    }
                    else if (item.month == "January")
                    {
                        BasFacultyClass.JanuaryPresentDays = item.NoofPresentDays;
                    }
                    else if (item.month == "February")
                    {
                        BasFacultyClass.FebruaryPresentDays = item.NoofPresentDays;
                    }
                    else if (item.month == "March")
                    {
                        BasFacultyClass.MarchPresentDays = item.NoofPresentDays;
                    }

                    if (totalpresentdays >= RequiredPresentDays)
                    {
                        FacultyBasFlagCount++;
                        if (item.month == "July")
                        {
                            BasFacultyClass.July = item.month;

                        }
                        else if (item.month == "August")
                        {
                            BasFacultyClass.August = item.month;

                        }
                        else if (item.month == "September")
                        {
                            BasFacultyClass.September = item.month;

                        }
                        else if (item.month == "October")
                        {
                            BasFacultyClass.October = item.month;

                        }
                        else if (item.month == "November")
                        {
                            BasFacultyClass.November = item.month;

                        }
                        else if (item.month == "December")
                        {
                            BasFacultyClass.December = item.month;

                        }
                        else if (item.month == "January")
                        {
                            BasFacultyClass.January = item.month;

                        }
                        else if (item.month == "February")
                        {
                            BasFacultyClass.February = item.month;

                        }
                        else if (item.month == "March")
                        {
                            BasFacultyClass.March = item.month;

                        }
                    }
                    else
                    {

                    }


                }
                var ReqiredMonthCount = FacultyMonthCount - months;
                if (FacultyBasFlagCount >= ReqiredMonthCount)
                {
                    FalgBas = false;
                }
                else
                {
                    FalgBas = true;
                }

                if (FalgBas == false)
                {
                    BasFacultyClass.Cleared = "Cleared";
                    CollegeWiseFacultyBasCount++;
                }
                else
                {
                    BasFacultyClass.Cleared = "NotCleared";
                }
                CollegeWiseBasFlagFaculty.Add(BasFacultyClass);
            }



            if (command == "Print")
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename = CollegeBASFacultyList.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/BASFlagDeficiencyFaculty/_BASFlagFacultyDataBasedOnRegistratinNumberExcel.cshtml", CollegeWiseBasFlagFaculty);

            }
            else
            {
                return View(CollegeWiseBasFlagFaculty);
            }
            return View();
        
            #endregion
            #region Newcode
            //// string[] RegNos = new string[] { "0007-180208-115525", "0009-150410-223407", "00150404-122843", "00150404-132143", "00150404-142150", "00150406-124851", "00150407-105515", "0017-150408-155908", "0017-160301-122830", "0033-160306-192726", "0037-150418-151850", "0043-170522-131138", "0045-180129-222401", "0055-150409-105615", "0062-150414-084913", "0063-171227-194032", "0065-161128-150056", "0070-160311-123513", "0070-170110-094030", "0076-150412-140821", "0078-170523-170724", "0079-180207-200335", "0086-170117-142511", "0089-160314-175435", "0089-161209-145730", "0091-150427-145248", "0104-150414-142722", "0105-180110-112802", "01150330-195657", "01150330-222845", "01150331-125001", "01150331-151224", "01150331-201207", "01150402-162629", "01150404-110704", "01150406-161150", "01150407-151610", "01150407-151833", "0123-150410-101600", "0138-150408-131246", "0149-150408-213453", "0159-150419-103024", "0164-171222-094759", "0171-150407-213332", "0183-150623-110537", "0183-161025-172827", "0188-170914-104044", "0191-171129-114721", "0197-170114-211327", "0206-160527-190725", "0209-150410-101942", "0212-150418-165845", "0214-160301-202618", "02150331-120556", "02150402-134808", "02150403-220700", "02150404-110529", "02150404-113151", "02150406-222518", "0228-170523-212632", "0229-150508-113304", "0232-160312-200514", "0237-170208-160211", "0249-170523-120150", "0254-150408-125530", "0255-150418-113623", "0261-150420-121809", "0264-170523-191942", "0266-160320-155426", "0271-160529-123746", "0278-150420-155450", "0289-150409-112844", "0290-161128-113918", "0294-150427-103920", "0297-161126-112913", "0298-180202-155940", "0304-150423-124746", "0311-150415-172025", "0311-170122-203935", "0314-150420-143233", "03150331-231935", "03150401-222957", "03150404-221644", "03150406-164545", "0316-170108-121812", "0321-150413-212744", "0328-150409-104223", "0330-150430-152111", "0334-150409-132111", "0338-150505-123322", "0340-170213-144245", "0343-150416-102005", "0349-150624-185052", "0354-170523-153347", "0364-170131-014034", "0368-161227-154357", "0370-150418-131438", "0377-150409-133622", "0383-170112-114722", "0387-171025-174106", "0389-171116-123400", "0391-160303-162613", "0398-160315-143304", "0398-170521-100904", "0399-161228-131657", "0406-150418-131001", "0411-161202-112915", "0411-180127-105509", "04150402-162346", "04150404-125957", "04150404-162859", "04150404-164306", "04150407-044411", "04150407-173505", "0422-160304-224140", "0429-150420-110211", "0433-170522-175603", "0435-150420-195826", "0438-171122-122044", "0452-160310-154804", "0457-150408-215923", "0462-160305-131508", "0472-160313-111912", "0479-180110-220632", "0481-161025-134621", "0494-150411-212235", "0501-150415-145453", "0508-150427-161539", "0512-161123-121032", "05150404-114612", "05150404-143934", "05150404-150310", "05150404-160107", "05150406-114345", "05150407-141345", "0518-170201-100649", "0520-180102-220148", "0522-150427-225629", "0527-170623-161203", "0534-160222-124511", "0553-171226-122921", "0554-150409-140823", "0564-170208-180025", "0565-170207-220306", "0567-161212-102048", "0574-150410-111014", "0579-150506-133602", "0591-150424-142958", "0595-180201-161147", "0598-150408-175353", "0611-180208-172255", "0612-160919-173944", "0612-161210-123014", "0614-160314-122412", "06150331-111246", "06150331-130643", "06150401-152726", "06150402-153653", "06150404-113145", "06150404-132713", "06150405-115535", "06150406-115949", "06150406-133044", "06150407-113602", "06150407-131623", "06150407-133809", "06150407-142347", "0619-180131-095845", "0622-150413-150200", "0627-171208-145014", "0635-170131-011730", "0640-150412-202441", "0647-170521-101358", "0653-160220-110651", "0661-161031-001234", "0664-160314-171217", "0666-150408-135631", "0670-150419-142418", "0678-150409-134601", "0680-160529-145540", "0684-150427-123326", "0690-160224-143435", "0695-160209-105750", "0695-170116-211154", "0695-180202-135315", "0697-170521-115623", "0704-180208-133828", "0707-161025-140625", "0714-150408-163208", "07150330-103619", "07150331-132917", "07150402-114612", "07150402-165852", "07150404-125837", "07150407-000328", "07150407-134148", "0723-170206-142206", "0723-170523-223421", "0732-150413-223536", "0755-180208-162637", "0756-150411-130143", "0765-171222-151335", "0769-160305-224725", "0772-160309-102912", "0778-150410-125550", "0788-180208-101027", "0790-170213-204405", "0791-150411-144110", "0793-160529-180203", "0795-171124-160448", "0796-170522-142021", "0798-171220-133506", "0804-170913-180210", "0807-150408-125629", "0810-150421-120331", "0814-160314-164124", "08150404-232135", "08150407-111738", "08150407-134210", "0816-150409-144756", "0816-151231-123436", "0819-180208-214939", "0827-161022-175318", "0837-161129-122004", "0843-150415-195740", "0846-170111-120917", "0851-150409-170037", "0861-150413-142903", "0861-170916-141035", "0869-171227-162556", "0871-160315-145356", "0874-161103-150002", "0879-160314-164629", "0884-160315-001044", "0895-171222-113335", "0897-170915-161247", "0899-150419-090852", "0900-170523-223048", "0902-150506-165503", "0903-160203-143127", "0913-150408-140109", "09150330-185602", "09150330-233548", "09150331-151951", "09150401-182955", "09150402-132227", "0918-150421-113133", "0920-171212-144956", "0931-160204-115104", "0936-150420-114612", "0941-150506-162018", "0947-170126-071811", "0948-160223-154330", "0954-171222-165544", "0962-160105-143546", "0963-150413-125053", "0972-170126-091920", "0972-170131-005308", "0973-150408-164314", "0973-161030-093220", "0978-170913-201423", "0996-170131-023348", "1001-150411-152829", "1011-150408-152618", "10150403-133250", "1016-170213-095525", "1019-150413-125218", "1023-170522-160534", "1025-171227-133210", "1028-150505-134104", "1034-150411-145802", "1038-150408-121059", "1039-160313-141051", "1044-171215-150733", "1046-161126-133019", "1059-170125-103246", "1061-160306-221302", "1062-170123-140150", "1065-161205-164740", "1067-150412-120929", "1075-161207-155320", "1076-180220-234151", "1082-160210-102657", "1089-150409-125700", "1089-170208-175418", "1093-180201-120151", "1098-160223-134532", "1112-161023-142238", "1112-170116-130834", "11150405-154738", "11150405-230141", "11150406-114427", "11150407-103414", "11150407-105535", "11150407-143836", "1131-160305-165031", "1134-160306-155739", "1148-160224-190015", "1154-160309-121215", "1161-180208-115655", "1167-170203-110751", "1184-180206-172014", "1187-160210-145559", "1192-170207-012809", "1193-150417-233334", "1193-180208-162745", "1194-161205-171645", "1195-170521-131948", "1195-170915-155908", "1196-150409-212536", "12150331-022559", "12150401-143701", "12150402-111935", "12150402-152647", "12150402-153633", "12150403-161135", "12150404-113532", "12150404-154539", "12150407-102002", "1225-161118-155245", "1226-160528-130834", "1227-170520-154157", "1235-160206-123159", "1235-170131-182202", "1236-161128-123648", "1237-171013-150434", "1239-160307-230546", "1244-150625-160443", "1250-180208-164904", "1255-170208-172403", "1258-180130-143059", "1260-170102-141915", "1263-160303-180642", "1266-150411-023607", "1267-150420-140505", "1272-150425-191320", "1277-150413-124141", "1279-161228-105341", "1280-170203-113618", "1286-170131-201713", "1291-180206-135319", "1297-160314-172232", "1298-170523-121831", "1303-160315-103412", "1304-160223-103618", "1312-161026-160254", "1313-171123-150641", "13150402-163345", "13150403-162842", "13150404-123131", "13150406-152951", "13150407-160822", "13150407-162554", "1324-150413-200901", "1325-160112-185306", "1328-150410-102032", "1332-170201-103806", "1334-160302-111355", "1335-170520-121434", "1336-170911-162939", "1339-150425-223809", "1348-150420-110623", "1348-180202-145152", "1362-170119-154826", "1373-150426-233659", "1375-171206-145411", "1377-150412-112802", "1379-150409-161635", "1380-170523-161434", "1382-170521-153441", "1386-171018-151234", "1412-161221-102343", "14150402-115407", "14150402-120027", "14150402-121235", "14150402-155349", "14150404-072853", "14150404-153743", "14150406-102925", "14150406-150508", "1422-180129-150230", "1423-160219-094426", "1423-170201-231848", "1426-150411-125711", "1434-150415-215340", "1439-150408-145337", "1439-150413-123239", "1445-160204-134145", "1459-180103-215657", "1466-150419-190407", "1479-171218-143112", "1480-150410-132808", "1482-150408-160212", "1493-170915-144826", "1497-171011-105355", "1500-150408-113059", "1510-170117-111457", "1511-150409-125904", "15150403-161534", "15150403-231653", "15150404-165417", "15150405-122355", "15150405-150802", "15150407-113619", "15150407-155302", "1516-150417-230501", "1517-160128-162616", "1519-171223-161429", "1523-150408-150209", "1538-160527-181237", "1539-150410-142048", "1539-170522-113513", "1540-180106-094729", "1546-150417-152331", "1550-170203-110650", "1551-160302-115953", "1566-160529-204221", "1572-150410-114653", "1572-171215-162418", "1583-150413-163325", "1585-180127-144655", "1586-150424-152459", "1592-160320-134143", "1593-150409-141240", "1595-180201-180017", "1597-160528-152545", "1603-180131-191237", "1608-150409-104757", "16150403-102930", "16150406-120431", "16150406-121938", "16150406-123557", "16150406-130251", "16150406-153238", "16150407-122941", "16150407-133453", "1620-161024-152052", "1623-170128-131044", "1632-170111-132912", "1633-170523-190006", "1636-170117-152946", "1643-150411-123804", "1653-150410-171027", "1657-180202-162022", "1658-160314-135529", "1665-160529-234630", "1693-170213-114848", "1696-150413-145115", "1703-180201-161103", "1704-180201-132810", "1706-150413-151751", "1708-170109-145010", "1715-170918-131805", "17150331-181635", "17150401-154407", "17150402-124709", "17150404-114317", "17150406-115454", "17150406-145030", "1717-171026-163553", "1719-170102-125403", "1720-150408-122540", "1724-160313-120949", "1724-170116-125032", "1732-161125-134800", "1736-161207-142623", "1738-161128-150415", "1739-170602-135119", "1740-150411-173641", "1744-161123-171157", "1745-150419-203504", "1745-151222-114535", "1747-170623-161422", "1747-180208-144342", "1774-170209-191821", "1776-151231-121925", "1778-161022-132131", "1778-170213-154848", "1783-160217-122916", "1786-161025-180517", "1791-150417-215632", "1792-180201-113733", "1799-170523-145346", "1803-170522-170602", "1804-150504-223903", "1812-161025-154059", "1814-150418-121540", "1815-170208-201455", "18150401-145131", "18150403-235223", "18150407-150227", "18150407-183429", "1821-161220-155939", "1828-150420-120347", "1834-170131-041404", "1834-180208-210350", "1855-150417-193607", "1874-171101-112851", "1881-160307-111846", "1884-180201-111249", "1886-161215-110840", "1887-150409-204914", "1889-160319-164942", "1898-150416-103606", "1902-170131-181438", "19150331-095841", "19150402-125919", "19150403-074232", "19150404-105700", "19150407-121546", "1923-150411-103741", "1928-160306-195235", "1932-160229-073352", "1933-170131-011734", "1938-171227-190729", "1941-150408-195342", "1959-160529-184524", "1968-150420-124535", "1981-170207-181640", "1985-160229-144455", "1987-170103-151550", "1988-161213-123229", "2002-170911-162313", "2006-160219-113546", "20150401-191342", "20150402-112655", "20150403-232819", "20150404-015659", "20150404-111254", "20150405-113026", "20150406-164452", "2023-170915-153502", "2025-170523-133203", "2026-150409-165352", "2027-150410-121044", "2031-161231-114645", "2041-160215-133636", "2051-150410-135522", "2054-160219-143542", "2054-170127-102825", "2054-170201-122900", "2073-150408-154940", "2080-150413-104303", "2084-170129-104621", "2097-150408-114705", "2098-151228-160426", "2112-160719-110859", "2115-150415-093549", "21150330-130127", "21150330-230024", "21150330-230655", "21150403-133944", "21150406-170045", "21150407-111354", "21150407-114517", "21150407-154836", "2117-180201-163235", "2129-160319-161407", "2136-170126-072826", "2139-170126-101355", "2147-160129-155537", "2152-150408-105423", "2167-170112-111334", "2173-151229-143024", "2195-150408-123749", "2198-160609-105544", "2214-150410-121050", "2215-150414-145324", "22150331-155326", "22150401-125835", "22150407-112351", "22150407-122732", "2220-170201-061419", "2223-150408-123716", "2231-160528-214659", "2234-171221-111633", "2237-170208-172133", "2238-150427-144228", "2243-161210-144310", "2257-150428-055331", "2257-170113-190851", "2264-150502-173109", "2275-160222-140546", "2285-170116-203354", "2286-170107-145605", "2295-170103-155344", "2299-150409-154511", "2305-150413-144834", "2310-160304-201247", "2315-150407-224158", "23150331-092724", "23150401-101417", "23150402-094942", "23150403-145605", "23150404-113644", "2324-171208-105321", "2333-170117-120120", "2336-161027-130400", "2338-160529-082400", "2344-180206-103028", "2351-150507-075609", "2353-170203-160228", "2355-170131-191226", "2357-160105-123059", "2368-150420-161213", "2368-150507-190142", "2368-170104-151452", "2386-150620-152649", "2386-151218-155645", "2387-180108-143722", "2392-180201-105143", "2396-171207-162649", "2399-170202-134902", "2409-180208-160922", "2413-170520-123446", "2415-150409-150012", "2415-160218-123944", "24150330-154256", "24150331-105215", "24150403-155438", "24150404-144929", "24150404-145458", "24150405-131544", "24150406-113409", "24150407-144012", "2422-150419-214353", "2422-170127-062248", "2423-160222-130130", "2425-180208-152024", "2426-160313-095146", "2439-150410-193012", "2439-160211-123708", "2440-170112-101852", "2447-150413-140800", "2447-170202-055024", "2451-150408-161052", "2453-170118-113454", "2455-150411-144816", "2459-161227-100217", "2460-171220-144420", "2468-150410-103024", "2468-151222-143411", "2477-150413-174417", "2478-170914-151129", "2482-160205-161712", "2488-160108-160128", "2488-170201-141648", "2488-170914-144209", "2489-170207-162302", "2493-150418-142538", "2500-160307-125857", "25150331-103600", "25150404-152451", "25150406-152025", "2518-150417-130617", "2526-170211-140330", "2529-160129-145834", "2533-160310-110814", "2534-171117-123628", "2552-160305-142949", "2560-150408-211950", "2563-170203-111533", "2578-160213-110706", "2580-160309-104648", "2589-160315-121543", "2605-160310-131957", "2606-170913-110531", "2611-170207-130847", "26150328-102440", "26150331-100957", "26150402-142054", "26150403-155145", "26150404-133450", "26150406-182344", "26150407-125656", "2618-150416-174141", "2625-151218-121808", "2627-180202-101038", "2639-170201-061450", "2641-150412-134821", "2644-170105-122433", "2657-170602-182604", "2660-160310-180718", "2661-180202-172741", "2665-170104-132243", "2672-170112-132640", "2677-150408-174826", "2694-150622-111010", "2697-150415-115426", "2697-180130-124807", "2699-150409-154605", "2704-170520-130953", "2705-150506-170154", "2705-160218-125920", "2709-170131-184117", "2709-170520-111218", "27150331-114004", "27150331-122933", "27150331-153714", "27150401-135253", "27150403-141104", "27150404-105355", "27150406-132203", "27150407-121048", "2718-160307-011834", "2724-150408-171821", "2726-150408-010119", "2727-150419-111029", "2736-170208-161002", "2741-170520-103020", "2747-150411-113720", "2748-150413-133433", "2753-161020-113059", "2759-150419-112924", "2767-170523-225515", "2770-161028-140111", "2772-160226-193511", "2773-170109-102839", "2775-150421-132007", "2776-150426-174034", "2784-161027-112616", "2787-150427-172144", "2802-150409-125545", "2803-160727-013739", "2809-150408-115117", "2812-180208-171137", "28150406-114527", "28150406-134111", "28150407-132807", "28150407-193405", "2817-160225-103934", "2818-170127-111644", "2822-150409-103812", "2823-180202-111019", "2825-150420-202531", "2831-151216-143314", "2832-160305-123343", "2836-150408-154433", "2839-150414-142831", "2840-150428-120751", "2847-180205-145703", "2860-170522-154549", "2867-150408-181929", "2870-150415-145856", "2872-150417-112832", "2875-160307-112908", "2876-150413-192246", "2880-160218-223616", "2883-161206-120759", "2886-161117-110449", "2892-160320-155417", "2901-171208-150453", "2903-160305-155631", "2909-160129-100443", "29150402-115939", "29150403-194155", "29150404-175402", "29150406-103116", "29150406-121824", "29150406-142128", "29150406-184245", "2919-170120-152602", "2921-171009-152053", "2924-150411-160319", "2924-160320-132525", "2931-150411-173542", "2944-161022-132123", "2947-170208-064336", "2949-160309-110305", "2951-170202-052356", "2954-150416-103048", "2961-160307-111238", "2971-161024-163038", "2976-150409-150941", "2982-170523-172149", "2984-170520-154553", "2991-150410-135537", "2997-171223-162403", "2999-170122-190110", "3011-150506-143432", "3015-150418-164955", "3015-161105-102504", "30150331-124825", "30150401-172054", "30150402-161632", "30150404-172610", "30150404-185435", "30150406-101910", "30150407-124348", "30150407-162249", "3022-150408-105758", "3023-150407-213026", "3024-160308-162234", "3024-170914-132719", "3040-170915-115017", "3042-150410-130143", "3042-161025-155320", "3046-150408-123016", "3050-170116-142118", "3052-171114-104657", "3055-161125-201020", "3068-160305-201328", "3071-171124-124104", "3077-161025-092554", "3079-160528-224153", "3108-160320-201556", "3109-180201-163126", "31150404-102907", "31150404-140528", "31150404-145725", "31150404-195745", "31150406-132701", "3119-171016-164221", "3126-170131-010521", "3127-160529-195347", "3127-170126-034042", "3132-150426-111616", "3133-150410-105936", "3138-180131-233327", "3146-150410-162710", "3149-170109-134623", "3150-150413-162254", "3163-150413-211938", "3164-170208-062428", "3173-170914-150723", "3174-170207-172505", "3175-170913-105811", "3176-170601-161536", "3176-180202-133056", "3186-171227-091033", "3193-160302-125505", "3194-151223-155854", "3198-170523-121112", "3206-180202-181333", "3210-160108-115118", "3210-170131-042425", "32150330-184849", "32150331-101020", "32150331-122023", "32150331-140308", "32150403-191223", "32150404-153853", "32150404-172740", "32150406-151851", "3216-150420-102634", "3221-180208-163912", "3226-150409-144409", "3226-150515-114327", "3228-180202-190533", "3229-150419-022130", "3248-150427-153018", "3252-150408-154953", "3254-170208-051838", "3259-161221-153152", "3264-161024-213028", "3280-170201-144304", "3284-160315-130736", "3285-150413-145210", "3286-171227-145359", "3292-150420-162826", "3296-170118-103603", "33150331-120220", "33150402-123137", "33150403-121738", "33150404-154141", "33150406-143448", "33150407-111134", "33150407-130622", "3332-160309-114126", "3333-161213-134400", "3335-180201-145236", "3345-150409-122730", "3346-160213-161530", "3357-171218-144646", "3370-160315-134028", "3371-160303-174942", "3371-170129-075834", "3388-170129-082213", "3403-180201-115811", "3407-150410-145259", "34150330-174403", "34150401-111127", "34150401-142710", "34150406-131850", "3417-150410-102238", "3420-170523-203751", "3422-180201-131330", "3425-150418-120359", "3431-160219-135556", "3432-161231-121013", "3432-170129-072851", "3433-160302-170919", "3436-161231-133451", "3439-160220-111619", "3448-170131-225634", "3453-150412-185020", "3465-150411-165219", "3465-160120-121726", "3467-150409-155417", "3473-180201-170519", "3477-171227-113508", "3483-170126-095859", "3487-170912-102813", "3489-150422-155607", "3495-180201-150702", "3503-160314-112537", "3509-150410-115632", "3510-170111-130947", "35150331-203743", "35150402-142741", "35150404-152522", "35150405-223509", "35150406-172430", "35150407-142303", "3531-161124-122237", "3532-170213-192053", "3539-150408-131514", "3540-180208-183821", "3556-150409-105506", "3558-171129-141433", "3572-170523-162953", "3573-170207-162957", "3582-150413-152652", "3591-150410-125429", "3592-160313-222024", "3593-170207-001055", "3611-150415-152649", "3612-161202-125003", "3615-150411-112857", "3615-160312-133633", "36150403-130305", "36150406-161812", "36150407-100632", "36150407-121220", "3616-150410-102804", "3624-150409-132438", "3624-161027-121204", "3627-160314-170421", "3630-160306-210237", "3634-150428-100300", "3639-160315-010107", "3645-150506-161115", "3649-150409-094050", "3654-160310-153344", "3659-170131-043726", "3664-170207-113804", "3668-150419-121933", "3675-170117-101707", "3677-151228-155128", "3697-180124-130440", "3699-150410-132226", "3711-160320-172323", "3714-150408-151324", "37150331-140447", "37150331-201236", "37150404-135532", "37150404-142550", "37150406-133923", "37150406-142515", "3719-150409-222430", "3724-170213-125740", "3730-180207-164802", "3743-150409-112440", "3743-170126-121220", "3748-160320-163418", "3759-170131-051039", "3759-170207-200452", "3772-161221-161618", "3774-150408-122959", "3774-170212-091314", "3780-150424-125834", "3786-150408-101716", "3792-170915-171037", "3800-160108-113339", "3804-170203-234126", "38150331-113831", "38150331-114808", "38150402-131017", "38150403-104851", "38150406-113008", "38150406-151849", "38150406-153922", "38150407-114922", "38150407-141045", "3820-180207-153748", "3824-160315-163757", "3825-151218-135715", "3826-170107-145702", "3826-170129-115321", "3826-171227-233704", "3827-160311-182024", "3828-161019-161619", "3830-171206-150315", "3832-160221-111516", "3844-170208-174725", "3846-150408-114505", "3847-150410-113219", "3855-170213-144041", "3864-171223-150936", "3869-150410-152514", "3870-160314-121218", "3885-160209-102257", "3890-170523-223725", "3893-170522-131419", "3907-150420-152803", "3915-150506-163007", "39150402-122047", "39150404-213627", "39150406-112650", "39150406-124038", "39150407-121907", "39150407-125323", "3918-150420-101926", "3930-160204-155854", "3931-160526-154811", "3936-150418-164408", "3939-160526-122855", "3952-150408-115449", "3953-160102-113841", "3961-150424-120438", "3962-150515-123610", "3976-170915-180353", "3983-150511-233602", "3992-150419-080217", "4005-160216-090715", "4007-150415-145649", "4007-160310-100844", "40150402-170443", "40150406-124742", "40150406-151446", "40150407-125423", "4024-150414-201804", "4032-180201-151029", "4036-170208-040826", "4045-150418-063749", "4046-150408-180421", "4049-150501-153313", "4049-160225-135523", "4054-170130-052316", "4054-170204-110401", "4066-170116-153454", "4075-171128-125052", "4079-180208-231222", "4098-161213-112929", "4099-170522-122816", "4100-150421-121555", "4101-150411-105236", "4101-150504-170027", "4107-150425-151419", "4110-160120-144233", "4112-171222-171302", "41150402-133226", "41150404-113040", "41150406-010905", "4135-150424-152211", "4141-150427-143138", "4142-171224-215950", "4144-150408-152803", "4147-150415-121607", "4153-170115-172006", "4157-170522-165424", "4157-170913-131650", "4179-180202-163132", "4189-180201-202900", "4207-161020-164022", "4212-170520-154800", "4213-161128-093740", "4215-161223-144848", "4215-180208-161720", "42150331-113515", "42150406-152311", "42150407-110147", "42150407-123232", "42150407-154522", "4232-170522-141133", "4235-170201-133626", "4243-160306-074048", "4245-150408-132734", "4248-180202-171805", "4253-150418-162514", "4256-170105-144438", "4258-150413-171320", "4265-170522-122220", "4269-180207-145304", "4274-150408-160332", "4275-160308-133315", "4286-160528-145039", "4287-170523-172633", "4297-180201-101513", "4298-160316-104022", "4300-170208-163059", "4307-160217-123734", "4310-150415-235250", "4310-170126-130653", "43150330-144912", "43150330-161340", "43150331-121811", "43150402-150152", "43150402-162415", "43150404-143444", "43150406-101205", "4316-160222-150902", "4336-150409-162429", "4342-170111-091417", "4345-160524-165457", "4349-180123-140200", "4357-161210-150550", "4359-150419-125809", "4361-170520-161917", "4368-180202-145156", "4370-170125-102826", "4373-170915-165932", "4374-170104-125349", "4375-160229-131312", "4388-170205-120331", "4404-160528-230641", "4406-170523-181600", "4411-170523-114449", "4415-160228-114657", "44150401-163328", "44150402-125710", "44150403-104659", "44150407-122156", "4418-171220-150248", "4422-180201-144914", "4423-160229-120013", "4423-170522-124702", "4428-170127-105214", "4430-170109-140217", "4439-170913-131636", "4440-160312-085119", "4454-160315-024113", "4455-171223-165021", "4460-180131-172933", "4463-170912-151337", "4463-180201-140846", "4464-170126-024023", "4473-170115-183328", "4479-170117-130507", "4482-161125-142436", "4499-170522-103724", "4501-170119-144342", "4512-180119-225729", "45150330-135118", "45150331-161436", "45150406-142306", "45150407-075620", "4518-160229-153912", "4531-160229-113726", "4534-160311-130611", "4535-161128-152900", "4538-170106-121618", "4539-180201-172827", "4540-160309-153739", "4556-170523-162349", "4560-170116-192548", "4563-161027-123114", "4570-150427-094955", "4571-150504-181413", "4577-150408-152856", "4584-160305-122810", "4585-161201-160209", "4586-151222-194918", "4587-150408-154522", "4590-150416-143638", "4602-170204-162205", "4610-150407-200049", "4610-150410-155151", "46150406-125500", "46150407-112830", "46150407-144715", "4618-150409-121045", "4622-170120-134908", "4623-161126-131853", "4625-160526-220611", "4626-180208-134747", "4628-170205-124643", "4632-160315-144107", "4648-150408-212254", "4649-150414-120316", "4651-160222-151703", "4651-180111-002201", "4652-170523-121022", "4656-171122-114730", "4662-150427-075211", "4680-161104-140308", "4681-150410-101558", "4683-161129-151048", "4683-170113-154219", "4684-150420-135135", "4686-150409-125921", "4686-170211-143137", "4688-160306-003949", "4695-170213-170659", "4697-160217-203935", "4699-150410-110411", "4702-160311-180754", "4712-160313-143055", "47150331-113435", "47150331-182034", "47150402-093652", "47150402-131222", "47150404-190607", "47150406-163118", "47150407-132746", "47150407-152328", "4728-180202-082643", "4731-160306-121731", "4731-170130-143850", "4731-170522-102502", "4735-170915-161133", "4743-170522-160430", "4744-150410-141034", "4744-160220-155006", "4751-160130-144156", "4758-150411-123622", "4763-160226-172443", "4768-160219-134349", "4789-161128-112441", "4801-170126-125032", "4802-150409-000442", "4803-170131-180155", "4811-171226-140908", "4814-160217-165341", "48150331-044557", "48150402-130357", "48150405-232624", "48150406-111721", "48150406-132519", "48150407-125105", "48150407-154050", "4822-150410-114424", "4831-170522-151851", "4841-160315-184911", "4843-170108-135437", "4854-150414-125905", "4857-150411-163252", "4859-160313-194649", "4861-170201-151335", "4865-171016-113947", "4872-180202-103956", "4874-160215-165007", "4876-160213-150901", "4877-170116-111735", "4881-161224-112706", "4896-150619-162217", "4898-160130-112028", "4900-180206-120242", "4903-170207-115247", "4906-180104-104507", "49150404-115121", "49150404-142008", "49150404-170305", "49150406-114716", "49150406-145609", "49150407-114156", "49150407-143839", "4917-171022-202121", "4929-150409-144539", "4935-170117-102242", "4938-170202-095110", "4945-150409-184906", "4946-170523-231520", "4956-151229-124632", "4964-160314-114954", "4970-150411-140828", "4971-160128-140101", "4973-150408-132054", "4973-170113-202737", "4974-150408-150749", "4977-170213-145706", "4978-161025-153504", "4983-150619-153638", "4984-160303-115900", "4999-160118-121910", "50150327-201344", "50150331-145021", "50150331-190305", "50150405-080016", "50150405-134030", "50150407-175359", "5020-160223-110315", "5025-160229-183358", "5025-180201-191319", "5034-150409-110833", "5038-160224-131100", "5043-150408-125708", "5050-150409-132929", "5056-180207-170351", "5057-171021-105147", "5065-161231-130124", "5086-160529-112201", "5086-161102-112239", "5088-150502-135231", "5092-180202-050911", "5100-180202-154235", "5102-160306-195354", "5106-180208-194841", "5107-161025-144741", "5111-150408-175736", "51150405-112628", "51150406-150532", "51150406-154756", "51150407-103019", "5118-150421-095123", "5118-170106-120828", "5119-170913-145107", "5132-160302-122835", "5140-171023-143126", "5142-160306-155551", "5149-160106-233105", "5153-150420-172013", "5174-160320-154659", "5198-150410-113217", "5202-150506-171710", "5203-150409-142249", "5203-170122-115917", "52150404-154809", "52150404-201425", "52150406-133435", "52150407-152647", "5216-160225-115159", "5218-150420-155112", "5218-170121-063910", "5223-150412-122604", "5228-170520-115145", "5231-151222-124931", "5231-180202-165111", "5232-150410-132702", "5233-150408-152752", "5235-160223-164927", "5237-170520-164625", "5240-160307-120829", "5252-180127-160522", "5254-160217-135516", "5255-170520-175435", "5256-150420-121814", "5256-171220-130151", "5261-161114-101437", "5270-170208-012320", "5271-170208-060658", "5273-150413-152031", "5280-160422-215925", "5283-170916-110540", "5286-150413-164156", "5290-170117-121901", "5293-170213-160813", "5294-150410-135203", "5297-161126-162209", "5299-160312-183316", "5308-180202-115849", "5311-170914-151625", "53150331-204441", "53150401-204835", "53150402-115601", "53150404-145759", "53150406-161513", "53150407-173532", "5329-150410-150448", "5332-170913-131123", "5333-170106-122129", "5342-180201-122118", "5346-150408-162529", "5346-150409-103615", "5358-160314-115856", "5377-150416-143530", "5384-151231-113215", "5390-150420-124958", "5406-161201-144923", "5408-160315-171742", "5410-161221-154840", "5411-150409-114451", "5414-150414-135918", "5414-160222-121906", "54150331-085207", "54150331-143516", "54150402-102202", "54150404-101332", "54150407-113636", "54150407-145020", "5417-150416-173107", "5417-150418-113930", "5450-150623-165900", "5454-161126-134359", "5455-170914-112243", "5456-171113-110854", "5465-160318-162400", "5470-170522-142615", "5474-160225-110911", "5480-180202-112059", "5491-171103-142932", "5502-170120-125609", "5503-180208-144157", "5509-150414-174240", "55150401-173858", "55150403-155536", "55150404-164801", "55150406-110725", "55150406-124635", "5516-150413-163541", "5526-150422-175939", "5527-150416-221102", "5532-170206-234514", "5536-170105-143357", "5540-150409-151342", "5548-160128-224323", "5549-150507-183713", "5574-150422-123317", "5576-150410-101336", "5576-160529-153845", "5582-180131-143419", "5590-150413-174841", "5598-160817-100637", "5603-150410-113109", "5609-150409-144212", "5609-170523-235819", "5610-160311-113147", "5614-150420-155913", "56150331-144109", "56150331-171430", "56150402-155057", "56150406-112840", "56150406-132532", "56150407-104220", "5617-150513-152145", "5630-150420-125100", "5632-150504-143830", "5632-160212-144142", "5641-180202-114133", "5645-170126-130728", "5659-171206-155619", "5662-160529-185837", "5666-150422-095935", "5668-150408-211212", "5670-170522-125002", "5672-180202-153808", "5679-171012-101129", "5685-150409-131541", "5687-161214-141826", "5693-150507-191303", "5697-161021-135336", "57150331-103126", "57150401-123553", "57150406-004107", "57150406-141835", "57150406-145821", "57150406-150411", "57150407-125915", "5716-161220-161902", "5725-150505-114121", "5726-160311-111601", "5730-170522-100531", "5736-171118-102011", "5738-150413-125429", "5753-171224-192424", "5767-160310-115054", "5772-160224-120936", "5772-160315-153243", "5775-170201-064952", "5777-150413-125906", "5777-150415-093419", "5783-160213-133323", "5784-171106-142610", "5785-170523-181523", "5791-170110-125752", "5801-150409-143524", "5807-171011-144105", "5807-180127-114604", "5812-150408-173001", "58150402-145745", "58150405-142724", "5817-160217-161421", "5826-150408-151029", "5826-150413-164352", "5826-170913-103020", "5829-150511-222513", "5831-170106-110151", "5853-150420-170627", "5853-160310-120216", "5859-150409-160341", "5867-170602-152447", "5870-170131-185010", "5889-161025-155838", "5889-170204-180351", "5893-160529-230109", "5893-170129-094042", "5905-180203-132313", "5910-160310-105743", "5913-170201-122857", "5913-170212-101154", "5914-170523-202326", "59150401-102004", "59150402-121114", "59150406-115722", "59150406-153330", "5916-180127-115342", "5923-150502-143512", "5924-170520-140225", "5926-150407-215837", "5931-150413-155104", "5931-160229-122123", "5945-171226-180924", "5951-150409-153728", "5958-150413-154246", "5963-161230-134120", "5966-150410-145859", "5970-170102-115818", "5974-180215-131145", "5990-161024-161122", "5990-180202-173614", "5992-170203-094437", "6000-150414-182704", "6009-150408-162747", "6015-170522-131621", "60150402-142433", "60150402-151814", "60150405-201023", "60150406-102734", "60150406-210110", "60150407-112825", "60150407-125343", "6022-150505-152515", "6023-161105-103235", "6049-180130-180018", "6051-150524-174209", "6053-171221-173151", "6056-161025-162035", "6065-150516-161613", "6066-150414-155209", "6069-150411-130050", "6076-170112-142605", "6082-150408-155301", "6085-160301-113156", "6087-150422-221630", "6089-171020-132823", "6090-150415-112755", "6090-170914-125045", "6092-170913-151305", "6093-160529-124758", "6093-180202-121519", "6097-170127-072854", "6100-150414-111354", "6104-170914-124210", "6105-160305-173122", "6108-170128-041206", "6110-180219-170550", "6113-161213-144347", "61150330-214829", "61150404-141824", "61150404-163123", "61150404-163421", "61150405-121514", "61150406-154123", "61150407-120333", "61150407-143545", "6117-170122-165007", "6118-160113-100901", "6121-170128-094759", "6127-170201-122205", "6128-170112-142512", "6139-150408-225135", "6143-150427-153323", "6161-160302-130228", "6180-171017-135900", "6195-150409-145559", "6197-180131-175206", "6200-150418-144716", "6206-150409-165306", "6213-161022-184405", "6213-170201-120909", "62150331-112856", "62150331-155134", "62150403-100019", "62150404-101335", "62150404-160151", "62150406-113441", "62150406-164412", "62150406-190933", "62150407-131841", "6217-170112-095414", "6222-160220-124116", "6222-160526-113115", "6245-170127-105030", "6247-170912-140539", "6248-170914-115407", "6251-170522-175143", "6253-171201-152124", "6260-170520-125955", "6275-160312-223645", "6275-170109-110637", "6279-160220-155410", "6281-150626-112130", "6283-150413-135346", "6283-150413-140008", "6286-171204-120854", "6307-180208-211616", "6310-150410-104450", "6310-170111-095925", "6315-150421-164537", "63150328-184136", "63150401-151206", "63150405-105142", "63150406-103802", "63150407-143633", "6317-170131-201009", "6319-170120-141113", "6329-150408-140045", "6336-170914-140628", "6346-150417-165042", "6377-150409-103655", "6378-160222-113410", "6386-150507-161158", "6387-160319-165638", "6394-150411-124909", "6398-150408-125955", "6403-170130-041622", "6403-170916-124035", "6404-170207-161917", "6404-171221-160659", "6410-170118-160550", "64150402-135939", "64150404-113919", "64150406-014615", "6419-150416-160033", "6422-160305-115826", "6425-170913-144201", "6432-180201-143834", "6441-150414-142344", "6445-180201-175912", "6447-161229-061433", "6452-180124-143253", "6468-150409-124940", "6481-180201-093758", "6483-170118-154709", "6489-170112-153338", "6491-161229-101555", "6503-170201-145903", "6513-170121-071548", "65150330-234021", "65150404-142726", "65150406-144550", "6523-170918-163733", "6525-170125-120940", "6529-150419-111624", "6532-170104-113621", "6542-150408-154656", "6545-180208-143606", "6553-160314-010349", "6553-170201-092800", "6557-180201-192130", "6557-180202-145448", "6562-150423-155047", "6575-160227-133435", "6586-160113-102256", "6590-170201-091037", "6597-150410-152806", "6605-150428-152517", "66150402-143755", "66150404-195728", "66150406-154147", "66150407-134734", "6620-180201-154426", "6625-161024-141332", "6631-160305-114438", "6641-170208-130829", "6647-150409-104416", "6648-150408-132922", "6660-170523-164407", "6661-160216-162733", "6662-161025-185352", "6666-170131-055211", "6670-170122-151816", "6672-160128-161546", "6672-160529-170707", "6676-150410-113019", "6678-170131-180933", "6678-170208-142356", "6679-171221-123106", "6683-160517-122817", "6683-170108-190235", "6690-180130-151009", "6700-170522-125315", "6701-160227-154452", "6701-171225-202147", "6705-160524-103804", "6706-160305-183000", "6709-150413-140634", "6711-161231-160817", "67150402-152106", "67150402-155800", "67150406-154925", "67150407-123931", "67150407-152411", "6719-150416-192442", "6722-170915-131506", "6726-170121-140429", "6731-161024-120556", "6732-170110-124840", "6734-170116-122937", "6741-161022-121423", "6744-150412-204559", "6755-171222-155054", "6756-170520-123238", "6758-150410-152127", "6775-150409-164545", "6775-160223-100217", "6775-160527-113422", "6775-170916-094129", "6785-150408-131328", "6785-161031-114837", "6797-150424-233740", "6800-150408-155528", "6801-170128-134104", "6803-160223-164826", "6814-170128-121951", "68150402-113151", "68150402-144056", "68150403-015716", "68150404-113848", "68150406-152433", "68150407-140826", "68150407-144135", "68150407-180808", "6821-161130-144758", "6824-150408-124106", "6835-180208-222923", "6847-160309-144345", "6856-150605-145326", "6860-160131-154850", "6861-150422-153721", "6863-171214-115350", "6865-170102-162623", "6870-150410-115811", "6880-150409-151334", "6887-180127-160016", "6900-151223-152448", "6905-170911-160320", "6907-170110-124333", "69150402-105714", "69150404-145213", "69150404-162418", "69150404-171926", "69150405-124021", "69150406-144209", "69150406-174533", "6916-180221-145748", "6936-150414-142652", "6938-150411-145450", "6939-161229-125054", "6945-150408-105112", "6945-150408-110500", "6946-160528-100557", "6947-160304-161325", "6950-170522-184238", "6950-180201-112952", "6953-180202-135203", "6954-170520-130548", "6957-170915-155204", "6965-150414-114950", "6967-170522-130121", "6970-150417-090333", "6978-160307-121248", "6980-150408-142820", "6981-170112-104726", "6991-150420-130530", "6991-160108-153948", "6996-151223-150556", "6999-161220-164130", "7004-160219-111846", "7008-160307-141820", "7009-180126-210613", "70150402-150611", "70150403-145538", "70150405-151757", "70150406-160033", "70150407-142347", "7016-161024-152425", "7021-151217-115954", "7022-161025-113405", "7025-180208-155728", "7030-170107-122049", "7040-161229-114700", "7050-170131-192946", "7061-150420-105321", "7065-170915-154035", "7073-150418-124548", "7077-171128-114354", "7090-150417-130402", "7094-150427-205136", "7105-170208-083051", "7111-170111-113028", "7114-170213-180625", "7115-171212-162116", "71150401-160927", "71150402-122058", "71150402-153329", "71150404-131652", "71150404-134718", "71150404-233209", "7116-180201-174044", "7130-150428-112901", "7136-170918-135738", "7137-150408-140831", "7138-170127-100321", "7140-160310-133505", "7148-171125-120143", "7149-150507-192126", "7152-160225-110807", "7160-150407-204742", "7163-170129-021621", "7164-150422-204237", "7167-150408-170456", "7173-170104-134234", "7178-180208-173403", "7186-150422-130450", "7190-150416-102348", "7195-180129-130106", "7197-150417-181337", "7206-160311-141220", "7211-150409-131146", "7211-160302-145355", "72150330-203208", "72150401-095414", "72150401-171750", "72150401-224326", "72150402-163454", "72150403-071321", "72150404-140928", "72150404-150052", "72150404-151751", "72150407-143538", "7216-150408-153009", "7219-170523-202302", "7227-170207-190900", "7233-150519-175453", "7253-160309-112152", "7254-150409-111929", "7258-150409-102513", "7262-170211-180641", "7263-170128-114432", "7264-160225-131221", "7269-170523-150603", "7274-160116-143639", "7274-160320-191731", "7275-160301-132339", "7283-160314-173516", "7286-170104-160426", "7293-150414-132411", "7301-150414-112533", "7308-170207-114320", "7313-150409-150255", "73150403-160105", "73150403-184501", "73150407-124535", "73150407-125907", "73150407-153702", "7316-150408-120539", "7329-171227-111349", "7330-150408-112254", "7331-150414-124517", "7331-150422-101654", "7331-170913-110206", "7332-171214-192349", "7344-150418-125845", "7344-170208-165015", "7349-150409-145546", "7355-150413-180225", "7356-160225-112618", "7356-161103-155127", "7362-150412-131129", "7376-150502-153240", "7377-150414-181559", "7379-170125-103830", "7383-150408-170052", "7385-180202-115303", "7391-160304-125258", "7392-150507-152523", "7392-170911-165820", "7393-170210-175855", "7393-170523-205444", "7395-160308-154146", "7396-170523-211803", "7401-170201-135706", "7406-170127-105338", "7413-170523-224350", "74150329-123802", "74150404-132947", "74150407-110315", "7440-160315-210929", "7442-160529-180103", "7443-170523-213227", "7447-160223-160623", "7454-171009-094302", "7461-150427-072900", "7461-151223-122616", "7479-150421-105533", "7484-171223-135049", "7490-150411-125727", "7490-170110-120834", "7490-180202-170112", "7492-150417-151225", "7503-160312-222447", "7503-161121-152932", "7504-160307-120525", "75150329-075458", "75150330-224454", "75150331-181534", "75150401-123052", "75150406-114801", "75150407-174713", "7516-150416-103352", "7516-160130-171629", "7519-160309-120141", "7521-150701-152543", "7528-180104-151357", "7534-150410-120811", "7541-170914-102048", "7542-160310-161113", "7546-171227-153645", "7553-170204-164547", "7555-170601-152305", "7558-150408-191150", "7558-160213-151019", "7565-160320-175703", "7567-160601-153731", "7584-160218-124328", "7589-150622-220108", "7594-150430-103524", "7595-161207-140122", "7598-150414-111151", "7613-160315-162203", "76150330-131131", "76150402-125750", "76150402-165017", "76150404-132522", "76150404-141308", "76150406-121030", "7617-150420-164551", "7617-180207-150550", "7623-161229-113737", "7624-160311-164633", "7631-150410-124109", "7639-170202-140733", "7654-150425-192119", "7656-170520-113447", "7657-161207-110654", "7659-171227-132507", "7660-180202-173444", "7663-150408-120134", "7664-170110-135411", "7672-170201-130726", "7682-161028-141703", "7684-161024-224045", "7685-160129-134207", "7690-160529-171804", "7691-160804-172710", "7697-161231-094117", "7703-170208-120019", "7708-160311-153502", "77150403-144321", "77150404-153002", "77150405-153609", "77150406-101642", "7726-150413-123823", "7727-161104-160427", "7727-180208-134717", "7730-150418-150918", "7735-150412-124002", "7735-170202-021128", "7735-170207-015614", "7737-150423-151340", "7737-180130-121442", "7741-180131-160943", "7749-150415-234927", "7749-151219-181423", "7757-170109-141614", "7758-161024-102822", "7762-170213-183458", "7766-150408-114311", "7770-170105-142123", "7780-180202-171308", "7783-160204-153400", "7783-171223-170550", "7788-161025-163652", "7802-180130-105715", "7804-150410-150606", "7805-160227-114301", "7811-160307-100456", "7811-170522-185130", "78150331-095755", "78150331-155131", "78150403-113956", "78150404-162446", "78150405-203601", "78150407-121739", "78150407-133428", "78150407-162814", "7825-160127-150135", "7831-150507-204325", "7832-171227-095236", "7837-161128-193550", "7837-170108-205317", "7837-171225-073920", "7846-150424-014125", "7846-170207-161315", "7852-150410-181459", "7855-180207-144034", "7859-170207-230403", "7860-150409-093946", "7860-180207-125814", "7865-160228-110547", "7876-170125-123105", "7892-150622-113440", "7892-170208-120939", "7893-170206-154526", "7905-170203-112653", "7906-160306-151937", "79150402-112951", "79150403-133318", "79150406-120644", "7919-160528-201710", "7925-180104-111645", "7948-150623-175333", "7952-160304-174835", "7957-160305-202129", "7964-151223-150848", "7972-150424-152550", "7984-170103-155512", "7986-151221-121137", "7992-161022-112313", "7993-160123-141250", "7999-170131-184403", "7999-170915-105640", "8003-161219-105150", "8004-170911-225045", "8008-150702-132923", "80150331-104357", "80150404-161319", "80150406-145457", "8045-180202-145914", "8051-170201-192944", "8052-160317-163026", "8053-170107-153808", "8064-180207-165114", "8068-171121-113931", "8084-170106-141141", "8088-170103-103649", "8096-150410-101848", "8097-170112-011609", "81150402-112837", "81150406-145946", "8123-180207-180154", "8127-150409-121259", "8129-170916-103002", "8132-150409-172532", "8148-170523-140155", "8162-150419-091405", "8166-180201-155956", "8175-180219-111532", "8184-150418-160647", "8196-180202-172813", "8198-170131-190831", "8199-160218-202931", "8199-161128-133048", "8212-170127-111118", "8214-180208-155336", "82150401-193318", "82150402-165457", "82150404-100029", "82150404-140906", "82150405-165030", "82150406-004933", "82150406-112355", "8217-150410-144924", "8223-150413-130432", "8232-160309-131947", "8234-170105-124758", "8235-180201-145327", "8236-171224-222547", "8242-160525-114043", "8246-150425-182432", "8251-170201-063604", "8264-150408-150608", "8266-170126-100222", "8273-150413-133528", "8276-150620-203646", "8276-171207-141230", "8286-160529-184616", "8288-160302-131732", "8296-150408-112959", "8310-150504-134212", "8313-160209-172511", "8314-160529-155357", "83150331-223450", "83150404-160632", "83150404-162428", "83150406-122012", "83150406-124406", "83150406-131744", "83150406-133331", "83150406-144419", "83150406-190942", "8320-160308-132419", "8338-170117-120418", "8339-150504-133749", "8340-151223-164445", "8348-170202-174618", "8348-170522-171005", "8349-151216-215143", "8353-150426-150841", "8363-170915-160832", "8363-170918-150425", "8366-160129-224526", "8382-150419-134710", "8386-180131-163104", "8394-170107-134712", "8397-170522-122330", "8412-170213-000212", "8413-170111-131021", "84150406-134458", "84150406-143933", "84150406-155716", "84150407-164929", "8423-150407-204612", "8423-170201-112533", "8423-180202-132307", "8429-150421-124351", "8441-160319-161629", "8446-170119-141054", "8450-170522-150835", "8453-150417-180221", "8456-150408-105204", "8458-170126-070010", "8464-170129-101557", "8485-150501-153652", "8486-170117-154715", "8488-170521-214919", "8495-180202-090027", "8497-161125-135857", "8500-160203-162140", "8504-170523-150741", "8505-161031-162642", "8505-170522-170146", "8507-180202-193045", "8509-170108-174702", "85150330-182657", "85150331-125201", "85150401-152830", "85150402-161743", "85150403-230538", "85150404-104708", "85150404-121036", "85150404-150450", "85150405-220535", "85150407-161927", "8516-170523-172116", "8519-150428-103953", "8519-170120-131034", "8523-160218-125327", "8525-150524-215348", "8526-150408-163516", "8529-180219-160550", "8559-150408-174854", "8559-150414-164627", "8566-170131-183003", "8569-150426-133149", "8569-170913-124549", "8574-150506-123244", "8579-160109-111853", "8583-160314-145126", "8585-180130-140303", "8586-171220-144129", "8592-150424-101416", "8594-160226-115349", "8594-170914-131547", "8601-150407-215242", "8608-150409-110811", "8614-150502-134914", "86150329-133252", "86150330-214902", "86150404-152523", "86150406-150924", "86150407-162748", "86150407-165203", "8617-150507-162025", "8622-171125-123346", "8628-160306-231747", "8630-150409-133956", "8631-170126-075712", "8635-170131-235318", "8636-150416-130230", "8638-170131-052904", "8640-150504-132831", "8641-170523-145306", "8649-160525-151623", "8656-170119-144222", "8657-160307-045533", "8667-180208-205639", "8671-170523-202702", "8686-150419-155102", "8690-150409-173054", "8710-160123-150322", "8713-150415-223120", "87150330-162853", "87150405-172820", "8732-180201-124258", "8733-161025-105702", "8733-171120-114422", "8735-150410-160617", "8736-150504-162728", "8741-150413-125346", "8741-180202-162322", "8742-161026-113357", "8748-150409-122037", "8748-150418-152733", "8763-171222-101259", "8769-160130-190541", "8771-160209-123747", "8772-150409-121657", "8776-160524-170553", "8777-161128-190236", "8779-161025-113529", "8784-170213-172950", "8787-160223-142327", "8789-160528-172939", "8792-150408-140040", "8792-160316-121428", "8795-160203-175048", "8801-150408-072304", "8803-170523-161809", "8804-170120-124339", "88150401-143550", "88150402-132154", "88150405-055945", "88150406-163814", "88150407-105745", "88150407-123130", "8817-180128-202102", "8818-150416-131333", "8835-150409-215651", "8843-160308-105802", "8851-170208-160447", "8860-170210-174656", "8866-150407-222739", "8869-171222-152359", "8883-170213-145216", "8884-171226-224049", "8886-160309-111629", "8894-180123-123932", "8905-150414-105738", "8905-171007-194736", "89150330-163653", "89150402-152459", "89150402-164401", "89150403-074443", "89150406-220242", "8916-180202-115622", "8923-161209-154209", "8923-180201-192015", "8934-170520-161054", "8945-150414-190947", "8966-170127-095009", "8966-171028-153855", "8968-161217-114914", "8979-160611-115937", "8981-170131-025200", "8990-150410-151058", "8990-180201-112944", "8993-180201-172018", "8994-160227-143825", "9003-150420-141939", "9006-150409-113541", "90150401-100832", "90150402-113500", "90150406-150306", "90150406-160310", "90150407-101404", "90150407-133919", "90150407-144628", "9034-161025-121526", "9048-160525-200023", "9053-151223-174358", "9063-160121-151725", "9063-180208-220957", "9064-150425-180139", "9079-150410-103817", "9083-150413-130544", "9084-180131-155141", "9091-150409-165116", "9099-160529-181342", "9101-150414-152316", "9107-160305-165217", "91150331-110143", "91150402-124503", "91150404-163458", "91150406-023716", "91150406-101941", "91150406-141429", "91150407-125932", "91150407-164824", "9117-150417-111650", "9122-180204-153030", "9123-151218-225853", "9126-170915-115651", "9127-150429-155346", "9130-150408-153601", "9132-170208-133934", "9145-150507-182141", "9157-160203-163715", "9162-180208-203320", "9165-150413-144341", "9173-170118-120624", "9174-170522-071728", "9175-161210-122028", "9185-171224-202234", "9188-171102-144554", "9191-150408-103715", "9193-160204-123623", "9196-170128-035827", "9199-160309-144048", "9202-170118-101846", "9208-160213-100842", "9213-170131-224323", "92150331-160512", "92150401-140923", "92150401-162534", "92150401-202528", "92150402-152307", "92150405-183642", "92150406-102807", "92150407-114946", "92150407-153555", "9217-170106-124756", "9218-170126-104946", "9232-170119-104104", "9234-160215-121151", "9239-170109-115744", "9241-170913-144954", "9242-170523-172200", "9251-150414-062519", "9251-160305-194801", "9257-160226-122157", "9257-170916-062524", "9257-171227-172832", "9263-160209-165332", "9268-160306-002552", "9268-160528-134441", "9270-151221-162216", "9281-170523-112402", "9285-180202-153742", "9293-150409-152758", "9296-160225-161725", "9298-160306-173510", "9312-170125-121922", "93150401-074526", "93150402-120841", "93150402-131530", "93150407-151138", "9324-171225-083429", "9325-160129-142318", "9330-170520-144722", "9333-160313-223222", "9349-160310-130516", "9352-150408-112832", "9369-150417-214628", "9371-150410-115605", "9371-160302-155838", "9376-170131-185206", "9377-150419-145913", "9377-161126-125717", "9378-150408-150232", "9382-160314-115007", "9387-150411-133252", "9387-160301-152443", "9393-160311-170945", "9400-160124-113800", "9400-170129-112153", "9403-180207-133701", "9408-150418-064356", "9408-170131-052211", "94150330-113934", "94150331-122435", "94150331-164036", "94150331-225018", "94150401-151409", "94150402-111241", "94150402-115132", "94150402-165951", "94150403-212613", "94150405-001740", "94150405-004032", "94150405-111226", "94150406-114354", "94150407-115002", "94150407-130011", "9420-150409-104558", "9420-150501-093802", "9438-170119-132009", "9444-160314-162918", "9447-180212-120756", "9448-171220-142737", "9450-160302-134955", "9470-180206-132125", "9477-150411-120806", "9481-161206-124104", "9484-171223-154701", "9499-150411-114632", "9501-160303-113550", "9510-170112-102057", "9515-170120-113720", "95150331-110148", "95150404-122605", "95150404-214824", "95150406-144038", "95150406-150453", "95150406-190716", "95150407-160455", "95150407-165536", "9521-150413-110745", "9521-160305-153603", "9524-160527-113642", "9526-170102-112112", "9544-160119-115501", "9548-171011-150852", "9549-160309-161222", "9552-160320-103611", "9552-171124-165313", "9562-180202-121510", "9565-171215-115921", "9568-170207-150317", "9573-170522-165517", "9579-150409-143445", "9583-180202-045640", "9585-150420-105758", "9590-160306-003250", "9595-150408-135620", "9595-180202-145749", "9604-170615-133142", "9610-170109-100316", "9612-150426-155734", "9614-150414-150648", "96150401-170945", "96150405-090312", "96150406-213444", "96150407-121047", "96150407-181227", "9628-150409-172430", "9629-150408-125905", "9638-160311-171407", "9643-150414-154048", "9651-160314-214710", "9662-180204-211536", "9665-161207-131759", "9674-170522-151400", "9679-150408-140728", "9683-160221-140840", "9683-170601-162530", "9685-150507-145511", "9692-150413-174620", "9698-171026-131429", "9704-161210-115658", "9708-150410-144632", "9711-160301-110434", "97150331-094004", "97150331-154428", "97150401-122002", "97150402-120847", "97150402-154036", "97150404-203443", "97150406-124918", "97150407-113543", "97150407-114851", "97150407-123426", "9723-180131-105948", "9725-170207-021441", "9727-161126-122908", "9733-150415-095034", "9745-160529-150403", "9745-171225-131721", "9747-171009-130842", "9750-150420-141510", "9760-150512-160713", "9760-170916-124258", "9768-171227-113501", "9770-170201-133826", "9784-150410-120836", "9785-170104-124821", "9785-170601-153856", "9790-150409-121614", "9795-150409-135353", "9801-170110-065949", "9805-150416-115518", "9805-160316-211552", "9806-180207-174847", "9807-150412-180800", "98150403-213601", "98150404-151104", "98150405-185754", "98150406-133128", "98150406-145704", "98150407-100015", "9817-150413-181840", "9818-171227-144625", "9828-150501-044104", "9835-160126-200806", "9837-170520-132409", "9839-150426-103701", "9840-161216-115247", "9841-170213-080749", "9853-160314-115912", "9857-170916-131945", "9859-170107-105738", "9862-151217-115409", "9862-170128-132409", "9867-180208-122028", "9869-150408-120428", "9872-171020-153028", "9874-161022-140522", "9878-150430-140038", "9878-180201-105606", "9882-180203-163139", "9884-170105-162951", "9891-160225-100923", "9901-170105-125815", "9902-150409-124700", "9905-170112-093839", "9906-160125-181402", "9914-150522-154121", "9918-170122-172510", "9922-170523-135124", "9926-150413-123146", "9927-150411-101844", "9928-150413-161333", "9931-150409-123511", "9932-180201-151804", "9946-150410-132838", "9980-170102-120339", "9986-170111-100814" };
            //string Regnos = new string[] { };
            //List<jntuh_college_basreport> basFlagTotal = db.jntuh_college_basreport.Where(z => RegNos.Contains(z.RegistrationNumber)).Select(e => e).ToList();
            //var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => RegNos.Contains(e.RegistrationNumber)).Select(e => e).ToList();
            //var Jntuh_registered_faculty = db.jntuh_registered_faculty.Where(r => RegNos.Contains(r.RegistrationNumber)).Select(e => e).ToList();
            //var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            //var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).ToList();

            //int days = Convert.ToInt32(ConfigurationManager.AppSettings["Days"]);
            //int months = Convert.ToInt32(ConfigurationManager.AppSettings["Months"]); ;

            //List<SelectListItem> MonthNameAndIds = new List<SelectListItem>();
            //MonthNameAndIds.Add(new SelectListItem() { Text = "January", Value = "1" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "February", Value = "2" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "March", Value = "3" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "April", Value = "4" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "May", Value = "5" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "June", Value = "6" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "July", Value = "7" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "August", Value = "8" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "September", Value = "9" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "October", Value = "10" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "November", Value = "11" });
            //MonthNameAndIds.Add(new SelectListItem() { Text = "December", Value = "12" });

            //List<CollegeWiseBasFlagFacultyNew> CollegeWiseBasFlagFaculty = new List<CollegeWiseBasFlagFacultyNew>();

            //var SelectedMonths = MonthNameAndIds.Where(e => e.Text == "July" || e.Text == "August" || e.Text == "September" || e.Text == "October" || e.Text == "November" || e.Text == "December").Select(e => e.Text).ToArray();

            //int CollegeWiseFacultyBasCount = 0;
            //foreach (var EachFaculty in RegNos)
            //{
            //    bool FalgBas = true;
            //    int FacultyBasFlagCount = 0;
            //    var FacultyMonthCount = 0;
            //    var FacultyData = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == EachFaculty).Select(e => e).FirstOrDefault();
            //    var BasData = basFlagTotal.Where(e => e.RegistrationNumber.Trim() == EachFaculty).Select(e => e).ToList();

            //    CollegeWiseBasFlagFacultyNew BasFacultyClass = new CollegeWiseBasFlagFacultyNew();
            //    BasFacultyClass.CollegeCode = jntuh_college.Where(e => e.id == FacultyData.collegeId).Select(e => e.collegeCode).FirstOrDefault();
            //    BasFacultyClass.CollegeName = jntuh_college.Where(e => e.id == FacultyData.collegeId).Select(e => e.collegeName).FirstOrDefault();
            //    BasFacultyClass.Registraionnumber = EachFaculty;
            //    BasFacultyClass.Name = Jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == EachFaculty.Trim()).Select(e => e.FirstName + " " + e.MiddleName + " " + e.LastName).FirstOrDefault();
            //    BasFacultyClass.Mobile = Jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == EachFaculty.Trim()).Select(e => e.Mobile).FirstOrDefault();
            //    BasFacultyClass.Email = Jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == EachFaculty.Trim()).Select(e => e.Email).FirstOrDefault();
            //    BasFacultyClass.Department = jntuh_department.Where(e => e.id == FacultyData.DepartmentId).Select(e => e.departmentName).FirstOrDefault();
            //    string date = BasData.Select(e => e.joiningDate).FirstOrDefault().ToString();
            //    if (date == null || date == "")
            //    {

            //    }
            //    else
            //    {
            //        BasFacultyClass.BASJoiningDate = Convert.ToDateTime(date).ToString("dd-MM-yyyy");
            //    }
            //    BasFacultyClass.TotalWorkingDays = BasData.Select(e => e.totalworkingDays).Sum();
            //    BasFacultyClass.TotalPresentDays = BasData.Select(e => e.NoofPresentDays).Sum();
            //    FacultyMonthCount = BasData.Count();

            //    var BasDataNew = BasData.Where(m => SelectedMonths.Contains(m.month)).Select(e => e.month).Count();
            //    var NotRequiredMonths = BasData.Where(m => m.month == "January" || m.month == "February" || m.month == "March").Select(e => e.month).Count();

            //    foreach (var item in BasData)
            //    {
            //        int? totalworkingdays = item.totalworkingDays;
            //        int? totalpresentdays = item.NoofPresentDays;
            //        int? RequiredPresentDays = days;

            //        if (item.month == "July")
            //        {
            //            BasFacultyClass.JulyPresentDays = item.NoofPresentDays;
            //        }
            //        else if (item.month == "August")
            //        {
            //            BasFacultyClass.AugustPresentDays = item.NoofPresentDays;
            //        }
            //        else if (item.month == "September")
            //        {
            //            BasFacultyClass.SeptemberPresentDays = item.NoofPresentDays;
            //        }
            //        else if (item.month == "October")
            //        {
            //            BasFacultyClass.OctoberPresentDays = item.NoofPresentDays;
            //        }
            //        else if (item.month == "November")
            //        {
            //            BasFacultyClass.NovemberPresentDays = item.NoofPresentDays;
            //        }
            //        else if (item.month == "December")
            //        {
            //            BasFacultyClass.DecemberPresentDays = item.NoofPresentDays;
            //        }
            //        else if (item.month == "January")
            //        {
            //            BasFacultyClass.JanuaryPresentDays = item.NoofPresentDays;
            //        }
            //        else if (item.month == "February")
            //        {
            //            BasFacultyClass.FebruaryPresentDays = item.NoofPresentDays;
            //        }
            //        else if (item.month == "March")
            //        {
            //            BasFacultyClass.MarchPresentDays = item.NoofPresentDays;
            //        }

            //            if (totalpresentdays >= RequiredPresentDays)
            //            {

            //                if (item.month == "July")
            //                {

            //                    BasFacultyClass.July = item.month;
            //                    FacultyBasFlagCount++;

            //                }
            //                else if (item.month == "August")
            //                {
            //                    BasFacultyClass.August = item.month;
            //                    FacultyBasFlagCount++;
            //                }
            //                else if (item.month == "September")
            //                {
            //                    BasFacultyClass.September = item.month;
            //                    FacultyBasFlagCount++;
            //                }
            //                else if (item.month == "October")
            //                {
            //                    BasFacultyClass.October = item.month;
            //                    FacultyBasFlagCount++;
            //                }
            //                else if (item.month == "November")
            //                {
            //                    BasFacultyClass.November = item.month;
            //                    FacultyBasFlagCount++;
            //                }
            //                else if (item.month == "December")
            //                {
            //                    BasFacultyClass.December = item.month;
            //                    FacultyBasFlagCount++;
            //                }
            //                else if (item.month == "January")
            //                {
            //                    BasFacultyClass.January = item.month;
            //                }
            //                else if (item.month == "February")
            //                {
            //                    BasFacultyClass.February = item.month;
            //                }
            //                else if (item.month == "March")
            //                {
            //                    BasFacultyClass.March = item.month;
            //                }

            //            }
            //            else
            //            {

            //            }


            //    }
            //    if (BasDataNew == 0)
            //        {
            //            if (NotRequiredMonths == 1)
            //            {
            //                if (BasFacultyClass.March != null)
            //                {
            //                    FalgBas = false;
            //                }
            //                else
            //                {
            //                    FalgBas = true;
            //                }
            //            }
            //            else if (NotRequiredMonths == 2)
            //            {
            //                if (BasFacultyClass.February != null && BasFacultyClass.March != null)
            //                {
            //                    FalgBas = false;
            //                }
            //                else
            //                {
            //                    FalgBas = true;
            //                }
            //            }
            //            else if (NotRequiredMonths == 3)
            //            {
            //                if (BasFacultyClass.January != null && BasFacultyClass.February != null && BasFacultyClass.March != null)
            //                {
            //                    FalgBas = false;
            //                }
            //                else
            //                {
            //                    FalgBas = true;
            //                }
            //            }


            //        }
            //        else
            //        {
            //            var ReqiredMonthCount = BasDataNew - months;
            //            if (FacultyBasFlagCount >= ReqiredMonthCount)
            //            {
            //                if (NotRequiredMonths == 1)
            //                {
            //                    if (BasFacultyClass.March != null)
            //                    {
            //                        FalgBas = false;
            //                    }
            //                    else
            //                    {
            //                        FalgBas = true;
            //                    }
            //                }
            //                else if (NotRequiredMonths == 2)
            //                {
            //                    if (BasFacultyClass.February != null && BasFacultyClass.March != null)
            //                    {
            //                        FalgBas = false;
            //                    }
            //                    else
            //                    {
            //                        FalgBas = true;
            //                    }
            //                }
            //                else if (NotRequiredMonths == 3)
            //                {
            //                    if (BasFacultyClass.January != null && BasFacultyClass.February != null && BasFacultyClass.March != null)
            //                    {
            //                        FalgBas = false;
            //                    }
            //                    else
            //                    {
            //                        FalgBas = true;
            //                    }
            //                }
            //                // FalgBas = false;
            //            }
            //            else
            //            {
            //                FalgBas = true;
            //            }
            //        }

            //    if (FalgBas == false)
            //    {
            //        BasFacultyClass.Cleared = "Cleared";
            //        CollegeWiseFacultyBasCount++;
            //    }
            //    else
            //    {
            //        BasFacultyClass.Cleared = "NotCleared";
            //    }
            //    CollegeWiseBasFlagFaculty.Add(BasFacultyClass);
            //}



            //if (command == "Print")
            //{
            //    Response.ClearContent();
            //    Response.Buffer = true;
            //    Response.AddHeader("content-disposition", "attachment; filename = CollegeBASFacultyList.xls");
            //    Response.ContentType = "application/vnd.ms-excel";
            //    return PartialView("~/Views/BASFlagDeficiencyFaculty/_BASFlagFacultyDataBasedOnRegistratinNumberExcel.cshtml", CollegeWiseBasFlagFaculty);

            //}
            //else
            //{
            //    return View(CollegeWiseBasFlagFaculty);
            //}
            //return View();
            #endregion
        }
        #endregion


        
    }
}
