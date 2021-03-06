﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ed.it.Models;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using System.Web.Hosting;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http.ExceptionHandling;
using System.Globalization;
using System.Threading;
using Microsoft.Office.Interop.PowerPoint;
using System.Drawing;
using Aspose.Slides;
using System.Configuration;
using DataTable = System.Data.DataTable;

namespace Ed.it.Controllers
{
    //public sealed class Presentation : IPresentation,
    //IPresentationComponent, IDisposable
    public class UserController : ApiController
    {
        bool Local = true;//עובדים על השרת או מקומי

        string UrlServer = "http://proj.ruppin.ac.il/igroup20/prod/uploadedPictures";//ניתוב שרת
        string UrlLocal = @"C:\Users\programmer\ed.it_client\public\uploadedPicturesPub\\";//ניתוב מקומי
        string UrlLocalAlmog = @"C:\Users\almog\Desktop\final project development\client\ed.it_client\public\uploadedPicturesPub\\";
        enum Activity { watch,like,addTag };//עדכון ניקוד תגיות בהתאם למקרה


        [HttpPost]
        [Route("api/User/GetUserDetails")]
        public User GetUserDetails([FromBody]User NewUser)//
        {
            try
            {
                NewUser = NewUser.GetUserDetails();
                return NewUser;//אם מחזיר Null אז משתמש הזין פרטים לא נכוננים
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
    
        }

    


        [HttpGet]
        [Route("api/User/GetTags")]
        public List<string> GetTags()
        {
            try
            {
                TagsUser tagsUser = new TagsUser();
                return tagsUser.GetTagsList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        [HttpGet]
        [Route("api/User/GetUsersForSearch")]
        public List<string> GetUsersForSearch()
        {
            try
            {
                User user= new User();
                return user.GetUserList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        /// <summary>
        /// יצירת יוזר חדש
        /// </summary>
        [HttpPost]
        [Route("api/User/CreateUser")]
        public bool CreateUser([FromBody]User NewUser)
        {
            try
            {
                NewUser.CreateUser();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        [HttpPost]
        [Route("api/AddPic/{Email}")]
        public HttpResponseMessage UploadPic(string Email)
        {
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            List<string> imageLinks = new List<string>();
            try
            {
                var httpContext = HttpContext.Current;

                // Check for any uploaded file  
                if (httpContext.Request.Files.Count > 0)
                {
                    //Loop through uploaded files  
                    for (int i = 0; i < httpContext.Request.Files.Count; i++)
                    {
                        HttpPostedFile httpPostedFile = httpContext.Request.Files[i];

                        // this is an example of how you can extract addional values from the Ajax call
                        string name = httpContext.Request.Form["user"];


                        if (httpPostedFile != null)
                        {
                            // Construct file save path  
                            //var fileSavePath = Path.Combine(HostingEnvironment.MapPath(ConfigurationManager.AppSettings["fileUploadFolder"]), httpPostedFile.FileName);
                            string fname = Email.Split('@').First() + "." + httpPostedFile.FileName.Split('\\').Last().Split('.').Last();//שם הקובץ יהיה שם משתמש
                            var fileSavePath="";
                            if (Local)
                            {
                                fileSavePath = Path.Combine(UrlLocalAlmog, fname);//אם עובדים לוקלי ישמור תמונות בתיקיית פבליק של הקליינט
                            }
                            else
                            {
                                //fileSavePath= Path.Combine(HostingEnvironment.MapPath("~/uploadedPicture"), fname);//אם עובדים על השרת שומרים תמונות בתיקייה של השרת

                                fileSavePath = Path.Combine(HostingEnvironment.MapPath("~/uploadedPictures"), fname);
                                
                            }
                            // Save the uploaded file  
                            httpPostedFile.SaveAs(fileSavePath);
                            //imageLinks.Add("uploadedPicture/" + fname);
                            imageLinks.Add("uploadedPictures/" + fname);
         

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); //Will display localized message
                ExceptionLogger el = new ExceptionLogger(ex);
                System.Threading.Thread t = new System.Threading.Thread(el.DoLog);
                t.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                t.Start();
            }
            return Request.CreateResponse(HttpStatusCode.Created, imageLinks);

        }

        [HttpPost]
        [Route("api/User/update/{UrlPicture}/{Email}/1")]
        public HttpResponseMessage UpdatePic(string UrlPicture, string Email)
        {
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            List<string> imageLinks = new List<string>();

         
            try
            {
                var httpContext = HttpContext.Current;

                // Check for any uploaded file  
                if (httpContext.Request.Files.Count > 0)
                {
                    //Loop through uploaded files  
                    for (int i = 0; i < httpContext.Request.Files.Count; i++)
                    {
                        HttpPostedFile httpPostedFile = httpContext.Request.Files[i];

                        // this is an example of how you can extract addional values from the Ajax call
                        string name = httpContext.Request.Form["user"];


                        if (httpPostedFile != null)
                        {
                            // Construct file save path  
                            //var fileSavePath = Path.Combine(HostingEnvironment.MapPath(ConfigurationManager.AppSettings["fileUploadFolder"]), httpPostedFile.FileName);
               
                            try
                            {
                                if (Local)
                                {
                                    if (File.Exists(Path.Combine(UrlLocalAlmog, UrlPicture)))
                                    {
                                        // If file found, delete it    
                                        File.Delete(Path.Combine(UrlLocalAlmog, UrlPicture));
                                        Console.WriteLine("File deleted.");
                                    }
                                    else Console.WriteLine("File not found");
                                }
                                // Check if file exists with its full path    
                               else
                                {
                                    if (File.Exists(Path.Combine(HostingEnvironment.MapPath("~/uploadedPictures"), UrlPicture)))
                                    {
                                        
                                        // If file found, delete it    
                                        File.Delete(Path.Combine(HostingEnvironment.MapPath("~/uploadedPictures"), UrlPicture));
                                        Console.WriteLine("File deleted.");
                                    }
                                    else Console.WriteLine("File not found");
                                }
                            }
                            
                            catch (IOException ioExp)
                            {
                                Console.WriteLine(ioExp.Message);
                            }

                            string fname = Email.Split('@').First() + "." + httpPostedFile.FileName.Split('\\').Last().Split('.').Last();//שם הקובץ יהיה שם משתמש
                             var fileSavePath = "";
                            if (Local)
                            {
                                fileSavePath = Path.Combine(UrlLocalAlmog, fname);//אם עובדים לוקלי ישמור תמונות בתיקיית פבליק של הקליינט
                            }
                            else
                            {
                                //fileSavePath = Path.Combine(HostingEnvironment.MapPath("~/uploadedPicture"), fname);//אם עובדים על השרת שומרים תמונות בתיקייה של השרת
                                fileSavePath = Path.Combine(HostingEnvironment.MapPath("~/uploadedPictures"), fname);
                                //fileSavePath = Path.Combine(UrlServer, fname);
                            }
                            // Save the uploaded file  
                            httpPostedFile.SaveAs(fileSavePath);
                            //imageLinks.Add("uploadedPicture/" + fname);

                            User u = new User();
                            u.UpdatePic(Email, fname);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); //Will display localized message
                ExceptionLogger el = new ExceptionLogger(ex);
                System.Threading.Thread t = new System.Threading.Thread(el.DoLog);
                t.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                t.Start();
            }
            return Request.CreateResponse(HttpStatusCode.Created, imageLinks);

        }

        [HttpPut]
        [Route("api/User/UpdateUser")]
        public int Put([FromBody]User NewUser)
        {
            try
            {
                int numEffected = NewUser.UpdateDetails();
                return numEffected;
            }       
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/User/TOPUserLikedContent/{UserName}")]
        public DataTable GetTOPUserLikedContent(string UserName)
        {
            User user = new User();
            DBservices dbs = new DBservices();
            dbs = user.GetTOPUserLikedContent(UserName);
            return dbs.dt;
        }

        /// <summary>
        /// עדכון ניקוד עבור יוזר בעקבות צפיה בתוכן
        /// </summary>
        [HttpPut]
        [Route("api/User/UpdateScore/{ContentID}/{UserName}/{Case}")]
        public void UpdateScore(int ContentID, string UserName,string Case)
        {
            User user = new Models.User();
            user.UpdateScore(Case, UserName,ContentID);
        }

        [HttpGet]
        [Route("api/User/GetUserProfile/{UserName}")]
        public User GetUserProfile(string UserName)
        {
            User user = new User();
            return user.GetUserProfile(UserName);

        }

        //שליפת כל היוזרים
        [HttpGet]
        [Route("api/User/GetUsers2")]
        public List<User> GetUsers2()
        {
            User user = new User();
            return user.GetUsers2();

        }

        //חסימת או שחרור של מתשמש
        [HttpPost]
        [Route("api/User/Block/{UserName}/{Blocked}")]
        public int Block (string UserName, string Blocked)
        {
            User user = new User();
            return user.Block(UserName, Blocked);
        }


        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }



        class ExceptionLogger
        {
            Exception _ex;

            public ExceptionLogger(Exception ex)
            {
                _ex = ex;
            }

            public void DoLog()
            {
                Console.WriteLine(_ex.ToString()); //Will display en-US message
            }
        }
    }
}
