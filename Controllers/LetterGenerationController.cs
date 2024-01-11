using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
namespace UAAAS.Controllers
{
    //[ErrorHandling]
    public class LetterGenerationController : Controller
    {
        //
        // GET: /LetterGeneration/
        uaaasDBContext db = new uaaasDBContext();

        List<jntuh_academic_year> jntuh_academic_year_db = new List<jntuh_academic_year>();
        int actualYearDb = 0;
        int academicYearIdDb = 0;
        int PresentYearDb = 0;
        public LetterGenerationController()
        {
            jntuh_academic_year_db = db.jntuh_academic_year.ToList();
            actualYearDb = jntuh_academic_year_db.Where(w => w.isActive == true && w.isPresentAcademicYear == true).Select(e => e.actualYear).FirstOrDefault();
            academicYearIdDb = jntuh_academic_year_db.Where(w => w.actualYear == (actualYearDb + 1)).Select(e => e.id).FirstOrDefault();
            PresentYearDb = jntuh_academic_year_db.Where(w => w.actualYear == (actualYearDb + 1)).Select(e => e.actualYear).FirstOrDefault();
        }

        //[Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }

        //[Authorize(Roles = "Admin")]
        public ActionResult PhDVerificationlettergenerate()
        {

            return View();
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult PhDVerificationlettergenerate(HttpPostedFileBase Excelfile, string type)
        {
            if (Excelfile != null)
            {
                string fileName = string.Empty;
                var filepath = string.Empty;

                string excelpath = "~/Content/Upload/LetterGeneration/ExcelFileSaving";
                if (!Directory.Exists(Server.MapPath(excelpath)))
                    Directory.CreateDirectory(Server.MapPath(excelpath));

                fileName = Path.GetFileName(Excelfile.FileName);
                filepath = Path.Combine(Server.MapPath("~/Content/Upload/LetterGeneration/ExcelFileSaving"), fileName);
                FileInfo f = new FileInfo(filepath);
                if (f.Exists)
                {
                    f.Delete();
                }
                var ext = Path.GetExtension(Excelfile.FileName);
                if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                    Excelfile.SaveAs(filepath);
                else
                    return RedirectToAction("PhDVerificationlettergenerate");
                

                //var ext = Path.GetExtension(Excelfile.FileName);
                //if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                //    Excelfile.SaveAs(string.Format("{0}/{1}", Server.MapPath(excelpath), Excelfile.FileName));
                //else
                //    return RedirectToAction("PhDVerificationlettergenerate");

                string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path.Combine(Server.MapPath(excelpath), Excelfile.FileName) + ";Extended Properties=Excel 12.0;Persist Security Info=False";

                //Create Connection to Excel work book                   
                OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                excelConnection.Close();
                //Create OleDbCommand to fetch data from Excel
                excelConnection.Open();
                OleDbCommand cmd1 = new OleDbCommand("select count(*) from [" + excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["Table_Name"].ToString() + "]", excelConnection);
                var rows = (int)cmd1.ExecuteScalar();
                OleDbCommand cmd2 = new OleDbCommand("select * from [" + excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["Table_Name"].ToString() + "]", excelConnection);

                DataTable dt = new DataTable();
                OleDbDataAdapter oleda = new OleDbDataAdapter();
                oleda.SelectCommand = cmd2;
                DataSet ds = new DataSet();
                oleda.Fill(ds);
                dt = ds.Tables[0];
                List<lettergeneratedfileds> SuccessData = new List<lettergeneratedfileds>();
                List<lettergeneratedfileds> WithoutCollegeCodes = new List<lettergeneratedfileds>();
                List<lettergeneratedfileds> FailureData = new List<lettergeneratedfileds>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i][1].ToString()))
                        {
                            lettergeneratedfileds data = new lettergeneratedfileds();
                            data.collegecode = dt.Rows[i][1].ToString().Trim();
                            data.collegename = dt.Rows[i][2].ToString().Trim();
                            data.registrationno = dt.Rows[i][3].ToString().Trim();
                            data.facultyname = dt.Rows[i][4].ToString().Trim();
                            if (!string.IsNullOrEmpty(dt.Rows[i][5].ToString()))
                            {
                                data.date = dt.Rows[i][5].ToString().Trim();
                            }
                            if (!string.IsNullOrEmpty(dt.Rows[i][6].ToString()))
                            {
                                data.time = dt.Rows[i][6].ToString().Trim();
                            }
                            if (!string.IsNullOrEmpty(dt.Rows[i][7].ToString()))
                            {
                                data.venue = dt.Rows[i][7].ToString().Trim();
                            }
                            WithoutCollegeCodes.Add(data);
                            continue;
                        }
                        else if (!string.IsNullOrEmpty(dt.Rows[i][1].ToString()))
                        {
                            lettergeneratedfileds data = new lettergeneratedfileds();
                            data.collegecode = dt.Rows[i][1].ToString().Trim();
                            data.collegename = dt.Rows[i][2].ToString().Trim();
                            data.registrationno = dt.Rows[i][3].ToString().Trim();
                            data.facultyname = dt.Rows[i][4].ToString().Trim();
                            if (!string.IsNullOrEmpty(dt.Rows[i][5].ToString()))
                            {
                                data.date = dt.Rows[i][5].ToString().Trim();
                            }
                            if (!string.IsNullOrEmpty(dt.Rows[i][6].ToString()))
                            {
                                data.time = dt.Rows[i][6].ToString().Trim();
                            }
                            if (!string.IsNullOrEmpty(dt.Rows[i][7].ToString()))
                            {
                                data.venue = dt.Rows[i][7].ToString().Trim();
                            }
                            SuccessData.Add(data);
                            continue;
                        }

                    }
                    catch (Exception)
                    {
                        lettergeneratedfileds data = new lettergeneratedfileds();
                        data.collegecode = dt.Rows[i][1].ToString().Trim();
                        data.collegename = dt.Rows[i][2].ToString().Trim();
                        data.registrationno = dt.Rows[i][3].ToString().Trim();
                        data.facultyname = dt.Rows[i][4].ToString().Trim();
                        if (!string.IsNullOrEmpty(dt.Rows[i][5].ToString()))
                        {
                            data.date = dt.Rows[i][5].ToString().Trim();
                        }
                        if (!string.IsNullOrEmpty(dt.Rows[i][6].ToString()))
                        {
                            data.time = dt.Rows[i][6].ToString().Trim();
                        }
                        if (!string.IsNullOrEmpty(dt.Rows[i][7].ToString()))
                        {
                            data.venue = dt.Rows[i][7].ToString().Trim();
                        }
                        FailureData.Add(data);
                        continue;
                    }
                }

                excelConnection.Close();

                var CollegeCodes = SuccessData.GroupBy(e => e.collegecode).Select(r => r.Key).ToList();

                ZipFile zip = new ZipFile();
                zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                var directorypath = "PhD_Verification_Documents" + DateTime.Now.ToString("yyyy-MMM-dd-HHmm");
                zip.AddDirectoryByName(directorypath);

                string timeslot = DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second;

                foreach (var item in CollegeCodes)
                {
                    string path = GeneratePdf(item, timeslot, SuccessData.Where(r => r.collegecode == item).ToList());
                }
                string datepath = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
                string[] filePaths = Directory.GetFiles(Server.MapPath("~/Content/Upload/LetterGeneration/PhDVerification/" + datepath + "/" + timeslot + "/"));

                foreach (var row in filePaths)
                {
                    string filePath = row;
                    zip.AddFile(filePath, directorypath);
                }



                Response.Clear();
                Response.BufferOutput = false;
                string zipName = String.Format(directorypath + ".zip");
                Response.ContentType = "application/zip";
                Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
                zip.Save(Response.OutputStream);
                Response.End();
            }
            return View();
        }

        public string GeneratePdf(string collegecode ,string timeslot , List<lettergeneratedfileds> ObjListData)
        {
            string Content = string.Empty;
            int Count = 1;
            var Colleges = db.jntuh_college.ToList();
            var address = db.jntuh_address.ToList();
            var district = db.jntuh_district.Where(r => r.isActive == true).ToList();

            var collegeaddress = (from c in Colleges
                                  join a in address on c.id equals a.collegeId
                                  join d in district on a.districtId equals d.id
                                  where a.addressTye == "COLLEGE"
                                  select new
                                  {
                                      collegeId = c.id,
                                      code = c.collegeCode,
                                      name = c.collegeName,
                                      college_address = c.collegeName + ", <br/>" + a.address + ", " + a.townOrCity + ", " + a.mandal + ", <br/>" + d.districtName + " - " + a.pincode
                                  }).ToList();


            var CollegeData = collegeaddress.Where(r => r.code == collegecode).Select(r => r).FirstOrDefault();
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 50);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string datepath = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
            //string timeslot = DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second;
            datepath += "/" + timeslot;
            string path = Server.MapPath("~/Content/Upload/LetterGeneration/PhDVerification/" + datepath + "/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.code.ToUpper() + "_PhDVerification.pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.code.ToUpper() + "_PhDVerification.pdf";

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/Letter_Generation.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##COLLEGE_CODE##", CollegeData.code.ToUpper());
            contents = contents.Replace("##COLLEGE_ADDRESS##", CollegeData.college_address.ToUpper());
            contents = contents.Replace("##CurrentDate##", DateTime.Now.ToString("dd-MM-yyy"));

            var jntuh_generated_letters = db.jntuh_generated_letters.Where(r => r.letterCode == "PhD_Ver").Select(r => r).FirstOrDefault();
            string letter_information = string.Empty;

            letter_information += "<table border='0' cellpadding='3' cellspacing='0' width='100%' style='font-family:Times New Roman; font-size: 11px; font-weight: normal; margin: 0 auto;'>";
            letter_information += "<tr>";
            letter_information += "<td align='left' width='7%' valign='top'>Sub :- " + jntuh_generated_letters.letterSubject + "";
            letter_information += "</td>";
            letter_information += "</tr>";
            letter_information += "</table>";
            letter_information += "<br/>";

            if (jntuh_generated_letters.isRef == true)
            {
                letter_information += "<table border='0' cellpadding='3' cellspacing='0' width='100%' style='font-family:Times New Roman; font-size: 11px; font-weight: normal; margin: 0 auto;'>";
                letter_information += "<tr>";
                letter_information += "<td align='left' width='7%' valign='top'>Ref :- " + jntuh_generated_letters.letterReference + "";
                letter_information += "</td>";
                letter_information += "</tr>";
                letter_information += "</table>";
                letter_information += "<br/>";
            }

            letter_information += "<table border='0' cellpadding='3' cellspacing='0' width='100%' style='font-family:Times New Roman; font-size: 11px; font-weight: normal; margin: 0 auto;'>";
            letter_information += "<tr>";
            letter_information += "<td align='left' width='7%' valign='top'>Dear Sir/Madam,";
            letter_information += "</td>";
            letter_information += "</tr>";
            letter_information += "</table>";

            letter_information += "<br/>";
            letter_information += "<table border='0' cellpadding='3' cellspacing='0' width='100%' style='font-family:Times New Roman; font-size: 11px;; margin: 0 auto;'>";
            letter_information += "<tr>";
            letter_information += "<td align='justify'  width='7%' valign='top' style='line-height: 16px;'>" + jntuh_generated_letters.letterDescription + "";
            letter_information += "</td>";
            letter_information += "</tr>";
            letter_information += "</table>";
            letter_information += "<br/>";

            contents = contents.Replace("##letter_information##", letter_information);
            //contents = contents.Replace("##letter_subject##", jntuh_generated_letters.letterSubject.ToUpper());
            //contents = contents.Replace("##letter_description##", jntuh_generated_letters.letterDescription.ToUpper());

            List<lettergeneratedfileds> facultyListdata = new List<lettergeneratedfileds>();
            facultyListdata = ObjListData.Where(r => r.collegecode == collegecode).Select(a => new lettergeneratedfileds { registrationno = a.registrationno, facultyname = a.facultyname, date = a.date, time = a.time }).ToList();

            Content += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 11px;'>";
            Content += "<tbody>";
            Content += "<tr style='font-weight:bold;'>";
            Content += "<td width='4%'><p align='center'>S.No</p></td>";
            Content += "<td width='15%'><p align='center'>RegistrationNumber</p></td>";
            Content += "<td width='15%'><p align='center'>Name</p></td>";
            Content += "<td width='12%'><p align='center'>Schedule</p></td>";
            Content += "</tr>";

            foreach (var item2 in facultyListdata)
            {
                string datetime = string.Empty;
                Content += "<tr>";
                Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item2.registrationno + "</p></td>";
                Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item2.facultyname + "</p></td>";
                if (!string.IsNullOrEmpty(item2.date))
                {
                    datetime += item2.date.Split(' ')[0];
                }
                if (!string.IsNullOrEmpty(item2.time))
                {
                    datetime += " " + item2.time.Split(' ')[1];
                }
                //var datetime = item2.date.Split(' ')[0] + item2.time.Split(' ')[1];
                Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + datetime + "</p></td>";
                Content += "</tr>";
            }

            Content += "</tbody>";
            Content += "</table>";

            contents = contents.Replace("##ScheduleTable##", Content);

            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {
                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }
                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 60, 50);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 60, 50);
                                pageRotated = false;
                            }
                        }
                        pdfDoc.NewPage();
                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        //[Authorize(Roles = "Admin")]
        public ActionResult Letters()
        {
            List<jntuh_generated_letters> jntuh_generated_letters = db.jntuh_generated_letters.ToList();
            List<genereatedColumns> listdata = new List<genereatedColumns>();
            foreach (var item in jntuh_generated_letters)
            {
                genereatedColumns obj = new genereatedColumns();
                obj.id = item.id;
                obj.lettercode = item.letterCode;
                obj.lettercodedesc = item.lettercodeDescription;
                obj.lettersub = item.letterSubject;
                obj.letterref = item.letterReference;
                obj.letterdesc = item.letterDescription;
                obj.isactive = Convert.ToBoolean(item.isActive);
                obj.generatedDate = item.letterGeneratedDate.ToString();
                listdata.Add(obj);
            }
            return View(listdata);
        }

        //[Authorize(Roles = "Admin")]
        public ActionResult LetterInformation(int id)
        {
            if (id != null)
            {
                jntuh_generated_letters letter = db.jntuh_generated_letters.Where(e => e.id == id).FirstOrDefault();
                genereatedColumns obj = new genereatedColumns();
                obj.id = letter.id;
                obj.lettercode = letter.letterCode;
                obj.lettercodedesc = letter.lettercodeDescription;
                obj.lettersub = letter.letterSubject;
                obj.isref = Convert.ToBoolean(letter.isRef);
                obj.letterref = letter.letterReference;
                obj.letterdesc = letter.letterDescription;
                obj.isactive = Convert.ToBoolean(letter.isActive);
                obj.generatedDate = letter.letterGeneratedDate.ToString();
                return View(obj);
            }
            return RedirectToAction("Letters");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult LetterInformation()
        {
            genereatedColumns obj= new genereatedColumns();
            if (obj != null)
            {
                jntuh_generated_letters jntuh_generated_letters = db.jntuh_generated_letters.Where(e => e.id == obj.id).FirstOrDefault();
                if (jntuh_generated_letters != null)
                {
                    jntuh_generated_letters.lettercodeDescription = obj.lettercodedesc;
                    jntuh_generated_letters.letterDescription = obj.letterdesc;
                    jntuh_generated_letters.letterSubject = obj.lettersub;
                    jntuh_generated_letters.letterReference = obj.letterref;
                    jntuh_generated_letters.isRef = obj.isref;

                    if (obj.generatedDate != null)
                        jntuh_generated_letters.letterGeneratedDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(obj.generatedDate);

                    jntuh_generated_letters.updatedOn = DateTime.Now;
                    jntuh_generated_letters.updatedBy = 450;
                    db.Entry(jntuh_generated_letters).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Letter Information is Updated Successfully.";
                }
            }
            else
            {
                TempData["ERROR"] = "Some thing Went Wrong.Please Try Again.";

            }
            return RedirectToAction("Letters");
        }

        public ActionResult TestingView()
        {
            genereatedColumns obj = new genereatedColumns();
            return View(obj);
        }

    }


    public class lettergeneratedfileds
    {
        public int collegeId { get; set; }
        public string collegecode { get; set; }
        public string collegename { get; set; }
        public string registrationno { get; set; }
        public string facultyname { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string venue { get; set; }
    }

    public class genereatedColumns
    {
        public int id { get; set; }
        public string lettercode { get; set; }
        public string lettercodedesc { get; set; }
        public string lettersub { get; set; }
        public string letterref { get; set; }
        [AllowHtml]
        public string letterdesc { get; set; }
        public string generatedDate { get; set; }
        public bool isref { get; set; }
        public bool isactive { get; set; }
    }

}
