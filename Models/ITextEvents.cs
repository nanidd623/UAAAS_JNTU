using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class ITextEvents : PdfPageEventHelper
    {
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public string formType { get; set; }

        // This is the contentbyte object of the writer
        PdfContentByte cb;

        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;

        // this is the BaseFont we are going to use for the header / footer
        BaseFont bf = null;

        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;

        #region Fields
        private string _header;
        #endregion

        #region Properties
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }

        #endregion

        //public override void OnStartPage(PdfWriter writer, Document document)
        //{
        //    base.OnStartPage(writer, document);
        //}



        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                PrintTime = DateTime.Now;
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;
                headerTemplate = cb.CreateTemplate(50, 50);
                footerTemplate = cb.CreateTemplate(50, 50);
            }
            catch (DocumentException de)
            {
                throw de.InnerException;
            }
            catch (System.IO.IOException ioe)
            {
                throw ioe.InnerException;
            }
        }

        public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            base.OnEndPage(writer, document);

            if (writer.PageNumber != 1)//&& writer.PageNumber != 2 && writer.PageNumber != 3
            {
                iTextSharp.text.Font baseFontSmall = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);

                iTextSharp.text.Font baseFontNormal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);

                iTextSharp.text.Font baseFontBig = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

                //Create PdfTable object
                PdfPTable pdfTab = new PdfPTable(4);

                String text = "Page " + writer.PageNumber + " of ";

                //Add paging to header
                {
                    cb.BeginText();
                    cb.SetFontAndSize(bf, 10);
                    cb.SetTextMatrix(document.PageSize.GetRight(115), document.PageSize.GetTop(45));
                    cb.ShowText(text);
                    cb.EndText();
                    float len = bf.GetWidthPoint(text, 10);
                    //Adds "12" in Page 1 of 12
                    cb.AddTemplate(headerTemplate, document.PageSize.GetRight(115) + len, document.PageSize.GetTop(45));
                }

                ////Add paging to footer
                //{
                //    cb.BeginText();
                //    cb.SetFontAndSize(bf, 10);
                //    cb.SetTextMatrix(document.PageSize.GetRight(115), document.PageSize.GetBottom(40));
                //    cb.ShowText(text);
                //    cb.EndText();
                //    float len = bf.GetWidthPoint(text, 10);
                //    cb.AddTemplate(footerTemplate, document.PageSize.GetRight(115) + len, document.PageSize.GetBottom(40));
                //}
                PdfPCell pdfCellName = new PdfPCell();
                if (formType != "")
                {
                    pdfCellName = new PdfPCell(new Phrase("[" + CollegeCode + "] - " + PrintTime.ToShortDateString() + string.Format(" {0:t}", DateTime.Now), baseFontNormal));
                }
                else
                {
                    pdfCellName = new PdfPCell(new Phrase(PrintTime.ToString("dd-MMM-yyy") + string.Format(" {0:t}", DateTime.Now), baseFontNormal));
                }

                //PdfPCell pdfCellDate = new PdfPCell(new Phrase("Date:" + PrintTime.ToShortDateString() + string.Format(" {0:t}", DateTime.Now), baseFontNormal));
                PdfPCell pdfCellDate = new PdfPCell(new Phrase("", baseFontNormal));

                //set the alignment of all cells and set border to 0
                pdfCellDate.HorizontalAlignment = Element.ALIGN_RIGHT;
                pdfCellDate.VerticalAlignment = Element.ALIGN_BOTTOM;
                pdfCellName.Border = 0;
                pdfCellDate.Border = 0;

                pdfCellName.Colspan = 3;

                //add all cells into PdfTable
                pdfTab.AddCell(pdfCellName);
                pdfTab.AddCell(pdfCellDate);

                pdfTab.TotalWidth = document.PageSize.Width - 80f;
                pdfTab.WidthPercentage = 70;
                //pdfTab.HorizontalAlignment = Element.ALIGN_CENTER;

                //call WriteSelectedRows of PdfTable. This writes rows from PdfWriter in PdfTable
                //first param is start row. -1 indicates there is no end row and all the rows to be included to write
                //Third and fourth param is x and y position to start writing
                pdfTab.WriteSelectedRows(0, -1, 60, document.PageSize.Height - 35, writer.DirectContent);
                //set pdfContent value

                PdfPTable pdfTabFooter = new PdfPTable(2);
                PdfPCell pdfCellLeft = new PdfPCell();
                PdfPCell pdfCellRight = new PdfPCell();
                //PdfPCell pdfCellCenter = new PdfPCell();
                //if (formType == "A-114")
                if (formType == "A-423")
                {
                    pdfCellLeft = new PdfPCell(new Phrase("Name & Signature of Principal/Director", baseFontSmall));
                    pdfCellRight = new PdfPCell(new Phrase("Name & Signature of Chairman/Secretary", baseFontSmall));
                }
                //if (formType == "A-414")
                if (formType == "A-423" && (writer.PageNumber != 2 && writer.PageNumber != 3))
                {

                    //pdfCellRight = new PdfPCell(new Phrase("Total Number of corrections in page:_______", baseFontSmall));
                    pdfCellRight = new PdfPCell(new Phrase("Name & Signature of the Committee Member", baseFontSmall));
                    pdfCellLeft = new PdfPCell(new Phrase("Name & Signature of the Management", baseFontSmall));
                }

                //set the alignment of all cells and set border to 0
                pdfCellLeft.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfCellRight.HorizontalAlignment = Element.ALIGN_CENTER;
                //pdfCellCenter.HorizontalAlignment = Element.ALIGN_CENTER;

                pdfCellLeft.VerticalAlignment = Element.ALIGN_TOP;
                pdfCellRight.VerticalAlignment = Element.ALIGN_TOP;
                //pdfCellCenter.VerticalAlignment = Element.ALIGN_TOP;

                pdfCellLeft.Border = 0;
                pdfCellRight.Border = 0;
                //pdfCellCenter.Border = 0;

                //add all cells into PdfTable
                pdfTabFooter.AddCell(pdfCellLeft);
                pdfTabFooter.AddCell(pdfCellRight);
                //pdfTabFooter.AddCell(pdfCellCenter);

                pdfTabFooter.TotalWidth = document.PageSize.Width - 80f;
                pdfTabFooter.WidthPercentage = 70;

                pdfTabFooter.WriteSelectedRows(0, -1, 60, document.PageSize.GetBottom(50), writer.DirectContent);

                //Move the pointer and draw line to separate header section from rest of page
                cb.MoveTo(60, document.PageSize.Height - 50);
                cb.LineTo(document.PageSize.Width - 50, document.PageSize.Height - 50);
                cb.Stroke();

                if (formType != "")
                {
                    //Move the pointer and draw line to separate footer section from rest of page
                    cb.MoveTo(60, document.PageSize.GetBottom(50));
                    cb.LineTo(document.PageSize.Width - 50, document.PageSize.GetBottom(50));
                    cb.Stroke();
                }
            }

        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            headerTemplate.BeginText();
            headerTemplate.SetFontAndSize(bf, 10);
            headerTemplate.SetTextMatrix(0, 0);
            headerTemplate.ShowText((writer.PageNumber - 1).ToString());
            headerTemplate.EndText();

            //footerTemplate.BeginText();
            //footerTemplate.SetFontAndSize(bf, 10);
            //footerTemplate.SetTextMatrix(0, 0);
            //footerTemplate.ShowText((writer.PageNumber - 1).ToString());
            //footerTemplate.EndText();
        }
    }
}