using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CSCAssignment2.Controllers
{
    public class TalentsController : Controller
    {
        public IActionResult CreateTalent()
        {
            return View();
        }
        public IActionResult UpdateTalent()
        {
            return View();
        }

        [Authorize(Roles = "PaidPlanUser")]
        public IActionResult ViewTalents()
        {
            return View();
        }

        public IActionResult UploadTalents()
        {
            return View();
        }

    }
}
