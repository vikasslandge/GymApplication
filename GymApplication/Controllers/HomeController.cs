using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GymApplication.Models;

namespace GymApplication.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        //Database entities object of GymModel
        GymDataBaseEntities entities = new GymDataBaseEntities();
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [HandleError(View ="Error1")]
        public ActionResult Login(LoginDetail loginDetail)
        {
            //throw new Exception();  
            var loginVaildate = entities.LoginDetails.Where(s => s.UserName == loginDetail.UserName 
            && s.Password == loginDetail.Password).FirstOrDefault();
            if (loginVaildate!=null)
            {
                if (loginVaildate.Role=="user")
                {
                    Session["UserName"] = loginVaildate.UserName;
                    Session["Password"] = loginVaildate.Password;    
                    return RedirectToAction("UserHome");
                }
                if (loginVaildate.Role == "admin")
                {
                    Session["UserName"] = loginVaildate.UserName;
                    Session["Password"] = loginVaildate.Password;

                    return RedirectToAction("AdminHome");
                }
            }
            return View();
        }
        //Session logout
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index");
        }
        //User Section Code
        public ActionResult UserHome()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index");
            }
            else  
            {
                return View();
            }
        }
        public  PartialViewResult Register()
        {
            ViewBag.Scheme = new SelectList(entities.SchemeDetails.ToList(), "SchemeId", "SchemeName");
            ViewBag.Plan = new SelectList(entities.PlanDetails.ToList(), "PlanId", "PlanName");
            return PartialView("_RegisterMember");
        }
        [HttpPost]
        public ActionResult Register(FormCollection memberDetail)
        {
            MemberDetail member = new MemberDetail()
            {
                MemberName = memberDetail["MemberName"],
                PlanID =Convert.ToInt32( memberDetail["Plan"]),
                SchemeID =Convert.ToInt32( memberDetail["Scheme"]),
                PlanStartDate =Convert.ToDateTime( memberDetail["PlanStartDate"]),
               
               
            };
            entities.MemberDetails.Add(member);
            entities.SaveChanges();
            var months = entities.PlanDetails.Where(d => d.PlanID == member.PlanID).Select(s => s.PlanDuration).Single();

            ViewBag.amount=Convert.ToDouble(entities.GetTotalAmount(Convert.ToInt32(member.MemberID)).Single());
            member.Amount = ViewBag.amount;
             member.PlanEndDate = Convert.ToDateTime(memberDetail["PlanStartDate"]).AddMonths(Convert.ToInt32( months));
            ViewBag.lastDate = member.PlanEndDate;
            entities.SaveChanges();
            return View("UserHome");
        }
        //Admin section code
        public ActionResult AdminHome()
        {
            if (Session["UserName"]==null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
         
        }
        [HttpPost]
        public ActionResult UpdateScheme(SchemeDetail schemeDetail)
        {
            SchemeDetail scheme = new SchemeDetail();
            scheme.SchemeName = schemeDetail.SchemeName;
            scheme.MonthlyFees = schemeDetail.MonthlyFees;
       
            entities.SchemeDetails.Add(scheme);
            entities.SaveChanges();
            return  View("AdminHome");
        }
        public PartialViewResult Scheme()
        {
            return PartialView("_AddScheme");
        }
        public PartialViewResult Plan()
        {
            return PartialView("_AddPlan");
        }
        [HttpPost]
        public ActionResult AddPlan(PlanDetail planDetail)
        {
            PlanDetail plan = new PlanDetail();
            plan.PlanName = planDetail.PlanName;
            plan.PlanDuration = planDetail.PlanDuration;
        
            entities.PlanDetails.Add(plan);
            entities.SaveChanges();
            return View("AdminHome");
        }
        public PartialViewResult AddUser()
        {
            return PartialView("_AddUser");
        }
        [HttpPost]
        public ActionResult AddUser(LoginDetail loginDetail)
        {
            LoginDetail login = new LoginDetail();
            login.UserName = loginDetail.UserName;
            login.Password = loginDetail.Password;
            login.Role = loginDetail.Role;
            entities.LoginDetails.Add(login);
            entities.SaveChanges();
            return View("AdminHome");
        }
        public PartialViewResult DeleteUser()
        {
            return PartialView("_DeleteUser",entities.LoginDetails);
        }
        
        public ActionResult Delete(int id )
        {
            
            var user = entities.LoginDetails.Find(id);
            entities.LoginDetails.Remove(user);
            entities.SaveChanges();
            return View("AdminHome");
        }
        public PartialViewResult Edit(int id)
        {
            var user = entities.LoginDetails.Find(id);
            return PartialView("_EditUser", user);
        }
        [HttpPost]
        public ActionResult Edit(LoginDetail loginDetail)
        {

            var user = entities.LoginDetails.Find(loginDetail.UserID);
            user.UserName = loginDetail.UserName;
            user.Password = loginDetail.Password;
            user.Role = loginDetail.Role;
            entities.SaveChanges();
            return View("AdminHome");
        }
    }
}
