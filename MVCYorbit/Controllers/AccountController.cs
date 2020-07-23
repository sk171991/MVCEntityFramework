using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCYorbit.Models;

namespace MVCYorbit.Controllers
{
    public class AccountController : Controller
    {
        readonly MVCService.MVCYorbitServiceClient mVCYorbitService = new MVCService.MVCYorbitServiceClient();
        [HttpGet]
        public ActionResult UserAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserAccount(UserAccount userAccount)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (mVCYorbitService.ValidateRegistration(userAccount.Username, userAccount.Email) == 1)
                    {
                        ViewBag.Message = "Users already Exits !!! Please try again with different Username and Email";
                        return View();
                    }
                    else
                    {
                        try
                        {

                            MVCService.Account account = new MVCService.Account
                            {
                                Email = userAccount.Email,
                                Password = userAccount.Password,
                                UserName = userAccount.Username,
                                UserType = 2,
                                CreatedTimeStamp = DateTime.Now,
                                LastModifiedTimeStamp = DateTime.Now,
                                MetaActive = 1
                            };

                            if (mVCYorbitService.Register(account) == "success")
                            {
                                ViewBag.Message = "success";
                            }
                            else
                            {
                                ViewBag.Message = "failure";
                            }
                        }
                        catch (Exception ex)
                        {

                            ViewBag.Message = ex.Message;
                        }

                    }
                }
                catch (Exception ex)
                {
                    MVCYorbit.LOG.Logging(ex.Message);
                }
            }

            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserAccount userAccount)
        {
            string uName = userAccount.Username;
            string password = userAccount.Password;
            try
            {
                if (mVCYorbitService.Validate(uName, password) == 1)
                {
                    string userRole = mVCYorbitService.GetUser(uName, password);
                    Session["Role"] = userRole;
                    Session["uName"] = uName;
                    return RedirectToAction("HomePage", "Home");

                }
                else
                {
                    ViewBag.Message = "Wrong UserName/Password . Please try again !!";
                   
                }
            }
            catch (Exception ex)
            {
                MVCYorbit.LOG.Logging(ex.Message);
               
            }
            return View();
        }

       
    }
}