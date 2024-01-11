using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.Users
{
    [ErrorHandling]
    public class DownloadsController : Controller
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /Downloads/

        public class Download
        {
            public int rowNumber { get; set; }
            public string title { get; set; }
            public string word { get; set; }
            public string excel { get; set; }
            public string pdf { get; set; }
        }
        public ActionResult Index()
        {
            List<Download> lst = db.jntuh_download.AsEnumerable().Where(d => d.isActive == true).Select((d, index) => new Download
            {
                rowNumber = index + 1,
                title = d.downloadTitle,
                word = d.downloadWord,
                excel = d.downloadExcel,
                pdf = d.downloadPDF
            }).ToList();
            ViewBag.Downloads = lst;
            return View();
        }

        // GET: /CollegeInformation/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {

            return View();
        }
        public class ViewDataUploadFilesResult
        {
            public string Name { get; set; }
            public int Length { get; set; }
        }

        //public ActionResult Create()
        //{
        //    return View();
        //}

        // POST: /CollegeInformation/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        public ActionResult Create(jntuh_download jntuh_download)
        {
            if (ModelState.IsValid)
            {
                var rowExists = db.jntuh_download.Where(d => d.downloadTitle == jntuh_download.downloadTitle).Select(d => d.downloadTitle).ToList();

                if (rowExists == null || rowExists.Count() == 0)
                {
                    int Id = db.jntuh_download.Count() > 0 ? db.jntuh_download.Select(d => d.id).Max() : 0;
                    Id = Id + 1;

                    if (!Directory.Exists(Server.MapPath("~/Content/Upload/Downloads")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Upload/Downloads"));
                    }

                    if (jntuh_download.uploadFile1 != null)
                    {
                        var ext = Path.GetExtension(jntuh_download.uploadFile1.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            jntuh_download.uploadFile1.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/Downloads"), Id, ext));
                            jntuh_download.downloadPDF = string.Format("{0}/{1}{2}", "~/Content/Upload/Downloads", Id, ext);
                        }

                        if (ext.ToUpper().Equals(".DOC") || ext.ToUpper().Equals(".DOCX"))
                        {
                            jntuh_download.uploadFile1.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/Downloads"), Id, ext));
                            jntuh_download.downloadWord = string.Format("{0}/{1}{2}", "~/Content/Upload/Downloads", Id, ext);
                        }

                        if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                        {
                            jntuh_download.uploadFile1.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/Downloads"), Id, ext));
                            jntuh_download.downloadExcel = string.Format("{0}/{1}{2}", "~/Content/Upload/Downloads", Id, ext);
                        }
                    }

                    if (jntuh_download.uploadFile2 != null)
                    {
                        var ext = Path.GetExtension(jntuh_download.uploadFile2.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            jntuh_download.uploadFile2.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/Downloads"), Id, ext));
                            jntuh_download.downloadPDF = string.Format("{0}/{1}{2}", "~/Content/Upload/Downloads", Id, ext);
                        }

                        if (ext.ToUpper().Equals(".DOC") || ext.ToUpper().Equals(".DOCX"))
                        {
                            jntuh_download.uploadFile2.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/Downloads"), Id, ext));
                            jntuh_download.downloadWord = string.Format("{0}/{1}{2}", "~/Content/Upload/Downloads", Id, ext);
                        }

                        if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                        {
                            jntuh_download.uploadFile2.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/Downloads"), Id, ext));
                            jntuh_download.downloadExcel = string.Format("{0}/{1}{2}", "~/Content/Upload/Downloads", Id, ext);
                        }
                    }

                    if (jntuh_download.uploadFile3 != null)
                    {
                        var ext = Path.GetExtension(jntuh_download.uploadFile3.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            jntuh_download.uploadFile3.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/Downloads"), Id, ext));
                            jntuh_download.downloadPDF = string.Format("{0}/{1}{2}", "~/Content/Upload/Downloads", Id, ext);
                        }

                        if (ext.ToUpper().Equals(".DOC") || ext.ToUpper().Equals(".DOCX"))
                        {
                            jntuh_download.uploadFile3.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/Downloads"), Id, ext));
                            jntuh_download.downloadWord = string.Format("{0}/{1}{2}", "~/Content/Upload/Downloads", Id, ext);
                        }

                        if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                        {
                            jntuh_download.uploadFile3.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/Downloads"), Id, ext));
                            jntuh_download.downloadExcel = string.Format("{0}/{1}{2}", "~/Content/Upload/Downloads", Id, ext);
                        }
                    }

                    db.jntuh_download.Add(jntuh_download);
                    db.SaveChanges();

                    TempData["Success"] = "Added successfully.";
                    ModelState.Clear();
                }
                else
                {
                    TempData["Error"] = "Download is already exists. Please enter a different Download Title.";
                }
            }
            return View(jntuh_download);
        }
    }
}
