using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MVCYorbit.Models;

namespace MVCYorbit.Controllers
{
    public class HomeController : Controller
    {
        readonly MVCService.MVCYorbitServiceClient mVCYorbitService = new MVCService.MVCYorbitServiceClient();
        [HttpGet]
        public ActionResult HomePage()
        {
            ViewBag.Role = Session["Role"];
            return View();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Login","Account");
        }

        [HttpPost]
        public JsonResult ViewAndEdit(AddMember member)
        {
            Session["AppID"] = member.ApplicationID;
            List<AddMember> memberData = new List<AddMember>();
            memberData.Add(member);
            Session["MemberSave"] = memberData;
            if (Session["MemberSave"] != null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet]
        public ActionResult AddMember()
        {
           AddMember memberData = new AddMember();
            string uName = (string)Session["uName"];
            try
            {
                if (Session["AppID"] != null)
                {
                    string appId = (string)Session["AppID"];
                    var savedMember = (List<AddMember>)Session["MemberSave"];
                    foreach (var saved in savedMember)
                    {
                        memberData.FirstName = saved.FirstName;
                        memberData.LastName = saved.LastName;
                        memberData.MiddleName = saved.MiddleName;
                        memberData.Suffix = saved.Suffix;
                        var dt = saved.DateOfBirth.ToString("MM/dd/yyyy");
                        ViewBag.Date = DateTime.ParseExact(dt, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        ViewBag.Gender = saved.Gender;
                    }
                }
                if (mVCYorbitService.GetMemberDataCount(uName) >= 1)
                {
                    ViewBag.MemberData = mVCYorbitService.GetMembers(uName);
                }
                if (memberData != null)
                {
                    return View(memberData);
                }
            }
            catch(Exception ex)
            {
                MVCYorbit.LOG.Logging(ex.Message);
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddMember(AddMember addMember)
        {
            string uName = (string)Session["uName"];
            if (ModelState.IsValid)
            {
                try
                {
                    int checkAge = AgeCalculator(addMember.DateOfBirth);
                    if (checkAge <= 0 || checkAge > 125)
                    {
                        ViewBag.Error = "Age Should be in between 0 to 125";
                        return View();
                    }
                    if (mVCYorbitService.GetMemberDataCount(uName) >= 1)
                    {

                        AddMember addMember1 = new AddMember
                        {
                            DateOfBirth = Convert.ToDateTime(mVCYorbitService.GetFirstMemberData())
                        };
                        int firstMemberAge = AgeCalculator(addMember1.DateOfBirth);
                        if (checkAge > firstMemberAge)
                        {
                            ViewBag.Error = "Incorrect Age : The Date of Birth must be less than the first member Date of Birth";
                        }
                        else
                        {
                            if (mVCYorbitService.GetMemberDataCount(uName) < 5)
                            {
                                Session["MemberData"] = addMember;
                                ViewBag.Message = AddMemberData();
                            }
                            else
                            {
                                ViewBag.Error = "Family members cannot be exceeded more than 5 !!!";
                            }
                        }

                    }
                    else
                    {
                        Session["MemberData"] = addMember;
                        ViewBag.Message = AddMemberData();
                    }
                }
                catch (Exception ex)
                {
                    MVCYorbit.LOG.Logging(ex.Message);
                }
            }
            ViewBag.MemberData = mVCYorbitService.GetMembers(uName);
            return View();
        }

        public string AddMemberData()
        {
            string status = string.Empty ;
            try
            {
                MVCService.AddMember addMemberData = new MVCService.AddMember();
                AddMember addMember = (AddMember)Session["MemberData"];
                string uName = (string)Session["uName"];
                addMemberData.FirstName = addMember.FirstName;
                addMemberData.LastName = addMember.LastName;
                if (addMember.MiddleName != null)
                {
                    addMemberData.MiddleName = addMember.MiddleName;
                }
                addMemberData.Suffix = addMember.Suffix;
                addMemberData.Gender = addMember.Gender;
                addMemberData.DateOfBirth = addMember.DateOfBirth;
                addMemberData.CreatedTimeStamp = DateTime.Now;
                addMemberData.LastModifiedTimeStamp = DateTime.Now;
                addMemberData.MetaActive = 1;
                status = mVCYorbitService.AddMember(addMemberData, uName);
            }
            catch (Exception ex)
            {
                MVCYorbit.LOG.Logging(ex.Message);
            }
            return status;
        }

        [HttpPost]
        public JsonResult Edit(AddMember memberData)
        {
            Session["MemberEdit"] = memberData;
            try
            {
                if (ModelState.IsValid)
                {
                    int checkAge = AgeCalculator(memberData.DateOfBirth);
                    if (checkAge <= 0 || checkAge > 125)
                    {
                        ViewBag.Error = "Age Should be in between 0 to 125";
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (AddMemberEdit())
                        {
                            return Json(true, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MVCYorbit.LOG.Logging(ex.Message);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public bool AddMemberEdit()
        {
            AddMember addMember = (AddMember)Session["MemberEdit"];
            MVCService.AddMember addMemberData = new MVCService.AddMember
            {
                ID = addMember.ID,
                FirstName = addMember.FirstName,
                LastName = addMember.LastName
            };
            if (addMember.MiddleName != null)
            {
                addMemberData.MiddleName = addMember.MiddleName;
            }
            addMemberData.Suffix = addMember.Suffix;
            addMemberData.Gender = addMember.Gender;
            addMemberData.DateOfBirth = addMember.DateOfBirth;
            addMemberData.CreatedTimeStamp = DateTime.Now;
            addMemberData.LastModifiedTimeStamp = DateTime.Now;
            addMemberData.MetaActive = 1;

            bool status = mVCYorbitService.EditMember(addMemberData);
            return status;
            
        }

        [HttpPost]
        public JsonResult MemberDelete(int memberID)
        {
           int retVal = mVCYorbitService.MemberDel(memberID);
            if(retVal == 1)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            
        }

        [HttpPost]
        public JsonResult Save(AddMember memberData)
        {
          
            if (ModelState.IsValid)
            { try
                {
                    if (Session["AppID"] == null)
                    {
                        //memberData.ApplicationID = ApplicationID();
                        List<AddMember> saveMember = new List<AddMember>();
                        saveMember.Add(memberData);
                        Session["AppID"] = ApplicationID();
                        Session["MemberSave"] = saveMember;
                        return Json(true, JsonRequestBehavior.AllowGet);
                        
                    }
                    else
                    {
                     
                        Session["MemberSave"] = memberData;
                        return Json(true, JsonRequestBehavior.AllowGet);
                        
                    }
                }
                catch (Exception ex)
                {
                    MVCYorbit.LOG.Logging(ex.Message);
                }

            }
            return Json(ModelState.IsValid);
        }

        [HttpPost]
        public JsonResult Next()
        {
            string uName = (string)Session["uName"];
            if (mVCYorbitService.GetMemberDataCount(uName) >= 2)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
               
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Relationship()
        {
            string uName = (string)Session["uName"];
            string[] values = (ConfigurationManager.AppSettings["Relations"]).Split(',').Select(sValue => sValue.Trim()).ToArray();
            List<SelectListItem> dropDowns = new List<SelectListItem>(); for (int i = 0; i < values.Length; i++)
            {
                dropDowns.Add(new SelectListItem { Text = values[i], Value = values[i] });
            }
            ViewBag.Relations = dropDowns; 

            ViewBag.Names = mVCYorbitService.GetMembers(uName); 
            return View();
        }

        [HttpPost]
        public JsonResult RelationShipSubmit(int memberId, string[] relatedmemberId, string[] relation)
        {
            try
            {
                MVCService.RelationShip relationShip = new MVCService.RelationShip
                {
                    MemberId = memberId
                };
                string uName = (string)Session["uName"];
                for(int i=0;i<relatedmemberId.Length;i++)
                {
                    relationShip.RelatedMemberId = relatedmemberId[i];
                    relationShip.RelationType = relation[i];
                    relationShip.CreatedTimeStamp = DateTime.Now;
                    relationShip.LastModifiedTimeStamp = DateTime.Now;
                    relationShip.MetaActive = 1;
                    mVCYorbitService.SubmitRelationShip(relationShip,uName);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                MVCYorbit.LOG.Logging(ex.Message);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ApplicationSubmit()
        {
            string uName = (string)Session["uName"];
            string appID = ApplicationID();
            if (mVCYorbitService.SubmitApplication(uName, appID) == "success")
            {
                ViewBag.Status = "Your Application has been submitted. For transaction receipt please download the file";
            }
            else
            {
                ViewBag.Status = "Some Error Occurred ";
            }
            return View();
        }

        [HttpGet]
        public ActionResult SearchApplication()
        {
            string uName = (string)Session["uName"];
            string[] values = (ConfigurationManager.AppSettings["Status"]).Split(',').Select(sValue => sValue.Trim()).ToArray();
            List<SelectListItem> dropDowns = new List<SelectListItem>(); for (int i = 0; i < values.Length; i++)
            {
                dropDowns.Add(new SelectListItem { Text = values[i], Value = values[i] });
            }
            ViewBag.Status = dropDowns;
            ViewBag.Search = mVCYorbitService.SearchApplication();
            return View();
        }

        [HttpPost]
        public ActionResult SearchApplication(Search search)
        {
            string uName = (string)Session["uName"];
            string[] values = (ConfigurationManager.AppSettings["Status"]).Split(',').Select(sValue => sValue.Trim()).ToArray();
            List<SelectListItem> dropDowns = new List<SelectListItem>(); for (int i = 0; i < values.Length; i++)
            {
                dropDowns.Add(new SelectListItem { Text = values[i], Value = values[i] });
            }

            ViewBag.Status = dropDowns;
            ViewBag.Search = mVCYorbitService.SearchMember(search.FirstName, search.LastName, search.DOB, search.ApplicationID);
            return View();
        }

        public string SessionData()
         {
            string status = string.Empty;
            try
            {
                MVCService.AddMember addMemberData = new MVCService.AddMember();
                AddMember addMember = (AddMember)Session["MemberSave"];

                string uName = (string)Session["uName"];
                addMemberData.FirstName = addMember.FirstName;
                addMemberData.LastName = addMember.LastName;
                if (addMember.MiddleName != null)
                {
                    addMemberData.MiddleName = addMember.MiddleName;
                }
                addMemberData.Suffix = addMember.Suffix;
                addMemberData.Gender = addMember.Gender;
                addMemberData.DateOfBirth = addMember.DateOfBirth;
                addMemberData.CreatedTimeStamp = DateTime.Now;
                addMemberData.LastModifiedTimeStamp = DateTime.Now;
                addMemberData.MetaActive = 1;
                addMemberData.ApplicationID = addMember.ApplicationID;
                status = mVCYorbitService.AddMember(addMemberData, uName);
                
            }
            catch (Exception ex)
            {
                MVCYorbit.LOG.Logging(ex.Message);
            }
            return status;
        }

        public string SaveAppId()
        {
            string status = string.Empty;
            try
            {
                MVCService.AddMember addMemberData = new MVCService.AddMember();
                AddMember addMember = (AddMember)Session["MemberSave"];
                addMemberData.FirstName = addMember.FirstName;
                addMemberData.LastName = addMember.LastName;
                if (addMember.MiddleName != null)
                {
                    addMemberData.MiddleName = addMember.MiddleName;
                }
                addMemberData.Suffix = addMember.Suffix;
                addMemberData.Gender = addMember.Gender;
                addMemberData.DateOfBirth = addMember.DateOfBirth;
                addMemberData.CreatedTimeStamp = DateTime.Now;
                addMemberData.LastModifiedTimeStamp = DateTime.Now;
                addMemberData.MetaActive = 1;
                addMemberData.ApplicationID = addMember.ApplicationID;
                status = mVCYorbitService.SaveAppId(addMemberData);
            }
            catch (Exception ex)
            {
                MVCYorbit.LOG.Logging(ex.Message);
                
            }
            return status;
        }

        public static int AgeCalculator(DateTime dateOfBirth)
        {
            int age;
            age = DateTime.Now.Year - dateOfBirth.Year;
            if (DateTime.Now.DayOfYear < dateOfBirth.DayOfYear)
                age -= 1;
            return age;
        }

        public string ApplicationID()
        {
            Random r = new Random();
            var x = r.Next(0, 1000000);
            string appID = x.ToString("000000");
            return appID;
        }

        public FileResult CreatePDF()
        {
                MemoryStream workStream = new MemoryStream();
                DateTime dTime = DateTime.Now;
                //file name to be created   
                string strPDFFileName = string.Format("SamplePdf" + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
                Document doc = new Document();
                doc.SetMargins(0f, 0f, 0f, 0f);
                //Create PDF Table with 5 columns  
                PdfPTable tableLayout = new PdfPTable(6);
                doc.SetMargins(0f, 0f, 0f, 0f);
                //Create PDF Table  

                //file will created in this path  
                string strAttachment = Server.MapPath("~/Images/" + strPDFFileName);


                PdfWriter.GetInstance(doc, workStream).CloseStream = false;
                doc.Open();
            try
            {

                //Add Content to PDF   
                doc.Add(Add_Content_To_PDF(tableLayout));

                // Closing the document  
                doc.Close();

                byte[] byteInfo = workStream.ToArray();
                workStream.Write(byteInfo, 0, byteInfo.Length);
                workStream.Position = 0;

            }
            catch (Exception ex)
            {
                MVCYorbit.LOG.Logging(ex.Message);
            }


            return File(workStream, "application/pdf", strPDFFileName);
        }

        protected PdfPTable Add_Content_To_PDF(PdfPTable tableLayout)
        {
            string uName = (string)Session["uName"];
            float[] headers = { 24, 24, 45, 35, 50 , 35 }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            tableLayout.HeaderRows = 1;
            //Add Title to the PDF file at the top  
            try
            {
                var members = mVCYorbitService.GetInfo(uName);
                string appID = mVCYorbitService.GetRelationAppID(uName);
                mVCYorbitService.AppIDinAddMembers(appID);
                tableLayout.AddCell(new PdfPCell(new Phrase("Application ID - " + appID, new Font(Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });


                ////Add header  
                AddCellToHeader(tableLayout, "Title");
                AddCellToHeader(tableLayout, "FirstName");
                AddCellToHeader(tableLayout, "LastName");
                AddCellToHeader(tableLayout, "Gender");
                AddCellToHeader(tableLayout, "DateOfBirth");
                AddCellToHeader(tableLayout, "RelationShip");

                ////Add body  

                foreach (var items in members)
                {
                    AddCellToBody(tableLayout, items.Suffix);
                    AddCellToBody(tableLayout, items.FirstName);
                    AddCellToBody(tableLayout, items.LastName);
                    AddCellToBody(tableLayout, items.Gender);
                    AddCellToBody(tableLayout, items.DateOfBirth.ToShortDateString());
                    AddCellToBody(tableLayout, items.Relation);
                }

            }
            catch (Exception ex)
            {
                MVCYorbit.LOG.Logging(ex.Message);
            }

            return tableLayout;
        }
        // Method to add single cell to the Header  
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.YELLOW)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(128, 0, 0)
    });
        }

        // Method to add single cell to the body  
        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.BLACK)))
             {
                HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
    });
        }
    }
}