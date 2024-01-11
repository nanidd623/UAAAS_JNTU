using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data;

namespace UAAAS.Controllers.Admin
{
    [ErrorHandling]
    public class APIDataPushController : BaseController
    {
        private static uaaasDBContext db = new uaaasDBContext();
        private static string BaseAPI = "https://tstsaebasapi.telangana.gov.in/attendeesapi/";
        private static string ApiKey = "ckeorq32zsdsok4afdlfdlkfd343";

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult APIData()
        {
            return View();
        }

        public async Task<ActionResult> StudentData()
        {
            //await StudentsPushAPIData();
            return RedirectToAction("APIData");
        }

        public static async Task StudentsPushAPIData()
        {
            string apiUrl = BaseAPI + "api/attendee/studentadd";
            try
            {
                var StudentsDataExists = db.jntuh_apistudentspush_data.AsNoTracking().Where(i => i.ApiStatus == "No" && i.GeneratedAttendeeId == null).ToList();
                if (StudentsDataExists.Count > 0)
                {
                    foreach (var item in StudentsDataExists)
                    {
                        using (var httpclient = new HttpClient())
                        {
                            var InputData = new InputParams()
                            {
                                RecordType = "1",
                                GroupId = "190745",
                                DistrictId = "1",
                                DistName = "1",
                                MandalName = "1",
                                VillageName = "1",
                                HostelName = item.CollegeName, // CollegeName
                                HostelId = item.CollegeCode,  // CollegeCode
                                StudentId = item.StudentHallTicketNo, // Student Hallticketno
                                StudentName = item.StudentName,
                                AadharNumber = item.AadhaarNo,
                                PresClass = item.Branch,
                                StudentStatus = item.StudentStatus,
                                Gender = item.Gender
                            };
                            httpclient.DefaultRequestHeaders.Add("apikey", ApiKey);
                            var response = await httpclient.PostAsJsonAsync(apiUrl, InputData);
                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var myDeserializedClass = JsonConvert.DeserializeObject<Root>(result);
                                if (myDeserializedClass.description == "Success")
                                {
                                    var studData = db.jntuh_apistudentspush_data.Find(item.Id);
                                    studData.GeneratedAttendeeId = myDeserializedClass.attendeecode;
                                    studData.ResponseMsg = myDeserializedClass.remarks;
                                    studData.ApiStatus = "Yes";
                                    studData.UpdatedOn = DateTime.Now;
                                    studData.UpdatedBy = 450;
                                    db.Entry(studData).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var myDeserializedClass = JsonConvert.DeserializeObject<ErrorResp>(result);
                                var studData = db.jntuh_apistudentspush_data.Find(item.Id);
                                studData.ResponseMsg = myDeserializedClass.respcode + "-" + myDeserializedClass.respdesc;
                                studData.UpdatedOn = DateTime.Now;
                                studData.UpdatedBy = 450;
                                db.Entry(studData).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<ActionResult> StaffNewlyAddedData()
        {
            await StaffNewPushAPIData();
            return RedirectToAction("APIData");
        }

        public static async Task StaffNewPushAPIData()
        {
            string apiUrl = BaseAPI + "api/attendee/staffadd";
            try
            {
                var StaffDataExists = db.jntuh_apistaffpush_data.AsNoTracking().Where(i => i.StaffType == "New_Faculty" && i.ApiStatus == "No" && i.GeneratedAttendeeId == null).ToList();
                if (StaffDataExists.Count > 0)
                {
                    foreach (var item in StaffDataExists)
                    {
                        using (var httpclient = new HttpClient())
                        {
                            var InputData = new StaffInputParams()
                            {
                                RecordType = "1",
                                GroupId = "190745",
                                HostelId = item.CollegeCode,  // CollegeCode
                                EmpId = item.StaffRegistrationNo, // Staff RegistrationNumber
                                EmpName = item.StaffName,
                                AadharNumber = item.AadhaarNo,
                                Designation = item.Designation,
                                EmployeeStatus = item.StaffStatus,
                                Gender = item.Gender
                            };
                            httpclient.DefaultRequestHeaders.Add("apikey", ApiKey);
                            var response = await httpclient.PostAsJsonAsync(apiUrl, InputData);
                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var myDeserializedClass = JsonConvert.DeserializeObject<Root>(result);
                                if (myDeserializedClass.description == "Success")
                                {
                                    var staffData = db.jntuh_apistaffpush_data.Find(item.Id);
                                    staffData.GeneratedAttendeeId = myDeserializedClass.attendeecode;
                                    staffData.ResponseMsg = myDeserializedClass.remarks;
                                    staffData.ApiStatus = "Yes";
                                    staffData.UpdatedOn = DateTime.Now;
                                    staffData.UpdatedBy = 450;
                                    db.Entry(staffData).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var myDeserializedClass = JsonConvert.DeserializeObject<ErrorResp>(result);
                                var staffData = db.jntuh_apistaffpush_data.Find(item.Id);
                                staffData.ResponseMsg = myDeserializedClass.respcode + "-" + myDeserializedClass.respdesc;
                                staffData.UpdatedOn = DateTime.Now;
                                staffData.UpdatedBy = 450;
                                db.Entry(staffData).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<ActionResult> StaffDeletedData()
        {
            await StaffDeletedPushAPIData();
            return RedirectToAction("APIData");
        }

        public static async Task StaffDeletedPushAPIData()
        {
            string apiUrl = BaseAPI + "api/attendee/staffadd";
            try
            {
                var StaffDataExists = db.jntuh_apistaffpush_data.AsNoTracking().Where(i => i.StaffType == "Deleted_Faculty" && i.ApiStatus == "No" && i.GeneratedAttendeeId == null).ToList();
                if (StaffDataExists.Count > 0)
                {
                    foreach (var item in StaffDataExists)
                    {
                        using (var httpclient = new HttpClient())
                        {
                            var InputData = new StaffInputParams()
                            {
                                RecordType = "1",
                                GroupId = "190745",
                                HostelId = item.CollegeCode,  // CollegeCode
                                EmpId = item.StaffRegistrationNo, // Staff RegistrationNumber
                                EmpName = item.StaffName,
                                AadharNumber = item.AadhaarNo,
                                Designation = item.Designation,
                                EmployeeStatus = item.StaffStatus,
                                Gender = item.Gender
                            };
                            httpclient.DefaultRequestHeaders.Add("apikey", ApiKey);
                            var response = await httpclient.PostAsJsonAsync(apiUrl, InputData);
                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var myDeserializedClass = JsonConvert.DeserializeObject<Root>(result);
                                if (myDeserializedClass.description == "Success")
                                {
                                    var staffData = db.jntuh_apistaffpush_data.Find(item.Id);
                                    staffData.GeneratedAttendeeId = myDeserializedClass.attendeecode;
                                    staffData.ResponseMsg = myDeserializedClass.remarks;
                                    staffData.ApiStatus = "Yes";
                                    staffData.UpdatedOn = DateTime.Now;
                                    staffData.UpdatedBy = 450;
                                    db.Entry(staffData).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var myDeserializedClass = JsonConvert.DeserializeObject<ErrorResp>(result);
                                var staffData = db.jntuh_apistaffpush_data.Find(item.Id);
                                staffData.ResponseMsg = myDeserializedClass.respcode + "-" + myDeserializedClass.respdesc;
                                staffData.UpdatedOn = DateTime.Now;
                                staffData.UpdatedBy = 450;
                                db.Entry(staffData).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<ActionResult> StaffTransferData()
        {
            await StaffTransferPushAPIData();
            return RedirectToAction("APIData");
        }

        public static async Task StaffTransferPushAPIData()
        {
            string apiUrl = BaseAPI + "api/attendee/staffadd";
            try
            {
                var StaffDataExists = db.jntuh_apistaffpush_data.AsNoTracking().Where(i => i.StaffType == "Transfer_Faculty" && i.ApiStatus == "No" && i.GeneratedAttendeeId == null).ToList();
                if (StaffDataExists.Count > 0)
                {
                    foreach (var item in StaffDataExists)
                    {
                        using (var httpclient = new HttpClient())
                        {
                            var InputData = new StaffInputParams()
                            {
                                RecordType = "1",
                                GroupId = "190745",
                                HostelId = item.CollegeCode,  // CollegeCode
                                EmpId = item.StaffRegistrationNo, // Staff RegistrationNumber
                                EmpName = item.StaffName,
                                AadharNumber = item.AadhaarNo,
                                Designation = item.Designation,
                                EmployeeStatus = item.StaffStatus,
                                Gender = item.Gender
                            };
                            httpclient.DefaultRequestHeaders.Add("apikey", ApiKey);
                            var response = await httpclient.PostAsJsonAsync(apiUrl, InputData);
                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var myDeserializedClass = JsonConvert.DeserializeObject<Root>(result);
                                if (myDeserializedClass.description == "Success")
                                {
                                    var staffData = db.jntuh_apistaffpush_data.Find(item.Id);
                                    staffData.GeneratedAttendeeId = myDeserializedClass.attendeecode;
                                    staffData.ResponseMsg = myDeserializedClass.remarks;
                                    staffData.ApiStatus = "Yes";
                                    staffData.UpdatedOn = DateTime.Now;
                                    staffData.UpdatedBy = 450;
                                    db.Entry(staffData).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var myDeserializedClass = JsonConvert.DeserializeObject<ErrorResp>(result);
                                var staffData = db.jntuh_apistaffpush_data.Find(item.Id);
                                staffData.ResponseMsg = myDeserializedClass.respcode + "-" + myDeserializedClass.respdesc;
                                staffData.UpdatedOn = DateTime.Now;
                                staffData.UpdatedBy = 450;
                                db.Entry(staffData).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
    public class InputParams
    {
        public string RecordType { get; set; }
        public string GroupId { get; set; }
        public string DistrictId { get; set; }
        public string DistName { get; set; }
        public string MandalName { get; set; }
        public string VillageName { get; set; }
        public string HostelName { get; set; }
        public string HostelId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string AadharNumber { get; set; }
        public string PresClass { get; set; }
        public string StudentStatus { get; set; }
        public string Gender { get; set; }
    }
    public class StaffInputParams
    {
        public string RecordType { get; set; }
        public string GroupId { get; set; }
        public string HostelId { get; set; }
        public string EmpId { get; set; }
        public string EmpName { get; set; }
        public string AadharNumber { get; set; }
        public string Designation { get; set; }
        public string EmployeeStatus { get; set; }
        public string Gender { get; set; }
    }
    public class Root
    {
        public string status { get; set; }
        public string description { get; set; }
        public string attendeecode { get; set; }
        public string employeeid { get; set; }
        public string aadhar { get; set; }
        public string schoolscode { get; set; }
        public string stafftype { get; set; }
        public string remarks { get; set; }
        public string statuscode { get; set; }
    }
    public class ErrorResp
    {
        public string respcode { get; set; }
        public string respdesc { get; set; }
    }

}
