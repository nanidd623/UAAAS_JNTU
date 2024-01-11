using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
namespace UAAAS.Controllers
{
    public class DocumentsGettingController : Controller
    {
        uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /DocumentsGetting/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PanCardDocumentsGetting()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PanCardDocumentsGetting(HttpPostedFileBase UploadFile)
        {
            if (UploadFile != null)
            {
                string PanNumberExcelFilePath = "~/Content/Upload/DevelopersPurpose";
                string ServerPANDocSavingPath = "C:/JNTUH/ServerPANDocSavingPath";
                string fileName = string.Empty;
                string filePath = string.Empty;
                List<string> PanNumbersList = new List<string>();
                if (UploadFile != null)
                {
                    if (!Directory.Exists(Server.MapPath(PanNumberExcelFilePath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(PanNumberExcelFilePath));
                    }
                    fileName = Path.GetFileName(UploadFile.FileName);
                    filePath = Path.Combine(Server.MapPath("~/Content/Upload/DevelopersPurpose"), fileName);
                    FileInfo f = new FileInfo(filePath);
                    if (f.Exists)
                    {
                        f.Delete();
                    }
                   
                    UploadFile.SaveAs(string.Format("{0}/{1}", Server.MapPath(PanNumberExcelFilePath), fileName));

                    #region Excel File Code
                    string extension = Path.GetExtension(UploadFile.FileName);
                    string conString = string.Empty;
                    switch (extension)
                    {
                        case ".xls":
                            conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                            break;
                        case ".xlsx":
                            conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                            break;
                    }                
                    conString = string.Format(conString, filePath);
                    using (OleDbConnection connection = new OleDbConnection(conString))
                    {
                        connection.Open();
                        OleDbCommand command = new OleDbCommand("select * from [Sheet1$]", connection);
                         using (OleDbDataReader dr = command.ExecuteReader())
                        {
                             if(dr.HasRows) {
                                 while (dr.Read()){
                                     var row1Col0 = dr[1];
                                     PanNumbersList.Add(row1Col0.ToString());
                                 }
                             }
                        }
                    }

                    var jntuh_registered_faculty_PanDocumentPaths = db.jntuh_registered_faculty.Where(w => PanNumbersList.Contains(w.PANNumber)).Select(q => q).ToList();
                   
                    ServerPANDocSavingPath = ServerPANDocSavingPath + "/" + DateTime.Now.ToString("dd-MM-yyyy");
                    if (!Directory.Exists(ServerPANDocSavingPath))
                    {
                        Directory.CreateDirectory(ServerPANDocSavingPath);
                    }

                    foreach (var item in jntuh_registered_faculty_PanDocumentPaths)
                    {
                        var ServerPath = "http://jntuhaac.in/Content/Upload/Faculty/PANCARDS/" + item.PANDocument;
                        string NewFilename = item.PANNumber + "_PANDoc.Jpeg";

                        byte[] data;
                        using (WebClient client = new WebClient()){
                            data = client.DownloadData(ServerPath);
                        }

                       // System.IO.File.WriteAllBytes(@"C:\JNTUH\ServerPANDocSavingPath\" + OldFilename + "", data);
                        using (Image image = Image.FromStream(new MemoryStream(data))){
                            image.Save(string.Format("{0}/{1}", ServerPANDocSavingPath, NewFilename), ImageFormat.Jpeg);  // Or Png
                        }

                    }
                    #endregion
                }
            }
            return View();
        }
    }

}
