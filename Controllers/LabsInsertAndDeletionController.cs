using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class LabsInsertAndDeletionController : BaseController
    {
        //
        // GET: /LabsDeletion/
        uaaasDBContext db = new uaaasDBContext();
        List<SelectListItem> CollegeTypelist = new List<SelectListItem>()
            {
                new SelectListItem(){Value = "1",Text = "Autonomous"},
                new SelectListItem(){Value = "2",Text = "Non-Autonomous"} 
            };

        List<SelectListItem> YearList = new List<SelectListItem>()
            {
                new SelectListItem(){Value = "-1",Text = "---Select---"},
                new SelectListItem(){Value = "0",Text = "0"},
                new SelectListItem(){Value = "1",Text = "1"},
                new SelectListItem(){Value = "2",Text = "2"},
                new SelectListItem(){Value = "3",Text = "3"},
                new SelectListItem(){Value = "4",Text = "4"}               
            };
        List<SelectListItem> SemesterList = new List<SelectListItem>()
            {
                new SelectListItem(){Value = "-1",Text = "---Select---"},
                new SelectListItem(){Value = "0",Text = "0"},
                new SelectListItem(){Value = "1",Text = "1"},
                new SelectListItem(){Value = "2",Text = "2"}                          
            };

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {

            List<jntuh_degree> degereelist = db.jntuh_degree.ToList();
            List<jntuh_department> departmentlist = db.jntuh_department.ToList();
            List<jntuh_specialization> specializationlist = db.jntuh_specialization.ToList();

            List<jntuh_lab_master> lablist = db.jntuh_lab_master.Take(20).ToList();

            List<LabsList> objlab = new List<LabsList>();
            foreach (var item in lablist)
            {
                LabsList lab = new LabsList();
                lab.Degree = degereelist.Where(e => e.id == item.DegreeID).Select(e => e.degree).FirstOrDefault();
                lab.Department = departmentlist.Where(e => e.id == item.DepartmentID).Select(e => e.departmentName).FirstOrDefault();
                lab.Specialization = specializationlist.Where(e => e.id == item.SpecializationID).Select(e => e.specializationName).FirstOrDefault();
                lab.Year = item.Year;
                lab.Semester = item.Semester;
                lab.Labcode = item.Labcode;
                lab.LabName = item.LabName;
                lab.EquipmentName = item.EquipmentName;
                objlab.Add(lab);
            }

            return View(objlab);
        }

        public ActionResult DeleteLaboratories(LabsList Item1)
        {
            var jntuh_degrees = db.jntuh_degree.Where(de => de.isActive == true).ToList();

            ViewBag.degree = jntuh_degrees;

            LabsList item1 = new LabsList();
            List<LabsList> lablistdata = new List<LabsList>();

            if (Item1.Degree != null && Item1.Department != null && Item1.Year != 0 && Item1.Semester != 0)
            {
                if (ModelState.IsValid)
                {
                    int degreeid = Convert.ToInt32(Item1.Degree);
                    int departmentid = Convert.ToInt32(Item1.Department);
                    int Specializationid = Convert.ToInt32(Item1.Specialization);
                    if (Item1.Year != 0 && Item1.Semester != 0 && degreeid != 0 && departmentid != 0)
                    {
                        lablistdata = db.jntuh_lab_master.Where(e => e.Year == Item1.Year
                                                               && e.Semester == Item1.Semester
                                                               && e.DegreeID == degreeid
                                                               && e.DepartmentID == departmentid)
                                                               .Select(e => new
                                                               LabsList
                                                               {
                                                                   Id = e.id,
                                                                   Labcode = e.Labcode,
                                                                   EquipmentName = e.EquipmentName,
                                                                   LabName = e.LabName,
                                                                   Year = e.Year,
                                                                   Semester = e.Semester,
                                                                   SpecializationId = e.SpecializationID,
                                                                   CollegeId = e.CollegeId
                                                               }).ToList();

                        if (Specializationid == 0 || Specializationid == null)
                        {
                            if (Item1.Labcode == null)
                            {
                                if (Item1.CollegeId == 0 || Item1.CollegeId == null) { }
                                else
                                    lablistdata = lablistdata.Where(e => e.CollegeId == Item1.CollegeId).ToList();
                            }
                            else
                            {
                                if (Item1.CollegeId == 0 || Item1.CollegeId == null)
                                    lablistdata = lablistdata.Where(e => e.Labcode == Item1.Labcode).ToList();
                                else
                                    lablistdata = lablistdata.Where(e => e.Labcode == Item1.Labcode && e.CollegeId == item1.CollegeId).ToList();
                            }
                        }
                        else
                        {
                            if (Item1.Labcode == null)
                            {
                                if (Item1.CollegeId == 0 || Item1.CollegeId == null)
                                    lablistdata = lablistdata.Where(e => e.SpecializationId == Specializationid).ToList();
                                else
                                    lablistdata = lablistdata.Where(e => e.SpecializationId == Specializationid && e.CollegeId == Item1.CollegeId).ToList();
                            }
                            else
                            {
                                if (Item1.CollegeId == 0 || Item1.CollegeId == null)
                                    lablistdata = lablistdata.Where(e => e.SpecializationId == Specializationid && e.Labcode == Item1.Labcode).ToList();
                                else
                                    lablistdata = lablistdata.Where(e => e.SpecializationId == Specializationid && e.Labcode == Item1.Labcode && e.CollegeId == item1.CollegeId).ToList();
                            }
                        }
                    }
                    else
                    {
                        return View(Item1);
                    }
                }
                else
                {
                    @TempData["Error"] = "model state is not valid";
                    return View(Item1);
                }
                ViewBag.list = lablistdata;
            }
            return View(model: new Tuple<LabsList, List<LabsList>>(Item1, lablistdata));
        }

        public JsonResult GetColleges(int? TypeId)
        {
            if (TypeId != null)
            {
                var AutonomousCollegeIds = db.jntuh_college_affiliation.Where(a => a.affiliationTypeId == 7 && a.affiliationStatus == "Yes").Select(s => s.collegeId).ToList();
                if (TypeId == 1)
                {
                    //var Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && AutonomousCollegeIds.Contains(c.co.id)).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
                    var Colleges = db.jntuh_college.Where(c => c.isActive == true && AutonomousCollegeIds.Contains(c.id)).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
                    return Json(new { Data = Colleges }, "application/json", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //var Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && !AutonomousCollegeIds.Contains(c.co.id)).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
                    var Colleges = db.jntuh_college.Where(c => c.isActive == true && !AutonomousCollegeIds.Contains(c.id)).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
                    return Json(new { Data = Colleges }, "application/json", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                // var DataNew = null;
                return Json(new { Data = "" }, "application/json", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetDepartments(int degreeId)
        {
            object Dept = null;
            if (degreeId != 0)
            {
                var jntuh_departments = db.jntuh_department.Where(d => d.isActive == true).ToList();

                Dept = jntuh_departments.Where(e => e.degreeId == degreeId).Select(e => new
                {
                    DeptId = e.id,
                    DeptName = e.departmentName
                }).ToList();
            }
            return Json(new { Data = Dept }, "application/json", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSpecializations(int departmentid)
        {
            object Dept = null;
            if (departmentid != 0)
            {
                var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();
                Dept = jntuh_specialization.Where(e => e.departmentId == departmentid).Select(e => new
                {
                    speId = e.id,
                    speName = e.specializationName
                }).ToList();
            }
            return Json(new { Data = Dept }, "application/json", JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteBySem(List<LabsList> Item2)
        {
            try
            {
                var CollegeCheckedLabs = Item2.Where(a => a.IsChecked == true).Select(s => s).ToList();
                //College Labs Deletion
                foreach (var item in CollegeCheckedLabs)
                {
                    List<jntuh_college_laboratories> jntuh_clg_lab = db.jntuh_college_laboratories.Where(e => e.EquipmentID == item.Id).ToList();
                    foreach (var item1 in jntuh_clg_lab)
                    {
                        // var clg_lab = db.jntuh_college_laboratories.Find(item1.id);
                        db.jntuh_college_laboratories.Remove(item1);
                        db.SaveChanges();
                    }
                }

                //Lab Master Deletion
                foreach (var item4 in CollegeCheckedLabs)
                {
                    var jntuh_lab = db.jntuh_lab_master.Find(item4.Id);
                    db.jntuh_lab_master.Remove(jntuh_lab);
                    db.SaveChanges();
                    TempData["Success"] = "labs deleted successfully!";
                }

                ////College Physical Labs Deletion
                //foreach (var item3 in CollegeCheckedLabs)
                //{

                //    List<jntuh_physical_labmaster> jntuh_clg_physical_lab = db.jntuh_physical_labmaster.Where(e => e.Labcode == item3.Labcode).ToList();
                //    foreach (var item in jntuh_clg_physical_lab)
                //    {
                //        //var clg_lab = db.jntuh_physical_labmaster.Find(item3.id);
                //        db.jntuh_physical_labmaster.Remove(item);
                //        db.SaveChanges();
                //    }
                //}
            }
            catch (Exception)
            {
                TempData["Error"] = "labs not  deleted!";
                throw;
            }

            return RedirectToAction("DeleteLaboratories", "LabsDeletion");
        }

        public ActionResult DeletePhysicalLaboratories(int? collegeId)
        {
            List<LabsController.physicalLab> ObjPhysicalLab = new List<LabsController.physicalLab>();
            if (collegeId != null)
            {
                var physicalLabs = db.jntuh_physical_labmaster.Where(a => a.Collegeid == collegeId).ToList();
                ObjPhysicalLab = (from p in physicalLabs
                                  join s in db.jntuh_specialization.ToList() on p.SpecializationId equals s.id
                                  join d in db.jntuh_department.ToList() on p.DepartmentId equals d.id
                                  join de in db.jntuh_degree.ToList() on p.DegreeId equals de.id
                                  select new LabsController.physicalLab
                                  {
                                      id = p.Id,
                                      degree = de.degree,
                                      degreeid = de.id,
                                      department = d.departmentName,
                                      departmentid = d.id,
                                      specialization = s.specializationName,
                                      specializationid = s.id,
                                      year = p.Year,
                                      semister = p.Semister,
                                      Labid = p.Id,
                                      LabCode = p.Labcode,
                                      Labname = p.LabName,
                                      NoOfAvailabeLabs = p.Numberofavilablelabs
                                  }).ToList();
            }
            return View(ObjPhysicalLab);
        }

        public ActionResult GetEquipment(LabsList objlab)
        {
            if (ModelState.IsValid)
            {
                List<LabsList> lablistdata = new List<LabsList>();
                int degreeid = Convert.ToInt32(objlab.Degree);
                int departmentid = Convert.ToInt32(objlab.Department);
                int Specializationid = Convert.ToInt32(objlab.Specialization);
                if (objlab.Year != 0 && objlab.Semester != 0 && degreeid != 0 && departmentid != 0)
                {

                    lablistdata = db.jntuh_lab_master.Where(e => e.Year == objlab.Year
                                                                   && e.Semester == objlab.Semester
                                                                   && e.DegreeID == degreeid
                                                                   && e.DepartmentID == departmentid)
                                                                   .Select(e => new
                                                                   LabsList { Id = e.id, Labcode = e.Labcode, EquipmentName = e.EquipmentName, LabName = e.LabName, Year = e.Year, Semester = e.Semester }).ToList();
                    return View(lablistdata);

                    if (Specializationid == 0 || Specializationid == null)
                    {
                        if (objlab.Labcode == null)
                        {
                            return View(lablistdata);
                        }

                        else
                        {
                            lablistdata.Where(e => e.Labcode == objlab.Labcode).ToList();
                        }
                    }
                    else
                    {
                        if (objlab.Labcode == null)
                        {
                            lablistdata.Where(e => e.Specialization == "" + Specializationid).ToList();
                        }
                        else
                        {
                            lablistdata.Where(e => e.Specialization == "" + Specializationid && e.Labcode == objlab.Labcode).ToList();
                        }

                    }
                }
                else
                {
                    return RedirectToAction("DeleteLaboratories", "Account");
                }
            }
            else
            {
                @TempData["Error"] = "model state is not valid";
                return RedirectToAction("DeleteLaboratories", "Account");
            }
        }

        public ActionResult Getlabs(jntuh_lab_master objlab)
        {
            List<LabsList> lablistdata = new List<LabsList>();
            if (objlab.Year != 0 && objlab.Semester != 0 && objlab.Labcode != null)
            {
                lablistdata = db.jntuh_lab_master.Where(e => e.Year == objlab.Year && e.Semester == objlab.Semester && e.Labcode == objlab.Labcode).Select(e => new LabsList { Id = e.id, Labcode = e.Labcode, EquipmentName = e.EquipmentName, LabName = e.LabName, Year = e.Year, Semester = e.Semester }).ToList();
                return View(lablistdata);
            }
            else
            {
                return RedirectToAction("DeleteLaboratories", "Account");
            }
        }

        public ActionResult DeleteBylab(List<LabsList> objlablist)
        {
            try
            {
                foreach (var item in objlablist)
                {
                    List<jntuh_college_laboratories> jntuh_clg_lab = db.jntuh_college_laboratories.Where(e => e.EquipmentID == item.Id).ToList();

                    foreach (var item1 in jntuh_clg_lab)
                    {
                        var clg_lab = db.jntuh_college_laboratories.Find(item1.id);
                        db.jntuh_college_laboratories.Remove(clg_lab);
                        db.SaveChanges();
                    }
                }

                foreach (var item in objlablist)
                {
                    var jntuh_lab = db.jntuh_lab_master.Find(item.Id);
                    db.jntuh_lab_master.Remove(jntuh_lab);
                    db.SaveChanges();
                    TempData["Error"] = "labs deleted successfully!";
                }

            }
            catch (Exception)
            {
                TempData["Error"] = "labs not  deleted!";
                throw;
            }

            return RedirectToAction("DeleteLaboratories", "Account");
        }


        #region Labs Uploading from Excel

        //public FileResult DownloadExcel()
        //{
        //    string path = "/Content/Labs_Sample_Copy.xls";
        //    return File(path, "application/vnd.ms-excel", "Labs Sample Copy.xlsx");
        //}

        public ActionResult ExportToExcel(string Filename)
        {
            var gv = new GridView();
            gv.DataSource = TempData["FailureExcel"];
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=FailureExcel.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return RedirectToAction("importExcel");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult importExcel()
        {
            List<SelectListItem> Orderlist = new List<SelectListItem>()
            {
                new SelectListItem(){Value = "1",Text = "Autonomous"},
                new SelectListItem(){Value = "2",Text = "Non-Autonomous"} 
            };
            ViewBag.CollegeType = Orderlist;
            //if(TempData["FailureExcel"] != null)
            //{
            //    TempData["FailureExcel"] = TempData["FailureExcel"];
            //}
            LabsList labs = new LabsList();
            return View(labs);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult importExcel(LabsList Obj, HttpPostedFileBase FileUpload)
        {
            int count = 0;
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var message = string.Empty;
            string fileName = string.Empty;
            string SuccessfileName = string.Empty;
            string FailurefileName = string.Empty;
            var filepath = string.Empty;
            int collegeid = 0;
            int ActualCount = 0;
            int rows = 0;
            string ExcelUploadPath = "~/Content/Upload/LabsInsertion/UploadExcel";
            string ExcelSuccessPath = "~/Content/Upload/LabsInsertion/Success";
            string ExcelFailurePath = "~/Content/Upload/LabsInsertion/Failure";

            List<jntuh_degree> degereelist = db.jntuh_degree.ToList();
            List<jntuh_department> departmentlist = db.jntuh_department.ToList();
            List<jntuh_specialization> specializationlist = db.jntuh_specialization.ToList();

            if (FileUpload != null)
            {
                string ext = System.IO.Path.GetExtension(FileUpload.FileName);
                fileName = "Labs" + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ext;
                if (!Directory.Exists(Server.MapPath(ExcelUploadPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(ExcelUploadPath));
                }

                FileUpload.SaveAs(string.Format("{0}/{1}", Server.MapPath(ExcelUploadPath), fileName));

                if (ext.ToUpper().Equals(".XLS"))
                {
                    string splitstring = fileName.Split('.')[0].ToString();
                    SuccessfileName = splitstring + ".xlsx";
                    FailurefileName = splitstring + ".xlsx";
                }
                else
                {
                    SuccessfileName = fileName;
                    FailurefileName = fileName;
                }
                string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + System.IO.Path.Combine(Server.MapPath(ExcelUploadPath), fileName) + ";Extended Properties=Excel 12.0;Persist Security Info=False";
                // string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.4.0;Data Source=" + System.IO.Path.Combine(Server.MapPath(ExcelUploadPath), fileName) + ";Extended Properties=Excel 4.0;Persist Security Info=False";

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
                List<jntuh_lab_master> jntuh_lab_masterlist = new List<jntuh_lab_master>();
                List<FailureLabsList> labnotuploadedlist = new List<FailureLabsList>();
                dt = ds.Tables[0];

                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        count = Convert.ToInt32(dt.Rows[i][0].ToString());
                        try
                        {
                            if (!string.IsNullOrEmpty(dt.Rows[i][1].ToString()) && !string.IsNullOrEmpty(dt.Rows[i][2].ToString()) && !string.IsNullOrEmpty(dt.Rows[i][3].ToString()) && !string.IsNullOrEmpty(dt.Rows[i][4].ToString()) && !string.IsNullOrEmpty(dt.Rows[i][5].ToString()) && !string.IsNullOrEmpty(dt.Rows[i][6].ToString()) && !string.IsNullOrEmpty(dt.Rows[i][7].ToString()))
                            {
                                if (count == 825)
                                {

                                }
                                int degreeid = degereelist.Where(e => e.degree == dt.Rows[i][1].ToString().Trim()).Select(e => e.id).FirstOrDefault();
                                int departmentid = departmentlist.Where(e => e.departmentName == dt.Rows[i][2].ToString().Trim() && e.degreeId == degreeid).Select(e => e.id).FirstOrDefault();
                                int specilaizationid = specializationlist.Where(e => e.specializationName == dt.Rows[i][3].ToString().Trim() && e.departmentId == departmentid).Select(e => e.id).FirstOrDefault();
                                var labcodelength = dt.Rows[i][6].ToString().TrimStart().Length;
                                var labnamelength = dt.Rows[i][7].ToString().Trim().Length;
                                var equipmentnamelength = dt.Rows[i][8].ToString().TrimStart().Length;
                                if (degreeid != 0 && departmentid != 0 && specilaizationid != 0 && labcodelength <= 15 && equipmentnamelength <= 500 && labnamelength <= 250)
                                {
                                    jntuh_lab_master labmaster = new jntuh_lab_master();
                                    labmaster.DegreeID = degreeid;
                                    labmaster.DepartmentID = departmentid;
                                    labmaster.SpecializationID = specilaizationid;
                                    labmaster.Year = Convert.ToInt32(dt.Rows[i][4].ToString());
                                    labmaster.Semester = Convert.ToInt32(dt.Rows[i][5].ToString());
                                    labmaster.Labcode = dt.Rows[i][6].ToString().TrimStart();
                                    labmaster.LabName = dt.Rows[i][7].ToString().Trim();
                                    labmaster.EquipmentName = dt.Rows[i][8].ToString().Trim();
                                    labmaster.noofUnits = dt.Rows[i][9].ToString();
                                    labmaster.Remarks = dt.Rows[i][10].ToString();
                                    labmaster.ExperimentsIds = dt.Rows[i][11].ToString();
                                    if (labmaster.LabName.Length > 249 || labmaster.EquipmentName.Length > 499)
                                    {
                                        int str1 = labmaster.LabName.Length;
                                        int str2 = labmaster.EquipmentName.Length;
                                    }
                                    if (Obj.CollegeTypeId == 1)
                                    {
                                        labmaster.CollegeId = Obj.CollegeId;
                                    }
                                    //if (!string.IsNullOrEmpty(dt.Rows[i][12].ToString()))
                                    //{
                                    //    labmaster.CollegeId = Convert.ToInt32(dt.Rows[i][12].ToString());
                                    //}

                                    // labmaster.FileName = fileName;
                                    labmaster.isActive = true;
                                    labmaster.createdOn = DateTime.Now;
                                    labmaster.createdBy = userID;
                                    labmaster.updatedOn = null;
                                    labmaster.updatedBy = null;
                                    jntuh_lab_masterlist.Add(labmaster);
                                    continue;
                                }
                                else
                                {
                                    FailureLabsList labmasternotupload = new FailureLabsList();
                                    labmasternotupload.Id = Convert.ToInt32(dt.Rows[i][0].ToString());
                                    labmasternotupload.Degree = dt.Rows[i][1].ToString();
                                    labmasternotupload.Department = dt.Rows[i][2].ToString();
                                    labmasternotupload.Specialization = dt.Rows[i][3].ToString();
                                    if (!string.IsNullOrEmpty(dt.Rows[i][4].ToString()))
                                        labmasternotupload.Year = Convert.ToInt32(dt.Rows[i][4].ToString());
                                    if (!string.IsNullOrEmpty(dt.Rows[i][5].ToString()))
                                        labmasternotupload.Semester = Convert.ToInt32(dt.Rows[i][5].ToString());
                                    labmasternotupload.Labcode = dt.Rows[i][6].ToString().Trim();
                                    labmasternotupload.LabName = dt.Rows[i][7].ToString().Trim();
                                    labmasternotupload.EquipmentName = dt.Rows[i][8].ToString().Trim();
                                    labmasternotupload.noofUnits = dt.Rows[i][9].ToString();
                                    labmasternotupload.Remarks = dt.Rows[i][10].ToString();
                                    labmasternotupload.ExperimentsIds = dt.Rows[i][11].ToString();
                                    if (Obj.CollegeTypeId == 1)
                                    {
                                        labmasternotupload.CollegeId = Obj.CollegeId;
                                    }
                                    //if (!string.IsNullOrEmpty(dt.Rows[i][12].ToString()))
                                    //{
                                    //    labmasternotupload.CollegeId = Convert.ToInt32(dt.Rows[i][12].ToString()); ;
                                    //}
                                    // labmasternotupload.FileName = fileName;
                                    labnotuploadedlist.Add(labmasternotupload);
                                    continue;


                                }
                            }
                            else
                            {
                                FailureLabsList labmasternotupload = new FailureLabsList();
                                labmasternotupload.Id = Convert.ToInt32(dt.Rows[i][0].ToString());
                                labmasternotupload.Degree = dt.Rows[i][1].ToString();
                                labmasternotupload.Department = dt.Rows[i][2].ToString();
                                labmasternotupload.Specialization = dt.Rows[i][3].ToString();
                                if (!string.IsNullOrEmpty(dt.Rows[i][4].ToString()))
                                    labmasternotupload.Year = Convert.ToInt32(dt.Rows[i][4].ToString());
                                if (!string.IsNullOrEmpty(dt.Rows[i][5].ToString()))
                                    labmasternotupload.Semester = Convert.ToInt32(dt.Rows[i][5].ToString());
                                labmasternotupload.Labcode = dt.Rows[i][6].ToString().Trim();
                                labmasternotupload.LabName = dt.Rows[i][7].ToString().Trim();
                                labmasternotupload.EquipmentName = dt.Rows[i][8].ToString().Trim();
                                labmasternotupload.noofUnits = dt.Rows[i][9].ToString();
                                labmasternotupload.Remarks = dt.Rows[i][10].ToString();
                                labmasternotupload.ExperimentsIds = dt.Rows[i][11].ToString();
                                if (Obj.CollegeTypeId == 1)
                                {
                                    labmasternotupload.CollegeId = Obj.CollegeId;
                                }
                                //if (!string.IsNullOrEmpty(dt.Rows[i][12].ToString()))
                                //{
                                //    labmasternotupload.CollegeId = Convert.ToInt32(dt.Rows[i][12].ToString()); ;
                                //}
                                //labmasternotupload.FileName = fileName;
                                labnotuploadedlist.Add(labmasternotupload);
                                continue;
                            }
                        }
                        catch (Exception)
                        {
                            int a = count;
                            TempData["Error"] = "Your upload lab's Excel not in Correct Format.";
                            FailureLabsList labmasternotupload = new FailureLabsList();
                            labmasternotupload.Id = Convert.ToInt32(dt.Rows[i][0].ToString());
                            labmasternotupload.Degree = dt.Rows[i][1].ToString();
                            labmasternotupload.Department = dt.Rows[i][2].ToString();
                            labmasternotupload.Specialization = dt.Rows[i][3].ToString();
                            if (!string.IsNullOrEmpty(dt.Rows[i][4].ToString()))
                                labmasternotupload.Year = Convert.ToInt32(dt.Rows[i][4].ToString());
                            if (!string.IsNullOrEmpty(dt.Rows[i][5].ToString()))
                                labmasternotupload.Semester = Convert.ToInt32(dt.Rows[i][5].ToString());
                            labmasternotupload.Labcode = dt.Rows[i][6].ToString().Trim();
                            labmasternotupload.LabName = dt.Rows[i][7].ToString().Trim();
                            labmasternotupload.EquipmentName = dt.Rows[i][8].ToString().Trim();
                            labmasternotupload.noofUnits = dt.Rows[i][9].ToString();
                            labmasternotupload.Remarks = dt.Rows[i][10].ToString();
                            labmasternotupload.ExperimentsIds = dt.Rows[i][11].ToString();
                            if (Obj.CollegeTypeId == 1)
                            {
                                labmasternotupload.CollegeId = Obj.CollegeId;
                            }
                            //if (!string.IsNullOrEmpty(dt.Rows[i][12].ToString()))
                            //{
                            //    labmasternotupload.CollegeId = Convert.ToInt32(dt.Rows[i][12].ToString()); ;
                            //}
                            // labmasternotupload.FileName = fileName;
                            labnotuploadedlist.Add(labmasternotupload);
                            continue;

                        }
                    }

                    List<ExcelUpload> ExcelForSuccess = new List<ExcelUpload>();
                    foreach (var item in jntuh_lab_masterlist)
                    {
                        ExcelUpload lab = new ExcelUpload();

                        lab.Degree = degereelist.Where(e => e.id == item.DegreeID).Select(e => e.degree).FirstOrDefault();
                        lab.Department = departmentlist.Where(e => e.id == item.DepartmentID).Select(e => e.departmentName).FirstOrDefault();
                        lab.Specialization = specializationlist.Where(e => e.id == item.SpecializationID).Select(e => e.specializationName).FirstOrDefault();
                        lab.Year = item.Year;
                        lab.Semester = item.Semester;
                        lab.Labcode = item.Labcode.Trim();
                        lab.LabName = item.LabName.Trim();
                        lab.EquipmentName = item.EquipmentName.Trim();
                        lab.Remarks = item.Remarks;
                        lab.noofUnits = item.noofUnits;
                        lab.ExperimentsIds = item.ExperimentsIds;
                        lab.CollegeId = item.CollegeId;
                        ExcelForSuccess.Add(lab);
                    }

                    //Success Rows Insertion
                    DataTable dtlist = ToDataTable(ExcelForSuccess);
                    dtlist.TableName = "Success";
                    XLWorkbook workbook = new XLWorkbook();
                    workbook.Worksheets.Add(dtlist);

                    if (!Directory.Exists(Server.MapPath(ExcelSuccessPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(ExcelSuccessPath));
                    }
                    workbook.SaveAs(string.Format("{0}/{1}", Server.MapPath(ExcelSuccessPath), SuccessfileName));

                    //Failure Rows Insertion
                    DataTable Fdtlist = ToDataTable(labnotuploadedlist);
                    Fdtlist.TableName = "Failure";
                    XLWorkbook Fworkbook = new XLWorkbook();
                    Fworkbook.Worksheets.Add(Fdtlist);

                    if (!Directory.Exists(Server.MapPath(ExcelFailurePath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(ExcelFailurePath));
                    }
                    Fworkbook.SaveAs(string.Format("{0}/{1}", Server.MapPath(ExcelFailurePath), FailurefileName));

                    jntuh_lab_masterlist.ForEach(d => db.jntuh_lab_master.Add(d));
                    db.SaveChanges();
                    excelConnection.Close();
                    TempData["SUCCESS"] = "Labs Excel file is Uploaded Successfully.";
                    TempData["totalrows"] = rows;
                    TempData["successCount"] = jntuh_lab_masterlist.Count();
                    TempData["FailureCount"] = labnotuploadedlist.Count();
                    TempData["FailureExcel"] = Fdtlist.Rows.Count != 0 ? FailurefileName : null;
                    //if (rows >= ActualCount)
                    //{
                    //    TempData["Success"] = "Rows count:" + rows + "  Actual count:" + ActualCount;
                    //    //message = "Update1";
                    //    //message = "ExcelCout";
                    //}
                }
                catch (Exception ex)
                {
                    int a = count;
                    TempData["ERROR"] = "Something Went Wrong.Please Try Again.";
                    return RedirectToAction("importExcel");
                }
            }
            return RedirectToAction("importExcel");
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Labmasterrecord(EditLabsList labs, string test)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int UserId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            ViewBag.CollegeType = CollegeTypelist;

            var AutonomousCollegeIds = db.jntuh_college_affiliation.Where(a => a.affiliationTypeId == 7 && a.affiliationStatus == "Yes").Select(s => s.collegeId).ToList();
            if (labs.CollegeTypeId == 1)
            {
                var Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && AutonomousCollegeIds.Contains(c.co.id)).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
                ViewBag.Colleges = Colleges;
            }

            ViewBag.YearList = YearList;
            ViewBag.SemesterList = SemesterList;

            var jntuh_department = db.jntuh_department.Where(a => a.isActive == true).Select(a => a).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(a => a.isActive == true).Select(a => a).ToList();

            if (labs.CollegeTypeId == 1 && (labs.CollegeId != 0 && labs.CollegeId != null))
            {
                var labslist = db.jntuh_lab_master.Where(a => a.CollegeId == labs.CollegeId).Select(s => s).ToList();
                if (labs.Year != null && labs.Semister == null)
                {
                    var GroupbyLabcodes = labslist.Where(a => a.Year == labs.Year).Select(s => new EditLabsList
                   {
                       Id = s.id,
                       DegreeId = s.DegreeID,
                       Degree = jntuh_department.Where(a => a.id == s.DepartmentID).Select(a => a.jntuh_degree.degree).FirstOrDefault(),
                       DepartmentId = s.DepartmentID,
                       Department = jntuh_department.Where(a => a.id == s.DepartmentID).Select(a => a.departmentName).FirstOrDefault(),
                       SpecializationId = s.SpecializationID,
                       Specialization = jntuh_specialization.Where(z => z.id == s.SpecializationID).Select(a => a.specializationName).FirstOrDefault(),
                       Year = s.Year,
                       Semister = s.Semester,
                       Labcode = s.Labcode,
                       LabName = s.LabName,
                       EquipmentName = s.EquipmentName,
                       Remarks = s.Remarks,
                       noofUnits = s.noofUnits,
                       ExperimentsIds = s.ExperimentsIds
                   }).GroupBy(s => new { s.Labcode }).Select(s => s.First()).ToList();
                    ViewBag.objsList = GroupbyLabcodes;
                }
                else if (labs.Year != null && labs.Semister != null)
                {
                    var GroupbyLabcodes = labslist.Where(a => a.Year == labs.Year && a.Semester == labs.Semister).Select(s => new EditLabsList
                    {
                        Id = s.id,
                        DegreeId = s.DegreeID,
                        Degree = jntuh_department.Where(a => a.id == s.DepartmentID).Select(a => a.jntuh_degree.degree).FirstOrDefault(),
                        DepartmentId = s.DepartmentID,
                        Department = jntuh_department.Where(a => a.id == s.DepartmentID).Select(a => a.departmentName).FirstOrDefault(),
                        SpecializationId = s.SpecializationID,
                        Specialization = jntuh_specialization.Where(z => z.id == s.SpecializationID).Select(a => a.specializationName).FirstOrDefault(),
                        Year = s.Year,
                        Semister = s.Semester,
                        Labcode = s.Labcode,
                        LabName = s.LabName,
                        EquipmentName = s.EquipmentName,
                        Remarks = s.Remarks,
                        noofUnits = s.noofUnits,
                        ExperimentsIds = s.ExperimentsIds
                    }).GroupBy(s => new { s.Labcode }).Select(s => s.First()).ToList();
                    ViewBag.objsList = GroupbyLabcodes;
                }
            }
            else if (labs.CollegeTypeId == 2)
            {
                var labslist = db.jntuh_lab_master.Where(a => a.CollegeId == null).Select(s => s).ToList();
                if (labs.Year != null && labs.Semister == null)
                {
                    var GroupbyLabcodes = labslist.Where(a => a.Year == labs.Year).Select
                   (s => new EditLabsList
                   {
                       Id = s.id,
                       DegreeId = s.DegreeID,
                       Degree = jntuh_department.Where(a => a.id == s.DepartmentID).Select(a => a.jntuh_degree.degree).FirstOrDefault(),
                       DepartmentId = s.DepartmentID,
                       Department = jntuh_department.Where(a => a.id == s.DepartmentID).Select(a => a.departmentName).FirstOrDefault(),
                       SpecializationId = s.SpecializationID,
                       Specialization = jntuh_specialization.Where(z => z.id == s.SpecializationID).Select(a => a.specializationName).FirstOrDefault(),
                       Year = s.Year,
                       Semister = s.Semester,
                       Labcode = s.Labcode,
                       LabName = s.LabName,
                       EquipmentName = s.EquipmentName,
                       Remarks = s.Remarks,
                       noofUnits = s.noofUnits,
                       ExperimentsIds = s.ExperimentsIds
                   }).GroupBy(s => new { s.Labcode }).Select(s => s.First()).ToList();
                    ViewBag.objsList = GroupbyLabcodes;
                }
                else if (labs.Year != null && labs.Semister != null)
                {
                    var GroupbyLabcodes = labslist.Where(a => a.Year == labs.Year && a.Semester == labs.Semister).Select(s => new EditLabsList
                    {
                        Id = s.id,
                        DegreeId = s.DegreeID,
                        Degree = jntuh_department.Where(a => a.id == s.DepartmentID).Select(a => a.jntuh_degree.degree).FirstOrDefault(),
                        DepartmentId = s.DepartmentID,
                        Department = jntuh_department.Where(a => a.id == s.DepartmentID).Select(a => a.departmentName).FirstOrDefault(),
                        SpecializationId = s.SpecializationID,
                        Specialization = jntuh_specialization.Where(z => z.id == s.SpecializationID).Select(a => a.specializationName).FirstOrDefault(),
                        Year = s.Year,
                        Semister = s.Semester,
                        Labcode = s.Labcode,
                        LabName = s.LabName,
                        EquipmentName = s.EquipmentName,
                        Remarks = s.Remarks,
                        noofUnits = s.noofUnits,
                        ExperimentsIds = s.ExperimentsIds
                    }).GroupBy(s => new { s.Labcode }).Select(s => s.First()).ToList();
                    ViewBag.objsList = GroupbyLabcodes;
                }
            }
            return View(labs);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditLabmasterrecords(string labcode)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int UserId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (!String.IsNullOrEmpty(labcode))
            {
                EditLabsList lab = new EditLabsList();
                var obj = db.jntuh_lab_master.Where(s => s.Labcode == labcode).Select(s => s).FirstOrDefault();
                if (obj != null)
                {
                    var degrees = db.jntuh_degree.Where(a => a.isActive == true).ToList();
                    var departments = db.jntuh_department.Where(a => a.isActive == true).ToList();
                    var specializations = db.jntuh_specialization.Where(a => a.isActive == true).ToList();
                    ViewBag.degree = degrees.Select(s => new { id = s.id, degree = s.degree }).ToList();
                    ViewBag.departments = departments.Select(s => new { id = s.id, department = s.jntuh_degree.degree + "-" + s.departmentName }).OrderBy(q => q.department).ToList();
                    ViewBag.specializations = specializations.Select(s => new { id = s.id, specialization = s.jntuh_department.jntuh_degree.degree + "-" + s.specializationName }).OrderBy(q => q.specialization).ToList();

                    ViewBag.YearList = YearList;
                    ViewBag.SemesterList = SemesterList;

                    lab.Id = obj.id;
                    lab.CollegeTypeId = obj.CollegeId == null ? 2 : 1;
                    lab.CollegeId = obj.CollegeId;
                    lab.DegreeId = obj.DegreeID;
                    lab.Degree = db.jntuh_specialization.Where(a => a.id == obj.SpecializationID).Select(s => s.jntuh_department.jntuh_degree.degree).FirstOrDefault();
                    lab.DepartmentId = obj.DepartmentID;
                    lab.Department = db.jntuh_specialization.Where(a => a.id == obj.SpecializationID).Select(s => s.jntuh_department.departmentName).FirstOrDefault();
                    lab.SpecializationId = obj.SpecializationID;
                    lab.Specialization = db.jntuh_specialization.Where(a => a.id == obj.SpecializationID).Select(a => a.specializationName).FirstOrDefault();
                    lab.Year = obj.Year;
                    lab.Semister = obj.Semester;
                    lab.Labcode = obj.Labcode;
                    lab.LabName = obj.LabName;
                    lab.EquipmentName = obj.EquipmentName;
                    lab.Remarks = obj.Remarks;
                    lab.noofUnits = obj.noofUnits;
                    lab.ExperimentsIds = obj.ExperimentsIds;
                    return PartialView("~/Views/LabsInsertAndDeletion/EditLabmasterrecord.cshtml", lab);
                }
                else
                    return RedirectToAction("Labmasterrecord");
            }
            else
                return RedirectToAction("Labmasterrecord");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditLabmasterrecords(EditLabsList labs)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int UserId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if ((labs.Id != 0 && labs.Id != null) && labs.Year != null && labs.Semister != null && labs.Labcode != null)
            {
                var data = db.jntuh_lab_master.Where(a => a.id == labs.Id).Select(s => s).FirstOrDefault();
                if (data != null)
                {
                    try
                    {
                        var Labsdata = labs.CollegeTypeId == 1 ? db.jntuh_lab_master.Where(s => s.CollegeId == data.CollegeId && s.Year == data.Year && s.Semester == data.Semester && s.Labcode == data.Labcode).Select(s => s).ToList() :
                                                                db.jntuh_lab_master.Where(s => s.CollegeId == null && s.Year == data.Year && s.Semester == data.Semester && s.Labcode == data.Labcode).Select(s => s).ToList();
                        if (Labsdata.Count != 0)
                        {
                            foreach (var item in Labsdata)
                            {
                                item.DegreeID = labs.DegreeId;
                                item.DepartmentID = labs.DepartmentId;
                                item.SpecializationID = labs.SpecializationId;
                                item.Year = Convert.ToInt32(labs.Year);
                                item.Semester = labs.Semister;
                                item.Labcode = labs.Labcode;
                                item.LabName = labs.LabName;
                                //item.EquipmentName = labs.EquipmentName;
                                // item.Remarks = labs.Remarks;
                                //item.noofUnits = labs.noofUnits;
                                //item.ExperimentsIds = labs.ExperimentsIds;
                                item.updatedOn = DateTime.Now;
                                item.updatedBy = UserId;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            TempData["SUCCESS"] = "Labs are Updated Successfully.";
                            return RedirectToAction("Labmasterrecord", new { CollegeTypeId = labs.CollegeTypeId, CollegeId = labs.CollegeId, Year = labs.Year, Semister = labs.Semister });
                        }
                        else
                        {
                            TempData["ERROR"] = "No Data Found.";
                            return RedirectToAction("Labmasterrecord", new { CollegeTypeId = labs.CollegeTypeId, CollegeId = labs.CollegeId, Year = labs.Year, Semister = labs.Semister });
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ERROR"] = "SomeThing went wrong.Try Again";
                        return RedirectToAction("Labmasterrecord", new { CollegeTypeId = labs.CollegeTypeId, CollegeId = labs.CollegeId, Year = labs.Year, Semister = labs.Semister });
                    }
                }
                else
                {
                    TempData["ERROR"] = "Data is Not Found.";
                    return RedirectToAction("Labmasterrecord");
                }
            }
            else
            {
                TempData["ERROR"] = "SomeThing went wrong,Try Again";
                return RedirectToAction("Labmasterrecord");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ViewLabmasterrecords(string labcode)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int UserId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (!String.IsNullOrEmpty(labcode))
            {
                List<EditLabsList> labs = new List<EditLabsList>();
                var obj = db.jntuh_lab_master.Where(s => s.Labcode == labcode).Select(s => s).ToList();
                if (obj.Count != 0)
                {
                    foreach (var item in obj)
                    {
                        EditLabsList singleLab = new EditLabsList();
                        singleLab.Id = item.id;
                        singleLab.CollegeId = item.CollegeId;
                        singleLab.DegreeId = item.DegreeID;
                        singleLab.Degree = db.jntuh_specialization.Where(a => a.id == item.SpecializationID).Select(s => s.jntuh_department.jntuh_degree.degree).FirstOrDefault();
                        singleLab.DepartmentId = item.DepartmentID;
                        singleLab.Department = db.jntuh_specialization.Where(a => a.id == item.SpecializationID).Select(s => s.jntuh_department.departmentName).FirstOrDefault();
                        singleLab.SpecializationId = item.SpecializationID;
                        singleLab.Specialization = db.jntuh_specialization.Where(a => a.id == item.SpecializationID).Select(a => a.specializationName).FirstOrDefault();
                        singleLab.Year = item.Year;
                        singleLab.Semister = item.Semester;
                        singleLab.Labcode = item.Labcode;
                        singleLab.LabName = item.LabName;
                        singleLab.EquipmentName = item.EquipmentName;
                        singleLab.Remarks = item.Remarks;
                        singleLab.noofUnits = item.noofUnits;
                        singleLab.ExperimentsIds = item.ExperimentsIds;
                        labs.Add(singleLab);
                    }

                    return View(labs);
                }
                else
                    return RedirectToAction("Labmasterrecord");
            }
            else
                return RedirectToAction("Labmasterrecord");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteLabmasterrecord(int? id)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            if (id != null && id != 0)
            {
                string labcode = string.Empty;
                var data = db.jntuh_lab_master.Find(id);
                if (data != null)
                {
                    labcode = data.Labcode;
                    var collegeLab = db.jntuh_college_laboratories.Where(z => z.EquipmentID == data.id).Select(s => s).FirstOrDefault();
                    if (collegeLab != null)
                    {
                        db.jntuh_college_laboratories.Remove(collegeLab);
                        db.SaveChanges();
                    }
                    db.jntuh_lab_master.Remove(data);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Lab is deleted Successfully.";
                    return RedirectToAction("ViewLabmasterrecords", "LabsInsertAndDeletion", new { labcode = labcode });
                }
                else
                {
                    TempData["ERROR"] = "Record is Not Found";
                    return RedirectToAction("ViewLabmasterrecords");
                }
            }
            TempData["ERROR"] = "Something went Wrong,Try Again.";
            return RedirectToAction("Labmasterrecord");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditLabmasterSinglerecord(int? id)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int UserId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (id != null && id != 0)
            {
                EditLabsList lab = new EditLabsList();
                var obj = db.jntuh_lab_master.Where(s => s.id == id).Select(s => s).FirstOrDefault();
                if (obj != null)
                {
                    var degrees = db.jntuh_degree.Where(a => a.isActive == true).ToList();
                    var departments = db.jntuh_department.Where(a => a.isActive == true).ToList();
                    var specializations = db.jntuh_specialization.Where(a => a.isActive == true).ToList();
                    ViewBag.degree = degrees.Select(s => new { id = s.id, degree = s.degree }).ToList();
                    ViewBag.departments = departments.Select(s => new { id = s.id, department = s.jntuh_degree.degree + "-" + s.departmentName }).OrderBy(q => q.department).ToList();
                    ViewBag.specializations = specializations.Select(s => new { id = s.id, specialization = s.jntuh_department.jntuh_degree.degree + "-" + s.specializationName }).OrderBy(q => q.specialization).ToList();

                    ViewBag.YearList = YearList;
                    ViewBag.SemesterList = SemesterList;

                    lab.Id = obj.id;
                    lab.CollegeTypeId = obj.CollegeId == null ? 2 : 1;
                    lab.CollegeId = obj.CollegeId;
                    lab.DegreeId = obj.DegreeID;
                    lab.Degree = db.jntuh_specialization.Where(a => a.id == obj.SpecializationID).Select(s => s.jntuh_department.jntuh_degree.degree).FirstOrDefault();
                    lab.DepartmentId = obj.DepartmentID;
                    lab.Department = db.jntuh_specialization.Where(a => a.id == obj.SpecializationID).Select(s => s.jntuh_department.departmentName).FirstOrDefault();
                    lab.SpecializationId = obj.SpecializationID;
                    lab.Specialization = db.jntuh_specialization.Where(a => a.id == obj.SpecializationID).Select(a => a.specializationName).FirstOrDefault();
                    lab.Year = obj.Year;
                    lab.Semister = obj.Semester;
                    lab.Labcode = obj.Labcode;
                    lab.LabName = obj.LabName;
                    lab.EquipmentName = obj.EquipmentName;
                    lab.Remarks = obj.Remarks;
                    lab.noofUnits = obj.noofUnits;
                    lab.ExperimentsIds = obj.ExperimentsIds;
                    return PartialView("~/Views/LabsInsertAndDeletion/EditLabmasterSinglerecord.cshtml", lab);
                }
                else
                    return RedirectToAction("ViewLabmasterrecords");
            }
            else
                return RedirectToAction("ViewLabmasterrecords");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditLabmasterSinglerecord(EditLabsList labs)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int UserId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if ((labs.Id != 0 && labs.Id != null) && labs.Year != null && labs.Semister != null && labs.Labcode != null)
            {
                var data = db.jntuh_lab_master.Where(a => a.id == labs.Id).Select(s => s).FirstOrDefault();
                if (data != null)
                {
                    try
                    {
                        data.DegreeID = labs.DegreeId;
                        data.DepartmentID = labs.DepartmentId;
                        data.SpecializationID = labs.SpecializationId;
                        data.Year = Convert.ToInt32(labs.Year);
                        data.Semester = labs.Semister;
                        data.Labcode = labs.Labcode;
                        data.LabName = labs.LabName;
                        data.EquipmentName = labs.EquipmentName;
                        data.Remarks = labs.Remarks;
                        data.noofUnits = labs.noofUnits;
                        data.ExperimentsIds = labs.ExperimentsIds;
                        data.updatedOn = DateTime.Now;
                        data.updatedBy = UserId;
                        db.Entry(data).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["SUCCESS"] = "Lab is Updated Successfully.";
                        return RedirectToAction("ViewLabmasterrecords", "LabsInsertAndDeletion", new { labcode = labs.Labcode });
                    }
                    catch (Exception ex)
                    {
                        TempData["ERROR"] = "SomeThing went wrong.Try Again";
                        return RedirectToAction("ViewLabmasterrecords", "LabsInsertAndDeletion", new { labcode = labs.Labcode });
                    }
                }
                else
                {
                    TempData["ERROR"] = "Data is Not Found.";
                    return RedirectToAction("ViewLabmasterrecords", "LabsInsertAndDeletion", new { labcode = labs.Labcode });
                }
            }
            else
            {
                //TempData["ERROR"] = "SomeThing went wrong,Try Again";
                return RedirectToAction("ViewLabmasterrecords", "LabsInsertAndDeletion", new { labcode = labs.Labcode });
            }
        }

        #endregion

    }

    public class ExcelUpload
    {
        public int id { get; set; }
        //public int sno { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public string Specialization { get; set; }
        public int Year { get; set; }
        public Nullable<int> Semester { get; set; }
        public string Labcode { get; set; }
        public string LabName { get; set; }
        public string EquipmentName { get; set; }
        public string Remarks { get; set; }
        public string noofUnits { get; set; }
        public string ExperimentsIds { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public Nullable<int> CollegeId { get; set; }
        //public string FileName { get; set; }
    }

    public class EditLabsList
    {
        public int Id { get; set; }
        public int? CollegeTypeId { get; set; }
        public int? CollegeId { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int DegreeId { get; set; }
        public string Degree { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Specialization")]
        public int SpecializationId { get; set; }
        public string Specialization { get; set; }
        public int? Year { get; set; }
        // [Required]
        public int? Semister { get; set; }
        public string Labcode { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "LabName")]
        public string LabName { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "EquipmentName")]
        public string EquipmentName { get; set; }
        public string Remarks { get; set; }
        public string noofUnits { get; set; }
        public string ExperimentsIds { get; set; }
    }
}
