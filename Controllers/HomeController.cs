using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UsersApplication;
using System.Runtime.Caching;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace UsersApplication.Controllers
{
    public class HomeController : Controller
    {

        List<UsersTable> data = MemoryCache.Default.Get("users") as List<UsersTable>;

        public ActionResult Index(string sortOrder, string searchString)
        {
            ViewBag.FirstNameSortParam = string.IsNullOrEmpty(sortOrder) ? "FirstName_desc" : "";
            ViewBag.LastNameSortParam = sortOrder == "LastName" ? "LastName_desc" : "LastName";
            ViewBag.MobileNumberSortParam = sortOrder == "MobileNumber" ? "MobileNumber_desc" : "MobileNumber";

            using (var db = new UserDataEntities())
            {
                IQueryable<UsersTable> users;

                // Cache System
                
                if (data == null) // test if Cache is empty
                {
                    users = from u in db.UsersTables
                            select u;
                    MemoryCache.Default.Add("users", users.ToList(), DateTimeOffset.Now.AddHours(24)); // add users to memory Cache if Cache is empty
                }
                else
                {
                    users = data.AsQueryable(); // retrieve users from Cache when users is already loaded to it
                }

                // Filtering by first name
                if (!string.IsNullOrEmpty(searchString))
                {
                    if (!string.IsNullOrWhiteSpace(searchString)) { 
                    users = users.Where(u => u.FirstName.ToLower().Contains(searchString.ToLower()));
                    }
                }

                //sorting
                switch (sortOrder)
                {
                    case "LastName":
                        users = users.OrderBy(u => u.LastName);
                        break;
                    case "LastName_desc":
                        users = users.OrderByDescending(u => u.LastName);
                        break;
                    case "MobileNumber":
                        users = users.OrderBy(u => u.MobileNumber);
                        break;
                    case "MobileNumber_desc":
                        users = users.OrderByDescending(u => u.MobileNumber);
                        break;
                    case "FirstName_desc":
                        users = users.OrderByDescending(u => u.FirstName);
                        break;
                    default:
                        users = users.OrderBy(u => u.FirstName);
                        break;
                }

                if (!string.IsNullOrEmpty(searchString) || searchString =="")
                {

                   // if (string.IsNullOrWhiteSpace(searchString))
                   // {
                   //     return View(users.ToList());
                   //}
                    
                    string[] user;

                    int usersCount = users.Count();
                    user = new string[usersCount * 3]; // Multiply by 3 for FirstName, LastName, and MobileNumber

                    int i = 0;
                    foreach (var u in users)
                    {
                        user[i] = u.FirstName;
                        user[i + 1] = u.LastName;
                        user[i + 2] = u.MobileNumber;
                        i += 3;
                    }
                    return Json(user);
                }


                    return View(users.ToList());
            }
        }


        public ActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddUser(UsersTable user)
        {
            using (var db = new UserDataEntities())
            {
                
                
                db.UsersTables.Add(user);
                db.SaveChanges();

                MemoryCache.Default.Remove("users"); // clear Cache so it will retrive updated Users

                var users = from u in db.UsersTables // load updated users to Cache
                        select u;
                MemoryCache.Default.Add("users", users, DateTimeOffset.Now.AddHours(24)); // add users to memory Cache if Cache is empty


            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Search(string searchInput)
        {
            using (var db = new UserDataEntities())
            {

                IQueryable<UsersTable> users;
                

                // Cache System

                if (data == null) // test if Cache is empty
                {
                    users = from u in db.UsersTables
                            select u;
                    MemoryCache.Default.Add("users", users.ToList(), DateTimeOffset.Now.AddHours(24)); // add users to memory Cache if Cache is empty
                }
                else
                {
                    users = data.AsQueryable(); // retrieve users from Cache when users is already loaded to it
                   
                }


                var searchInputLower = searchInput.ToLower();

                var result = users
                    .Where(u => u.FirstName.ToLower().Contains(searchInputLower) ||
                                u.LastName.ToLower().Contains(searchInputLower) ||
                                u.MobileNumber.Contains(searchInput))
                    .ToList();

                if (string.IsNullOrWhiteSpace(searchInput))
                {
                    result.Clear();
                }

                return View("Index", result);
            }
        }



        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}