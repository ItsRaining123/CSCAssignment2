using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CSCAssignment2.Models;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using CSCAssignment2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using CSCAssignment2.Helpers;
using ExamScriptTS.Models;

namespace CSCAssignment2.Controllers
{
    public class HomeController : Controller
    {
        private IUserService _userService;
        private readonly AppSettings _appSettings;
        private CSCAssignment2DbContext _database;
        private ILogger<HomeController> _logger;

        public HomeController(
                  IUserService userService,
                  IOptions<AppSettings> appSettings,
            CSCAssignment2DbContext database,
            ILogger<HomeController> logger)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
            _database = database;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult LoginPage()
        {
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm] IFormCollection data)
        {//Referred to the code at this GitHub repo
         //https://github.com/minato128/aspnet-core20-auth-sample/blob/master/WebApplication6/Controllers/HomeController.cs
         //I am avoiding the usage of the ViewModel. You can go through the online
         //tutorials to replace the logic here by using ViewModel.
            if ((data["inputUserId"].ToString().Trim() == "") || (data["inputPassword"].ToString().Trim() == ""))
            {
                //Make a ViewBag
                //ViewBag lifecycle only last for this request cycle.
                ViewBag.Message = "User ID or password is missing";
                return View();
            }
            //Try remove await keyword from the expression below.
            //You will see error highlights at the var claimsIdentity section....
            var user = await _userService.AuthenticateAsync(data["inputUserId"], data["inputPassword"]);

            if (user == null)
            {
                //Make a ViewBag
                ViewBag.Message = "User ID or password is wrong";
                return View();
            }

            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            /********************************************************************/
            //The following block of code has been added to support token storage in Browser cookie
            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString() ),
                new Claim("fullName", user.FullName.ToString()),
                new Claim("userid", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
             }, "CookieAuthenticationScheme");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await Request.HttpContext.SignInAsync("CookieAuthenticationScheme", claimsPrincipal);
            /********************************************************************/

            if (user.Role.RoleName.Contains("FreePlanUser"))
            {
                return Redirect("/Home/Index");
            }
            if (user.Role.RoleName.Contains("PaidPlanUser"))
            {
                return Redirect("/Talents/ViewTalents");
            }
            return Redirect("/");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuthenticationScheme");
            return Redirect("/Home/Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
